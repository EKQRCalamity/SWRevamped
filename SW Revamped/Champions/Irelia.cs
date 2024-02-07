using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Spells;
using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal sealed class IreliaQDamageCalc : EffectCalc
    {
        internal static int[] Base = { 0, 5, 25, 45, 65, 85 };
        internal static float ADScaling = 0.6F;
        internal static int MinionDamage => 43 + (12 * Getter.Me().Level);

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = Base[Getter.QLevel];
                damage += ADScaling * Getter.TotalAD;
                damage += (target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient) ? MinionDamage : 0);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage) + CalculatorEx.CalculateOnHit(Getter.Me(), target, true);
            }
            return damage;
        }
    }

    internal sealed class IreliaQHealCalc : EffectCalc
    {
        internal static float[] Scaling = { 0, 0.09F, 0.1F, 0.11F, 0.12F, 0.13F };

        internal override float GetValue(GameObjectBase target)
        {
            return (target.IsMe ? Scaling[Getter.QLevel] * Getter.TotalAD : 0);
        }
    }

    internal sealed class IreliaECalc : EffectCalc
    {
        internal static int[] Base = { 0, 80, 125, 170, 215, 260 };
        internal static float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = Base[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class IreliaRCalc : EffectCalc
    {
        internal static int[] Base = { 0, 125, 250, 375 };
        internal static float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = Base[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Irelia : ChampionModule
    {
        Utility.Pathfinding pathfinder = new Utility.Pathfinding();

        internal Tab MainTab = new Tab("SW - Irelia");

        internal Group QGroup = new Group("Q - Settings");
        internal Switch QIsOn = new Switch("Enabled", true);
        internal Switch QAllowTower = new Switch("Allow Under Tower", false);
        internal Switch QSkipCheck = new Switch("Skip Check", false);
        internal Switch QHPGapClose = new Switch("Low HP Gap Close", true);
        internal Switch QUseForHeal = new Switch("Use Q to heal", true);
        internal Group QHealGroup = new Group("Heal");
        internal Group QLaneClearGroup = new Group("Laneclear");
        internal Switch LaneclearIsOn = new Switch("Enabled", true);
        internal Switch QLaneClearCombo = new Switch("Combo", true);
        internal Switch QLaneClearMaxStacks = new Switch("W/ Max Stacks", true);

        internal Group DrawingsGroup = new Group("Drawings");
        internal Switch QPathIsOn = new Switch("Show Q Path", true);
        internal Group EDrawingsGroup = new Group("Drawings");
        internal Group RDrawingsGroup = new Group("Drawings");

        internal Group EGroup = new Group("E - Settings");
        internal Switch EIsOn = new Switch("Enabled", true);
        internal ModeDisplay EMode = new ModeDisplay() { Title = "Mode", ModeNames = { "Short&Easy", "Interpolation", "Bezier", "Pred" }, SelectedModeName = "Pred" };

        internal Group RGroup = new Group("R - Settings");
        internal Counter MinHits = new Counter("Min Hits", 2, 1, 5);
        internal Switch RIsOn = new Switch("Enabled", true);
        internal Counter RdTargetHealth = new Counter("Target Health% below", 40, 0, 100);
        internal Switch Use1vs1 = new Switch("Use 1vs1", true);
        internal Group R1vs1Group = new Group("1vs1");
        internal Counter RTargetHealthAbove = new Counter("Target Health% above", 20, 0, 100);
        internal Counter RTargetHealth = new Counter("Target Health% below", 40, 0, 100);
        internal Counter RMeHealthAbove = new Counter("My Health% above", 20, 0, 100);
        internal Counter RMeHealthBelow = new Counter("My Health% below", 60, 0, 100);
        internal InfoDisplay info = new InfoDisplay() { Title = "Notice", Information = "Min Hits is in 1vs1" };

        internal static int QRange = 600;
        internal int QSpeed => 1400 + (int)Getter.Me().UnitStats.MoveSpeed;

        internal static int ERange = 775;
        internal static int ESpeed = 2000;

        internal static int RRange = 1000;
        internal static int RSpeed = 2000;
        internal static int RWidth = 320;

        internal List<GameObjectBase> currentPath = new();

        IreliaQDamageCalc QCalc = new IreliaQDamageCalc();
        IreliaQHealCalc QHealCalc = new IreliaQHealCalc();
        IreliaECalc ECalc = new IreliaECalc();
        IreliaRCalc RCalc = new IreliaRCalc();

        internal bool EnemyInAARange(int offset = 0)
        {
            return NEnemiesInAARange(offset) > 0;
        }

        internal int NEnemiesInAARange(int offset = 0)
        {
            return UnitManager.EnemyChampions.Where(x => x.Distance < Getter.AARange + offset).Count();
        }

        internal bool HasMark(GameObjectBase target)
        {
            var buff = target.BuffManager.GetBuffByName("ireliamark", false, true);
            return buff != null && buff.IsActive && buff.Stacks >= 1;
        }

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            MainTab.AddItem(QGroup);
            QGroup.AddItem(QIsOn);
            QGroup.AddItem(QAllowTower);
            QGroup.AddItem(QSkipCheck);
            QGroup.AddItem(QHPGapClose);
            QGroup.AddItem(QUseForHeal);
            QGroup.AddItem(QLaneClearGroup);
            QGroup.AddItem(QHealGroup);
            QGroup.AddItem(DrawingsGroup);
            DrawingsGroup.AddItem(QPathIsOn);
            QLaneClearGroup.AddItem(LaneclearIsOn);
            QLaneClearGroup.AddItem(QLaneClearCombo);
            QLaneClearGroup.AddItem(QLaneClearMaxStacks);
            MainTab.AddItem(EGroup);
            EGroup.AddItem(EIsOn);
            EGroup.AddItem(EMode);
            EGroup.AddItem(EDrawingsGroup);
            MainTab.AddItem(RGroup);
            RGroup.AddItem(RIsOn);
            RGroup.AddItem(MinHits);
            RGroup.AddItem(RdTargetHealth);
            RGroup.AddItem(Use1vs1);
            RGroup.AddItem(R1vs1Group);
            R1vs1Group.AddItem(RTargetHealthAbove);
            R1vs1Group.AddItem(RTargetHealth);
            R1vs1Group.AddItem(RMeHealthAbove);
            R1vs1Group.AddItem(RMeHealthBelow);
            RGroup.AddItem(RDrawingsGroup);
            RGroup.AddItem(info);

            EffectDrawer.Init();

            Effect QDmg = new Effect($"Q", true, 5, 10000, MainTab, DrawingsGroup, QCalc, Color.Blue);
            Effect QHeal = new Effect($"Q", true, 5, 10000, MainTab, QHealGroup, QHealCalc, Color.Green);
            Effect EDmg = new Effect($"E", true, 6, 10000, MainTab, EDrawingsGroup, ECalc, Color.Red);
            Effect RDmg = new Effect($"R", true, 7, 10000, MainTab, RDrawingsGroup, RCalc, Color.Orange);

            EffectDrawer.AddDamage(QDmg);
            EffectDrawer.AddBuff(QHeal);
            EffectDrawer.AddDamage(EDmg);
            EffectDrawer.AddDamage(RDmg);

            CoreEvents.OnCoreMainInputAsync += QSpell;
            CoreEvents.OnCoreLaneclearInputAsync += QLaneClear;
            CoreEvents.OnCoreMainInputAsync += ESpell;
            CoreEvents.OnCoreMainInputAsync += RSpell;

            CoreEvents.OnCoreRender += CalculatePath;
            CoreEvents.OnCoreMainTick += Tick;
        }

        private Task RSpell()
        {
            if (Getter.RLooseReady && RIsOn.IsOn && !Getter.Me().BuffManager.HasActiveBuff("ireliawdefense") && Getter.Me().Mana > 100)
            {
                GameObjectBase target = TargetSelector.GetBestHeroTarget(null, x => x.Distance < (RRange - 50));
                if (target != null)
                {
                    if (Use1vs1.IsOn)
                    {
                        if (target.HealthPercent > RTargetHealthAbove.Value && target.HealthPercent < RTargetHealth.Value && Getter.Me().HealthPercent < RMeHealthBelow.Value && Getter.Me().HealthPercent > RMeHealthAbove.Value)
                        {
                            Prediction.MenuSelected.PredictionOutput prediction = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, RRange - 50, RWidth, 0.4F, RSpeed, false);
                            if (prediction.HitChance > Prediction.MenuSelected.HitChance.High)
                            {
                                SpellCastProvider.CastSpell(CastSlot.R, prediction.CastPosition, 0.4F);
                            }
                        }
                    }
                    if (Getter.RLooseReady && target.HealthPercent < RdTargetHealth.Value)
                    {
                        Prediction.MenuSelected.PredictionOutput prediction = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, RRange - 50, RWidth, 0.4F, RSpeed, true);
                        if (prediction.HitChance > Prediction.MenuSelected.HitChance.High && prediction.CollisionObjects.Count > MinHits.Value)
                        {
                            SpellCastProvider.CastSpell(CastSlot.R, prediction.CastPosition);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        internal static int tick = 0;

        private Task Tick()
        {
            if (tick == 0 || tick % 3 == 0)
            {
                List<GameObjectBase> path = QPathFinder();
                currentPath = path;
            }
            tick++;
            return Task.CompletedTask;
        }

        private static Vector2 GetPointOnCircumference(Vector2 origin, int radius, double angle)
        {
            return new Vector2((float)(origin.X + (radius * Math.Cos(angle))), (float)(origin.Y + (radius * Math.Sin(angle))));
        }

        internal static int a = 180;
        internal static int GetAngle()
        {
            if (a == 180)
            {
                a = 360;
            } else
            {
                a = 180;
            } 
            return a;
        }

        private static GameObjectBase EObj => UnitManager.AllNativeObjects.Where(x => x.Name.Contains("E_Blades", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        
        
        private static bool firstCast = false;
        private Vector2 GetEPositionBezier(GameObjectBase player, GameObjectBase target, float t)
        {
            Vector2 playerPosition = player.Position.ToW2S();
            Vector2 targetPosition = target.Position.ToW2S();
            Vector2 intermediatePosition = new();
            if (!(EObj != null) && !firstCast)
            {
                firstCast = true;
                intermediatePosition = Vector2.Lerp(playerPosition, targetPosition, t).Extend(target, 100);
            } else if (firstCast && EObj != null)
            {
                intermediatePosition = EObj.Position.Extend(target, -100);
                firstCast = false;
            }
            return intermediatePosition;
        }


        private Vector2 GetEPositionInterpolated(GameObjectBase player, GameObjectBase target, float percentage)
        {
            Vector2 playerPosition = player.Position.ToW2S();
            Vector2 targetPosition = target.Position.ToW2S();
            Vector2 intermediatePosition = Vector2.Lerp(playerPosition, targetPosition, percentage);
            return intermediatePosition;
        }

        internal static bool firstCastPred = false;
        internal static int lastTick = -1;

        internal float GetEExtend(Vector3 castPos)
        {
            return ERange - Getter.Me().DistanceTo(castPos);
        }

        internal Vector2 GetEPositionPred(GameObjectBase player, GameObjectBase target)
        {
            if (!firstCastPred && EObj == null)
            {
                firstCastPred = true;
                lastTick = tick;
                return GetEPositionInterpolated(player, target, 0.8F);
            } else if (EObj != null && Getter.ESpell.SpellData.SpellName.Contains("IreliaE2")) 
            {
                Vector3 EObjPos = EObj.Position;
                Prediction.MenuSelected.PredictionOutput prediction = Prediction.MenuSelected.GetPrediction(Prediction.MenuSelected.PredictionType.Line, target, 750, 50, 0.264F, 2000, EObjPos, false);
                if (prediction.HitChance > Prediction.MenuSelected.HitChance.High)
                {
                    firstCastPred = false;
                    return prediction.CastPosition.Extend(EObj.Position, -GetEExtend(prediction.CastPosition)).ToW2S();
                }
            } else if (EObj == null && ((tick - lastTick) > 500) || !Getter.ESpell.SpellData.SpellName.Contains("IreliaE2"))
            {
                lastTick = tick;
                firstCastPred = false;
            }
                
            return Vector2.Zero;
        }

        private Task ESpell()
        {
            if (Getter.ELooseReady && EIsOn.IsOn && Getter.Me().Mana > 70)
            {
                GameObjectBase target = TargetSelector.GetBestHeroTarget(null, x => x.Distance < (ERange - 50) && !Getter.Me().BuffManager.HasActiveBuff("ireliawdefense"));
                if (target != null && target.Position.IsOnScreen())
                {
                    if (EMode.SelectedModeName == "Short&Easy")
                    {
                        Vector2 firstEPos = GetPointOnCircumference(target.Position.ToW2S(), (int)target.BoundingRadius, GetAngle());
                        SpellCastProvider.CastSpell(CastSlot.E, firstEPos, 0F);
                    } else if (EMode.SelectedModeName == "Interpolation") {
                        float percentage = 0.8f; // Set the desired interpolation percentage (0.0 to 1.0)
                        Vector2 ePos = GetEPositionInterpolated(Getter.Me(), target, percentage);
                        SpellCastProvider.CastSpell(CastSlot.E, ePos, 0.2F);
                    } else if (EMode.SelectedModeName == "Bezier")
                    {
                        float t = 0.8f; // Set the desired t value (0.0 to 1.0) for Bezier curve interpolation
                        Vector2 ePos = GetEPositionBezier(Getter.Me(), target, t);
                        SpellCastProvider.CastSpell(CastSlot.E, ePos, 0.4F);
                    } else if (EMode.SelectedModeName == "Pred")
                    {
                        Vector2 ePos = GetEPositionPred(Getter.Me(), target);
                        if (!ePos.IsZero)
                        {
                            SpellCastProvider.CastSpell(CastSlot.E, ePos, 0.2F);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task QLaneClear()
        {
            LaneClearExecute();
            return Task.CompletedTask;
        }

        private Task QSpell()
        {
            if (QIsOn.IsOn && Getter.QLooseReady && !Getter.Me().BuffManager.HasActiveBuff("ireliawdefense") && Getter.Me().Mana > 20)
            {
                if (currentPath.Count > 0 && currentPath[1].IsAlive)
                {
                    /* If Is Object Minion check if Menu Option MaxStacks is on if so return true, otherways test for HasMaxStacks => true = false; false = true;
                     * Not Enemy In AA Range // Should change
                     * Under Tower & Nexus check with menu option
                     * QSkip Check option handle and enemy with mark
                     * QCanKill handle
                     * END OF MINION HANDLING
                     * 
                     * 
                     */
                    if (
                        ((currentPath[1].IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient))
                        ? (QLaneClearMaxStacks.IsOn
                        ? true : !HasMaxStacks())
                        && (QUseForHeal.IsOn ? !EnemyInAARange(50) ? true : QCalc.CanKill(currentPath[1]) && currentPath[1].DistanceTo(UnitManager.EnemyChampions.Where(x => x.IsAlive && x.IsTargetable && x.IsValidTarget()).OrderBy(x => x.DistanceTo(Getter.Me().Position)).ToList().FirstOrDefault().Position) > Getter.Me().TrueAttackRange + 50 : false)
                        && (QAllowTower.IsOn? true : !General.InTowerRange(currentPath[1])) 
                        && !General.InNexusRange(currentPath[1]) 
                        && (QSkipCheck.IsOn ? true : HasMark(currentPath[1]) 
                        || QCalc.CanKill(currentPath[1])) : false)
                        || ((QSkipCheck.IsOn ? true : HasMark(currentPath[1]) 
                        || QCalc.CanKill(currentPath[1]) 
                        || (NEnemiesInAARange() > 1 ? false : (QHPGapClose.IsOn ? ((currentPath[1].Health - (QCalc.GetValue(currentPath[1]) * 2)) < 0) 
                        && currentPath[1].Distance > Getter.AARange : false)))))
                    {
                        SpellCastProvider.CastSpell(CastSlot.Q, currentPath[1].Position, 0);
                    }
                }
            }
            return Task.CompletedTask;
        }
        internal bool HasMaxStacks()
        {
            return (PassiveStacks() >= 4) ? true : false;
        }

        internal float PassiveStacks()
        {
            var buff = UnitManager.MyChampion.BuffManager.GetBuffByName("ireliapassivestacks", false, true);
            return (buff == null) ? 0 : buff.Stacks;
        }

        internal bool ShouldCastQ(GameObjectBase target, bool combo)
        {
            if (target.UnitComponentInfo.SkinName.Contains("Minion") && combo && HasMaxStacks() && !QLaneClearMaxStacks.IsOn)
            {
                return false;
            }
            if (target.UnitComponentInfo.SkinName.Contains("_P_TentacleAvatarActivex"))
                return false;
            if (!QAllowTower.IsOn &&General.InNexusRange(target))
                return false;
            if (!QAllowTower.IsOn)
            {
                if (General.InTowerRange(target) || General.InNexusRange(target))
                    return false;
            }
            if (Getter.Me().Mana < ((Getter.QLevel >= 1) ? 20 : 0))
                return false;
            if (target.Distance >= QRange)
                return false;
            return QCanReset(target) ? true : QCalc.CanKill(target);
        }
        internal bool QCanReset(GameObjectBase target)
        {
            if (target == null || !target.IsAlive || target.Distance > QRange || !TargetSelector.IsAttackable(target))
                return false;
            if (Getter.Me().Mana < ((Getter.QLevel >= 1) ? 20 : 0))
                return false;
            if (QSkipCheck.IsOn && UnitManager.EnemyChampions.deepCopy().Any(x => target.Name == x.Name))
                return true;
            return HasMark(target);
        }

        internal GameObjectBase GetLaneclearTarget()
        {
            GameObjectBase? resetMinion = UnitManager.EnemyMinions.FirstOrDefault(x => x.IsAlive && x.Health > 1 && x.Distance <= QRange && ShouldCastQ(x, false));
            if (resetMinion != null)
            {
                return resetMinion;
            }
            GameObjectBase? resetMob = UnitManager.EnemyJungleMobs.FirstOrDefault(x => x.IsAlive && x.Health > 1 && !x.Name.Contains("Ward") && TargetSelector.IsAttackable(x) && x.Distance <= QRange && ShouldCastQ(x, false));
            if (resetMob != null)
            {
                return resetMob;
            }
            return null;
        }

        private void LaneClearExecute()
        {
            if (Getter.QLooseReady)
            {
                GameObjectBase target = GetLaneclearTarget();
                if (target != null)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q, target.Position, 0);
                }
            }
        }

        internal List<GameObjectBase> QPathFinder()
        {
            if (Getter.QLooseReady)
            {
                List<GameObjectBase> list = UnitManager.EnemyChampions.ConvertAll(x => (GameObjectBase)x).Where(x => x.Distance < QRange * 4 && (QAllowTower.IsOn ? true : !General.InTowerRange(x)) && !General.InNexusRange(x) && (QSkipCheck.IsOn ? true : HasMark(x) || QCalc.CanKill(x) || (QHPGapClose.IsOn ? ((x.Health - (QCalc.GetValue(x) * 2)) < 0) : false))).ToList();
                //Logger.Log(UnitManager.EnemyMinions.ConvertAll(x => (GameObjectBase)x).Where(x => x.Distance < QRange * 4 && (QAllowTower.IsOn ? true : !GenGeneral.InTowerRange(x) && !General.InNexusRange(x)) && (QSkipCheck.IsOn ? true : HasMark(x) || QCalc.CanKill(x))).Count());
                //Logger.Log(UnitManager.EnemyMinions.Count());
                list.AddRange(UnitManager.EnemyMinions.ConvertAll(x => (GameObjectBase)x).Where(x => x.Distance < QRange * 4 && (QAllowTower.IsOn ? true : !General.InTowerRange(x) && !General.InNexusRange(x)) && (QSkipCheck.IsOn ? true : HasMark(x) || QCalc.CanKill(x))));
                List<GameObjectBase> validTargets = new() { Getter.Me() };
                GameObjectBase mainTarget = null;

                if (!list.Any(x => x.Distance < QRange))
                    return new();

                foreach (var item in list)
                {
                    if (item.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient))
                    {
                        if (item.UnitComponentInfo.SkinName.Contains("Minion", StringComparison.OrdinalIgnoreCase) && item.Distance < QRange * 4 && item.IsVisible)
                        {
                            validTargets.Add(item);
                        }
                    }
                    else if (item.Distance < QRange * 5 && item.IsVisible && item.IsValidTarget())
                    {
                        if (mainTarget == null)
                        {
                            mainTarget = item;
                        }
                        else if (QCalc.GetValue(item) > QCalc.GetValue(mainTarget))
                        {
                            mainTarget = item;
                        }
                        validTargets.Add(item);
                    }
                }
                validTargets = validTargets.OrderBy(x => x.Distance).ToList();
                if (mainTarget == null && validTargets.Count > 1)
                {
                    mainTarget = validTargets[validTargets.Count - 1];
                }
                if (validTargets.Count > 1)
                {
                    List<GameObjectBase> shortestPath = (mainTarget.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient))? pathfinder.FindLongestPath(validTargets, Getter.Me(), mainTarget, 600) : pathfinder.FindShortestPath(validTargets, Getter.Me(), mainTarget);
                    if (shortestPath != null)
                    {
                        return shortestPath;
                    }
                }
            }
            return new();
        }

        private void CalculatePath()
        {
            Oasys.Common.Settings.Orbwalker.HoldTargetChampsOnlyWhileMoving = false;
            Oasys.Common.Settings.Orbwalker.HoldTargetChampsOnly = false;
            if (QPathIsOn.IsOn)
            {
                List<GameObjectBase> path = currentPath;
                if (path.Count > 0)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (i + 1 == path.Count) break;
                        if (i == 0)
                        {
                            RenderFactory.DrawLine(Oasys.Common.LeagueNativeRendererManager.WorldToScreenSpell(Getter.Me().Position).X, Oasys.Common.LeagueNativeRendererManager.WorldToScreenSpell(Getter.Me().Position).Y, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1].Position).X, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1].Position).Y, 2, Color.White);
                        }
                        else
                        {
                            RenderFactory.DrawLine(LeagueNativeRendererManager.WorldToScreenSpell(path[i].Position).X, LeagueNativeRendererManager.WorldToScreenSpell(path[i].Position).Y, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1].Position).X, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1].Position).Y, 2, Color.White);
                        }
                    }
                }
            }
        }
    }
}
