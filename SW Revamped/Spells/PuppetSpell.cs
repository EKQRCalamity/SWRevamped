using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
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
    internal class PuppetSpell : SpellBase
    {
        internal bool IsActivated => PuppeteeringSwitch.IsOn;
        internal Switch PuppeteeringSwitch = new Switch("Auto Puppet Attack", false);
        internal string puppetName;

        internal float delay => new Random().NextFloat(100, 500);
        internal float delayStart;
        internal float delayEnd;

        internal bool PuppetAlive => (Puppet == null) ? false : true;
        internal GameObjectBase? Puppet => UnitManager.Allies.FirstOrDefault(x => x.Name.Contains(puppetName, StringComparison.OrdinalIgnoreCase));

        internal PuppetSpell(CastSlot castSlot, SpellSlot spellSlot, int range, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, string entityname)
        {
            MainTab = Getter.MainTab;
            SpellCastSlot = castSlot;
            Slot = spellSlot;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            Range = range;

            puppetName = entityname;

            SpellGroup.AddItem(PuppeteeringSwitch);
            
            MainTab.AddGroup(SpellGroup);

            SelfCheck = selfCheck;
            TargetCheck = targetCheck;

            Init();
        }

        private void Init()
        {
            CoreEvents.OnCoreMainInputAsync += ComboInput;
        }

        internal bool EnemyNearPuppet() => UnitManager.EnemyChampions.Any(x => x.DistanceTo(Puppet.Position) <= Range);
        private Task ComboInput()
        {
            if (PuppetAlive)
            {
                if (GameEngine.GameTime > delayEnd)
                {
                    GameObjectBase puppet = Puppet;
                    if (EnemyNearPuppet())
                    {
                        GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.DistanceTo(Puppet.Position) <= Range);
                        SpellCastProvider.CastSpell(SpellCastSlot, target.Position, 0);
                        delayStart = GameEngine.GameTime;
                        delayEnd = GameEngine.GameTime + delay;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
