using Oasys.Common.Enums.GameEnums;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Oasys.Common.Logic.EB.Prediction;
using System.Windows.Forms;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.SDK.Tools;
using Oasys.Common.GameObject.Clients;

namespace SWRevamped.Base
{
    internal class ExecuteEffect
    {
        internal Color Color => GetColor();
        internal ModeDisplay ColorDisplay { get; set; }
        internal Color GetColor()
        {
            return ColorConverter.GetColor(ColorDisplay.SelectedModeName);
        }

        internal bool IsOn { get => _isOn; }
        private bool _isOn => (IsOnSwitch == null) ? true : IsOnSwitch.IsOn;

        internal Func<AIHeroClient, bool> IsOnSelfCheck { get; set; }
        internal Func<GameObjectBase, bool> IsOnTargetCheck { get; set; }

        internal Tab MainTab { get; set; }
        internal Group MainGroup { get; set; }
        internal Switch IsOnSwitch { get; set; }
        internal Switch LineMode { get; set; }
        internal Switch ShowOnHPBar { get; set; }

        internal EffectCalc Calculator { get; private set; }

        internal bool ColorEquals(Color color, Color otherColor)
        {

            return color.A == otherColor.A && color.B == otherColor.B && color.G == otherColor.G && color.A == otherColor.A;
        }

        internal string ColorToName(Color color)
        {
            string name = "";
            // Blue, Red, OrangeRed, White, Black, Orange, Green, LightGreen, Yellow
            switch (color)
            {
                case var value when ColorEquals(color, Color.Red):
                    name = "Red";
                    break;
                case var value when ColorEquals(color, Color.Orange):
                    name = "Orange";
                    break;
                case var value when ColorEquals(color, Color.Green):
                    name = "Green";
                    break;
                case var value when ColorEquals(color, Color.Blue):
                    name = "Blue";
                    break;
                case var value when ColorEquals(color, Color.Black):
                    name = "Black";
                    break;
                case var value when ColorEquals(color, Color.Yellow):
                    name = "Yellow";
                    break;
                case var value when ColorEquals(color, Color.LightGreen):
                    name = "LightGreen";
                    break;
                case var value when ColorEquals(color, Color.White):
                    name = "White";
                    break;
                case var value when ColorEquals(color, Color.OrangeRed):
                    name = "OrangeRed";
                    break;
                default:
                    name = "White";
                    break;
            }
            return name;
        }

