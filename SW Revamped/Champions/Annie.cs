using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
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
    internal sealed class AnnieQCalc : EffectCalc
    {
        internal readonly int[] BaseDamage = new int[] { 0, 80, 115, 150, 185, 220 };
        internal readonly float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class AnnieWCalc : EffectCalc
    {
        internal readonly int[] BaseDamage = new int[] { 0, 70, 115, 160, 205, 250 };
        internal readonly float APScaling = 0.85F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel > 0)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }


    internal sealed class AnnieRCalc : EffectCalc
    {
        internal readonly int[] BaseDamage = new int[] { 0, 150, 275, 400 };
        internal readonly float APScaling = 0.75F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel > 0)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += Getter.TotalAP * APScaling;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Annie : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Annie");
        internal Switch ROnlyUseMaxStacks = new Switch("Use w/ Maxstacks", true);

        internal static readonly int QRange = 600;
        internal static readonly float QCastTime = 0.25F;

        internal static readonly int WRange = 600;
        internal static readonly int WAngle = 49;
        internal static readonly float WCastTime = 0.25F;

        internal static readonly int RRange = 600;
        internal static readonly int RRadius = 250;
        internal static readonly float RCastTime = 0.25F;

        AnnieQCalc QCalc = new();
        AnnieWCalc WCalc = new();
        AnnieRCalc RCalc = new();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            PointAndClickSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                QRange,
                100000,
                QRange,
                QCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QRange,
                x => Getter.Me().Position,
                Color.Red,
                80,
                false,
                true,
                false
                );
            ConeSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                WAngle,
                WRange,
                100000,
                WRange,
                WCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QRange,
                x => Getter.Me().Position,
                Color.Blue,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 10000, 0),
                4
                );
            CircleSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                Oasys.Common.Enums.GameEnums.SpellSlot.R,
                RCalc,
                RRadius,
                RRange,
                100000,
                RRange,
                RCastTime,
                false,
                x => x.IsAlive && (ROnlyUseMaxStacks.IsOn) ? x.BuffManager.GetBuffList().Any(x => x.Name.Contains("anniepassiveprimed", StringComparison.OrdinalIgnoreCase) && x.Stacks == 1) : true,
                x => x.IsAlive && x.Distance < QRange,
                x => Getter.Me().Position,
                Color.Yellow,
                100,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 10000, 0),
                9
                );
            MainTab.GetGroup("R Settings").AddItem(ROnlyUseMaxStacks);
        }
    }
}