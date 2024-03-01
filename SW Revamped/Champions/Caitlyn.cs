using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using Oasys.SDK.Tools;
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
    internal sealed class CaitQCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 50, 90, 130, 170, 210 };
        internal float[] ADScaling = new float[] { 0, 1.25F, 1.45F, 1.65F, 1.85F, 2.05F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += ADScaling[Getter.QLevel] * Getter.TotalAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class CaitECalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 80, 130, 180, 230, 280 };
        internal float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel >= 1)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class CaitRCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 300, 525, 750 };
        internal float ADScaling = 1.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage += BaseDamage[Getter.RLevel];
                damage += ADScaling * Getter.BonusAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class Caitlyn : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Caitlyn");

        internal const int QRange = 1300;
        internal const int QWidth = 180;
        internal const int QSpeed = 2200;
        internal const float QCastTime = 0.625F;

        internal const int ERange = 800;
        internal const int EWidth = 140;
        internal const int ESpeed = 1600;
        internal const float ECastTime = 0.15F;

        internal const int RRange = 3500;
        internal const int RWidth = 80;
        internal const float RCastTime = 0.375F;

        CaitQCalc QCalc = new();
        CaitECalc ECalc = new();
        CaitRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                75,
                new(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false, false, false,
                QCastTime,
                false
                );
           
            new LineSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                ECalc,
                EWidth,
                ERange,
                ESpeed,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                80,
                new CollisionCheck(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                ECastTime,
                false,
                7);
            new PointAndClickSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRange,
                10000,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Black,
                100,
                false,
                true,
                false,
                9);
        }
    }
}