        public ExecuteEffect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color)
        {
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorToName(color)}" };
            IsOnSelfCheck = x => x.IsAlive;
            IsOnTargetCheck = x => x.IsAlive;
            LineMode = new Switch("Line Mode", false);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            Calculator = calculator;
            InitMenu(mainGroup);
        }

        public ExecuteEffect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color, Func<GameObjectBase, bool> isOnSelfCheck)
        {
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorConverter.GetColors().FirstOrDefault()}" };
            IsOnSelfCheck = isOnSelfCheck;
            IsOnTargetCheck = x => x.IsAlive;
            LineMode = new Switch("Line Mode", false);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            Calculator = calculator;
            InitMenu(mainGroup);
        }

        public ExecuteEffect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color, Func<GameObjectBase, bool> isOnSelfCheck, Func<GameObjectBase, bool> isOnTargetCheck)
        {
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorConverter.GetColors().FirstOrDefault()}" };
            IsOnSelfCheck = isOnSelfCheck;
            IsOnTargetCheck = isOnTargetCheck;
            LineMode = new Switch("Line Mode", false);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            Calculator = calculator;
            InitMenu(mainGroup);
        }

        internal void InitMenu(Group group)
        {
            group.AddItem(IsOnSwitch);
            group.AddItem(LineMode);
            group.AddItem(ColorDisplay);

            CoreEvents.OnCoreRender += Drawer;
        }

        private void Drawer()
        {
            if (IsOn)
            {
                if (IsOnSelfCheck(Getter.MeHero))
                {
                    foreach (var champ in UnitManager.EnemyChampions)
                    {
                        if (!IsOnTargetCheck(champ))
                            continue;
                        DrawingsExtended.DrawExecuteBar(champ, Calculator.GetValue(champ), GetColor(), LineMode.IsOn);
                    }
                }
            }
        }

        internal float GetHealthAffect(GameObjectBase target)
        {
            return Calculator.GetValue(target);
        }

        internal void Deactivate()
        {
            IsOnSwitch.IsOn = false;
        }

        internal void Activate()
        {
            IsOnSwitch.IsOn = true;

        }
    }

    internal class Effect
    {
        internal string OrigName { get; set; }
        internal string Name
        {
            get => GetName();
        }
        internal bool ShowName => (ShowNameSwitch != null) ? ShowNameSwitch.IsOn : true;
        internal string GetName()
        {
            if (ShowName)
            {
                return OrigName;
            }
            return "";
        }

        internal bool ShouldDrawName(GameObjectBase target)
        {
            return ((GetHealthAffect(target) / target.MaxHealth) * 100 > EffectDrawer.ThresholdCounter.Value);
        }

        internal string NameToDraw(GameObjectBase target)
        {
            if (ShouldDrawName(target))
                return GetName();
            return "";
        } 

        internal Color Color => GetColor();
        internal ModeDisplay ColorDisplay { get; set; }
        internal Color GetColor()
        {
            return ColorConverter.GetColor(ColorDisplay.SelectedModeName);
        }

        internal bool IsOn { get => _isOn; }
        private bool _isOn => (IsOnSwitch == null)? true : IsOnSwitch.IsOn;

        internal Func<GameObjectBase, bool> IsOnSelfCheck { get; set; }
        internal Func<GameObjectBase, bool> IsOnTargetCheck { get; set; }

        internal uint Priority => (uint)PriorityCounter.Value;
        internal Counter PriorityCounter { get; set; }

        internal Tab MainTab { get; set; }
        internal Group MainGroup { get; set; }
        internal Switch IsOnSwitch { get; set; }
        internal Switch ShowNameSwitch { get; set; }

        internal Switch ShowOnHPBar { get; set; }

        internal EffectCalc Calculator { get; private set; }

        internal bool ColorEquals(Color color, Color otherColor)
        {

            return color.A == otherColor.A && color.B == otherColor.B && color.G == otherColor.G && color.A == otherColor.A;
        }

        internal string ColorToName(Color color)
        {
            string name = "";
            // Blue, Red, OrangeRed, White, Black, Orange, Green, LightGreen, Yellow
            switch (color)
            {
                case var value when ColorEquals(color, Color.Red):
                    name = "Red";
                    break;
                case var value when ColorEquals(color, Color.Orange):
                    name = "Orange";
                    break;
                case var value when ColorEquals(color, Color.Green):
                    name = "Green";
                    break;
                case var value when ColorEquals(color, Color.Blue):
                    name = "Blue";
                    break;
                case var value when ColorEquals(color, Color.Black):
                    name = "Black";
                    break;
                case var value when ColorEquals(color, Color.Yellow):
                    name = "Yellow";
                    break;
                case var value when ColorEquals(color, Color.LightGreen):
                    name = "LightGreen";
                    break;
                case var value when ColorEquals(color, Color.White):
                    name = "White";
                    break;
                case var value when ColorEquals(color, Color.OrangeRed):
                    name = "OrangeRed";
                    break;
                default:
                    name = "White";
                    break;
            }
            return name;
        }

        public Effect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color)
        {
            OrigName = origName;
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorToName(color)}"};
            IsOnSelfCheck = x => x.IsAlive;
            IsOnTargetCheck = x => x.IsAlive;
            PriorityCounter = new Counter("Prio", prio, 0, 10);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            ShowNameSwitch = new Switch("Show Name", ShowName);
            Calculator = calculator;
            InitMenu(mainGroup, new Group("Effect Draws"));
        }

        public Effect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color, Func<GameObjectBase, bool> isOnSelfCheck)
        {
            OrigName = origName;
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorConverter.GetColors().FirstOrDefault()}" };
            IsOnSelfCheck = isOnSelfCheck;
            IsOnTargetCheck = x => x.IsAlive;
            PriorityCounter = new Counter("Prio", prio, 0, 10);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            ShowNameSwitch = new Switch("Show Name", ShowName);
            Calculator = calculator;
            InitMenu(mainGroup, new Group("Effect Draws"));
        }

        public Effect(string origName, bool isOn, int prio, int drawrange, Tab mainTab, Group mainGroup, EffectCalc calculator, Color color, Func<GameObjectBase, bool> isOnSelfCheck, Func<GameObjectBase, bool> isOnTargetCheck)
        {
            OrigName = origName;
            ColorDisplay = new ModeDisplay() { Title = "Color", ModeNames = ColorConverter.GetColors(), SelectedModeName = $"{ColorConverter.GetColors().FirstOrDefault()}" };
            IsOnSelfCheck = isOnSelfCheck;
            IsOnTargetCheck = isOnTargetCheck;
            PriorityCounter = new Counter("Prio", prio, 0, 10);
            MainTab = mainTab;
            MainGroup = mainGroup;
            IsOnSwitch = new Switch("Drawings", _isOn);
            ShowNameSwitch = new Switch("Show Name", ShowName);
            Calculator = calculator;
            InitMenu(mainGroup, new Group("Effect Draws"));
        }

        internal void InitMenu(Group group, Group draws)
        {
            group.AddItem(draws);
            draws.AddItem(IsOnSwitch);
            draws.AddItem(ShowNameSwitch);
            draws.AddItem(PriorityCounter);
            draws.AddItem(ColorDisplay);
        }

        internal float GetHealthAffect(GameObjectBase target)
        {
            return Calculator.GetValue(target);
        }

        internal void Deactivate()
        {
            IsOnSwitch.IsOn = false;
        }

        internal void Activate()
        {
            IsOnSwitch.IsOn = true;

        }
    }
    internal static class DrawingsExtended
    {

        internal static void DrawBox(float x, float y, float w, float h, float strokeWidth, Color color)
        {
            Vector2[] vertices =
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h),
                new Vector2(x, y)
            };

            RenderFactory.D3D9BoxLine.Width = strokeWidth;
            RenderFactory.D3D9BoxLine.Begin();
            RenderFactory.D3D9BoxLine.Draw(vertices, color);
            RenderFactory.D3D9BoxLine.End();
        }

        internal static void DrawMultiEffects(GameObjectBase target, List<Effect> effectList, bool onHPBar = false, int alpha = 255)
        {
            if (target == null)
                throw new Exception("Null target given. Specify a target thats not null.");
            if (effectList.Count == 0) return;

            effectList = effectList.OrderByDescending(x => x.Priority).ToList();

            float _health = target.Health;
            List<Effect> actualEffects = new();
            List<float> EffectAffections = new();
            List<Color> EffectColors = new();
            List<string> EffectNames = new();
            for (int i = 0; i < effectList.Count; i++)
            {
                Effect effect = effectList[i];
                float prioDamage = effect.GetHealthAffect(target);
                float tempPrioDamage = prioDamage;

                _health -= prioDamage;
                float tempFulldamage = (target.Health - _health);

                if (tempFulldamage > target.Health)
                    tempPrioDamage = prioDamage - (tempFulldamage - target.Health);

                if (_health >= -(prioDamage))
                {
                    actualEffects.Add(effect);
                    EffectAffections.Add(effect.GetHealthAffect(target));
                    EffectColors.Add(ColorConverter.GetColorWithAlpha(effect.Color, alpha));
                    EffectNames.Add(effect.Name);
                }
                else
                {
                    break;
                }
            }

            if (EffectAffections.Count != EffectColors.Count || EffectAffections.Count != EffectNames.Count)
                return;

            float fulldamage = 0;
            foreach (Effect effect in actualEffects)
            {
                fulldamage += effect.GetHealthAffect(target);
            }

            for (int i = 0; i <= EffectAffections.Count - 1; i++)
            {
                int index = EffectAffections.Count - (i + 1);
                DrawDamage(target, fulldamage, EffectColors[index], EffectNames[index], onHPBar, actualEffects[index].ShouldDrawName(target), false);
                fulldamage -= EffectAffections[index];
            }
        }

        internal static void DrawDamage(GameObjectBase target, float damage, Color color, string name = "", bool onBar = false, bool drawName = true, bool line = false)
        {
            if (target is null || !target.W2S.IsValid() || !target.IsVisible || damage <= 1)
            {
                return;
            }
            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }

            var healthBarOffset = resolution.Width switch
            {
                >= 2560 => isHero ? new Vector2(-14, -22) :
                           isBaron ? new Vector2(-60, -28) :
                           isDragon ? new Vector2(-32, -70) :
                           isHerald ? new Vector2(-45, -22) :
                           isCrab ? new Vector2(-45, -22) :
                           isJungleBuff ? new Vector2(-45, -22) :
                           isJungle ? new Vector2(-14, -4) :
                           new Vector2(3, -4),
                >= 1920 => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
                _ => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
            };

            var barWidth = resolution.Width switch
            {
                >= 2560 => isHero ? 125 :
                           isBaron ? 200 :
                           isDragon ? 170 :
                           isHerald ? 170 :
                           isCrab ? 170 :
                           isJungleBuff ? 170 :
                           isJungle ? 110 :
                           75,
                >= 1920 => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
                _ => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
            };

            var barHeight = resolution.Height switch
            {
                >= 1440 => isHero ? 13 :
                           isBaron ? 13 :
                           isDragon ? 13 :
                           isHerald ? 13 :
                           isCrab ? 13 :
                           isJungleBuff ? 13 :
                           isJungle ? 4 :
                           4,
                >= 1080 => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
                _ => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
            };
            var pos = target.HealthBarScreenPosition;
            pos += healthBarOffset;
            if (!onBar)
                pos.Y -= 16;
            float thickness = barHeight;
            if (line)
                thickness = 1;

            var end = pos;
            end.X += barWidth * Math.Max(float.Epsilon, target.Health / target.MaxHealth);

            var start = pos;
            var dmgPercent = Math.Max(float.Epsilon, target.Health - damage) / target.MaxHealth;
            start.X += barWidth * dmgPercent;

            RenderFactory.DrawLine(start.X, start.Y, end.X, end.Y, thickness, color);
            if (drawName)
                RenderFactory.DrawText(name, new Vector2(start.X + 3, start.Y - 22), Color.Black, false);

        }

        internal static void DrawExecuteBar(GameObjectBase target, float damage, Color color, bool line = false)
        {
            if (target is null || !target.W2S.IsValid() || !target.IsVisible || damage <= 1)
            {
                return;
            }
            var resolution = Screen.PrimaryScreen.Bounds;
            var isHero = target.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIHeroClient);
            var isJungle = false;
            var isJungleBuff = false;
            var isCrab = false;
            var isDragon = false;
            var isBaron = false;
            var isHerald = false;
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Krug", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Gromp", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Murkwolf", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("Super", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Equals("SRU_Razorbeak", StringComparison.OrdinalIgnoreCase))
            {
                isJungle = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Red", StringComparison.OrdinalIgnoreCase) ||
                target.UnitComponentInfo.SkinName.Contains("SRU_Blue", StringComparison.OrdinalIgnoreCase))
            {
                isJungleBuff = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Crab", StringComparison.OrdinalIgnoreCase))
            {
                isCrab = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Baron", StringComparison.OrdinalIgnoreCase))
            {
                isBaron = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_Dragon", StringComparison.OrdinalIgnoreCase))
            {
                isDragon = true;
            }
            if (target.UnitComponentInfo.SkinName.Contains("SRU_RiftHerald", StringComparison.OrdinalIgnoreCase))
            {
                isHerald = true;
            }

            var healthBarOffset = resolution.Width switch
            {
                >= 2560 => isHero ? new Vector2(-14, -22) :
                           isBaron ? new Vector2(-60, -28) :
                           isDragon ? new Vector2(-32, -70) :
                           isHerald ? new Vector2(-45, -22) :
                           isCrab ? new Vector2(-45, -22) :
                           isJungleBuff ? new Vector2(-45, -22) :
                           isJungle ? new Vector2(-14, -4) :
                           new Vector2(3, -4),
                >= 1920 => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
                _ => isHero ? new Vector2(-6, -18) :
                           isBaron ? new Vector2(-50, -22) :
                           isDragon ? new Vector2(-32, -55) :
                           isHerald ? new Vector2(-35, -18) :
                           isCrab ? new Vector2(-35, -18) :
                           isJungleBuff ? new Vector2(-35, -18) :
                           isJungle ? new Vector2(-6, -4) :
                           new Vector2(8, -4),
            };

            var barWidth = resolution.Width switch
            {
                >= 2560 => isHero ? 125 :
                           isBaron ? 200 :
                           isDragon ? 170 :
                           isHerald ? 170 :
                           isCrab ? 170 :
                           isJungleBuff ? 170 :
                           isJungle ? 110 :
                           75,
                >= 1920 => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
                _ => isHero ? 105 :
                           isBaron ? 175 :
                           isDragon ? 150 :
                           isHerald ? 150 :
                           isCrab ? 150 :
                           isJungleBuff ? 145 :
                           isJungle ? 95 :
                           60,
            };

            var barHeight = resolution.Height switch
            {
                >= 1440 => isHero ? 13 :
                           isBaron ? 13 :
                           isDragon ? 13 :
                           isHerald ? 13 :
                           isCrab ? 13 :
                           isJungleBuff ? 13 :
                           isJungle ? 4 :
                           4,
                >= 1080 => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
                _ => isHero ? 10 :
                           isBaron ? 10 :
                           isDragon ? 10 :
                           isHerald ? 10 :
                           isCrab ? 10 :
                           isJungleBuff ? 10 :
                           isJungle ? 3 :
                           3,
            };

            var pos = target.HealthBarScreenPosition;
            pos += healthBarOffset;
            pos.Y -= 5;

            if (!line)
                DrawBox(pos.X, pos.Y, barWidth * Math.Max(float.Epsilon, damage / target.MaxHealth), barHeight, 2, color);
            else
            {
                pos.X += barWidth * (damage / target.MaxHealth);
                RenderFactory.DrawLine(pos.X, pos.Y, pos.X, pos.Y + barHeight, 2, color);
            }
        }
    }

    internal static class EffectDrawer
    {
        private static List<Effect> friendlyEffects = new List<Effect>();
        private static List<Effect> otherEffects = new List<Effect>();

        private static Switch showOnHP = new Switch("Show on HP Bar", false);

        private static bool showOnHPBar => showOnHP.IsOn;

        internal static Counter ThresholdCounter = new Counter("Draw Threshold", 5, 0, 100);
        internal static Counter TransparencyCounter = new Counter("Transparency", 155, 0, 255);

        internal static bool Initialized = false;

        internal static void Add(Effect effect, bool friendly)
        {
            if (friendly)
            {
                friendlyEffects.Add(effect);
            } else
            {
                otherEffects.Add(effect);
            }
        }

        internal static void AddDamage(Effect effect)
        {
            Add(effect, false);
        }

        internal static void AddBuff(Effect effect)
        {
            Add(effect, true);
        }

        // Shouldnt be called more often than once but still checking
        internal static void Init()
        {
            Group group = new Group("SW - Drawings");
            group.AddItem(showOnHP);
            group.AddItem(ThresholdCounter);
            group.AddItem(TransparencyCounter);
            MenuManagerProvider.GetTab("Drawings").AddGroup(group);
            if (Initialized) return;
            CoreEvents.OnCoreRender += Render;
            Initialized = true;
        }

        private static void Render()
        {
            List<Hero> enemyTargets = UnitManager.EnemyChampions.Where(x => x != null && x.IsAlive && x.IsVisible && x.Position.IsOnScreen()).ToList();
            List<Hero> allyTargets = UnitManager.AllyChampions.Where(x => x != null && x.IsAlive && x.Position.IsOnScreen()).ToList();
            List<Effect> damages = otherEffects.Where(x => x.IsOn).ToList();
            List<Effect> buffs = friendlyEffects.Where(x => x.IsOn).ToList();
            foreach (Hero target in enemyTargets)
            {
                DrawingsExtended.DrawMultiEffects(target, damages, showOnHPBar, TransparencyCounter.Value);
            }
            foreach (Hero target in allyTargets)
            {
                DrawingsExtended.DrawMultiEffects(target, buffs, showOnHPBar, TransparencyCounter.Value);
            }
        }
    }
}
