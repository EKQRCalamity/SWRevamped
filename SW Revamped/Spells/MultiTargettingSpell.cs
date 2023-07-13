using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Spells
{
    internal class MultiTargettingSpell : SpellBase
    {
        internal bool UseCanKill;

        internal Counter MinMana;

        internal Switch HarassIsOn;

        internal MultiTargettingSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int range, float casttime, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Color drawColor, bool useCanKill = false, int minMana = 0, bool harass = false, int drawprio = 5)
        {
            Color color = drawColor;

            MainTab = Getter.MainTab;
            Slot = spellSlot;
            SpellCastSlot = castSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");

            UseCanKill = useCanKill;

            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            MinMana = new Counter("Min Mana", minMana, 0, 10000);
            SpellGroup.AddItem(MinMana);
            HarassIsOn = new Switch() { Title = "Harass", IsOn = false };

            Width = 0;
            Range = range;
            Speed = 0;
            CastRange = range;
            CastTime = casttime;
            effectCalc = eCalc;

            if (harass)
            {
                SpellGroup.AddItem(HarassIsOn);
                HarassIsOn.IsOn = true;
            }
            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, color);
            EffectDrawer.Add(effect, (TeamFlag.Chaos == Getter.Me().Team) ? TeamFlag.Order : TeamFlag.Chaos);

            SelfCheck = selfCheck;
            TargetCheck = targetCheck;

            Init();
        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
            if (HarassIsOn.IsOn)
            {
                CoreEvents.OnCoreHarassInputAsync += HarassInput;
            }
        }

        private Task HarassInput()
        {
            if (IsOn)
            {
                List<GameObjectBase> targetList = UnitManager.EnemyChampions.Where(x => TargetCheck(x) && x.Distance < CastRange).ToList<GameObjectBase>();
                foreach (GameObjectBase target in targetList)
                {
                    if (TargetCheck(target) && SelfCheck(Getter.Me()) && Getter.Me().Mana >= MinMana.Value && (UseCanKill) ? target.Health - effectCalc.GetValue(target) < 0 : true)
                    {
                        SpellCastProvider.CastSpell(SpellCastSlot, CastTime);
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task ComboInput()
        {
            if (IsOn)
            {
                List<GameObjectBase> targetList = UnitManager.EnemyChampions.Where(x => TargetCheck(x) && x.Distance < CastRange).ToList<GameObjectBase>();
                foreach (GameObjectBase target in targetList)
                {
                    if (TargetCheck(target) && SelfCheck(Getter.Me()) && Getter.Me().Mana >= MinMana.Value && (UseCanKill) ? target.Health - effectCalc.GetValue(target) < 0 : true)
                    {
                        SpellCastProvider.CastSpell(SpellCastSlot, CastTime);
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
