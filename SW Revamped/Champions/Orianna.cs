using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
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
    internal sealed class OriQCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 60, 90, 120, 150, 180 };
        internal float APScaling = 0.5F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.QLevel >= 1)
            {
                damage = BaseDamage[Getter.QLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
            
        }
    }

    internal sealed class OriWCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 60, 105, 150, 195, 240 };
        internal float APScaling = 0.7F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.WLevel >= 1)
            {
                damage = BaseDamage[Getter.WLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class OriECalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 60, 90, 120, 150, 180 };
        internal float APScaling = 0.3F;

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

    internal sealed class OriEShieldCalc : EffectCalc
    {
        internal int[] Base = { 0, 55, 90, 125, 160, 195 };
        internal float APScaling = 0.45F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = Base[Getter.ELevel];
                damage += APScaling * Getter.TotalAP;
            }
            return damage;
        }
    }

    internal sealed class OriRCalc : EffectCalc
    {
        internal int[] BaseDamage = { 0, 250, 400, 550};
        internal float APScaling = 0.95F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.RLevel >= 1)
            {
                damage = BaseDamage[Getter.RLevel];
                damage += APScaling * Getter.TotalAP;
                damage = DamageCalculator.CalculateActualDamage(Getter.Me(), target, 0, damage, 0);
            }
            return damage;
        }
    }

    internal sealed class Orianna : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Orianna");

        internal Switch DrawWRangeSwitch = new Switch("Draw W Range", false);
        internal Switch DrawRRangeSwitch = new Switch("Draw R Range", false);

        OriQCalc QCalc = new();
        OriWCalc WCalc = new();
        OriECalc ECalc = new();
        OriRCalc RCalc = new();


        internal static GameObjectBase Ball => UnitManager.AllNativeObjects.FirstOrDefault(x => x.Name == "TheDoomBall" && x.IsAlive && x.Health >= 1);

        internal static bool IsBallOnMe()
        {
            var buffs = UnitManager.MyChampion.BuffManager.Buffs.deepCopy();
            var buff = buffs.FirstOrDefault(x => x.Name == "orianaghostself");
            return buff != null && buff.IsActive && buff.Stacks > 0;
        }

        internal static Hero? getBallHolder()
        {
            if (Ball == null)
            {
                List<Hero> clients = UnitManager.AllyChampions.deepCopy();
                foreach (Hero client in clients)
                {
                    List<BuffEntry> buffs = client.BuffManager.Buffs.deepCopy();
                    BuffEntry buff = buffs.FirstOrDefault(x => x.Name == "orianaghost" || x.Name == "orianaghostself");
                    if (client.IsAlive && buff != null && buff.IsActive && buff.Stacks > 0)
                        return client;
                }
            }
            return null;
        }

        internal static Vector3 GetBallPosition()
        {
            if (Ball == null)
            {
                Hero? holder = getBallHolder();
                if (holder != null)
                {
                    return holder.Position;
                }
                else
                {
                    return Getter.Me().Position;
                }
            }
            else
            {
                return Ball.Position;
            }
        }

        internal static Vector3 QPosition() => GetBallPosition();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();

            LineSpell qSpell = new LineSpell(Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                160,
                825,
                1400,
                x => x.IsAlive,
                x => x.IsAlive,
                x => QPosition(),
                Color.Blue,
                50,
                new(false, new()),
                Prediction.MenuSelected.HitChance.VeryHigh,
                true,
                true,
                true,
                0.0f,
                false,
                6);
            PositionCenteredSpell wSpell = new(Oasys.SDK.SpellCasting.CastSlot.W,
                WCalc,
                225,
                x => x.IsAlive,
                x => x.IsAlive,
                x => QPosition(),
                Color.Red,
                80,
                new CollisionCheck(true, new() { new(0, CollisionModes.Hero, CollLogic.Min) }),
                true,
                true,
                true,
                0f,
                false,
                5);
            BuffSpell eSpell = new BuffSpell(Oasys.SDK.SpellCasting.CastSlot.E,
                Oasys.Common.Enums.GameEnums.SpellSlot.E,
                ECalc,
                1120,
                0,
                x => x.IsAlive,
                x => x.IsAlive,
                x => QPosition(),
                Color.Green,
                60,
                3,
                70);
            PositionCenteredSpell rSpell = new(Oasys.SDK.SpellCasting.CastSlot.R,
                RCalc,
                400,
                x => x.IsAlive,
                x => x.IsAlive,
                x => QPosition(),
                Color.OrangeRed,
                100,
                new CollisionCheck(true, new() { new(2, CollisionModes.Hero, CollLogic.Min) }),
                true,
                false,
                true,
                0.25f,
                false,
                4);
        }
    }
}
