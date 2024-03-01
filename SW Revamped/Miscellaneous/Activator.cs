using Oasys.Common.Enums.GameEnums;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;
using SWRevamped.Utility;

namespace SWRevamped.Miscellaneous
{
    internal class MobType
    {
        internal enum Type
        {
            None = 0,
            Common = 1,
            Buff = 2,
            Epic = 3,
        }
    }
    internal class Activator : UtilityModule
    {
        public override string Author => "EKQRKotlin";
        public override string Name => "Activator";
        public override string Description => "Automatically Casts Items and Summoners.";
        public override string Version => "1.0.0.0";

        internal Group ActivatorGroup = new Group("Activator");

        internal Switch UseOnTick = new Switch("Use On Tick", false);

        internal Group Ignite = new Group("Ignite");
        internal Switch UseIgnite = new Switch("Use Ignite", true);
        

        internal Group Smite = new Group("Smite");
        internal Switch UseSmite = new Switch("Use Smite", true);
        internal Group Smiteables = new Group("Smiteables");
        internal Switch SmiteCommon = new Switch("Normal Camps", false);
        internal Switch SmiteBuffs = new Switch("Buff Camps", false);
        internal Switch SmiteEpics = new Switch("Epic Camps", true);

        internal Group Shield = new Group("Shield");
        internal Switch UseShield = new Switch("Use Shield", true);
        internal Counter ShieldCounter = new Counter("Health Threshold", 20, 0, 100);

        internal Group Heal = new Group("Heal");
        internal Switch UseHeal = new Switch("Use Heal", true);
        internal Switch UsePots = new Switch("Use Pots", true);
        internal Switch UseForAllies = new Switch("Use for Allies", true);
        internal Counter HealthCounter = new Counter("Health Threshold", 20, 0, 100);

        internal Group Cleanse = new Group("Cleanse");
        internal Switch SpellCleanse = new Switch("Use Cleanse", true);
        internal Switch CleanseSlow = new Switch("Cleanse slows", false);

        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(ActivatorGroup);

            ActivatorGroup.AddItem(UseOnTick);

            ActivatorGroup.AddItem(Ignite);
            Ignite.AddItem(UseIgnite);

            ActivatorGroup.AddItem(Smite);
            Smite.AddItem(UseSmite);
            Smite.AddItem(Smiteables);
            Smiteables.AddItem(SmiteCommon);
            Smiteables.AddItem(SmiteBuffs);
            Smiteables.AddItem(SmiteEpics);

            ActivatorGroup.AddItem(Shield);
            Shield.AddItem(UseShield);
            Shield.AddItem(ShieldCounter);

            ActivatorGroup.AddItem(Heal);
            Heal.AddItem(UseHeal);
            Heal.AddItem(UsePots);
            Heal.AddItem(UseForAllies);
            Heal.AddItem(HealthCounter);

            ActivatorGroup.AddItem(Cleanse);
            Cleanse.AddItem(SpellCleanse);
            Cleanse.AddItem(CleanseSlow);
            //Cleanse.AddItem(ItemCleanse);

            CoreEvents.OnCoreMainInputAsync += ComboKey;
            CoreEvents.OnCoreMainTick += Tick;
        }

        float lastpottime = 0;

