using Oasys.Common.GameObject;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
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
    internal sealed class SorakaQCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 85, 120, 155, 190, 225 };
        internal float APScaling = 0.35F;

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

    internal sealed class SorakaWCalc : EffectCalc
    {
        internal int[] BaseHeal = { 0, 90, 110, 130, 150, 170 };
        internal float APScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float heal = 0;
            if (Getter.WLevel > 0)
            {
                heal = BaseHeal[Getter.WLevel];
                heal += Getter.TotalAP * APScaling;
            }
            return heal;
        }
    }

    internal sealed class SorakaECalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 70, 95, 120, 145, 170 };
        internal float APScaling = 0.4F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class SorakaRCalc : EffectCalc
    {
        internal int[] BaseHeal = { 0, 150, 250, 350 };
        internal float APScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float heal = 0;
            if (Getter.RLevel > 0)
            {
                heal = BaseHeal[Getter.RLevel];
                heal += Getter.TotalAP * APScaling;
            }
            return heal;
        }
    }

    internal class Soraka : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Soraka");

        internal const int QRange = 800;
        internal const int QRadius = 265;
        internal const float QCastTime = 0.25F;

        internal const int WRange = 550;
        internal const float WCastTime = 0.25F;

        internal const int ERange = 925;
        internal const int ERadius = 260;
        internal const float ECastTime = 0.25F;

        internal const int RRange = 100000;
        internal const float RCastTime = 0.25F;

        SorakaQCalc QCalc = new SorakaQCalc();
        SorakaWCalc WCalc = new SorakaWCalc();
        SorakaECalc ECalc = new SorakaECalc();
        SorakaRCalc RCalc = new SorakaRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QRadius,
                QRange,
                100000,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                80,
                new(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                QCastTime,
                false);
            new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRange,
                WCastTime,
                x => x.IsAlive,
                x => x.IsAlive && !x.IsMe,
                x => Getter.Me().Position,
                Color.Green,
                80,
                5,
                70);
            new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                ECalc,
                ERadius,
                ERange,
                100000,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                80,
                new(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                ECastTime,
                false,
                4);
            new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRange,
                RCastTime,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                80,
                7,
                15);

        }
    }
}
