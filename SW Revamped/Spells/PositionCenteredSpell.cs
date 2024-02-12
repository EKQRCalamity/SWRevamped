using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Events;
using Oasys.SDK.Rendering;
using Oasys.SDK.SpellCasting;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Oasys.SDK.Prediction.MenuSelected;

namespace SWRevamped.Spells
{
    internal class PositionCenteredSpell : SpellBase
    {
        internal bool Execute = false;
        internal bool Friendly = false;
        internal EffectCalc Calculator;
        

        internal Counter MinMana;
        internal Switch useHarass;
        internal Switch useLaneclear;
        internal Switch useLasthit;
        internal Switch drawRange;
        internal ModeDisplay drawColorDisplay;
        internal Group rangeDrawGroup = new Group("Range Drawings");

        internal Color color => Colors.NameToColor(drawColorDisplay.SelectedModeName);

        internal CollisionCheck collisionCheck;
        internal Func<GameObjectBase, Vector3> SourcePosition;
        internal Func<GameObjectBase, bool> SelfCheck;
        internal Func<GameObjectBase, bool> TargetCheck;

        internal PositionCenteredSpell(CastSlot castSlot, EffectCalc calculator, int radius, Func<GameObjectBase, bool> selfCheck, Func<GameObjectBase, bool> targetCheck, Func<GameObjectBase, Vector3> sourcePosition, SharpDX.Color drawColor, int minmana, CollisionCheck collCheck, bool laneclear = false, bool lasthit = false, bool harass = false, float casttime = 0.0f, bool execute = false, int drawPrio = 5, bool friendlySpell = false)
        {
            collisionCheck = collCheck;
            Friendly = friendlySpell;
            SourcePosition = sourcePosition;
            SelfCheck = selfCheck;
            TargetCheck = targetCheck;
            Slot = Utility.Slots.CastSlotToSpellSlot(castSlot);
            SpellCastSlot = castSlot;
            Width = radius;
            CastTime = casttime;
            Execute = execute;
            Calculator = calculator;
            Range = Width;

            // Menu stuff
            MainTab = Getter.MainTab;
            SpellGroup = new Group($"{SpellSlotToString()} Settings");
            drawColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = Oasys.SDK.ColorConverter.GetColors(), SelectedModeName = $"{Colors.ColorToName(drawColor)}" };
            MinMana = new Counter("Min Mana", minmana, 0, 1000);
            drawRange = new Switch("Draw Range", true);

            MainTab.AddGroup(SpellGroup);
            SpellGroup.AddItem(IsOnSwitch);
            SpellGroup.AddItem(MinMana);

            rangeDrawGroup.AddItem(drawRange);
            rangeDrawGroup.AddItem(drawColorDisplay);
            SpellGroup.AddItem(rangeDrawGroup);

            if (collisionCheck.MinCollisionObjectsCounter.Value > 0)
                SpellGroup.AddItem(collisionCheck.MinCollisionObjectsCounter);

            useLaneclear = new Switch("Laneclear", laneclear);
            useLasthit = new Switch("Lasthit", lasthit);
            useHarass = new Switch("Harass", harass);


            if (laneclear)
            {
                SpellGroup.AddItem(useLaneclear);
            }
            if (lasthit)
            {
                SpellGroup.AddItem(useLasthit);
            }
            if (harass)
            {
                SpellGroup.AddItem(useHarass);
            }

            Effect effect = new Effect($"{SpellSlotToString()}", true, drawPrio, Range, MainTab, SpellGroup, Calculator, drawColor);
            EffectDrawer.Add(effect, Friendly);

            CoreEvents.OnCoreMainInputAsync += ComboInput;
            CoreEvents.OnCoreLasthitInputAsync += LasthitInput;
            CoreEvents.OnCoreHarassInputAsync += HarassInput;
            CoreEvents.OnCoreLaneclearInputAsync += LaneclearInput;
            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            if (!Getter.Me().IsAlive || !drawRange.IsOn) return;
            RenderFactory.DrawNativeCircle(SourcePosition(Getter.Me()), Range, color, 2);
        }

        private List<GameObjectBase> GetNearbyUnits(int range = 0, TargetModes mode = TargetModes.Hero)
        {
            if (range == 0) return new();
            List<GameObjectBase> nearby = new List<GameObjectBase>();
            if (mode == TargetModes.Hero
                || mode == TargetModes.HeroMinion)
            {
                foreach (GameObjectBase target in (Friendly)? UnitManager.AllyChampions : UnitManager.EnemyChampions)
                {
                    if (SourcePosition(Getter.Me()).Distance(target.Position) < range
                        && target.IsAlive
                        && target.IsVisible)
                        nearby.Add(target);
                }
            }
            if (mode == TargetModes.Minion
                || mode == TargetModes.HeroMinion)
            {
                foreach (GameObjectBase target in ((Friendly) ? UnitManager.AllyMinions : UnitManager.EnemyMinions))
                {
                    if (SourcePosition(Getter.Me()).Distance(target.Position) < range
                        && target.IsAlive
                        && target.IsVisible)
                        nearby.Add(target);
                }
            }
            return nearby;
        }

        private Task ComboInput()
        {
            if (!IsOn)
                return Task.CompletedTask;
            List<GameObjectBase> enemies = GetNearbyUnits(Range);
            if ((collisionCheck.MinCollisionObjectsCounter.Value > 0)? enemies.Count >= collisionCheck.MinCollisionObjectsCounter.Value : enemies.Count >= 1
                && SpellIsReady())
            {
                SpellCastProvider.CastSpell(SpellCastSlot);
            }
            return Task.CompletedTask;
        }
        private Task HarassInput()
        {
            if (!IsOn || !useHarass.IsOn)
                return Task.CompletedTask;
            List<GameObjectBase> enemies = GetNearbyUnits(Range);
            if ((collisionCheck.MinCollisionObjectsCounter.Value > 0) ? enemies.Count >= collisionCheck.MinCollisionObjectsCounter.Value : enemies.Count >= 1
                && SpellIsReady())
            {
                SpellCastProvider.CastSpell(SpellCastSlot);
            }
            return Task.CompletedTask;
        }

        private Task LaneclearInput()
        {
            if (!IsOn || !useLaneclear.IsOn)
                return Task.CompletedTask;
            List<GameObjectBase> enemies = GetNearbyUnits(Range, TargetModes.HeroMinion);
            if ((collisionCheck.MinCollisionObjectsCounter.Value > 0) ? enemies.Count >= collisionCheck.MinCollisionObjectsCounter.Value : enemies.Count >= 1
                && SpellIsReady())
            {
                SpellCastProvider.CastSpell(SpellCastSlot);
            }
            return Task.CompletedTask;
        }


        private Task LasthitInput()
        {
            if (!IsOn || !useLasthit.IsOn)
                return Task.CompletedTask;
            List<GameObjectBase> enemies = GetNearbyUnits(Range, TargetModes.HeroMinion);
            if ((collisionCheck.MinCollisionObjectsCounter.Value > 0) ? enemies.Count >= collisionCheck.MinCollisionObjectsCounter.Value : enemies.Count >= 1
                && SpellIsReady())
            {
                SpellCastProvider.CastSpell(SpellCastSlot);
            }
            return Task.CompletedTask;
        }

    }
}
