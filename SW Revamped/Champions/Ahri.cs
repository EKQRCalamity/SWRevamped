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
    internal sealed class AhriQCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 40, 65, 90, 115, 140 };
        internal float APScaling = 0.45F;

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

    internal sealed class AhriWCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 50, 75, 100, 125, 150 };
        internal float APScaling = 0.3F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class AhriECalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 80, 110, 140, 170, 200};
        internal float APScaling = 0.6F;

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

    internal sealed class AhriRCalc : EffectCalc
    {
        internal int[] BaseDamage = new int[] { 0, 60, 90, 120 };
        internal float APScaling = 0.35F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }


    internal class Ahri : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Ahri");

        internal static readonly int QRange = 900;
        internal static readonly int QWidth = 200;
        internal static readonly int QSpeed = 1550;
        internal static readonly float QCastTime = 0.25F;

        internal static readonly int WRange = 725;
        internal static readonly float WCastTime = 0;

        internal static readonly int ERange = 1000;
        internal static readonly int EWidth = 120;
        internal static readonly int ESpeed = 1550;
        internal static readonly float ECastTime = 0.25F;

        internal static readonly int RRange = 600;
        internal static readonly float RCastTime = 0;

        AhriQCalc QCalc = new();
        AhriWCalc WCalc = new();
        AhriECalc ECalc = new();
        AhriRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.Q,
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
                Color.Green,
                95,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 999, 0),
                7
                );
            SelfCastingSpell wSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRange,
                WCastTime,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                30,
                false,
                true,
                false,
                6
                );
            LineSpell eSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                EWidth,
                ERange,
                ESpeed,
                ERange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                95,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0),
                8
                );
            DashSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Orange,
                100,
                9
                );
        }
    }
}
