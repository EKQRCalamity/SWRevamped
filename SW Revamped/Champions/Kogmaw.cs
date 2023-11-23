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
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions 
{ 
    internal sealed class KogQCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 90, 140, 190, 240, 290 };
        internal static float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Getter.QLevel] + (Getter.TotalAP * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KogECalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 75, 120, 165, 210, 255 };
        internal static float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Getter.ELevel] + (Getter.TotalAP * APScaling);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KogRCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 100, 140, 180 };
        internal static float APScaling = 0.35F;
        internal static float BADScaling = 0.65F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Getter.RLevel] + (Getter.TotalAP * APScaling) + (Getter.Stats.BonusArmor * BADScaling);
                if (target.HealthPercent > 40)
                {
                    damage = damage + (damage * (target.HealthPercent * 0.883F) / 100);
                }
                else
                {
                    damage = damage * 2;
                }
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class KogWCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class Kogmaw : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Kogmaw");
        internal Switch DrawRangeCircle = new Switch("Draw W Range", true);

        internal static float QCastTime = 0.25F;
        internal static int QRange = 1200;
        internal static int QSpeed = 1650;
        internal static int QWidth = 140;

        internal static int[] WExtraRange = new int[] { 0, 130, 150, 170, 190, 210 };

        internal static float ECastTime = 0.25F;
        internal static int ERange = 1360;
        internal static int ESpeed = 1400;
        internal static int EWidth = 240;

        internal static float RCastTime = 0.25F;
        internal static int[] RTargetRange = new int[] { 0, 1300, 1550, 1800 };
        internal static int RRadius = 240;

        KogQCalc qCalc = new();
        KogECalc eCalc = new();
        KogRCalc rCalc = new();
        KogWCalc wCalc = new();

        internal bool WActive()
        {

            return Getter.Me().BuffManager.ActiveBuffs.Where(x => x.Name.Contains("KogMawBioArcaneBarrage", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null;
        }

        internal int CalculateRangeWithW()
        {
            return (WActive())? (int)Getter.Me().TrueAttackRange : (int)Getter.Me().TrueAttackRange + WExtraRange[Getter.WLevel];
        }

        CircleSpell r;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new LineSpell(
                Oasys.SDK.SpellCasting.CastSlot.Q,
                qCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance <= QRange,
                x => Getter.Me().Position,
                Color.Blue,
                80,
                new CollisionCheck(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                QCastTime,
                false,
                7,
                false,
                SpellCastMode.AfterAutoAttack);
            SelfCastingSpell wSpell = new SelfCastingSpell(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                wCalc,
                (int)Getter.Me().TrueAttackRange,
                0.25F,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < CalculateRangeWithW(),
                x => Getter.Me().Position,
                Color.Yellow,
                40,
                false,
                false,
                false,
                5
                );
            LineSpell eSpell = new LineSpell(
                Oasys.SDK.SpellCasting.CastSlot.E,
                eCalc,
                EWidth,
                ERange,
                ESpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance <= ERange,
                x => Getter.Me().Position,
                Color.Red,
                80,
                new CollisionCheck(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                true,
                ECastTime,
                false,
                6
                );
            r = new CircleSpell(
                Oasys.SDK.SpellCasting.CastSlot.R,
                rCalc,
                RRadius,
                10000,
                3000,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < RTargetRange[Getter.RLevel] && x.Distance > 400,
                x => Getter.Me().Position,
                Color.OrangeRed,
                160,
                new CollisionCheck(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                RCastTime,
                true,
                10,
                false,
                SpellCastMode.AfterAutoAttack
                );
            Tab drawingsTab = MenuManagerProvider.GetTab("Drawings");
            drawingsTab.AddItem(DrawRangeCircle);
            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            r.Range = RTargetRange[Getter.RLevel];
            if (DrawRangeCircle.IsOn)
                RenderFactory.DrawNativeCircle(Getter.Me().Position, CalculateRangeWithW(), new Color(Color.Red.R, Color.Red.G, Color.Red.B, 0.3F), 2, false);
        }
    }
}
