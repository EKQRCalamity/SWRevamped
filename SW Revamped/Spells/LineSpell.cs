using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.InputProviders;
using Oasys.Common.Tools.Devices;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Utility;

using static Oasys.SDK.Prediction.MenuSelected;
using Oasys.SDK.Tools;

namespace SWRevamped.Spells
{
    internal class LineSpell : SpellBase
    {
        internal bool Execute = false;
        internal bool Friendly = false;
        internal EffectCalc Calculator;
        internal SpellCastMode SpellCastMode;

        internal bool InCombo = false;

        internal ModeDisplay hitChanceDisplay;
        internal Counter minMana;
        internal Switch useHarass;
        internal Switch useLaneclear;
        internal Switch useLasthit;
        internal Switch drawRange;
        internal ModeDisplay drawColorDisplay;
        internal Group rangeDrawGroup = new Group("Range Drawings");
        internal Counter MinUsageDist = new Counter("Min Usage Dist", 800, 0, 3000);

        internal Color color => Colors.NameToColor(drawColorDisplay.SelectedModeName);
        internal HitChance hitChance => GetHitchanceFromName(hitChanceDisplay.SelectedModeName);

        internal CollisionCheck collisionCheck;
        internal Func<GameObjectBase, Vector3> SourcePosition;
        internal Func<GameObjectBase, bool> SelfCheck;
        internal Func<GameObjectBase, bool> TargetCheck;


        internal LineSpell(CastSlot castSlot, EffectCalc calculator, int width, int range, int speed, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, SharpDX.Color drawColor, int minmana, CollisionCheck collCheck, HitChance minHitChance, bool laneclear = false, bool lasthit = false, bool harass = false, float casttime = 0.0f, bool execute = false, int drawPrio = 5, bool friendlySpell = false, SpellCastMode spellCastMode = SpellCastMode.Spam)
        {
            collisionCheck = collCheck;
            Friendly = friendlySpell;
            SourcePosition = sourcePosition;
            SelfCheck = selfCheck;
            TargetCheck = targetCheck;
            Slot = Utility.Slots.CastSlotToSpellSlot(castSlot);
            SpellCastSlot = castSlot;
            Width = width;
            Range = range;
            Speed = speed;
            CastTime = casttime;
            Execute = execute;
            Calculator = calculator;
            SpellCastMode = spellCastMode;

            // Menu stuff
            MainTab = Getter.MainTab;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            drawColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = Oasys.SDK.ColorConverter.GetColors(), SelectedModeName = $"{Colors.ColorToName(drawColor)}" };
            hitChanceDisplay = new ModeDisplay() { Title = "Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = GetNameFromHitchance(minHitChance) };
            minMana =  new Counter("Min Mana", minmana, 0, 1000);
            drawRange = new Switch("Draw Range", true);

            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            SpellGroup.AddItem(hitChanceDisplay);
            SpellGroup.AddItem(minMana);

            if (collisionCheck.MinCollisionObjectsCounter.Value > 0)
                SpellGroup.AddItem(collisionCheck.MinCollisionObjectsCounter);

            useLaneclear = new Switch("Laneclear", laneclear);
            useLasthit = new Switch("Lasthit", lasthit);
            useHarass = new Switch("Harass", harass);

            if (laneclear)
            {
                SpellGroup.AddItem(useLaneclear);
            }
            if (lasthit)
            {
                SpellGroup.AddItem(useLasthit);
            }
            if (harass)
            {
                SpellGroup.AddItem(useHarass);
            }
            if (Range < 3000)
            {
                rangeDrawGroup.AddItem(drawRange);
                rangeDrawGroup.AddItem(drawColorDisplay);
                SpellGroup.AddItem(rangeDrawGroup);
            } else
            {
                SpellGroup.AddItem(MinUsageDist);
            }

            Effect effect = new Effect($"{SpellSlotToString()}", true, drawPrio, Range, MainTab, SpellGroup, Calculator, drawColor);
            EffectDrawer.Add(effect, Friendly);

            CoreEvents.OnCoreMainInputAsync += ComboInput;
            if (spellCastMode == SpellCastMode.BeforeAutoAttack)
            {
                CoreEvents.OnCoreMainInputBeforeBasicAttackAsync += BeforeAuto;
            } else if (spellCastMode == SpellCastMode.AfterAutoAttack)
            {
                Orbwalker.OnOrbwalkerAfterBasicAttack += AfterAuto;
            }
            CoreEvents.OnCoreLasthitInputAsync += LasthitInput;
            CoreEvents.OnCoreHarassInputAsync += HarassInput;
            CoreEvents.OnCoreLaneclearInputAsync += LaneclearInput;
            CoreEvents.OnCoreRender += Render;
            KeyboardProvider.OnKeyPress += OnPress;

        }

