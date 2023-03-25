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
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            
            return damage;
        }
    }

    internal sealed class EzrealWCalc : EffectCalc
    {

        internal static int[] WBonusDamage = new int[] { 0, 80, 135, 190, 245, 300 };
        internal static float WADScaling = 0.6F;
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
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class EzrealRCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 350, 500, 650 };
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
        internal const float ECastTime = 0.25F;

        internal const int RRange = 100000;
        internal const int RWidth = 320;
        internal const int RSpeed = 2000;
        internal const float RCastTime = 1;

        EzrealQCalc qCalc = new EzrealQCalc();
        EzrealWCalc wCalc = new EzrealWCalc();
        EzrealECalc eCalc = new EzrealECalc();
        EzrealRCalc rCalc = new EzrealRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                qCalc,
                QWidth,
                QRange,
                QSpeed,
                QRange,
                QCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QRange,
                x => Getter.Me().Position,
                Color.Red,
                40,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                true,
                new CollisionCheck(true, 0, 0),
                7);
            LineSpell wSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                wCalc,
                WWidth,
                WRange,
                WSpeed,
                WRange,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < WRange,
                x => Getter.Me().Position,
                Color.Red,
                50,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 9999, 0),
                9);
            LineSpell rSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                rCalc,
                RWidth,
                RRange,
                RSpeed,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < RRange,
                x => Getter.Me().Position,
                Color.Red,
                100,
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0),
                6);


        }
    }
}
