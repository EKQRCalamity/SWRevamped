using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SWRevamped.Miscellaneous
{
    internal enum SLevel
    {
        Normal,
        Good,
        Pro,
        External,
        Internal,
    }

    internal class Waypoint
    {
        internal float Time;
        internal List<Vector3> Points;

        public Waypoint(List<Vector3> points)
        {
            Points = points;
            Time = GameEngine.GameTime;
        }
    }

    internal class Player
    {
        internal int totalWaypointChanges = 0;
        internal int waypointChanges = 0;
        internal float waypointsPerSec => Waypoints.Count;
        internal float totalWaypointsPerSec => Waypointsraw.Count / GameEngine.GameTime;
        internal SLevel level;
        internal SLevel totalLevel;
        internal GameObjectBase Target { get; set; }

        internal List<Waypoint> Waypointsraw = new List<Waypoint>();
        internal List<Waypoint> Waypoints => Waypointsraw.Where(x => (GameEngine.GameTime - x.Time) < 1).ToList();
        List<Vector3> lastWaypoints { get; set; }
        internal List<Vector3> currentWaypoints => Target.AIManager.GetNavPoints();

        internal bool WaypointsEqual => lastWaypoints.ListEquals(currentWaypoints.deepCopy());

        internal void UpdateWaypoint()
        {
            lastWaypoints = currentWaypoints.deepCopy();
        }

        public Player(GameObjectBase target)
        {
            Target = target;
            lastWaypoints = currentWaypoints.deepCopy();
        }
    }

    internal static class ScripterManager
    {
        internal static int tick = 0;
        internal static List<Player> enemies = new List<Player>();
        internal static List<Player>  allies = new List<Player>(); 

        internal static void Init()
        {
            foreach (GameObjectBase Base in UnitManager.EnemyChampions) 
            {
                enemies.Add(new Player(Base));
            }
            foreach (GameObjectBase Base in UnitManager.AllyChampions)
            {
                allies.Add(new Player(Base));
            }
            CoreEvents.OnCoreMainTick += Tick;
        }

        internal static SLevel GetLevelFromChange(float change)
        {
            // Need fix
            if (change < 4)
            {
                return SLevel.Normal;
            } else if (change < 6)
            {
                return SLevel.Good;
            } else if (change < 9)
            {
                return SLevel.Pro;
            } else if (change < 14)
            {
                return SLevel.External;
            } else
            {
                return SLevel.Internal;
            }
        }

        private static Task Tick()
        {
            Logger.Log(GameEngine.GameTime);
            tick++;
            for (int i = 0; i < enemies.Count; i++)
            {
                Player player = enemies[i];
                if (!player.WaypointsEqual
                    && player.currentWaypoints.Count > 1)
                {
                    player.Waypointsraw.Add(new(player.Target.AIManager.GetNavPoints().deepCopy()));
                }
                player.totalLevel = GetLevelFromChange(player.totalWaypointsPerSec);
                player.level = GetLevelFromChange(player.waypointsPerSec);
                player.UpdateWaypoint();
            }
            for (int i = 0; i < allies.Count; i++)
            {
                Player player = allies[i];
                if (!player.WaypointsEqual
                    && player.currentWaypoints.Count > 1)
                {
                    player.Waypointsraw.Add(new(player.Target.AIManager.GetNavPoints().deepCopy()));
                }
                player.totalLevel = GetLevelFromChange(player.totalWaypointsPerSec);
                player.level = GetLevelFromChange(player.waypointsPerSec);
                player.UpdateWaypoint();
            }
            return Task.CompletedTask;
        }
    }
    internal class ScripterDetector : UtilityModule
    {
        public override string Author => "EKQRKotlin";
        public override string Name => "ScripterDetector";
        public override string Description => "Shows if someone is likely to script.";
        public override string Version => "0.9.0.0";

        internal Group MainGroup = new Group("ScripterDetector");
        internal Switch IsOn = new Switch("Enabled", true);
        internal Switch Allies = new Switch("Allies", true);

        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(MainGroup);
            MainGroup.AddItem(IsOn);
            MainGroup.AddItem(Allies);
            ScripterManager.Init();
            CoreEvents.OnCoreRender += Render;
        }

        private void Render()
        {
            if (!IsOn.IsOn) return;
            for (int i = 0; i < ScripterManager.enemies.Count; i++)
            {
                Player player = ScripterManager.enemies[i];
                if (player.Target.IsAlive && player.Target.IsVisible && player.Target.Position.IsOnScreen())
                {
                    Vector2 pos = player.Target.Position.ToW2S();
                    pos.Y -= 20;
                    String outPut = (player.level > SLevel.Good) ? (player.level > SLevel.Pro) ? (player.level > SLevel.External) ? "Scripter" : "Likely Scripter" : "Maybe Scripter" : "";
                    if (outPut != "")
                        RenderFactory.DrawText($"{outPut}", pos, Color.Black, true);
                }
            }
            if (!Allies.IsOn) return;
            for (int i = 0; i < ScripterManager.allies.Count; i++)
            {
                Player player = ScripterManager.allies[i];
                if (player.Target.IsAlive && player.Target.IsVisible && player.Target.Position.IsOnScreen())
                {
                    Vector2 pos = player.Target.Position.ToW2S();
                    pos.Y -= 20;
                    String outPut = (player.level > SLevel.Good) ? (player.level > SLevel.Pro) ? (player.level > SLevel.External) ? "Scripter" : "Likely Scripter" : "Maybe Scripter" : "";
                    if (outPut != "")
                        RenderFactory.DrawText($"{outPut}", pos, Color.Black, true);
                }
            }
        }
    }
}
