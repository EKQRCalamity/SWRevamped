using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
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

namespace SWRevamped.Spells
{
    internal class DashSpell : SpellBase
    {
        internal bool UseCanKill = false;
        internal Counter MinMana;
        internal TeamFlag flag;
        internal ModeDisplay DashMode;

        internal Func<GameObjectBase, Vector3> SourcePosition;


        internal bool EnemyInRange()
        {
            bool inRange = false;
            foreach (AIHeroClient client in UnitManager.EnemyChampions)
            {
                if (client.Distance < Range && TargetCheck(client))
                {
                    inRange = true;
                    break;
                }
            }
            return inRange;
        }

        internal DashSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int range, float casttime, bool useCanKill, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, Color drawColor, int minMana = 0, int drawprio = 0)
        {
            Color color = drawColor;

            MainTab = Getter.MainTab;
            Slot = spellSlot;
            SpellCastSlot = castSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");

            SourcePosition = sourcePosition;
            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            MinMana = new Counter("Min Mana", minMana, 0, 10000);
            DashMode = new ModeDisplay() { SelectedModeName = "MousePos", ModeNames = new() { "MousePos" }, Title = "Dash Mode" };
            SpellGroup.AddItem(MinMana);
            SpellGroup.AddItem(DashMode);

            Width = 0;
            Range = range;
            Speed = 0;
            CastRange = range;
            CastTime = casttime;
            effectCalc = eCalc;

            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, color);
            EffectDrawer.Add(effect, (TeamFlag.Chaos == Getter.Me().Team) ? TeamFlag.Order : TeamFlag.Chaos);

            SelfCheck = selfCheck;
            TargetCheck = targetCheck;

            Init();

        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
        }

        private Task ComboInput()
        {
            if (EnemyInRange() && IsOn && SelfCheck(Getter.Me()) && Getter.Me().Mana >= MinMana.Value && SpellIsReady())
            {
                SpellCastProvider.CastSpell(SpellCastSlot, CastTime);
            }
            return Task.CompletedTask;
        }
    }
}
