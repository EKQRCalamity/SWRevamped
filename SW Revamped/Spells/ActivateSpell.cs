using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
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
    internal class ActivateSpell : SpellBase
    {
        internal Counter MinMana;

        internal bool IsActivated = false;

        internal Func<GameObjectBase, Vector3> SourcePosition;
        internal Func<GameObjectBase, bool> DeactivateCheck;
        
        internal ActivateSpell(CastSlot castSlot, SpellSlot spellSlot, EffectCalc eCalc, int range, float casttime, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> enemyActivateCheck, Func<GameObjectBase, bool> deactivateCheck, Func<GameObjectBase, Vector3> sourcePosition, Color drawColor, int minMana = 0, int drawprio = 5, TeamFlag teamflag = TeamFlag.Unknown)
        {
            Color color = drawColor;
            MainTab = Getter.MainTab;
            SpellCastSlot = castSlot;
            Slot = spellSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            MinMana = new Counter("Min Mana", minMana, 0, 10000);

            if (TeamFlag.Unknown == teamflag)
            {
                teamflag = (Getter.Me().Team == TeamFlag.Order) ? TeamFlag.Chaos : TeamFlag.Order;
            }

            DeactivateCheck = deactivateCheck;
            Range = range;

            SourcePosition = sourcePosition;
            MainTab.AddGroup( SpellGroup );
            SpellGroup.AddItem(IsOnSwitch);
            SpellGroup.AddItem(MinMana);

            effectCalc = eCalc;
            Effect effect = new Effect($"{SpellSlotToString()}", true, drawprio, Range, MainTab, SpellGroup, effectCalc, color);
            EffectDrawer.Add(effect, teamflag);

            CastTime = casttime;

            SelfCheck = selfCheck;
            TargetCheck= enemyActivateCheck;

            Init();

        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
        }

        private Task ComboInput()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, (x => x.IsAlive && x.Distance < Range));
            if (target == null || !IsOn)
                return Task.CompletedTask;
            if (!IsActivated && SelfCheck(Getter.Me()) && TargetCheck(target) && Getter.Me().Mana >= MinMana.Value && SpellIsReady())
            {
                IsActivated = true;
                SpellCastProvider.CastSpell(SpellCastSlot, CastTime);
            } else
            {
                if (IsActivated && DeactivateCheck(target))
                {
                    IsActivated = false;
                    SpellCastProvider.CastSpell(SpellCastSlot, CastTime);
                }
            }
            return Task.CompletedTask;
        }
    }
}
