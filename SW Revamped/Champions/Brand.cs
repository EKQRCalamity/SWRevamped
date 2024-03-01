using Oasys.Common.GameObject;
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
    internal sealed class BrandQEffectCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 70, 100, 130, 160, 190 };
        internal float APScaling = 0.65F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class BrandWEffectCalc : EffectCalc
    {
        internal int[] WBaseDamage = new int[] { 0, 75, 120, 165, 210, 255 };
        internal float WAPScaling = 0.6F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = WBaseDamage[Getter.WLevel];
                damage += WAPScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class BrandEEffectCalc : EffectCalc
    {
        internal int[] EBaseDamage = new int[] { 0, 60, 90, 120, 150, 180 };
        internal float EAPScaling = 0.6F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = EBaseDamage[Getter.WLevel];
                damage += EAPScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Brand : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Brand");

        internal static readonly int QRange = 1040;
        internal static readonly int QWidth = 120;
        internal static readonly int QSpeed = 1600;
        internal static readonly float QCastTime = 0.25F;

        internal static readonly int WRange = 900;
        internal static readonly int WRadius = 260;
        internal static readonly float WCastTime = 0.25F;

        internal static readonly int ERange = 675;
        internal static readonly float ECastTime = 0.25F;

        BrandQEffectCalc QCalc = new();
        BrandWEffectCalc WCalc = new();
        BrandEEffectCalc ECalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.OrangeRed,
                80,
                new(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max) }),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                QCastTime,
                false,
                5);
            CircleSpell wSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                WCalc,
                WRadius,
                WRange,
                100000,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                80,
                new CollisionCheck(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                WCastTime,
                false,
                8);
            PointAndClickSpell eSpell = new PointAndClickSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERange,
                2000,
                ERange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                80,
                false,
                false,
                false,
                9
                );
        }
    }
}