        internal bool HasPots()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == ItemID.Health_Potion || item.ID == ItemID.Corrupting_Potion || item.ID == ItemID.Refillable_Potion)
                {
                    return true;
                }
            }
            return false;
        }

        

        internal CastSlot GetPotsSlot()
        {
            foreach (Item item in Getter.MeHero.Inventory.GetItemList())
            {
                if (item.ID == ItemID.Health_Potion || item.ID == ItemID.Corrupting_Potion || item.ID == ItemID.Refillable_Potion)
                {
                    return General.SpellToCastSlot(item.SpellCastSlot);
                }
            }
            return CastSlot.Item1;
        }

        internal bool HasSummoner(SummonerSpellsEnum summoner)
        {
            if (SummonerSpellsProvider.IHaveSpellOnSlot(summoner, SummonerSpellSlot.First))
                return true;
            else if (SummonerSpellsProvider.IHaveSpellOnSlot(summoner, SummonerSpellSlot.Second))
                return true;
            return false;
        }

        internal CastSlot GetCastSlot(SummonerSpellsEnum summoner)
        {
            if (SummonerSpellsProvider.IHaveSpellOnSlot(summoner, SummonerSpellSlot.First))
                return CastSlot.Summoner1;
            else
                return CastSlot.Summoner2;
        }

        internal SpellSlot GetSpellSlot(SummonerSpellsEnum summoner)
        {
            if (SummonerSpellsProvider.IHaveSpellOnSlot(summoner, SummonerSpellSlot.First))
                return SpellSlot.Summoner1;
            else
                return SpellSlot.Summoner2;
        }

        internal SpellClass GetSpellClass(SummonerSpellsEnum summoner)
        {
            if (SummonerSpellsProvider.IHaveSpellOnSlot(summoner, SummonerSpellSlot.First))
                return Getter.GetSpellBook().GetSpellClass(SpellSlot.Summoner1);
            else
                return Getter.GetSpellBook().GetSpellClass(SpellSlot.Summoner2);
        }

        private Task Tick()
        {
            if (UseOnTick.IsOn)
                HandleActivation();
            return Task.CompletedTask;
        }

        private Task ComboKey()
        {
            if (!UseOnTick.IsOn)
                HandleActivation();
            return Task.CompletedTask;
        }

        private void HandleActivation() 
        {
            if (Getter.IsDead(Getter.Me()))
                return;
            if (UseSmite.IsOn)
                ActivateSmite();
            if (UseIgnite.IsOn)
                ActivateIgnite();
            if (UseShield.IsOn)
                ActivateShield();
            if (UseHeal.IsOn)
                ActivateHeal();
            if (SpellCleanse.IsOn)
                ActivateCleanse();
        }

        internal bool IsCrowdControllButCanCleanse(BuffEntry buff, bool slowIsCC)
        {
            return buff.IsActive && buff.Stacks >= 1 &&
                   ((slowIsCC && buff.EntryType == BuffType.Slow) ||
                   buff.EntryType == BuffType.Stun || buff.EntryType == BuffType.Taunt ||
                   buff.EntryType == BuffType.Snare || buff.EntryType == BuffType.Charm ||
                   buff.EntryType == BuffType.Silence || buff.EntryType == BuffType.Blind ||
                   buff.EntryType == BuffType.Fear || buff.EntryType == BuffType.Polymorph ||
                   buff.EntryType == BuffType.Flee || buff.EntryType == BuffType.Sleep) &&
                   !buff.Name.Equals("yonerstun", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("landslidedebuff", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("CassiopeiaWSlow", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("megaadhesiveslow", System.StringComparison.OrdinalIgnoreCase) &&
                   !buff.Name.Equals("UnknownBuff", System.StringComparison.OrdinalIgnoreCase);
        }

        internal bool IsCrowdControlledButCanCleanse<T>(T obj) where T : GameObjectBase
        {
            return obj.BuffManager.GetBuffList().Any(x => IsCrowdControllButCanCleanse(x, CleanseSlow.IsOn));
        }

        private void ActivateCleanse()
        {
            if (HasSummoner(SummonerSpellsEnum.Cleanse) && IsCrowdControlledButCanCleanse(Getter.Me()))
            {
                SpellClass cleanse = GetSpellClass(SummonerSpellsEnum.Cleanse);
                if (cleanse.IsSpellReady)
                {
                    SpellCastProvider.CastSpell(GetCastSlot(SummonerSpellsEnum.Cleanse));
                }
            }
        }

        private void ActivateHeal()
        {
            if (HasSummoner(SummonerSpellsEnum.Heal) && GetSpellClass(SummonerSpellsEnum.Heal).IsSpellReady)
            {
                SpellClass heal = GetSpellClass(SummonerSpellsEnum.Heal);
                if (UseHeal.IsOn)
                {
                    if (Getter.Me().IsAlive && Getter.Me().HealthPercent < HealthCounter.Value)
                    {
                        SpellCastProvider.CastSpell(GetCastSlot(SummonerSpellsEnum.Heal));
                    }
                }
                if (UseForAllies.IsOn)
                {
                    GameObjectBase target = AllyTargetSelector.GetLowestHealthPercentTarget(x => x.Distance < 820);
                    if (target.IsAlive && target.HealthPercent < HealthCounter.Value)
                    {
                        SpellCastProvider.CastSpell(GetCastSlot(SummonerSpellsEnum.Heal), target.Position);
                    }
                }
            }
            if (UsePots.IsOn)
            {
                if (HasPots() && GameEngine.GameTime - lastpottime > 15)
                {
                    CastSlot potslot = GetPotsSlot();
                    if (Getter.Me().HealthPercent < HealthCounter.Value)
                    {
                        SpellCastProvider.CastSpell(potslot);
                        lastpottime = GameEngine.GameTime;
                    }
                }
            }
        }

        private void ActivateShield()
        {
            if (HasSummoner(SummonerSpellsEnum.Barrier))
            {
                SpellClass shield = GetSpellClass(SummonerSpellsEnum.Barrier);
                if (shield.IsSpellReady)
                {
                    if (Getter.Me().HealthPercent < ShieldCounter.Value)
                    {
                        SpellCastProvider.CastSpell(GetCastSlot(SummonerSpellsEnum.Barrier));
                    }
                }
            }
        }

        internal float SmiteDamage()
        {
            SpellClass? smite = GetSpellClass(SummonerSpellsEnum.Smite);
            if (smite == null)
                return 0F;
            if (smite.SpellData.SpellName.Contains("SummonerSmite") && smite.IsSpellReady)
            {
                if (smite.SpellData.SpellName.Contains("SmiteDuel") || smite.SpellData.SpellName.Contains("SmitePlayerGanker"))
                    return 899;
                if (smite.SpellData.SpellName.Contains("SmiteAvatar"))
                    return 1199;
                return 599;
            }
            return 0;
        }

        internal List<JungleMob> Monsters()
        {
            List<JungleMob> jungleMobs = UnitManager.AllyJungleMobs.Where(x => x.IsAlive && x.Distance <= 1000 && x.Position.IsOnScreen() && x.Health > 0 && x.IsValidTarget()).ToList();
            jungleMobs.AddRange(UnitManager.EnemyJungleMobs.Where(x => x.IsAlive && x.Distance <= 1000 && x.Position.IsOnScreen() && x.Health > 0 && x.IsValidTarget()).ToList());
            return jungleMobs;
        }

        internal MobType.Type GetMobType(JungleMob mob)
        {
            if (mob.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Buff;
            if (mob.UnitComponentInfo.SkinName.Equals("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Equals("SRU_Crab", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Common;
            if (mob.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase) ||
                mob.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
                return MobType.Type.Epic;
            return MobType.Type.None;
        }

        private bool ShouldCast(JungleMob mob)
        {
            if (SmiteEpics.IsOn && GetMobType(mob) == MobType.Type.Epic || SmiteBuffs.IsOn && GetMobType(mob) == MobType.Type.Buff || SmiteCommon.IsOn && GetMobType(mob) == MobType.Type.Common)
            {
                return true;
            }
            return false;
        }

        private void ActivateSmite()
        {
            if (HasSummoner(SummonerSpellsEnum.Smite) && GetSpellClass(SummonerSpellsEnum.Smite).IsSpellReady)
            {
                List<JungleMob> JungleMobs = Monsters();
                float damage = SmiteDamage();
                SpellClass smite = GetSpellClass(SummonerSpellsEnum.Smite);
                foreach (JungleMob mob in JungleMobs)
                {
                    if (mob.Health > 0 && mob.Health < damage && ShouldCast(mob) && smite.Charges >= 1 && smite.IsSpellReady)
                    {
                        SpellCastProvider.CastSpell(GetCastSlot(SummonerSpellsEnum.Smite), mob.Position.ToW2S());
                        break;
                    }
                }
            }
        }

        private void ActivateIgnite()
        {
            GameObjectBase target = Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < 600);
            if (target == null) return;
            if (HasSummoner(SummonerSpellsEnum.Ignite))
            {
                SpellClass ignite = GetSpellClass(SummonerSpellsEnum.Ignite);
                CastSlot slot = GetCastSlot(SummonerSpellsEnum.Ignite);
                if (ignite == null) return;
                if (ignite.IsSpellReady)
                {
                    if (target.HealthPercent < 15 && target.Position.IsOnScreen())
                    { 
                        SpellCastProvider.CastSpell(slot, target.Position.ToW2S());
                    }
                }
            }
        }
    }
}
