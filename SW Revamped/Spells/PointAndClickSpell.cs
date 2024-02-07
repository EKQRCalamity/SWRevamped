using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Oasys.SDK.Prediction.MenuSelected;

namespace SWRevamped.Spells
{
    internal class PointAndClickSpell : SpellBase
    {
        internal bool UseCanKill = false;
        internal Counter MinMana;
        internal bool IsFriendly;

        internal Switch HarassIsOn;
        internal Switch LaneclearIsOn;
        internal Switch LasthitIsOn;
        internal Func<GameObjectBase, Vector3> SourcePosition;

        internal PointAndClickSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int range, int speed, int castrange, float casttime, bool useCanKill, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, Color drawColor, int minMana = 0, bool laneclear = false, bool harass = false, bool lasthit = false, int drawprio = 5, bool isFriendly = false)
        {
            Color color = drawColor;

            IsFriendly = isFriendly;

            MainTab = Getter.MainTab;
            Slot = spellSlot;
            SpellCastSlot = castSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");

            SourcePosition = sourcePosition;
            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            MinMana = new Counter("Min Mana", minMana, 0, 10000);
            SpellGroup.AddItem(MinMana);
            HarassIsOn = new Switch() { Title = "Harass", IsOn = false };
            LaneclearIsOn = new Switch() { Title = "Laneclear", IsOn = false };
            LasthitIsOn = new Switch() { Title = "Lasthit", IsOn = false };

            Width = 0;
            Range = range - 5;
            Speed = speed;
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
            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, color);
            EffectDrawer.Add(effect, IsFriendly);

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

            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetMixedTargets(targets, x => TargetCheck(x) && x.Distance < Range).FirstOrDefault();

            if (target == null || !IsOn || !LaneclearIsOn.IsOn )
                return Task.CompletedTask;
            if (UseCanKill ? target.Health - (effectCalc.GetValue(target) + Utility.CalculatorEx.Collector(target)) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target) && Getter.Me().Mana >= MinMana.Value)
            {
                Vector3 pos = target.Position;
                Vector2 v2Pos = pos.ToW2S();
                if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                    v2Pos = pos.ToWorldToMap();
                SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
            }
            return Task.CompletedTask;
        }

        private Task HarassInput()
        {
            GameObjectBase target = (!IsFriendly) ? Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => TargetCheck(x) && x.Distance < Range)) : AllyTargetSelector.GetLowestHealthTarget(x => TargetCheck(x) && x.Distance < Range);
            if (target == null || !IsOn || !HarassIsOn.IsOn)
                return Task.CompletedTask;
            if (UseCanKill ? target.Health - (effectCalc.GetValue(target) + Utility.CalculatorEx.Collector(target)) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target) && Getter.Me().Mana >= MinMana.Value)
            {
                Vector3 pos = target.Position;
                Vector2 v2Pos = pos.ToW2S();
                if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                    v2Pos = pos.ToWorldToMap();
                SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
            }
            return Task.CompletedTask;
        }

        private Task LasthitInput()
        {
            List<GameObjectBase> targets = UnitManager.EnemyMinions.ToList<GameObjectBase>();
            targets.AddRange(UnitManager.EnemyJungleMobs.ToList<GameObjectBase>());
            targets.AddRange(UnitManager.AllyJungleMobs.ToList<GameObjectBase>());
            targets = targets.OrderBy(x => x.Health).ToList<GameObjectBase>();
            GameObjectBase? target = Oasys.Common.Logic.TargetSelector.GetMixedTargets(targets, x => TargetCheck(x) && x.Distance < Range).FirstOrDefault();

            if (target == null || !IsOn || !LasthitIsOn.IsOn)
                return Task.CompletedTask;
            if (target.Health - (effectCalc.GetValue(target) + Utility.CalculatorEx.Collector(target)) < 0 && SelfCheck(Getter.Me()) && TargetCheck(target) && Getter.Me().Mana >= MinMana.Value)
            {
                Vector3 pos = target.Position;
                Vector2 v2Pos = pos.ToW2S();
                if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                    v2Pos = pos.ToWorldToMap();
                SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
            }
            return Task.CompletedTask;
        }

        private Task ComboInput()
        {
            GameObjectBase target = (!IsFriendly) ? Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => TargetCheck(x) && x.Distance < Range)) : AllyTargetSelector.GetLowestHealthTarget(x => TargetCheck(x) && x.Distance < Range);
            if (target == null || !IsOn)
                return Task.CompletedTask;
            if (UseCanKill ? target.Health - (effectCalc.GetValue(target) + Utility.CalculatorEx.Collector(target)) < 0 : true && SelfCheck(Getter.Me()) && TargetCheck(target) && Getter.Me().Mana >= MinMana.Value)
            {
                Vector3 pos = target.Position;
                Vector2 v2Pos = pos.ToW2S();
                if (!pos.IsOnScreen() && target.DistanceTo(SourcePosition(Getter.Me())) < Range)
                    v2Pos = pos.ToWorldToMap();
                SpellCastProvider.CastSpell(SpellCastSlot, v2Pos, CastTime);
            }
            return Task.CompletedTask;
        }
    }
}