        private PredOut? PredictSpell(GameObjectBase target, int minHits, int maxHits, CollisionModes mainTargetMode = CollisionModes.HeroMinion)
        {
            if (target == null || !target.IsVisible) return null;
            Prediction.MenuSelected.PredictionOutput pred = GetPrediction(
                    PredictionType.Line,
                    target,
                    Range,
                    Width,
                    CastTime,
                    Speed,
                    (mainTargetMode == CollisionModes.None)? false : true
                );
            if (maxHits == 0)
                maxHits = 1;
            if (mainTargetMode != CollisionModes.None)
            {
                if (mainTargetMode == CollisionModes.Hero)
                {
                    PredOut output = new PredOut(
                            pred,
                            target,
                            true
                        );
                    if ((pred.HitChance >= hitChance)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Where(x => x.IsObject(ObjectTypeFlag.AIHeroClient)).Count() + 1 > minHits) : true)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Where(x => x.IsObject(ObjectTypeFlag.AIHeroClient)).Count() + 1 <= maxHits) : true)
                        && ((Range >= 3000) ? target.Distance >= MinUsageDist.Value : true))
                    {
                        output.Failed = false;
                    }
                    else
                    {
                        output.Failed = true;
                    }
                    return output;
                }
                else if (mainTargetMode == CollisionModes.Minion)
                {
                    PredOut output = new PredOut(
                            pred,
                            target,
                            true
                        );
                    if ((pred.HitChance >= hitChance)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Where(x => x.IsObject(ObjectTypeFlag.AIMinionClient)).Count() + 1 > minHits) : true)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Where(x => x.IsObject(ObjectTypeFlag.AIMinionClient)).Count() + 1 <= maxHits) : true)
                        && ((Range >= 3000) ? target.Distance >= MinUsageDist.Value : true))
                    {
                        output.Failed = false;
                    }
                    else
                    {
                        output.Failed = true;
                    }
                    return output;
                }
                else
                {

                    PredOut output = new PredOut(
                            pred,
                            target,
                            true
                        );
                    if ((pred.HitChance >= hitChance)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Count() + 1 > minHits) : true)
                        && ((collisionCheck.Collision) ? (pred.CollisionObjects.Count() + 1 <= maxHits) : true)
                        && ((Range >= 3000) ? target.Distance >= MinUsageDist.Value : true))
                    {
                        output.Failed = false;
                    }
                    else
                    {
                        output.Failed = true;
                    }
                    return output;
                }
            }
            else
            {
                return new PredOut(
                        pred,
                        target,
                        (pred.HitChance >= hitChance) ? false : true
                    );
            }
        }

        private int GetPoints(GameObjectBase target)
        {
            return Utility.ChampValueCalc.GetEnemyPointsExtended(target, Calculator, Range, SourcePosition);
        }

        private List<PredOut> PredictSpellAllList(CollisionModes mainCollMode = CollisionModes.HeroMinion, TargetModes mainTargetMode = TargetModes.Hero)
        {

            List<PredOut> preds = new List<PredOut>();
            if (mainTargetMode == TargetModes.Hero
                || mainTargetMode == TargetModes.HeroMinion)
            {
                foreach (GameObjectBase obj in (Friendly) ? UnitManager.AllyChampions : UnitManager.EnemyChampions)
                {
                    Coll? MinColl = collisionCheck.CollisionObjects.Where(x => x.Logic == CollLogic.Min).FirstOrDefault();
                    Coll? MaxColl = collisionCheck.CollisionObjects.Where(x => x.Logic == CollLogic.Max).FirstOrDefault();
                    PredOut? predOut = PredictSpell(obj, (MinColl != null) ? MinColl.CollisionCounter.Value : 0, (MaxColl != null) ? MaxColl.CollisionCounter.Value : 0, mainCollMode);
                    if (predOut != null)
                        preds.Add(predOut);
                }
            }
            if (mainTargetMode == TargetModes.Minion
                || mainTargetMode == TargetModes.HeroMinion)
            {
                foreach (GameObjectBase obj in (Friendly) ? UnitManager.AllyMinions : UnitManager.EnemyMinions)
                {
                    Coll? MinColl = collisionCheck.CollisionObjects.Where(x => x.Logic == CollLogic.Min).FirstOrDefault();
                    Coll? MaxColl = collisionCheck.CollisionObjects.Where(x => x.Logic == CollLogic.Max).FirstOrDefault();
                    PredOut? predOut = PredictSpell(obj, (MinColl != null) ? MinColl.CollisionCounter.Value : 0, (MaxColl != null) ? MaxColl.Num : 0, mainCollMode);
                    if (predOut != null)
                        preds.Add(predOut);

                }
            }
            HitChance highestHitChance = HitChance.Impossible;
            foreach (PredOut prediction in preds)
            {
                if (!prediction.Failed && prediction.Prediction.HitChance > highestHitChance)
                {
                    highestHitChance = prediction.Prediction.HitChance;
                }
            }
            return preds.Where(x => !x.Failed).ToList();
        }

        private PredOut? PredictSpellAll(CollisionModes mainCollMode = CollisionModes.HeroMinion, TargetModes mainTargetMode = TargetModes.Hero)
        {
            List<PredOut> preds = PredictSpellAllList(mainCollMode, mainTargetMode);
            HitChance highestHitChance = HitChance.Impossible;
            foreach (PredOut prediction in preds)
            {
                if (!prediction.Failed && prediction.Prediction.HitChance > highestHitChance)
                {
                    highestHitChance = prediction.Prediction.HitChance;
                }
            }
            return preds.Where(x => !x.Failed && (GetPoints(x.Target) > 70 ? (int)x.Prediction.HitChance - 2 >= (int)highestHitChance : x.Prediction.HitChance == highestHitChance) && TargetCheck(x.Target) && x.Target.IsVisible).OrderByDescending(x => GetPoints(x.Target)).FirstOrDefault();
        }

        private void AfterAuto(float gameTime, GameObjectBase target)
        {
            if (SpellCastMode != SpellCastMode.AfterAutoAttack
                || !SpellIsReady()
                || !InCombo
                || !IsOn)
                return;

            PredOut? bestTarget = PredictSpellAll();
            if (bestTarget != null
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SpellIsReady()
                && SelfCheck(Getter.Me())
                && Getter.Me().Distance(bestTarget.Target) < Getter.AARange)
            {
                // Handle execute spells
                if ((!Execute) ? true : Calculator.CanKill(bestTarget.Target))
                {
                    SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
                }
            }
        }

        private Task BeforeAuto()
        {
            if (SpellCastMode != SpellCastMode.BeforeAutoAttack
                || !SpellIsReady()
                || !InCombo
                || !IsOn)
                return Task.CompletedTask;

            PredOut? bestTarget = PredictSpellAll();
            if (bestTarget != null
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SpellIsReady()
                && SelfCheck(Getter.Me())
                && Getter.Me().Distance(bestTarget.Target) < Getter.AARange)
            {
                // Handle execute spells
                if ((!Execute) ? true : Calculator.CanKill(bestTarget.Target))
                {
                    SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
                }
            }
            return Task.CompletedTask;
        }

        private void OnPress(Keys keyBeingPressed, Keyboard.KeyPressState pressState)
        {
            if (pressState == Keyboard.KeyPressState.Down && keyBeingPressed == Oasys.Common.Settings.Orbwalker.GetComboKey() && !InCombo)
            {
                InCombo = true;
            } else if (pressState == Keyboard.KeyPressState.Up && keyBeingPressed == Oasys.Common.Settings.Orbwalker.GetComboKey() && InCombo)
            {
                InCombo = false;
            }
        }

        private void Render()
        {
            if (!Getter.Me().IsAlive || !drawRange.IsOn || Range >= 3000) return;
            RenderFactory.DrawNativeCircle(Getter.Me().Position, Range, color, 2); 
        }

        private Task ComboInput()
        {
            if (!IsOn)
                return Task.CompletedTask;
            PredOut? bestTarget = PredictSpellAll();
            //if (bestTarget != null)
            //    Logger.Log($"Pred: [{bestTarget.Target}, {bestTarget.Failed}, {bestTarget.Prediction.CollisionObjects.Count()}]");
            if (bestTarget != null 
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SpellIsReady()
                && SelfCheck(Getter.Me())
                && (SpellCastMode == SpellCastMode.Spam || (
                    Getter.Me().Distance(bestTarget.Target) >= Getter.AARange
                )))
            {
                // Handle execute spells
                if ((!Execute)? true : Calculator.CanKill(bestTarget.Target))
                {
                    SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
                }
            }
            return Task.CompletedTask;
        }
        private Task HarassInput()
        {
            if (!IsOn || !useHarass.IsOn)
                return Task.CompletedTask;
            PredOut? bestTarget = PredictSpellAll();
            if (bestTarget != null
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SelfCheck(Getter.Me())
                && SpellIsReady())
            {
                // Handle execute spells
                if ((!Execute) ? true : Calculator.CanKill(bestTarget.Target))
                {
                    SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
                }
            }
            return Task.CompletedTask;
        }

        private Task LaneclearInput()
        {
            if (!IsOn || !useLaneclear.IsOn)
                return Task.CompletedTask;
            PredOut? bestTarget = PredictSpellAll(CollisionModes.HeroMinion, TargetModes.Minion);
            if (bestTarget != null
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SpellIsReady()
                && SelfCheck(Getter.Me())
                && !Execute)
            {
                SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
            }
            return Task.CompletedTask;
        }


        private Task LasthitInput()
        {
            if (!IsOn || !useLasthit.IsOn)
                return Task.CompletedTask;
            PredOut? bestTarget = PredictSpellAll(CollisionModes.HeroMinion, TargetModes.Minion);
            if (bestTarget != null
                && !bestTarget.Failed
                && bestTarget.Prediction.HitChance >= hitChance
                && SpellIsReady()
                && SelfCheck(Getter.Me())
                && !Execute)
            {
                if (Calculator.CanKill(bestTarget.Target))
                    SpellCastProvider.CastSpell(SpellCastSlot, bestTarget.Prediction.CastPosition, CastTime);
            }
            return Task.CompletedTask;
        }
    }
}