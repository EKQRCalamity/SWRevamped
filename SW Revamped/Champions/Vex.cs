using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.SDK;
using Oasys.SDK.Events;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Spells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal sealed class VexQCalc : EffectCalc
    {
        internal static int[] Base = { 0, 60, 105, 150, 195, 240 };
        internal static float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = Base[Getter.QLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class VexWCalc : EffectCalc
    {
        internal static int[] Base = { 0, 80, 120, 160, 200, 240 };
        internal static float APScaling = 0.3F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = Base[Getter.WLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class VexECalc : EffectCalc
    {
        internal static int[] Base = { 0, 50, 70, 90, 110, 130 };
        internal static float[] APScaling = { 0, 40, 45, 50, 55, 60 };

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = Base[Getter.ELevel];
                damage += APScaling[Getter.ELevel] * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;

        }
    }

    internal sealed class VexRLineCalc : EffectCalc
    {
        internal static int[] Base = { 0, 70, 125, 175 };
        internal static int[] Base2 = { 0, 150, 250, 350 };
        internal static float APScaling = 0.2F;
        internal static float APScaling2 = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                // Commented out for now. Better to calculate both together and cast depending on it for now.
                //if (Getter.RSpell.SpellData.SpellName.Contains("VexR2"))
                //{
                    damage = Base2[Getter.RLevel];
                    damage += APScaling2 * Getter.TotalAP;
                //} else
                //{
                    damage += Base[Getter.RLevel];
                    damage += APScaling * Getter.TotalAP;
                //}
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal class Vex : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Vex");

        internal const int QRange = 1200;
        internal const int QSpeed = 1000;
        internal const int QWidth = 160;
        internal const float QCastTime = 0.15F;

        internal const int WRadius = 475;
        internal const float WCastTime = 0.25F;

        internal const int ERange = 800;
        internal const int ESpeed = 1300;
        internal const int ERadius = 200;
        internal const float ECastTime = 0.25F;

        internal const int RRange = 2000;
        internal const int RSpeed = 1600;
        internal const int RWidth = 260;
        internal const float RCastTime = 0.25F;

        VexQCalc QCalc = new VexQCalc();
        VexWCalc WCalc = new VexWCalc();
        VexECalc ECalc = new VexECalc();
        VexRLineCalc RCalc = new VexRLineCalc();

        LineSpell R;

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            new LineSpell(
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
                Color.Purple,
                65,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 99999, 0)
                );
            new CircleSpell(
                Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WRadius,
                WRadius,
                100000,
                WRadius,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < WRadius,
                x => Getter.Me().Position,
                Color.Blue,
                75,
                Prediction.MenuSelected.HitChance.High,
                false,
                true,
                false,
                new CollisionCheck(true, 99999, 0),
                4
                );
            new CircleSpell(
                Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                ERadius,
                ERange,
                ESpeed,
                ERange,
                ECastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Red,
                110,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 99999, 0),
                3
                );
            R = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RWidth,
                RRange,
                RSpeed,
                RRange,
                RCastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Magenta,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                new CollisionCheck(true, 0, 0),
                2
                );
            new MultiClassSpell(new SpellBase[] { R }, new Func<GameObjectBase, bool>[] { x => !Getter.RSpell.SpellData.SpellName.Contains("VexR2", StringComparison.OrdinalIgnoreCase) });

            CoreEvents.OnCoreMainTick += UpdateRRange;
        }

        internal int[] Range = { 0, 2000, 2500, 3000 };

        private Task UpdateRRange()
        {
            R.CastRange = Range[Getter.RLevel];
            R.Range = Range[Getter.RLevel];
            return Task.CompletedTask;
        }
    }
}
