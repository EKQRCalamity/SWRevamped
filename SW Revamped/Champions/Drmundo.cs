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

    internal sealed class MundoQCalc : EffectCalc
    {
        internal static int[] MinDamage = { 0, 80, 130, 180, 230, 280 };
        internal static float[] DefaultDamage = { 0, 0.2F, 0.225F, 0.25F, 0.275F, 0.3F };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = target.Health * DefaultDamage[Getter.QLevel];
                damage = (damage > MinDamage[Getter.QLevel])? damage : MinDamage[Getter.QLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class MundoWCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 80, 140, 200, 260, 320 };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = BaseDamage[Getter.WLevel];
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class MundoECalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class MundoRCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal class Drmundo : ChampionModule
    {
        internal Tab Maintab = new Tab("SW - DrMundo");

        internal const int QRange = 1050;
        internal const int QWidth = 120;
        internal const int QSpeed = 2000;
        internal const float QCastTime = 0.25F;

        internal const int WRange = 325;
        internal const int WCastTime = 0;

        internal int ERange => (int)Getter.Me().TrueAttackRange + 50;

        internal int RRange = 1200;

        MundoQCalc QCalc = new();
        MundoWCalc WCalc = new();
        MundoECalc ECalc = new();
        MundoRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(Maintab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
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
                Color.Red,
                0,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0),
                5);
            ActivateSpell wSpell = new ActivateSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRange,
                WCastTime,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < ERange,
                x => x.Distance > ERange,
                x => Getter.Me().Position,
                Color.Blue,
                0,
                7);
            SelfCastingSpell eSpell = new SelfCastingSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERange,
                0,
                x => x.IsAlive,
                x => x.IsAlive && Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < ERange) != null,
                x => Getter.Me().Position,
                Color.Yellow,
                0,
                false,
                false,
                false,
                6);
            BuffSpell rSpell = new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRange,
                0,
                x => x.IsAlive,
                x => x.IsMe && Oasys.Common.Logic.TargetSelector.GetBestHeroTarget(null, x => x.Distance < RRange - 200) != null,
                x => Getter.Me().Position,
                Color.Red,
                0,
                9);
        }
    }
}
