﻿using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.SDK;
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

    internal sealed class EzrealQCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 20, 45, 70, 95, 120 };
        internal static float ADScaling = 1.3F;
        internal static float APScaling = 0.15F;
        
        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling;
                damage += Getter.TotalAD * ADScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage) + Utility.CalculatorEx.CalculateAADamageWithOnHit(target);
            }
            
            return damage;
        }
    }

    internal sealed class EzrealWCalc : EffectCalc
    {

        internal static int[] WBonusDamage = new int[] { 0, 80, 135, 190, 245, 300 };
        internal static float WADScaling = 1F;
        internal static float[] WAPScaling = new float[] { 0, 0.7F, 0.75F, 0.8F, 0.85F, 0.9F };

        internal static bool WOnEnemy(GameObjectBase target)
        {
            return target.BuffManager.GetActiveBuff(x => x.Name.Contains("ezrealwattach") && x.Stacks > 0) != null;
        }

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (WOnEnemy(target))
            {
                damage = WBonusDamage[Getter.WLevel];
                damage += Getter.BonusAD * WADScaling;
                damage += Getter.TotalAP * WAPScaling[Getter.WLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class EzrealECalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 80, 130, 180, 230, 280 };
        internal static float APScaling = 0.75F;
        internal static float BonusADScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += Getter.BonusAD * BonusADScaling;
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class EzrealRCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 325, 500, 675 };
        internal static float ADScaling = 1;
        internal static float APScaling = 0.9F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.BonusAD * ADScaling;
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Ezreal : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Ezreal");

        internal const int QRange = 1200;
        internal const int QWidth = 120;
        internal const int QSpeed = 2000;
        internal const float QCastTime = 0.25F;

        internal const int WRange = 1200;
        internal const int WWidth = 160;
        internal const int WSpeed = 1700;
        internal const float WCastTime = 0.25F;

        internal const int ERange = 475;
        internal const int EEffectRange = 750;
        internal const float ECastTime = 0.25F;

        internal const int RRange = 100000;
        internal const int RWidth = 320;
        internal const int RSpeed = 2000;
        internal const float RCastTime = 1;

        EzrealQCalc QCalc = new EzrealQCalc();
        EzrealWCalc WCalc = new EzrealWCalc();
        EzrealECalc ECalc = new EzrealECalc();
        EzrealRCalc RCalc = new EzrealRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell wSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                WCalc,
                WWidth,
                WRange,
                WSpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < WRange * 0.97,
                x => Getter.Me().Position,
                Color.Red,
                50,
                new CollisionCheck(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                WCastTime,
                false,
                9,
                false,
                SpellCastMode.AfterAutoAttack);
            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QRange * 0.97,
                x => Getter.Me().Position,
                Color.Red,
                40,
                new CollisionCheck(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                true,
                QCastTime,
                false,
                7,
                false,
                SpellCastMode.AfterAutoAttack);
            
            DashSpell eSpell = new DashSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                EEffectRange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position, 
                Color.OrangeRed,
                8
                );

            // Other approach
            //DashSpell eSpell = new DashSpell(Oasys.SDK.SpellCasting.CastSlot.E,
            //Oasys.Common.Enums.GameEnums.SpellSlot.E,
            //    ECalc,
            //    ERange,
            //    ECastTime,
            //    false,
            //    x => x.IsAlive,
            //    x => x.IsAlive,
            //    x => GameEngine.WorldMousePosition,
            //    Color.OrangeRed,
            //    8
            //   );

            LineSpell rSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                RCalc,
                RWidth,
                RRange,
                RSpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < RRange,
                x => Getter.Me().Position,
                Color.Red,
                100,
                new CollisionCheck(false, new()),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                RCastTime,
                true,
                6);


        }
    }
}
