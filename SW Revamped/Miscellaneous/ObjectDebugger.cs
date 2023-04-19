using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.Clients;
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
    internal class ObjectDebugger : UtilityModule
    {
        internal Group ObjectGroup = new Group("Object Debugger");

        internal Switch DebugSwitch = new Switch("Debug", false);


        public override string Author => "EKQR Kotlin";
        public override string Description => "Debugs the objects under mouse";
        public override string Name => "Object Debugger";
        public override string Version => "0.0.0.1";

        internal override void Init()
        {
            ObjectGroup.AddItem(DebugSwitch);
            UtilityManager.DebuggerGroup.AddItem(ObjectGroup);
            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            if (DebugSwitch.IsOn)
            {
                AIBaseClient gameobj = GameEngine.HoveredGameObjectUnderMouse;
                if (gameobj != null)
                {
                    Vector2 pos = Getter.MousePosOnScreen;
                    pos.Y += 50;
                    RenderFactory.DrawText($"{gameobj.Name}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.ModelName}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.UnitComponentInfo.SkinName}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.UnitComponentInfo.SkinID}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.Health}/{gameobj.MaxHealth}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.Position}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.NetworkID}", pos, Color.Black);
                    pos.Y += 16;
                    RenderFactory.DrawText($"{gameobj.Team}", pos, Color.Black);
                }
            }
        }
    }
}
