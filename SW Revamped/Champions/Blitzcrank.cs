using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using SharpDX;
using SharpDX.DXGI;
using SWRevamped.Base;
using SWRevamped.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal sealed class BlitzQCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 105, 150, 195, 240, 285 };
        internal static float APScaling = 1.2F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }

            return damage;
        }
    }

    internal sealed class BlitzECalc : EffectCalc
    {
        internal static float BonusADScaling = 0.75F;
        internal static float APScaling = 0.25F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = Getter.BonusAD * BonusADScaling;
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class BlitzRCalc : EffectCalc
    {

        internal static int[] BaseDamage = { 0, 275, 400, 525 };
        internal static float APScaling = 1F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Blitzcrank : ChampionModule
    {
        internal Tab MainTab = new("SW - Blitzcrank");

        internal const int QRange = 1020;
        internal const int QWidth = 140;
        internal const int QSpeed = 1800;
        internal const float QCastTime = 0.25F;

        internal int ERange => (int)Getter.Me().TrueAttackRange;

        internal const int RRange = 0;
        internal const int RRadius = 600;
        internal const float RCastTime = 0.25F;

        BlitzQCalc QCalc = new BlitzQCalc();
        BlitzECalc ECalc = new BlitzECalc();
        BlitzRCalc RCalc = new BlitzRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                QRange,
                QCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QRange,
                x => Getter.Me().Position,
                Color.Red,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 0, 0),
                9

                );
            PointAndClickSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERange,
                100000,
                ERange,
                0,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < ERange,
                x => Getter.Me().Position,
                Color.Blue,
                40,
                false,
                true,
                false,
                3);
            CircleSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRadius,
                RRadius,
                100000,
                RRadius,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < RRadius,
                x => Getter.Me().Position,
                Color.Orange,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 9999, 0));

        }
    }
}
