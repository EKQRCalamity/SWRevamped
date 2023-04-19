using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject;
using Oasys.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWRevamped.Base;
using Oasys.Common.Menu;
using SharpDX;
using SWRevamped.Spells;
using Oasys.Common.Menu.ItemComponents;

namespace SWRevamped.Champions
{
    internal sealed class TwitchPassiveCalc : EffectCalc
    {
        internal static float PassiveDuration(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            BuffEntry? buff = buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom");
            if (buff == null)
                return 0;
            return buff.EndTime - GameEngine.GameTime;
        }

        internal static float EStacks(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom")?.Stacks ?? 0;
        }

        internal static float GetDamage(GameObjectBase target)
        {
            float damage = 0;
            float stacks = EStacks(target);
            float APScaling = 0.03F * stacks;
            float RawDamage = (float)(1 * Math.Floor((decimal)(Getter.Me().Level / 4))) * stacks;
            damage = (RawDamage + (APScaling * Getter.Me().UnitStats.TotalAbilityPower)) * (float)PassiveDuration(target);
            return damage;
        }

        internal override float GetValue(GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, 0, GetDamage(target));
        }
    }

    internal sealed class TwitchQCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class TwitchWCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal sealed class TwitchECalc : EffectCalc
    {
        internal float EStacks(GameObjectBase enemy)
        {
            List<BuffEntry> buffs = enemy.BuffManager.ActiveBuffs.deepCopy();
            return buffs.FirstOrDefault(x => x.Name == "TwitchDeadlyVenom")?.Stacks ?? 0;
        }

        internal int[] Damage = new int[] { 0, 20, 30, 40, 50, 60 };
        internal int[] PerStackDmg = new int[] { 0, 15, 20, 25, 30, 35 };
        internal float ADScaling = 0.35F;
        internal float APScaling = 0.35F;

        internal override float GetValue(GameObjectBase target)
        {
            if (Getter.EReady && Getter.ELevel > 0 && EStacks(target) >= 1)
            {
                float damage = Damage[Getter.ELevel];
                float apstackDamage = (Getter.Me().UnitStats.TotalAbilityPower * APScaling) * EStacks(target);
                float stackDamage = (PerStackDmg[Getter.ELevel] + (Getter.Me().UnitStats.BonusAttackDamage * ADScaling)) * EStacks(target);
                return DamageCalculator.CalculateActualDamage(
                    Getter.Me(),
                    target,
                    damage + stackDamage,
                    apstackDamage,
                    0
                    );
            }
            return 0;
        }
    }

    internal sealed class TwitchRCalc : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0;
        }
    }

    internal class Twitch : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Twitch");

        TwitchPassiveCalc PassiveCalc = new TwitchPassiveCalc();
        TwitchQCalc QCalc = new TwitchQCalc();
        TwitchWCalc WCalc = new TwitchWCalc();
        TwitchECalc ECalc = new TwitchECalc();
        TwitchRCalc RCalc = new TwitchRCalc();

        internal Counter QDistanceCounter = new Counter("Usage Range", 1000, 0, 1200);

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            Group PassiveGroup = new Group("P Settings");
            MainTab.AddGroup(PassiveGroup);
            Effect passiveEffect = new Effect("P", true, 4, 10000, MainTab, PassiveGroup, PassiveCalc, Color.Green);
            EffectDrawer.AddDamage(passiveEffect);



            SelfCastingSpell qSpell = new(Oasys.SDK.SpellCasting.CastSlot.Q,
                Oasys.Common.Enums.GameEnums.SpellSlot.Q,
                QCalc,
                1200,
                0.2F,
                x => x.IsAlive,
                x => x.IsAlive && x.Distance < QDistanceCounter.Value,
                x => Getter.Me().Position,
                Color.Red,
                40,
                false,
                false,
                false,
                0
                );
            qSpell.SpellGroup.AddItem(QDistanceCounter);

            CircleSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                Oasys.Common.Enums.GameEnums.SpellSlot.W,
                WCalc,
                275,
                950,
                1400,
                950,
                0.25F,
                false,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                70,
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                new CollisionCheck(true, 99999, 0),
                4);
            MultiTargettingSpell eSpell = new(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                1200,
                0.25F,
                x => x.IsAlive,
                x => x.IsAlive,
                Color.Orange,
                true,
                90,
                true,
                6);
        }
    }
}
