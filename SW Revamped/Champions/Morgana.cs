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
    internal sealed class MorganaQCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 80, 135, 190, 245, 300 };
        internal float APScaling = 0.9F;

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

    internal sealed class MorganaWCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 6, 11, 16, 21, 26 };
        internal float APScaling = 0.07F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class MorganaECalc : EffectCalc
    {
        internal int[] BaseShield = { 0, 80, 135, 190, 245, 300 };
        internal float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float shield = 0;
            if (Getter.ELevel > 0)
            {
                shield = BaseShield[Getter.ELevel];
                shield += Getter.TotalAP * APScaling;
            }
            return shield;
        }
    }

    internal sealed class MorganaRCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 150, 225, 300 };
        internal float APScaling = 0.7F;

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

    internal class Morgana : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Morgana");

        internal const int QRange = 1300;
        internal const int QWidth = 140;
        internal const int QSpeed = 1200;
        internal const float QCastTime = 0.25F;

        internal const int WRange = 900;
        internal const int WRadius = 275;
        internal const float WCastTime = 0.25F;

        internal const int ERange = 800;

        internal const int RRadius = 575;
        internal const float RCastTime = 0.35F;

        MorganaQCalc QCalc = new MorganaQCalc();
        MorganaWCalc WCalc = new MorganaWCalc();
        MorganaECalc ECalc = new MorganaECalc();
        MorganaRCalc RCalc = new MorganaRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
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
                Color.Blue,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0));
            new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRadius,
                WRange,
                1000000,
                WRange,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                90,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 9999, 0),
                4
                );
            new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERange,
                0,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                80,
                8);
            new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRadius,
                0,
                0,
                0,
                RCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Purple,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 99999, 0),
                6
                );
        }
    }
}
