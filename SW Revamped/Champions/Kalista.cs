using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.Common.Tools.Devices;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
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
    internal sealed class KalistaQEffectCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 20, 85, 150, 215, 280 };
        internal static float ADScaling = 1.05F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel > 0 && target.IsAlive && target.IsValidTarget())
            {
                damage = BaseDamage[Getter.QLevel] + (Getter.TotalAD * ADScaling);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage);
            }
            return damage;
        }
    }

    internal sealed class KalistaEEffectCalc : EffectCalc
    {
        internal static int[] BaseDamage = new int[] { 0, 20, 30, 40, 50, 60 };
        internal float ADScaling = 0.7F;
        internal static float APScaling = 0.2F;

        internal static int[] AdditionalDamage = new int[] { 0, 8, 12, 16, 20, 24 };
        internal static float[] AdditionalScaling = new float[] { 0, 0.25F, 0.3F, 0.35F, 0.4F, 0.45F };

        internal float GetEStacks(GameObjectBase target)
        {
            if (target.IsAlive && target.IsVisible)
            {
                return target.BuffManager.ActiveBuffs.FirstOrDefault(x => x.Name.Contains("kalistaexpungemarker"))?.Stacks ?? 0;
            }
            return 0;
        }

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            float stacks = GetEStacks(target);
            if (Getter.ELevel > 0 && target.IsAlive && target.IsValidTarget() && stacks > 0)
            {
                damage = BaseDamage[Getter.ELevel];
                damage += ADScaling * Getter.TotalAD + (Getter.TotalAP * APScaling);
                damage += (AdditionalDamage[Getter.ELevel] + (AdditionalScaling[Getter.ELevel] * Getter.TotalAD)) * (stacks - 1);
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, damage) - 25;
            }
            return damage;
        }
    }

    internal sealed class Kalista : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Kalista");

        internal Group WGroup = new Group("W - Settings");
        internal Switch WEnabled = new Switch("Use W", true);

        internal Group RGroup = new Group("R - Settings");
        internal Switch REnabled = new Switch("Use R", true);
        internal Counter RHealth = new Counter("Health Percent", 20, 0, 100);

        internal static float QCastTime = 0.25F;
        internal static int QRange = 1200;
        internal static int QSpeed = 2400;
        internal static int QWidth = 80;

        internal static float ECastTime = 0.25F;
        internal static int ERange = 1100;

        KalistaQEffectCalc qCalc = new();
        KalistaEEffectCalc eCalc = new();

        internal override void Init()
        {

            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            LineSpell qSpell = new LineSpell(
                Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                qCalc,
                QWidth,
                QRange,
                QSpeed,
                QRange,
                QCastTime,
                false,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance <= QRange,
                x => Getter.Me().Position,
                Color.Blue,
                80,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 1, 0));

            WGroup.AddItem(WEnabled);
            MainTab.AddGroup(WGroup);
            PointAndClickSpell eSpell = new PointAndClickSpell(
                Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                eCalc,
                ERange,
                1000000,
                ERange,
                ECastTime,
                true,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Orange,
                40,
                false, true, true, 7);
            RGroup.AddItem(REnabled);
            RGroup.AddItem(RHealth);
            MainTab.AddGroup(RGroup);
            GameEvents.OnCreateObject += WSpell;
            CoreEvents.OnCoreMainInputAsync += RSpell;
        }

        private bool HasBond()
        {
            if (GetBond() == null) return false;
            return true;
        }

        private GameObjectBase? GetBond()
        {
            return UnitManager.AllyChampions.FirstOrDefault(x => x.BuffManager.GetBuffList().Any(x => x.Name.Contains("kalistacoopstrikeally", StringComparison.OrdinalIgnoreCase)));
        }

        private Task RSpell()
        {
            if (REnabled.IsOn && Getter.RLevel > 0 && Getter.RLooseReady)
            {
                if (HasBond())
                {
                    GameObjectBase b = GetBond();
                    if (b != null)
                    {
                        if (b.IsAlive && b.HealthPercent < RHealth.Value && b.Distance < 1200)
                        {
                            SpellCastProvider.CastSpell(CastSlot.R);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task WSpell(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (WEnabled.IsOn)
            {
                if (callbackObject.Name.Contains("dragon", StringComparison.OrdinalIgnoreCase) || callbackObject.ModelName.Contains("dragon", StringComparison.OrdinalIgnoreCase))
                {
                    if (callbackObject.Name.Contains("spawn") || callbackObject.ModelName.Contains("spawn") || callbackObject.Name.Contains("Attack") || callbackObject.ModelName.Contains("Attack") || callbackObject.Name.Contains("base") || callbackObject.ModelName.Contains("base"))
                    {
                        if (Getter.WLooseReady && Getter.WLevel > 0)
                        {
                            Vector2 castPos = new Vector3(9731.721F, -71.2406F, 4591.2256F).ToWorldToMap();
                            SpellCastProvider.CastSpell(CastSlot.W, castPos, 0.5F);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
