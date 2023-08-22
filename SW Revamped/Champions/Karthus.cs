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
    internal sealed class KarthusQCalc : EffectCalc
    {
        internal float[] BaseDamage = { 0, 45, 62.5F, 80, 97.5F, 115 };
        internal float APScaling = 0.35F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KarthusWCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class KarthusECalc : EffectCalc
    {
        internal float[] BaseDamage = { 0, 7.5F, 12.5F, 17.5F, 22.5F, 27.5F };
        internal float APScaling = 0.05F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KarthusRCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 200, 350, 500 };
        internal float APScaling = 0.75F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Karthus : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Karthus");

        KarthusQCalc QCalc = new KarthusQCalc();
        KarthusWCalc WCalc = new KarthusWCalc();
        KarthusECalc ECalc = new KarthusECalc();
        KarthusRCalc RCalc = new KarthusRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            CircleSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                160,
                875,
                10000,
                875,
                0.25F,
                false,
                x => (x.IsAlive || x.IsZombie),
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                40,
                Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                true,
                new CollisionCheck(true, 10000, 0));
            
            CircleSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                400,
                1000,
                10000,
                1000,
                0.25F,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                70,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 10000, 0),
                0);
            ActivateSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                550,
                0F,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < 550,
                x => x.IsAlive && x.Distance > 570,
                x => Getter.Me().Position,
                Color.Red,
                78,
                4);
            MultiTargettingSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                100000,
                0.25F,
                x => x.IsZombie || x.IsAlive,
                x => x.IsAlive,
                Color.Orange,
                true,
                100,
                true,
                6);
        }
    }
}
