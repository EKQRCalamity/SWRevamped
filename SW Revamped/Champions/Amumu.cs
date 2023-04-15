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
    internal sealed class AmumuQCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 70, 95, 120, 145, 170 };
        internal static float APScaling = 0.85F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }

            return damage;
        }
    }

    internal sealed class AmumuWCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 6, 8, 10, 12, 14 };
        internal static float[] MaxHealthSc = { 0, 0.01F, 0.0115F, 0.013F, 0.0145F, 0.016F };
        internal static float MaxHealthPerAP = 0.0025F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += target.MaxHealth * (MaxHealthSc[Getter.WLevel] + (Getter.TotalAP / 100 * MaxHealthPerAP));
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class AmumuECalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 65, 100, 135, 170, 205 };
        internal static float APScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }

            return damage;
        }
    }

    internal sealed class AmumuRCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 200, 300, 400};
        internal static float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }

            return damage;
        }
    }

    internal class Amumu : ChampionModule
    {
        Tab MainTab = new Tab("SW - Amumu");

        AmumuQCalc QCalc = new AmumuQCalc();
        AmumuWCalc WCalc = new AmumuWCalc();
        AmumuECalc ECalc = new AmumuECalc();
        AmumuRCalc RCalc = new AmumuRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc, 160,
                1100,
                2000,
                1100,
                0.25F,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                65,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0));
            ActivateSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                350,
                0,
                x => x.IsAlive,
                x => x.Distance < 350,
                x => x.Distance > 350,
                x => Getter.Me().Position,
                Color.OrangeRed,
                8,
                4);
            CircleSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                350,
                350,
                1000,
                350,
                0.25F,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < 350,
                x => Getter.Me().Position,
                Color.Blue,
                35,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(false, 0, 0), 6);
            CircleSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                550,
                550,
                100000,
                550,
                0.25F,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < 550,
                x => Getter.Me().Position,
                Color.Blue,
                200,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 10000, 2), 6);
        }
    }
}
