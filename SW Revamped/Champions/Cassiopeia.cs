using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.SDK;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal sealed class CassioQCalc : EffectCalc
    {
        internal int[] QBaseDamage = new int[] { 0, 75, 110, 145, 180, 215 };
        internal float QAPScaling = 0.9F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = QBaseDamage[Getter.QLevel];
                damage += QAPScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CassioWCalc : EffectCalc
    {
        internal int[] WBaseDamage = new int[] { 0, 20, 25, 30, 35, 40 };
        internal float WAPScaling = 0.15F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = 2 * WBaseDamage[Getter.QLevel];
                damage += (2 * WAPScaling) * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CassioECalc : EffectCalc
    {
        internal int[] EBaseDamage = new int[] { 0, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120 };
        internal int[] EBonusDamage = new int[] { 0, 20, 40, 60, 80, 100 };
        internal float EAPScaling = 0.1F;
        internal float EBonusAPScaling = 0.6F;

        internal static bool IsPoisoned(GameObjectBase target)
        {
            BuffEntry poisonBuff = null;
            foreach (BuffEntry buff in target.BuffManager.GetBuffList())
            {
                if (buff.Name.Contains("cassiopeia", StringComparison.OrdinalIgnoreCase) && buff.IsActive && buff.Stacks >= 1)
                {
                    poisonBuff = buff;
                    break;
                }
            }
            return poisonBuff != null;
        }

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = EBaseDamage[((Getter.Level > 18) ? 18 : Getter.Level)];
                damage += EAPScaling * Getter.TotalAP;
                if (IsPoisoned(target))
                {
                    damage += EBonusDamage[Getter.ELevel] + (EBonusAPScaling * Getter.TotalAP);
                }

                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CassioRCalc : EffectCalc
    {
        internal int[] RBaseDamage = new int[] { 0, 150, 250, 350 };
        internal float RAPScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = RBaseDamage[Getter.RLevel];
                damage += (RAPScaling) * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Cassiopeia : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Cassiopeia");

        internal static readonly int QRange = 850;
        internal static readonly int QRadius = 200;
        internal static readonly float QCastTime = 0.25F;

        internal static readonly int WRange = 700;
        internal static readonly int WSpeed = 3000;
        internal static readonly int WRadius = 200;
        internal static readonly float WCastTime = 0.25F;

        internal static readonly int ERange = 700;
        internal static readonly int ESpeed = 2500;
        internal static readonly float ECastTime = 0.125F;

        internal static readonly int RRange = 830;
        internal static readonly int RAngle = 80;
        internal static readonly float RCastTime = 0.5F;

        CassioQCalc QCalc = new CassioQCalc();
        CassioWCalc WCalc = new CassioWCalc();
        CassioECalc ECalc = new CassioECalc();
        CassioRCalc RCalc = new CassioRCalc();
        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            CircleSpell qSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                QRadius,
                QRange,
                100000,
                QRange,
                QCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                false,
                new CollisionCheck(true, 9999),
                7
                );
            CircleSpell wSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRadius,
                WRange,
                WSpeed,
                WRange,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Purple,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                false,
                new CollisionCheck(true, 9999),
                8
                );
            PointAndClickSpell eSpell = new PointAndClickSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERange,
                ESpeed,
                ERange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Yellow,
                40,
                true,
                true,
                true,
                9
                );
            ConeSpell rSpell = new ConeSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RAngle,
                RRange,
                10000,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive && x.IsFacing(Getter.Me()),
                x => Getter.Me().Position,
                Color.Red,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 999),
                5
                );
        }

    }
}
