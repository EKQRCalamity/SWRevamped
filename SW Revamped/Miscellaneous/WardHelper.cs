using Microsoft.VisualBasic.Devices;
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.InputProviders;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;

namespace SWRevamped.Miscellaneous
{
    internal class WardHelper : UtilityModule
    {
        internal static Vector2 MousePosOnScreen => new Vector2(Cursor.Position.X, Cursor.Position.Y);

        internal Group WardHelperGroup = new Group("Ward Helper");
        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal Switch SWItemSwitch = new Switch("Use Stealth Wards", true);
        internal Switch PinkItemSwitch = new Switch("Use Pink Wards", true);
        internal Switch WardItemSwitch = new Switch("Use Ward Items", true);
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
            if (GameEngine.InGameInfo.MapID != Oasys.Common.Enums.GameEnums.MapIDFlag.SummonersRift)
                return;
            UtilityManager.MainTab.AddGroup(WardHelperGroup);
            WardHelperGroup.AddItem(IsOnSwitch);
            WardHelperGroup.AddItem(SWItemSwitch);
            WardHelperGroup.AddItem(PinkItemSwitch);
            WardHelperGroup.AddItem(WardItemSwitch);
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

        private CastSlot SpellCastSlot2CastSlot(SpellCastSlot slot)
        {
            switch (slot)
            {
                case SpellCastSlot.Item1:
                    return CastSlot.Item1;
                case SpellCastSlot.Item2:
                    return CastSlot.Item2;
                case SpellCastSlot.Item3:
                    return CastSlot.Item3;
                case SpellCastSlot.Item4:
                    return CastSlot.Item4;
                case SpellCastSlot.Item5:
                    return CastSlot.Item5;
                case SpellCastSlot.Item6:
                    return CastSlot.Item6;
                case SpellCastSlot.Item7:
                    return CastSlot.Item7;
                case SpellCastSlot.Q:
                    return CastSlot.Q;
                case SpellCastSlot.W:
                    return CastSlot.W;
                case SpellCastSlot.E:
                    return CastSlot.E;
                case SpellCastSlot.R:
                    return CastSlot.R;
                case SpellCastSlot.Summoner1:
                    return CastSlot.Summoner1;
                default:
                    return CastSlot.Summoner2;
            }
        }

        List<ItemID> wardItemIDs = new List<ItemID>() 
        {
            ItemID.Vigilant_Wardstone,
            ItemID.Pauldrons_of_Whiterock,
            ItemID.Harrowing_Crescent,
            ItemID.Frostfang,
            ItemID.Bulwark_of_the_Mountain,
            ItemID.Black_Mist_Scythe,
            ItemID.Runesteel_Spaulders,
            ItemID.Shard_of_True_Ice,
            ItemID.Targons_Buckler,
            ItemID.Watchful_Wardstone
        };
        private Task MainFunc()
        {
            if (Warding)
            {
                Ward? ward = WardManager.GetClosestWard(Getter.Me());
                if (ward.MovePosition.DistanceToPlayer() < 5)
                {
                    if (WardItemSwitch.IsOn && HasWardItem())
                    {
                        for (int i = 0; i < wardItemIDs.Count - 1; i++)
                        {
                            SpellCastSlot slot = GetCastSlotFromInv(wardItemIDs[i]);
                            if (slot != SpellCastSlot.Summoner2)
                            {
                                SpellCastProvider.CastSpell(SpellCastSlot2CastSlot(slot), ward.ClickPosition);
                                break;
                            }
                        }
                    }
                    if (SWItemSwitch.IsOn && HasNormalWard())
                        SpellCastProvider.CastSpell(CastSlot.Item4, ward.ClickPosition);
                    if (PinkItemSwitch.IsOn && HasPinkWard())
                    {
                        SpellCastSlot slot = GetCastSlotFromInv(ItemID.Control_Ward, false);
                        if (slot != SpellCastSlot.Summoner2)
                        {
                            SpellCastProvider.CastSpell(SpellCastSlot2CastSlot(slot), ward.ClickPosition);
                        }
                    }
                    Warding = false;
                }
            }
            return Task.CompletedTask;
        }

        private SpellCastSlot GetCastSlotFromInv(Oasys.Common.Enums.GameEnums.ItemID itemID, bool charge = true)
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == itemID && ((charge)? item.Charges > 0 : true))
                    return item.SpellCastSlot;
            }
            return SpellCastSlot.Summoner2;
        }

        private bool HasNormalWard()
        {
            return Getter.MeHero.Inventory.HasItem(Oasys.Common.Enums.GameEnums.ItemID.Stealth_Ward);
        }

        private bool HasPinkWard()
        {
            return Getter.MeHero.Inventory.HasItem(Oasys.Common.Enums.GameEnums.ItemID.Control_Ward);
        }

        private bool HasWardItem()
        {
            return Getter.MeHero.Inventory.HasItem(ItemID.Vigilant_Wardstone) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Pauldrons_of_Whiterock) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Harrowing_Crescent) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Frostfang) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Bulwark_of_the_Mountain) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Black_Mist_Scythe) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Runesteel_Spaulders) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Shard_of_True_Ice) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Targons_Buckler) ||
                   Getter.MeHero.Inventory.HasItem(ItemID.Watchful_Wardstone);
        }

        private void Draw()
        {
            if (IsOnSwitch.IsOn)
            {
                foreach (Ward ward in WardManager.GetKnownWards())
                {
                    if (ward.MovePosition.IsOnScreen())
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
                    Oasys.Common.Tools.Devices.Mouse.ClickAndBounce(close.MovePosition.ToW2S(), 200, false);
                }
            }
        }
    }
}
