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
    internal sealed class MalphQCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 70, 120, 170, 220, 270 };
        internal static float APScaling = 0.6F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class MalphWCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class MalphECalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 70, 110, 150, 190, 230 };
        internal static float APScaling = 0.6F;
        internal static float ArmorScaling = 0.4F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
                damage += ArmorScaling * Getter.Armor;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }

    }

    internal sealed class MalphRCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 200, 300, 400 };
        internal static float APScaling = 0.9F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Malphite : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Malphite");

        internal const int QRange = 625;
        internal const float QCastTime = 0.25F;
        internal const int QSpeed = 1200;

        internal const int WCastTime = 0;

        internal const int ERadius = 400;
        internal const float ECastTime = 0.2419F;

        internal const int RRange = 1000;
        internal const int RRadius = 325;
        internal int RSpeed => 1500 + (int)Getter.MoveSpeed;

        MalphQCalc QCalc = new MalphQCalc();
        MalphECalc ECalc = new MalphECalc();
        MalphWCalc WCalc = new MalphWCalc();
        MalphRCalc RCalc = new MalphRCalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            PointAndClickSpell qSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
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
                false,
                true,
                false,
                5
                );
            SelfCastingSpell wSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                Getter.AARange,
                WCastTime,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.White,
                80,
                false,
                false,
                false,
                0
                );
            CircleSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERadius,
                ERadius,
                0,
                ERadius,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Orange,
                50,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 100000, 0),
                4);
            CircleSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRadius,
                RRange,
                RSpeed,
                RRange,
                0,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new(true, 100000, 2),
                6);
        }
    }
}
