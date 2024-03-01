using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.SpellCasting;
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
    internal sealed class IvernQCalc : EffectCalc
    {
        internal static int[] BaseDamage = { 0, 80, 125, 170, 215, 260};
        internal static float APScaling = 0.7F;

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

    internal sealed class IvernECalc : EffectCalc
    {
        internal static int[] BaseShield = { 0, 85, 125, 165, 205, 245 };
        internal static float APScaling = 0.8F;

        internal override float GetValue(GameObjectBase target)
        {
            float damage = 0;
            if (Getter.ELevel > 0)
            {
                damage = BaseShield[Getter.ELevel];
                damage += Getter.TotalAP * APScaling;
            }
            return damage;
        }
    }
    internal sealed class Ivern : ChampionModule
    {
        internal Tab MainTab = new Tab("SW - Ivern");

        internal Group WSpellGroup = new Group("W Settings");
        internal Switch WRengarSupport = new Switch("Rengar Jump Helper", true);
        internal Switch WAntiKalista = new Switch("Anti Kalista", true);

        internal int QRange = 1150;
        internal int WRange = 1000;
        internal int ERange = 750;
        internal int RRange = 1400;

        internal int QWidth = 160;

        internal int QSpeed = 1300;

        internal int QMana = 60;
        internal int WMana = 30;
        internal int EMana = 70;

        internal float QCastTime = 0.25F;
        internal float WCastTime = 0.25F;
        internal float ECastTime = 0;

        IvernQCalc QCalc = new IvernQCalc();
        IvernECalc ECalc = new IvernECalc();

        internal override void Init()
        {
            MenuManagerProvider.AddTab(MainTab);
            EffectDrawer.Init();
            LineSpell qSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.Q,
                QCalc,
                QWidth,
                QRange,
                QSpeed,
                x => x.IsAlive,
                x => x.IsAlive && x.IsValidTarget(),
                x => Getter.Me().Position,
                Color.Red,
                QMana,
                new(true, new() { new(0, CollisionModes.HeroMinion, CollLogic.Max)}),
                Prediction.MenuSelected.HitChance.VeryHigh,
                false,
                false,
                false,
                QCastTime
            );

            MainTab.AddGroup(WSpellGroup);
            WSpellGroup.AddItem(WRengarSupport);
            WSpellGroup.AddItem(WAntiKalista);
            
            BuffSpell eSpell = new(
                Oasys.SDK.SpellCasting.CastSlot.E, 
                Oasys.Common.Enums.GameEnums.SpellSlot.E, 
                ECalc, ERange, 
                ECastTime, 
                x => x.IsAlive, 
                x => x.IsAlive && x.IsValidTarget(), 
                x => Getter.Me().Position, 
                Color.Blue, 
                EMana, 
                5, 
                70
            );

            PuppetSpell rSpell = new(CastSlot.R, Oasys.Common.Enums.GameEnums.SpellSlot.R, RRange, x => x.IsAlive, x => x.IsAlive, "IvernMinion");
            CoreEvents.OnCoreMainInputAsync += WSpell;
        }

        private Task WSpell()
        {

            if (Getter.WLooseReady)
            {
                // Rengar
                if (UnitManager.AllyChampions.Any(x => x.IsAlive && x.ModelName == "Rengar" && x.Distance < WRange))
                {
                    GameObjectBase rengar = UnitManager.AllyChampions.First(x => x.ModelName == "Rengar");
                    if (rengar.IsBasicAttacking)
                    {
                        SpellActiveEntry spell = rengar.GetCurrentCastingSpell();
                        if (spell.Targets.Count > 0)
                        {
                            SpellCastProvider.CastSpell(CastSlot.W, rengar.Position, WCastTime);
                        }
                    }
                }
                // Kalista
                if (UnitManager.EnemyChampions.Any(x => x.IsAlive && x.ModelName == "Kalista" && x.Distance < WRange))
                {
                    GameObjectBase rengar = UnitManager.EnemyChampions.First(x => x.ModelName == "Kalista");
                    if (rengar.IsBasicAttacking)
                    {
                        SpellActiveEntry spell = rengar.GetCurrentCastingSpell();
                        if (spell.Targets.Count > 0 && spell.Targets[0].Distance < WRange)
                        {
                            SpellCastProvider.CastSpell(CastSlot.W, rengar.Position, WCastTime);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
