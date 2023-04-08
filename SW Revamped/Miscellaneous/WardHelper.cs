using Microsoft.VisualBasic.Devices;
using Oasys.Common.Extensions;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.Events;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Miscellaneous
{
    internal class WardHelper : UtilityModule
    {
        internal static Vector2 MousePosOnScreen => new Vector2(Cursor.Position.X, Cursor.Position.Y);

        internal Group WardHelperGroup = new Group("Ward Helper");
        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal Switch DrawLocations = new Switch("Draw Locations", true);
        internal Group WardLocations = new Group("Locations");
        internal KeyBinding AutoWardKey = new KeyBinding() { Title = "Auto Ward Key", SelectedKey = System.Windows.Forms.Keys.CapsLock };

        internal bool Warding = false;

        public override string Name => "WardHelper";
        public override string Author => "EKQR Kotlin";
        public override string Version => "1.0.0.0";
        public override string Description => "A small module that helps with automizing wards and the process of placing them.";
        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(WardHelperGroup);
            WardHelperGroup.AddItem(IsOnSwitch);
            WardHelperGroup.AddItem(DrawLocations);
            WardHelperGroup.AddItem(WardLocations);
            WardHelperGroup.AddItem(AutoWardKey);
            foreach (Ward ward in WardManager.GetKnownWards())
            {
                WardLocations.AddItem(
                    new InfoDisplay() { Title = $"{ward.Name}" });
            }

            CoreEvents.OnCoreRender += Draw;
            KeyboardProvider.OnKeyPress += KeypressFunction;
            CoreEvents.OnCoreMainTick += MainFunc;
        }

        private Task MainFunc()
        {
            if (Warding)
            {
                Ward? ward = WardManager.GetClosestWard(Getter.Me());
                if (ward.MovePosition.DistanceToPlayer() < 25)
                {
                    SpellCastProvider.CastSpell(CastSlot.Item7, ward.ClickPosition);
                    Warding = false;
                    Oasys.SDK.Orbwalker.ForceMovePosition = Vector2.Zero;
                }
            }
            return Task.CompletedTask;
        }

        private void Draw()
        {
            if (IsOnSwitch.IsOn)
            {
                foreach (Ward ward in WardManager.GetKnownWards())
                {
                    if (!ward.MovePosition.IsOnScreen())
                    {
                        if (WardManager.StandsOnWard(Getter.Me(), ward))
                        {
                                RenderFactory.DrawNativeCircle(ward.MovePosition, 50, Color.Red, 2);
                                if (ward.ClickPosition.IsOnScreen())
                                {
                                    if (MousePosOnScreen.Distance(ward.ClickPosition.ToW2S()) <= 15)
                                    {
                                        RenderFactory.DrawNativeCircle(ward.ClickPosition, 15, Color.Red, 2);
                                        if (ward.WardPosition.IsOnScreen())
                                        {
                                            RenderFactory.DrawNativeCircle(ward.WardPosition, 50, Color.White, 2);
                                        }
                                    }
                                    else
                                    {
                                        RenderFactory.DrawNativeCircle(ward.ClickPosition, 15, Color.White, 2);
                                    }
                                }

                            
                        }
                        else
                        {
                            RenderFactory.DrawNativeCircle(ward.MovePosition, 50, Color.White, 2);
                        }
                }
                }
            }
        }

        private void KeypressFunction(Keys keyBeingPressed, Oasys.Common.Tools.Devices.Keyboard.KeyPressState pressState)
        {
            if (IsOnSwitch.IsOn && pressState == Oasys.Common.Tools.Devices.Keyboard.KeyPressState.Down && keyBeingPressed == AutoWardKey.SelectedKey)
            {
                Ward? close = WardManager.GetClosestWard(Getter.Me());
                if (close == null)
                    return;
                if (close.MovePosition.IsOnScreen() && close.MovePosition.DistanceToPlayer() < 750)
                {
                    Warding = true;
                    Oasys.SDK.Orbwalker.ForceMovePosition = close.MovePosition.ToW2S();
                }
            }
        }
    }
}
