using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Miscellaneous
{
    internal sealed class CollectorCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return (target.MaxHealth / 100) * 5;
        }
    }
    internal sealed class CollectorDrawings : UtilityModule
    {
        internal Group ItemGroup = new Group("Collector");

        public override string Name => "Collector Drawer";

        public override string Version => "1.0.0.0";

        public override string Description => "Shows the execution threshold for collector.";

        public override string Author => "EKQR Kotlin";

        internal override void Init()
        {
            ExecuteEffect collector = new ExecuteEffect("Collector", true, 0, 100000, UtilityManager.MainTab, ItemGroup, new CollectorCalc(), SharpDX.Color.Red);
            collector.IsOnSelfCheck = x => x.Inventory.HasItem(ItemID.The_Collector);
            UtilityManager.MainTab.AddGroup(ItemGroup);
        }
    }
}
