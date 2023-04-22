using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
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
    internal sealed class JinxQCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class JinxWCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 10, 60, 110, 160, 210 };
        internal float ADScaling = 1.6F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += ADScaling * Getter.TotalAD;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class JinxECalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 70, 120, 170, 220, 270 };
        internal float APScaling = 1F;

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

    internal sealed class JinxRCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 300, 450, 600 };
        internal float ADScaling = 1.2F;
        internal float[] MissHPScaling = { 0, 0.25F, 0.3F, 0.35F };
        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += ADScaling * Getter.TotalAD;
                damage += MissHPScaling[Getter.RLevel] * target.MissingHealth;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal class Jinx : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Jinx");
        internal Switch DrawRangeCircle = new Switch("Draw Q Range", true);

        internal static int[] QExtraRange = { 0, 80, 110, 140, 170, 200 };

        internal static int WRange = 1500;
        internal static int WWidth = 120;
        internal static int WSpeed = 3300;
        internal static float WCastTime = 0.6F;

        internal static int ERange = 925;
        internal static int ERadius = 225;
        internal static int ESpeed = 5000;

        internal static int RWidth = 280;
        internal static int RSpeed = 1700;
        internal static int RRange = 1000000;

        JinxQCalc QCalc = new JinxQCalc();
        JinxWCalc WCalc = new JinxWCalc();
        JinxECalc ECalc = new JinxECalc();
        JinxRCalc RCalc = new JinxRCalc();
        ActivateSpell qSpell;
        internal bool QActive()
        {
            return Getter.Me().BuffManager.GetBuffList().Any(x => x.IsActive && x.Name == "JinxQ");
        }

        internal float CalculateCurrentNormalRange()
        {
            return QActive() ? (int)Getter.Me().TrueAttackRange - QExtraRange[Getter.QLevel] : (int)Getter.Me().TrueAttackRange;
        }

        internal int CalculateRangeWithQ()
        {
            return QActive()? (int)Getter.Me().TrueAttackRange : (int)Getter.Me().TrueAttackRange + QExtraRange[Getter.QLevel];
        }

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            qSpell = new ActivateSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                1100,
                0,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance > CalculateCurrentNormalRange() && x.Distance < CalculateRangeWithQ(),
                x => x.Distance > CalculateRangeWithQ() + 100,
                x => Getter.Me().Position,
                Color.Red,
                40,
                7);
            LineSpell wSpell = new LineSpell(
                Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WWidth,
                WRange,
                WSpeed,
                WRange,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0));
            CircleSpell eSpell = new CircleSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERadius,
                ERange,
                ESpeed,
                ERange,
                0,
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
                new CollisionCheck(true, 99999, 0), 4);
            LineSpell rSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RWidth,
                RRange,
                RSpeed,
                RRange,
                0.6F,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Orange,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 99999, 0), 6);

            CoreEvents.OnCoreRender += Draw;
        }

        private void Draw()
        {
            if (qSpell.IsOn)
            {
                if (qSpell.IsActivated != QActive())
                {
                    qSpell.IsActivated = QActive();
                }
            }
            if (DrawRangeCircle.IsOn)
            {
                RenderFactory.DrawNativeCircle(Getter.Me().Position, CalculateRangeWithQ(), new Color(Color.Red.R, Color.Red.G, Color.Red.B, 0.3F), 2, false);
            }
        }
    }
}
