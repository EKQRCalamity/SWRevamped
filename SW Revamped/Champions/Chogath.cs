using Oasys.Common.Enums.GameEnums;
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
    internal sealed class ChogathQCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 80, 140, 200, 260, 320 };
        internal float APScaling = 1.0f;
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
    internal sealed class ChogathWCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 80, 135, 190, 245, 300 };
        internal float APScaling = 0.7f;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class ChogathECalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class ChogathRCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 300, 475, 650 };
        internal float APScaling = 0.5f;
        internal float BonusHPScaling = 0.1f;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage += BonusHPScaling * Getter.BonusHP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, 0, damage);
            }
            return damage;
        }
    }

    internal class Chogath : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - ChoGath");

        ChogathQCalc QCalc = new();
        ChogathWCalc WCalc = new();
        ChogathECalc ECalc = new();
        ChogathRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            CircleSpell circleSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                250,
                950,
                10000,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                50,
                new(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                0.25F,
                false
                );
            ConeSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                WCalc,
                60,
                650,
                10000,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < 625,
                x => Getter.Me().Position,
                Color.Green,
                90,
                new(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                0.5F,
                false,
                4);
            SelfCastingSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                SpellSlot.E,
                ECalc,
                1000,
                0,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < Getter.AARange + 25,
                x => Getter.Me().Position,
                Color.Red);
            PointAndClickSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                SpellSlot.R,
                RCalc,
                175,
                10000,
                175,
                0.25F,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Orange,
                100,
                false,
                false,
                false,
                6);
        }
    }
}
