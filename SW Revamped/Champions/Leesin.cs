using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Logic;
using Oasys.Common.Menu;
using SharpDX;
using SharpDX.Win32;
using SWRevamped.Base;
using SWRevamped.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal sealed class LeeQCalc : EffectCalc
    {
        internal static readonly int[] Damage = new int[] { 0, 55, 80, 105, 130, 155 };
        internal static readonly float BonusADScaling = 1.15F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = Damage[Getter.QLevel];
                damage += BonusADScaling * Getter.BonusAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class LeeWCalc : EffectCalc
    {
        // 50 / 100 / 150 / 200 / 250 (+ 80% AP)
        internal static readonly int[] Shield = new int[] { 0, 50, 100, 150, 200, 250 };
        internal static readonly float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float shield = 0;
            if (Getter.WLevel > 0)
            {
                shield = Shield[Getter.WLevel];
                shield += APScaling * Getter.TotalAP;
            }
            return shield;
        }
    }

    internal sealed class LeeECalc : EffectCalc
    {
        internal static readonly int[] Damage = new int[] { 0, 35, 65, 95, 125, 155 };
        internal static readonly float ADScaling = 1.0F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = Damage[Getter.ELevel];
                damage += ADScaling * Getter.BonusAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }

    }

    internal sealed class LeeRCalc : EffectCalc
    {
        internal static readonly int[] Damage = new int[] { 0, 175, 400, 625 };
        internal static readonly float BonusADScaling = 2.0F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = Damage[Getter.RLevel];
                damage += BonusADScaling * Getter.BonusAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class Leesin : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Leesin");

        internal const int QRange = 1200;
        internal const int QWidth = 120;
        internal const float QCastTime = 0.25F;
        internal const int QEnergy = 50;
        internal const int QSpeed = 1800;

        internal const int WRange = 700;
        internal const int WEnergy = 50;

        internal const int ERadius = 450;
        internal const float ECastTime = 0.25F;
        internal const int EEnergy = 50;

        internal const int RRange = 375;
        internal const float RCastTime = 0.25F;

        LeeQCalc QCalc = new();
        LeeWCalc WCalc = new();
        LeeECalc ECalc = new();
        LeeRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(
                Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.OrangeRed,
                QEnergy,
                new CollisionCheck(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                QCastTime,
                false,
                9
                );
            BuffSpell wSpell = new BuffSpell(
                Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRange,
                0F,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Green,
                WEnergy,
                6,
                60);
            PositionCenteredSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                ECalc,
                ERadius,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                100,
                new CollisionCheck(false, new()),
                true,
                true,
                true,
                0.25f,
                false
                );
            PointAndClickSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                SpellSlot.R,
                RCalc,
                RRange,
                10000,
                RRange,
                RCastTime,
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
            new MultiClassSpell(new SpellBase[] { qSpell, wSpell, eSpell }, new Func<GameObjectBase, bool>[] { x => !Getter.QSpell.SpellData.SpellName.Contains("two", StringComparison.OrdinalIgnoreCase), x => !Getter.WSpell.SpellData.SpellName.Contains("two", StringComparison.OrdinalIgnoreCase), x => !Getter.ESpell.SpellData.SpellName.Contains("two", StringComparison.OrdinalIgnoreCase) });

        }
    }
}
