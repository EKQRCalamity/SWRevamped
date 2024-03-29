﻿using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oasys.Common.Menu;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.SDK;
using SWRevamped.Utility;
using Oasys.SDK.Tools;

namespace SWRevamped.Spells
{
    internal class BuffSpell : SpellBase
    {
        internal Counter MinMana;
        internal Counter HealthCounter;

        internal Func<GameObjectBase, Vector3> SourcePosition;

        internal Priorities? prios = null;

        internal BuffSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int range, float casttime, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, Color drawColor, int minMana = 0, int drawprio = 5, int health = 80, bool priority = true)
        {
            Color color = drawColor;

            MainTab = Getter.MainTab;
            Slot = spellSlot;
            SpellCastSlot = castSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            HealthCounter = new Counter("Health %", health, 0, 100);
            MinMana = new Counter("Min Mana", minMana, 0, 10000);

            SourcePosition = sourcePosition;
            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            SpellGroup.AddItem(MinMana);
            SpellGroup.AddItem(HealthCounter);

            Width = 0;
            Range = range;
            Speed = 1000000;
            CastTime = casttime;

            effectCalc = eCalc;
            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, color);
            EffectDrawer.Add(effect, true);

            SelfCheck = selfCheck;
            TargetCheck = targetCheck;
            if (priority)
                prios = new Priorities(SpellGroup);
            Init();
        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
        }

        private Task ComboInput()
        {

            GameObjectBase target = AllyTargetSelector.GetLowestHealthTarget(x => TargetCheck(x) && x.Distance < Range);

            if (prios != null)
            {
                target = AllyTargetSelector.GetLowestHealthPrioTarget(x => TargetCheck(x) && x.Distance < Range, prios);
                if (target == null)
                    return Task.CompletedTask;
                if (prios.PriorityValues[target.Name] == -1 || prios.PriorityValues[target.Name] == 0)
                    return Task.CompletedTask;
            }
            if (target == null || !IsOn)
                return Task.CompletedTask;
            if (SelfCheck(Getter.Me()) && Getter.Me().Mana >= MinMana.Value && target.HealthPercent < HealthCounter.Value)
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
