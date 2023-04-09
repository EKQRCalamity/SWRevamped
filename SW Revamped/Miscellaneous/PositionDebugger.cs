using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Miscellaneous
{
    internal class PositionDebugger : UtilityModule
    {
        public override string Author => "EKQR Kotlin";
        public override string Description => "Debugs champ and mouse pos";
        public override string Name => "Position Debugger";
        public override string Version => "0.0.0.1";
        internal static Vector2 MousePosOnScreen => new Vector2(Cursor.Position.X, Cursor.Position.Y);

        internal override void Init()
        {
            CoreEvents.OnCoreRender += Draw;
        }

        private void Draw()
        {
            RenderFactory.DrawText($"{Getter.Me().Position}", Getter.Me().Position.ToW2S(), Color.Black, true);
            Logger.Log($"{Getter.Me().Position}");
            RenderFactory.DrawText($"{GameEngine.WorldMousePosition}", MousePosOnScreen, Color.Black, true);
            Logger.Log($"{GameEngine.WorldMousePosition}");
        }
    }
}
