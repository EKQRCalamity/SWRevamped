using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Tools;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SWRevamped.Miscellaneous
{
    class WardPing : UtilityModule
    {
        internal static float LastPing = 0;
        
        internal Group wardPing = new Group("Ward Ping");
        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal Switch Minimap = new Switch("Cast on Minimap", true);
        internal Switch NoFight = new Switch("Don't cast in fight", true);
        internal Counter MinDist = new Counter("Min Distance", 0, 0, 10000);
        internal Counter MaxDist = new Counter("Max Distance", 3000, 0, 100000);
        internal Counter PingDelay = new Counter("Ping delay", 100, 0, 100000);
        internal static Counter WardCounter = new Counter("Wards Pinged", 0, 0, 10000) { ValueFrequency = 0 };
        internal static Counter MoneyCounter = new Counter("Estimate Money", 0, 0, 10000) { ValueFrequency = 0 };


        public override string Name => "WardPing";
        public override string Author => "EKQR Kotlin";
        public override string Description => "Pings freshly placed enemy wards.";
        public override string Version => "1.0.0.0";

        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(wardPing);
            wardPing.AddItem(IsOnSwitch);
            wardPing.AddItem(Minimap);
            wardPing.AddItem(NoFight);
            wardPing.AddItem(MinDist);
            wardPing.AddItem(MaxDist);
            wardPing.AddItem(PingDelay);
            wardPing.AddItem(WardCounter);
            wardPing.AddItem(MoneyCounter);
            WardCounter.Value = 0;
            MoneyCounter.Value = 0;
            GameEvents.OnCreateObject += WardPinger;

            GameEvents.OnGameMatchComplete += ResetStats;
        }
        internal static bool IsFighting()
        {
            GameObjectBase target = TargetSelector.GetBestChampionTarget();
            if (target == null || !Getter.Me().IsCastingSpell) return false;
            return true;
        }

        private Task WardPinger(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.Name.Contains("SightWard", StringComparison.OrdinalIgnoreCase) || callbackObject.Name.Contains("JammerDevice", StringComparison.OrdinalIgnoreCase))
            {
                if (callbackObject.Distance > MaxDist.Value || callbackObject.Distance < MinDist.Value || !IsOnSwitch.IsOn)
                    return Task.CompletedTask;
                if (IsFighting() && NoFight.IsOn)
                    return Task.CompletedTask;
                if (callbackObject.Team != Getter.Me().Team && callbackObject.Team != Oasys.Common.Enums.GameEnums.TeamFlag.Unknown)
                {
                    if (callbackObject.IsVisible && callbackObject.IsAlive && ((GameEngine.GameTime) - (LastPing)) > PingDelay.Value / 10)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds((double)PingDelay.Value / 1000));
                        if (callbackObject.Position.IsOnScreen())
                        {
                            PingManager.PingTo(PingSlot.Vision, callbackObject.Position.ToW2S());
                            LastPing = GameEngine.GameTime;
                            WardCounter.Value += 1;
                            MoneyCounter.Value += 5;
                        }
                        else if (Minimap.IsOn)
                        {
                            PingManager.PingTo(PingSlot.Vision, callbackObject.Position.ToWorldToMap());
                            LastPing = GameEngine.GameTime;
                            WardCounter.Value += 1;
                            MoneyCounter.Value += 5;
                        }

                    }
                }
            }
            return Task.CompletedTask;
        }

        internal static Task ResetStats()
        {
            WardCounter.Value = 0;
            MoneyCounter.Value = 0;
            return Task.CompletedTask;
        }
    }
}
