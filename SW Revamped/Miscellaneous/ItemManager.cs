
using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SWRevamped.Base;
using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;

namespace SWRevamped.Miscellaneous
{
    internal class ItemManager : UtilityModule
    {
        public override string Author => "EKQRCalamity";
        public override string Description => "Automatically uses some items.";
        public override string Name => "Item Manager";
        public override string Version => "0.1.0";

        internal Group ActivatorGroup = new Group("Item Activator");

        internal Switch UseOnTick = new Switch("Use On Tick", false);

        internal Group Tiamat = new Group("Tiamat");
        internal Switch TiamatSwitch = new Switch("Use Tiamat", true);

        internal Group Ravenous = new Group("Ravenous");
        internal Switch RavenousSwitch = new Switch("Use Ravenous", true);
        internal Counter RavenousUnderX = new Counter("Under % Health", 100, 0, 100);

        internal Group Titanic = new Group("Titanic");
        internal Switch TitanicSwitch = new Switch("Use Titanic", true);

        internal Group Profane = new Group("Profane");
        internal Switch ProfaneSwitch = new Switch("Use Profane", true);
        internal Counter ProfaneUnderX = new Counter("Under % Health Enemy", 50, 0, 100);

        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(ActivatorGroup);

            ActivatorGroup.AddItem(UseOnTick);
            
            Tiamat.AddItem(TiamatSwitch);
            ActivatorGroup.AddItem(Tiamat);

            Ravenous.AddItem(RavenousSwitch);
            Ravenous.AddItem(RavenousUnderX);
            ActivatorGroup.AddItem(Ravenous);

            Titanic.AddItem(TitanicSwitch);
            ActivatorGroup.AddItem(Titanic);

            Profane.AddItem(ProfaneSwitch);
            Profane.AddItem(ProfaneUnderX);
            ActivatorGroup.AddItem(Profane);


            CoreEvents.OnCoreMainInputAsync += ComboHandler;
            CoreEvents.OnCoreMainTick += Tick;
        }

        internal bool HasTiamat()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.Tiamat)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasRavenous()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.Ravenous_Hydra)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasTitanic()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.Titanic_Hydra)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasProfane()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == Oasys.Common.Enums.GameEnums.ItemID.Profane_Hydra)
                {
                    return true;
                }
            }
            return false;
        }

        internal CastSlot? GetItemSlot(ItemID item)
        {
            foreach (Item item2 in Getter.MeHero.Inventory.GetItemList())
            {
                if (item2.ID == item)
                {
                    return General.SpellToCastSlot(item2.SpellCastSlot);
                }
            }
            return null;
        }

        internal Item? GetItem(ItemID item)
        {
            foreach (Item item2 in Getter.MeHero.Inventory.GetItemList())
            {
                if (item2.ID == item)
                {
                    return item2;
                }
            }
            return null;
        }

        internal bool EnemyInRange(int range, Func<GameObjectBase, bool> func)
        {
            foreach (AIHeroClient client in UnitManager.EnemyChampions)
            {
                if (func(client) && client.Distance < range && client.IsAlive && client.IsTargetable && client.IsValidTarget())
                {
                    return true;
                }
            }
            return false;
        }

        private Task Tick()
        {
            if (UseOnTick.IsOn && Getter.Me().IsAlive)
            {
                if (HasTiamat() && EnemyInRange(445, (x) => x.IsAlive) && TiamatSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Tiamat);
                    Item? item = GetItem(ItemID.Tiamat);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Tiamat);
                    }
                } else if (HasProfane() && EnemyInRange(445, (x) => x.IsAlive && x.HealthPercent <= ProfaneUnderX.Value) && ProfaneSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Profane_Hydra);
                    Item? item = GetItem(ItemID.Profane_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Profane_Hydra);
                    }
                } else if (HasRavenous() && EnemyInRange(445, (x) => x.IsAlive && Getter.Me().HealthPercent < RavenousUnderX.Value) && RavenousSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Ravenous_Hydra);
                    Item? item = GetItem(ItemID.Ravenous_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Ravenous_Hydra);
                    }
                } else if (HasTitanic() && EnemyInRange((int)Getter.Me().TrueAttackRange, (x) => x.IsAlive) && TitanicSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Titanic_Hydra);
                    Item? item = GetItem(ItemID.Titanic_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Titanic_Hydra);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task ComboHandler()
        {
            if (!UseOnTick.IsOn && Getter.Me().IsAlive)
            {
                if (HasTiamat() && EnemyInRange(445, (x) => x.IsAlive) && TiamatSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Tiamat);
                    Item? item = GetItem(ItemID.Tiamat);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Tiamat);
                    }
                }
                else if (HasProfane() && EnemyInRange(445, (x) => x.IsAlive && x.HealthPercent <= ProfaneUnderX.Value) && ProfaneSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Profane_Hydra);
                    Item? item = GetItem(ItemID.Profane_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Profane_Hydra);
                    }
                }
                else if (HasRavenous() && EnemyInRange(445, (x) => x.IsAlive && Getter.Me().HealthPercent < RavenousUnderX.Value) && RavenousSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Ravenous_Hydra);
                    Item? item = GetItem(ItemID.Ravenous_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Ravenous_Hydra);
                    }
                }
                else if (HasTitanic() && EnemyInRange((int)Getter.Me().TrueAttackRange, (x) => x.IsAlive) && TitanicSwitch.IsOn)
                {
                    CastSlot? slot = GetItemSlot(ItemID.Titanic_Hydra);
                    Item? item = GetItem(ItemID.Titanic_Hydra);
                    if (slot != null && item != null)
                    {
                        if (item.IsReady)
                            ItemCastProvider.CastItem(ItemID.Titanic_Hydra);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
