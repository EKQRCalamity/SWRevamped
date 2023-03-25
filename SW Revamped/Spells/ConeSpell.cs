using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.SpellCasting;
using Oasys.SDK;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.SDK.Events;
using SharpDX;
using Oasys.Common.Menu;
using static Oasys.SDK.Prediction.MenuSelected;
using Oasys.Common.Extensions;

namespace SWRevamped.Spells
{
    internal class ConeSpell : SpellBase
    {
        internal bool UseCanKill = false;
        internal bool useMinCollisions = false;

        internal ModeDisplay _HitChance;
        internal Counter MinMana;

        internal static CollisionCheck defaultCollisionCheck = new CollisionCheck(true, 0);

        internal Switch HarassIsOn;
        internal Switch LaneclearIsOn;
        internal Switch LasthitIsOn;
        internal Func<GameObjectBase, Vector3> SourcePosition;

        internal ConeSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int width, int range, int speed, int castrange, float casttime, bool useCanKill, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, Color drawColor, int minMana = 0, HitChance minHitChance = HitChance.VeryHigh, bool laneclear = false, bool harass = false, bool lasthit = false, CollisionCheck collisionCheck = null, int drawprio = 5, TeamFlag drawflag = TeamFlag.Unknown)
        {
            if (collisionCheck == null) collisionCheck = defaultCollisionCheck;
            defaultCollisionCheck = collisionCheck;

            if (defaultCollisionCheck.MinCollisionObjects > 0)
            {
                useMinCollisions = true;
            }

            if (drawflag == TeamFlag.Unknown)
                drawflag = (Getter.Me().Team == TeamFlag.Chaos) ? TeamFlag.Order : TeamFlag.Chaos;

            Color drawcolor = (Color)drawColor;
            MainTab = Getter.MainTab;
            Slot = spellSlot;
            SpellCastSlot = castSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");

            SourcePosition = sourcePosition;
            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            _HitChance = new ModeDisplay() { Title = "Hitchance", ModeNames = new() { "Impossible", "Unknown", "OutOfRange", "Dashing", "Low", "Medium", "High", "VeryHigh", "Immobile" }, SelectedModeName = GetNameFromHitchance(minHitChance) };
            SpellGroup.AddItem(_HitChance);
            MinMana = new Counter("Min Mana", minMana, 0, 10000);
            SpellGroup.AddItem(MinMana);
            HarassIsOn = new Switch() { Title = "Harass", IsOn = false };
            LaneclearIsOn = new Switch() { Title = "Laneclear", IsOn = false };
            LasthitIsOn = new Switch() { Title = "Lasthit", IsOn = false };

            Width = width;
            Range = range;
            Speed = speed;
            CastRange = castrange;
            CastTime = casttime;
            UseCanKill = useCanKill;
            effectCalc = eCalc;

            if (laneclear)
            {
                SpellGroup.AddItem(LaneclearIsOn);
                LaneclearIsOn.IsOn = true;
            }
            if (lasthit)
            {
                SpellGroup.AddItem(LasthitIsOn);
                LasthitIsOn.IsOn = true;
            }
            if (harass)
            {
                SpellGroup.AddItem(HarassIsOn);
                HarassIsOn.IsOn = true;
            }
            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, drawColor);
            EffectDrawer.Add(effect, drawflag);
            SelfCheck = selfCheck;
            TargetCheck = targetCheck;



