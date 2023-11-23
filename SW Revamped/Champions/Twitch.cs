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
using Oasys.Common.EventsProvider;
using Oasys.Common;
using Oasys.Common.Extensions;
using Oasys.SDK.Tools;
using SWRevamped.Utility;

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

        internal float GetValueWithHealthReg(GameObjectBase target)
        {
            return GetValue(target) - (CalculatorEx.CalculateHealthWithRegeneration(target, PassiveDuration(target)) - target.Health);
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
        internal bool usePassive = false;
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
                    ) + ((usePassive)? new TwitchPassiveCalc().GetValueWithHealthReg(target) : 0);
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
        internal Switch QTimeSwitch = new Switch("Draw Q Time", true);

        internal Switch EUsePassive = new Switch("Execute with passive damage", false);
        internal InfoDisplay EPInfo = new InfoDisplay() { Title = "Note", Information = "Deactive Passive Draw when using" };

        internal float QTime()
        {
            List<BuffEntry> buffs = Getter.Me().BuffManager.ActiveBuffs.deepCopy();
            BuffEntry? QBuff = buffs.FirstOrDefault(x => x.Name == "TwitchHideInShadows" && x.Stacks >= 1);
            if (QBuff != null)
            {
                float QTime = QBuff.RemainingDurationMs / 1000;
                return QTime;
            }
            return -1;
        }

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
            qSpell.SpellGroup.AddItem(QTimeSwitch);
            qSpell.SpellGroup.AddItem(QDistanceCounter);

            CircleSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                WCalc,
                275,
                950,
                1400,
                x => x.IsAlive,
                x => x.IsAlive,
                x => Getter.Me().Position,
                Color.Blue,
                70,
                new CollisionCheck(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                true,
                false,
                0.2f,
                false,
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
            eSpell.MainTab.AddItem(EUsePassive);
            eSpell.MainTab.AddItem(EPInfo);

            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            ECalc.usePassive = EUsePassive.IsOn;
            if (QTimeSwitch.IsOn)
            {
                if (QTime() > 0 && Getter.Me().Position.IsOnScreen())
                {
                    Vector2 position = Getter.Me().Position.ToW2S();
                    position.Y -= 10;
                    RenderFactoryProvider.DrawText($"{QTime().ToString("n2")}s", position, Color.Black);
                }
            }
        }
    }
}
