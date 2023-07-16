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
    internal sealed class VeigarQCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 80, 120, 160, 200, 240 };
        internal static float[] APScaling = new float[] { 0, 0.45F, 0.5F, 0.55F, 0.6F, 0.65F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling[Getter.QLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }

            return damage;
        }
    }

    internal sealed class VeigarWCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 85, 140, 195, 250, 305 };
        internal static float[] APScaling = new float[] { 0, 0.7F, 0.8F, 0.9F, 1F, 1.1F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += Getter.TotalAP * APScaling[Getter.WLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }

            return damage;
        }
    }

    internal sealed class VeigarECalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class VeigarRCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 175, 250, 325 };
        internal static float[] APScaling = new float[] { 0, 0.65F, 0.7F, 0.75F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.TotalAP * APScaling[Getter.RLevel];
                damage = damage * (1 + (target.MissingHealthPercent / 100));
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }

            return damage;
        }
    }

    internal sealed class Veigar : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Veigar");

        internal const int QRange = 990;
        internal const int QWidth = 140;
        internal const int QSpeed = 2200;
        internal const float QCastTime = 0.25F;

        internal const int WRange = 950;
        internal const int WRadius = 140;
        internal const int WSpeed = 100000;
        internal const float WCastTime = 0.25F;

        internal const int ERange = 725;
        internal const int ERadius = 400;
        internal const int ESpeed = 100000;
        internal const float ECastTime = 0.25F;

        internal const int RRange = 650;
        internal const int RSpeed = 100000;
        internal const float RCastTime = 0.25F;

        VeigarQCalc QCalc = new();
        VeigarWCalc WCalc = new();
        VeigarECalc ECalc = new();
        VeigarRCalc RCalc = new();

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
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                50,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 1, 0));
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
                Color.Blue,
                80,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 9999, 0),
                4);
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
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.White,
                80,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 9999, 0)
                );
            PointAndClickSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRange,
                RSpeed,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Black,
                100,
                false,
                false,
                false,
                9
                );
        }
    }
}
