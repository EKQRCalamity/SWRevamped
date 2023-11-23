using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
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
    internal sealed class DravenQCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 40, 45, 50, 55, 60 };
        internal float[] ADScaling = new float[] { 0, 0.75F, 0.85F, 0.95F, 1.05F, 1.15F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.BonusAD * ADScaling[Getter.QLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage) + Utility.CalculatorEx.CalculateAADamageWithOnHit(target);
            }
            return damage;
        }
    }

    internal sealed class DravenWCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class DravenECalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 75, 110, 145, 180, 215 };
        internal float ADScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += Getter.BonusAD * ADScaling;
            }
            return damage;
        }
    }

    internal sealed class DravenRCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 175, 275, 375 };
        internal float[] ADScaling = { 0, 1.1F, 1.3F, 1.5F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.BaseAD * ADScaling[Getter.RLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class Draven : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Draven");

        DravenQCalc QCalc = new DravenQCalc();
        DravenWCalc WCalc = new DravenWCalc();
        DravenECalc ECalc = new DravenECalc();
        DravenRCalc RCalc = new DravenRCalc();

        internal Counter RRangeCounter = new Counter("Ult Min Distance", 700, 0, 10000);

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            SelfCastingSpell qSpell = new SelfCastingSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                1000,
                0,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < Getter.Me().TrueAttackRange,
                x => Getter.Me().Position,
                Color.Red,
                45,
                true,
                true,
                false,
                2
                );
            SelfCastingSpell wSpell = new SelfCastingSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                1000,
                0,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < Getter.Me().TrueAttackRange,
                x => Getter.Me().Position,
                Color.Red,
                40,
                false,
                true,
                false,
                0);
            LineSpell eSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                ECalc,
                260,
                1100,
                1400,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                70,
                new CollisionCheck(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                0.25F,
                false,
                4,
                false,
                SpellCastMode.AfterAutoAttack
                );
            LineSpell rSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                RCalc,
                320,
                10000,
                2000,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.OrangeRed,
                100,
                new(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                0.25F,
                true,
                5);
        }
    }
}
