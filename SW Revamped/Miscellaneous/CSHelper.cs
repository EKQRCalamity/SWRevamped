

using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Miscellaneous
{
    internal sealed class CSHelper : UtilityModule
    {
        internal Group CSHelperGroup = new Group("CS Helper");
        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal Counter InRange = new Counter("Range", 2600, 0, 10000);

        internal Group DrawingOptions = new Group("Drawings");
        //internal Switch DrawHPBar = new Switch("Health Bar", false);
        internal Switch DrawCircle = new Switch("Circle", true);
        internal Counter Alpha = new Counter("Alpha", 150, 0, 255);

        public override string Name => "CSHelper";
        public override string Author => "EKQRKotlin";
        public override string Version => "1.0.0.0";
        public override string Description => "Draws what minion you can kill.";
        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(CSHelperGroup);
            CSHelperGroup.AddItem(IsOnSwitch);
            CSHelperGroup.AddItem(InRange);
            CSHelperGroup.AddItem(DrawingOptions);
            DrawingOptions.AddItem(Alpha);

            CoreEvents.OnCoreRender += Draw;
        }

        internal List<GameObjectBase> GetMinions(int range)
        {
            List<GameObjectBase> minions = new List<GameObjectBase>();
            foreach (GameObjectBase minion in UnitManager.EnemyMinions)
            {
                if (minion == null) continue;
                if (minion.Distance < range)
                {
                    if (Getter.AAsLeft(minion) < 2)
                        minions.Add(minion);
                }
            }
            minions = minions.OrderByDescending(x => Getter.AAsLeft(x)).ToList();
            return minions;
        }

        private void Draw()
        {
            if (IsOnSwitch.IsOn)
            {
                foreach (GameObjectBase minion in GetMinions(InRange.Value))
                {
                    if (Getter.AAsLeft(minion) < 1 && minion.IsAlive && minion.Position.IsOnScreen())
                    {
                        if (DrawCircle.IsOn)
                            RenderFactory.DrawNativeCircle(minion.Position, minion.BoundingRadius, new Color(Color.Red.R, Color.Red.G, Color.Red.G, Alpha.Value), 2);
                    }
                }
            }
        }
    }
}
