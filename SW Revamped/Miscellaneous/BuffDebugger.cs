using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Tools;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Miscellaneous
{
    internal class BuffDebugger : UtilityModule
    {
        internal Group BuffsGroup = new Group("Buffs");
        internal Switch SelfBuffSwitch = new Switch("Log SelfBuffs", false);
        internal Switch LogActiveBuffs = new Switch("Log Active", false);
        internal Switch LogUnknown = new Switch("Log Unknown Buffs", false);
        internal Switch LogWithChName = new Switch("Log with Champ name", false);
        public override string Author => "EKQR Kotlin";
        public override string Description => "Debugs champ buffs";
        public override string Name => "Buff Debugger";
        public override string Version => "0.0.0.1";

        internal override void Init()
        {
            BuffsGroup.AddItem(SelfBuffSwitch);
            BuffsGroup.AddItem(LogActiveBuffs);
            BuffsGroup.AddItem(LogUnknown);
            BuffsGroup.AddItem(LogWithChName);
            UtilityManager.DebuggerGroup.AddItem(BuffsGroup);
            CoreEvents.OnCoreMainTick += Tick;
        }

        private Task Tick()
        {
            if (SelfBuffSwitch.IsOn)
            {
                foreach (BuffEntry buff in Getter.Me().BuffManager.GetBuffList())
                {
                    if (LogActiveBuffs.IsOn && !buff.IsActive || buff.Name.Contains($"smite", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!LogUnknown.IsOn && buff.Name.Contains("Unknown", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (LogWithChName.IsOn && !buff.Name.Contains($"{Getter.Me().ModelName}", StringComparison.OrdinalIgnoreCase))
                        continue;
                    Logger.Log($"{buff.Name} - {buff.IsActive} - Dur: {buff.DurationMs}:{buff.RemainingDurationMs} - Stacks: {buff.Stacks}");

                }
            }
            return Task.CompletedTask;
        }
    }
}