            Init();
        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
            if (LasthitIsOn.IsOn)
            {
                CoreEvents.OnCoreLasthitInputAsync += LasthitInput;
            }
            if (HarassIsOn.IsOn)
            {
                CoreEvents.OnCoreHarassInputAsync += HarassInput;
            }
            if (LaneclearIsOn.IsOn)
            {
                CoreEvents.OnCoreLaneclearInputAsync += LaneclearInput;
            }
        }

        private Task LaneclearInput()
        {
            List<GameObjectBase> targets = UnitManager.EnemyMinions.ToList<GameObjectBase>();
            targets.AddRange(UnitManager.EnemyJungleMobs.ToList<GameObjectBase>());
            targets.AddRange(UnitManager.AllyJungleMobs.ToList<GameObjectBase>());
            targets = targets.OrderBy(x => x.Health).ToList<GameObjectBase>();

            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetMixedTargets(targets, x => x.Distance < CastRange).FirstOrDefault();

            if (target == null || !IsOn)
                return Task.CompletedTask;
            Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Oasys.SDK.Prediction.MenuSelected.GetPrediction(PredictionType.Cone, target, Range, Width, CastTime, Speed, SourcePosition(Getter.Me()), defaultCollisionCheck.Collision);
            if (defaultCollisionCheck.Collision && pred.Collision)
                if (pred.CollisionObjects.Count > defaultCollisionCheck.MaxCollisionObjects || (useMinCollisions) ? pred.CollisionObjects.Count > defaultCollisionCheck.MinCollisionObjects : true)
                    return Task.CompletedTask;
            if (pred.HitChance >= GetHitchanceFromName(_HitChance.SelectedModeName) && SpellIsReady() && Getter.Me().Mana > MinMana.Value)
            {
                if (UseCanKill ? target.Health - effectCalc.GetValue(target) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target))
                {
                    Vector3 pos = pred.CastPosition;
                    Vector2 v2Pos = pos.ToW2S();
                    if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                        v2Pos = pos.ToWorldToMap();
                    SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
                }
            }
            return Task.CompletedTask;
        }

        private Task HarassInput()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < CastRange));
            if (target == null || !IsOn)
                return Task.CompletedTask;
            Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Oasys.SDK.Prediction.MenuSelected.GetPrediction(PredictionType.Cone, target, Range, Width, CastTime, Speed, SourcePosition(Getter.Me()), defaultCollisionCheck.Collision);
            if (defaultCollisionCheck.Collision && pred.Collision)
                if (pred.CollisionObjects.Count > defaultCollisionCheck.MaxCollisionObjects || (useMinCollisions) ? pred.CollisionObjects.Count > defaultCollisionCheck.MinCollisionObjects : true)
                    return Task.CompletedTask;
            if (pred.HitChance >= GetHitchanceFromName(_HitChance.SelectedModeName) && SpellIsReady() && Getter.Me().Mana > MinMana.Value)
            {
                if (UseCanKill ? target.Health - effectCalc.GetValue(target) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target))
                {
                    Vector3 pos = pred.CastPosition;
                    Vector2 v2Pos = pos.ToW2S();
                    if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                        v2Pos = pos.ToWorldToMap();
                    SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
                }
            }
            return Task.CompletedTask;
        }

        private Task LasthitInput()
        {
            List<GameObjectBase> targets = UnitManager.EnemyMinions.ToList<GameObjectBase>();
            targets.AddRange(UnitManager.EnemyJungleMobs.ToList<GameObjectBase>());
            targets.AddRange(UnitManager.AllyJungleMobs.ToList<GameObjectBase>());
            targets = targets.OrderBy(x => x.Health).ToList<GameObjectBase>();

            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetMixedTargets(targets, x => x.Distance < CastRange).FirstOrDefault();

            if (target == null || !IsOn)
                return Task.CompletedTask;
            Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Oasys.SDK.Prediction.MenuSelected.GetPrediction(PredictionType.Cone, target, Range, Width, CastTime, Speed, SourcePosition(Getter.Me()), defaultCollisionCheck.Collision);
            if (defaultCollisionCheck.Collision && pred.Collision)
                if (pred.CollisionObjects.Count > defaultCollisionCheck.MaxCollisionObjects || (useMinCollisions) ? pred.CollisionObjects.Count > defaultCollisionCheck.MinCollisionObjects : true)
                    return Task.CompletedTask;
            if (pred.HitChance >= GetHitchanceFromName(_HitChance.SelectedModeName) && SpellIsReady() && Getter.Me().Mana > MinMana.Value)
            {
                if (target.Health - effectCalc.GetValue(target) < 0 && SelfCheck(Getter.Me()) && TargetCheck(target))
                {
                    Vector3 pos = pred.CastPosition;
                    Vector2 v2Pos = pos.ToW2S();
                    if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                        v2Pos = pos.ToWorldToMap();
                    SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
                }
            }
            return Task.CompletedTask;
        }

        private Task ComboInput()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.Distance < CastRange));
            if (target == null || !IsOn)
                return Task.CompletedTask;
            Oasys.SDK.Prediction.MenuSelected.PredictionOutput pred = Oasys.SDK.Prediction.MenuSelected.GetPrediction(PredictionType.Cone, target, Range, Width, CastTime, Speed, SourcePosition(Getter.Me()), defaultCollisionCheck.Collision);
            if (defaultCollisionCheck.Collision && pred.Collision)
                if (pred.CollisionObjects.Count > defaultCollisionCheck.MaxCollisionObjects || (useMinCollisions) ? pred.CollisionObjects.Count > defaultCollisionCheck.MinCollisionObjects : true)
                    return Task.CompletedTask;
            if (pred.HitChance >= GetHitchanceFromName(_HitChance.SelectedModeName) && SpellIsReady() && Getter.Me().Mana > MinMana.Value)
            {
                if (UseCanKill ? target.Health - effectCalc.GetValue(target) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target))
                {
                    Vector3 pos = pred.CastPosition;
                    Vector2 v2Pos = pos.ToW2S();
                    if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                        v2Pos = pos.ToWorldToMap();
                    SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
                }
            }
            return Task.CompletedTask;
        }
    }
}
