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
    internal sealed class LuxQCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 80, 120, 160, 200, 240 };
        internal static float APScaling = 0.6F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class LuxECalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 70, 120, 170, 220, 270 };
        internal static float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class LuxRCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 300, 400, 500 };
        internal static float APScaling = 1.2F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }

    }

    internal class Lux : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Lux");

        internal const int QRange = 1240;
        internal const int QSpeed = 1200;
        internal const int QWidth = 140;
        internal const float QCastTime = 0.25F;

        internal const int ERange = 1100;
        internal const int ERadius = 310;
        internal const int ESpeed = 1200;
        internal const float ECastTime = 0.25F;

        internal const int RRange = 3400;
        internal const int RSpeed = 100000;
        internal const int RWidth = 200;
        internal const float RCastTime = 1;

        LuxQCalc QCalc = new LuxQCalc();
        LuxECalc ECalc = new LuxECalc();
        LuxRCalc RCalc = new LuxRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
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
                Color.OrangeRed,
                40,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 1, 0),
                7
                );
            CircleSpell eSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERadius,
                ERange,
                ESpeed,
                ERange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < ERange,
                x => Getter.Me().Position,
                Color.Red,
                110,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 10000, 0),
                6);
            LineSpell rSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RWidth,
                RRange,
                RSpeed,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < RRange,
                x => Getter.Me().Position,
                Color.Blue,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 10000, 0),
                9);
        }
    }
}
