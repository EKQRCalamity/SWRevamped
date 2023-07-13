using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Logic;
using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static Oasys.Common.GameObject.Clients.ExtendedInstances.HeroInventory;

namespace SWRevamped.Utility
{
    internal static class CalculatorEx
    {
        internal static float NashorsTooth(GameObjectBase target)
        {
            return NashorsTooth(Getter.Me(), target);
        }
        
        internal static float NashorsTooth(GameObjectBase caster, GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(caster, target, 0, (15 + (Getter.TotalAP * 0.2F)), 0);
        }

        internal static float WitsEnd(GameObjectBase target)
        {
            return WitsEnd(Getter.Me(), target);
        }
        internal static float WitsEnd(GameObjectBase caster, GameObjectBase target)
        {
            float damage = (caster.Level == 1) ? 15 : (caster.Level == 9) ? 25 : (caster.Level < 15) ? 25 + ((caster.Level - 9) * 10) : 75 + ((caster.Level - 14) * 1.25F);
            return DamageCalculator.CalculateActualDamage(caster, target, 0, damage, 0);
        }

        internal static float Collector(GameObjectBase target)
        {
            float damage = 0;
            if (target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient))
            {
                damage = target.MaxHealth * 0.05F;
            }
            return damage;
        }

        internal static float Sheen(GameObjectBase target)
        {
            return Sheen(Getter.Me(), target);
        }

        internal static bool HasSheenBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("sheen", StringComparison.OrdinalIgnoreCase));
        }

        internal static float Sheen(GameObjectBase caster, GameObjectBase target)
        {
            float damage = caster.UnitStats.BaseAttackDamage;
            return DamageCalculator.CalculateActualDamage(caster, target, damage);
        }

        internal static bool HasIcebornBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("6662buff", StringComparison.OrdinalIgnoreCase));
        }

        internal static float Ardent(GameObjectBase target)
        {
            return Ardent(Getter.Me(), target);
        }

        internal static float Ardent(GameObjectBase caster, GameObjectBase target)
        {
            if (caster.BuffManager.GetBuffList().Any(x => x.Name.Contains("3504buff", StringComparison.OrdinalIgnoreCase)))
            {
                float[] damage = { 0, 5, 5.88F, 6.76F, 7.65F, 8.53F, 9.41F, 10.29F, 11.18F, 12.06F, 12.94F, 13.82F, 14.71F, 15.59F, 16.47F, 17.35F, 18.24F, 19.12F, 20 };
                return DamageCalculator.CalculateActualDamage(caster, target, 0, damage[caster.Level], 0);
            }
            return 0;
        }

        internal static bool HasEssenceBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("3508buff", StringComparison.OrdinalIgnoreCase));
        }

        internal static float EssenceReaver(GameObjectBase target)
        {
            return EssenceReaver(Getter.Me(), target);
        }

        internal static float EssenceReaver(GameObjectBase caster, GameObjectBase target)
        {
            float damage = caster.UnitStats.BaseAttackDamage + 0.4F * caster.UnitStats.BonusAttackDamage;
            return DamageCalculator.CalculateActualDamage(caster, target, damage);
        }

        internal static bool HasDivineBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("6632buff", StringComparison.OrdinalIgnoreCase));
        }

        internal static float Divine(GameObjectBase target)
        {
            return Divine(Getter.Me(), target);
        }

        internal static float Divine(GameObjectBase caster, GameObjectBase target)
        {
            float damage = caster.UnitStats.BaseAttackDamage * 1.25F;
            damage += target.MaxHealth * ((caster.AttackRange > 250) ? 0.03F : 0.06F);
            return DamageCalculator.CalculateActualDamage(caster, target, damage);
        }

        internal static bool HasLichbaneBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("lichbane", StringComparison.OrdinalIgnoreCase));
        }

        internal static float Lichbane(GameObjectBase target)
        {
            return Lichbane(Getter.Me(), target);
        }

        internal static float Lichbane(GameObjectBase caster, GameObjectBase target)
        {
            float damage = (caster.UnitStats.BaseAttackDamage * 0.75F) + (caster.UnitStats.TotalAbilityPower * 0.5F);
            return DamageCalculator.CalculateActualDamage(caster, target, 0, damage, 0);
        }

        internal static bool HasTrinityBuff(GameObjectBase target)
        {
            return target.BuffManager.GetBuffList().Any(x => x.IsActive && x.Name.Contains("3078trinityforce", StringComparison.OrdinalIgnoreCase));
        }

        internal static float Trinity(GameObjectBase target)
        {
            return Trinity(Getter.Me(), target);
        }

        internal static float Trinity(GameObjectBase caster, GameObjectBase target)
        {
            float damage = caster.UnitStats.BaseAttackDamage * 2;
            return DamageCalculator.CalculateActualDamage(caster, target, damage);
        }

        internal static float CalculateOnHit(GameObjectBase target)
        {
            return CalculateOnHit(Getter.Me(), target);
        }

        internal static float CalculateOnHit(GameObjectBase caster, GameObjectBase target, bool isOnHitSpell = false)
        {
            float damage = 0;
            foreach (Item item in caster.As<AIHeroClient>().Inventory.GetItemList())
            {
                switch (item.ID)
                {
                    case Oasys.Common.Enums.GameEnums.ItemID.Nashors_Tooth:
                        damage += NashorsTooth(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Wits_End:
                        damage += WitsEnd(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Sheen:
                        if (HasSheenBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Sheen(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Essence_Reaver:
                        if (HasEssenceBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += EssenceReaver(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Divine_Sunderer:
                        if (HasDivineBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Divine(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Frostfire_Gauntlet:
                        if (HasIcebornBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Sheen(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Lich_Bane:
                        if (HasLichbaneBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Lichbane(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Trinity_Force:
                        if (HasTrinityBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Trinity(caster, target);
                        break;
                    default:
                        break;
                }
            }
            damage += Ardent(caster, target);
            return damage;
        }

        internal static float CalculateAADamageWithOnHit(GameObjectBase target)
        {
            return CalculateAADamageWithOnHit(Getter.Me(), target);
        }

        internal static float CalculateAADamageWithOnHit(GameObjectBase caster, GameObjectBase target, bool isOnHitSpell = false)
        {
            float damage = DamageCalculator.CalculateActualDamage(caster, target, caster.UnitStats.TotalAttackDamage);
            foreach (Item item in caster.As<AIHeroClient>().Inventory.GetItemList())
            {
                switch (item.ID)
                {
                    case Oasys.Common.Enums.GameEnums.ItemID.Nashors_Tooth:
                        damage += NashorsTooth(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Wits_End:
                        damage += WitsEnd(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Sheen:
                        if (HasSheenBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Sheen(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Essence_Reaver:
                        if (HasEssenceBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += EssenceReaver(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Divine_Sunderer:
                        if (HasDivineBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Divine(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Frostfire_Gauntlet:
                        if (HasIcebornBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Sheen(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Lich_Bane:
                        if (HasLichbaneBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Lichbane(caster, target);
                        break;
                    case Oasys.Common.Enums.GameEnums.ItemID.Trinity_Force:
                        if (HasTrinityBuff(caster) || isOnHitSpell && item.IsReady)
                            damage += Trinity(caster, target);
                        break;
                    default:
                        break;
                }
            }

            damage += Ardent(caster, target);
            return damage;
        }

        internal static float CalculateHealthWithRegeneration(GameObjectBase target, float timeins)
        {
            float health = target.Health;
            float healthRegen = target.UnitStats.HPRegenRate;
            return health + (healthRegen * timeins);
        }
    }
}
