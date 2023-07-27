using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
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
    internal sealed class KayleQCalc : EffectCalc
    {
        internal static int[] Base = { 0, 60, 100, 140, 180, 220 };
        internal static float BonusADScaling = 0.6F;
        internal static float APScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = Base[Getter.QLevel];
                damage += BonusADScaling * Getter.BonusAD;
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KayleWCalc : EffectCalc
    {
        internal static int[] Base = { 0, 55, 80, 105, 130, 155 };
        internal static float APScaling = 0.25F;

        internal override float GetValue(GameObjectBase target)
        {
            float heal = 0;
            if (Getter.WLevel > 0)
            {
                heal = Base[Getter.WLevel];
                heal += APScaling * Getter.TotalAP;
            }
            return heal;
        }
    }

    internal sealed class KayleECalc : EffectCalc
    {
        internal static float[] Base = { 0, 0.08F, 0.085F, 0.09F, 0.095F, 0.1F };
        internal static float Per100 = 0.015F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = (Base[Getter.ELevel] + (Per100 * (float)Math.Floor((float)(Getter.TotalAP / 100)))) * target.MissingHealth;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, Getter.TotalAD, damage, 0);
            }
            return damage;
        }

    }

    internal sealed class KayleRCalc : EffectCalc
    {
        internal static int[] Base = { 0, 200, 300, 400 };
        internal static float BonusADScaling = 1F;
        internal static float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = (Base[Getter.RLevel]) + (APScaling * Getter.TotalAP) + (BonusADScaling * Getter.BonusAD);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Kayle : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Kayle");
        internal static int QRange = 900;
        internal static int QWidth = 150;
        internal static int QSPeed = 1600;

        internal static int WRange = 900;
        internal static float WCastTime = 0.25F;

        internal static int ERange = 525;
        internal static bool ShouldUseE(GameObjectBase target) => (Getter.Level >= 11) ? target.Distance < Getter.AARange : target.Distance < 525;

        internal static int RRange = 900;
        internal static int[] RRadius = { 0, 675, 675, 775 };

        KayleQCalc QCalc = new KayleQCalc();
        KayleWCalc WCalc = new KayleWCalc();
        KayleECalc ECalc = new KayleECalc();
        KayleRCalc RCalc = new KayleRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q, Oasys.Common.Enums.GameEnums.SpellSlot.Q, QCalc, QWidth, QRange, QSPeed, QRange, 0.05F, false, x => x.IsAlive, x => x.IsAlive, x => Getter.Me().Position, Color.Blue, 90, Prediction.MenuSelected.HitChance.VeryHigh, false, true, true, new(true, 2, 0));
            new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.W, Oasys.Common.Enums.GameEnums.SpellSlot.W, WCalc, WRange, WCastTime, x => x.IsAlive, x => x.IsAlive, x => Getter.Me().Position, Color.Green, 130, 5, 80, true);
        }
    }
}
