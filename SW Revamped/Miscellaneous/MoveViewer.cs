using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Miscellaneous
{
    internal class PathChain
    {
        GameObjectBase obj;
        Vector2 objPos => LeagueNativeRendererManager.WorldToScreenSpell(obj.Position);
        Vector2 first = Vector2.Zero;
        Vector2 last = Vector2.Zero;
        public PathChain(Vector3 a, Vector3 b, GameObjectBase Object)
        {
            first = LeagueNativeRendererManager.WorldToScreenSpell(a);
            last = LeagueNativeRendererManager.WorldToScreenSpell(b);
            obj = Object;
        }

        public Vector2 GetFirst()
        {
            return first;
        }

        public Vector2 GetSecond()
        {
            return last;
        }

        public Vector2 GetObjPos()
        {
            return objPos;
        }

        public bool OnPath()
        {
            double tolerance = 17;
            double minX = Math.Min(first.X, last.X) - tolerance;
            double maxX = Math.Max(first.X, last.X) + tolerance;
            double minY = Math.Min(first.Y, last.Y) - tolerance;
            double maxY = Math.Max(first.Y, last.Y) + tolerance;

            if (objPos.X >= maxX || objPos.X <= minX || objPos.Y >= maxY || objPos.Y <= minY) 
            {
                return false;
            }

            if (first.X == last.X)
            {
                if (Math.Abs(first.X - last.X) >= tolerance)
                {
                    return false;
                }
                return true;
            }

            if (first.Y == last.Y)
            {
                if (Math.Abs(first.Y - last.Y) >= tolerance)
                {
                    return false;
                }
                return true;
            }

            double dist = Math.Abs(((last.X - first.X) * (first.Y - objPos.Y)) - ((first.X - objPos.X) * (last.Y - first.Y))) / Math.Sqrt((last.X - first.X) * (last.X - first.X) + (last.Y - first.Y) * (last.Y - first.Y));

            if (dist >= tolerance) { return false; } else { return true; }
        }
    }
    internal class PathChainer
    {
        public List<PathChain> Path = new List<PathChain> ();

        public PathChainer(List<Vector3> path, GameObjectBase target)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Path.Add(new PathChain(path[i], path[i + 1], target));
            }
        }
    }
    internal class MoveViewer : UtilityModule
    {
        public override string Author => "EKQRKotlin";
        public override string Name => "MoveViewer";
        public override string Description => "Shows the Movement/Click Position of champions.";
        public override string Version => "1.0.0.0";

        internal Group MoveParentGroup = new Group("Move (Drawings)");

        
        internal Group MovePositionGroup = new Group("Move Position");
        internal Switch EnemiesMPSwitch = new Switch("Enemies", true);
        internal Switch AllyMPSwitch = new Switch("Allies", true);
        internal Switch SelfMPSwitch = new Switch("Self", true);

        internal Group MovementDirectionGroup = new Group("Move Path");
        internal Switch EnemiesMDSwitch = new Switch("Enemies", true);
        internal Switch AllyMDSwitch = new Switch("Allies", true);
        internal Switch SelfMDSwitch = new Switch("Self", true);

        internal PathChainer? chain;

        internal override void Init()
        {
            UtilityManager.MainTab.AddGroup(MoveParentGroup);

            
            MoveParentGroup.AddItem(MovePositionGroup);
            MovePositionGroup.AddItem(EnemiesMPSwitch);
            MovePositionGroup.AddItem(AllyMPSwitch);
            MovePositionGroup.AddItem(SelfMPSwitch);

            MoveParentGroup.AddItem(MovementDirectionGroup);
            MovementDirectionGroup.AddItem(EnemiesMDSwitch);
            MovementDirectionGroup.AddItem(AllyMDSwitch);
            MovementDirectionGroup.AddItem(SelfMDSwitch);

            CoreEvents.OnCoreRender += DrawMovePosition;
            CoreEvents.OnCoreRender += DrawPath;
        }

        internal void DrawPaths(List<GameObjectBase> targets)
        {
            foreach (GameObjectBase target in targets)
            {
                List<Vector3> path = target.AIManager.GetNavPoints();
                bool wOnPath = false;
                if (path.Count > 1) 
                {
                    chain = new PathChainer(path, target);
                    foreach (PathChain chainPiece in chain.Path)
                    {
                        if (!wOnPath && !chainPiece.OnPath())
                            continue;
                        if (chainPiece.OnPath())
                        {
                            wOnPath = true;
                            RenderFactory.DrawLine(chainPiece.GetObjPos().X, chainPiece.GetObjPos().Y, chainPiece.GetSecond().X, chainPiece.GetSecond().Y, 2, (target.OnMyTeam) ? (target.IsMe) ? new Color(23, 255, 58) : new Color(52, 128, 199) : new Color(252, 23, 65));
                        } else
                        {
                            RenderFactory.DrawLine(chainPiece.GetFirst().X, chainPiece.GetFirst().Y, chainPiece.GetSecond().X, chainPiece.GetSecond().Y, 2, (target.OnMyTeam) ? (target.IsMe) ? new Color(23, 255, 58) : new Color(52, 128, 199) : new Color(252, 23, 65));
                        }
                    }
                }
            }
        }

        private void DrawPath()
        {
            if (EnemiesMDSwitch.IsOn)
            {
                DrawPaths(UnitManager.EnemyChampions.ToList<GameObjectBase>());
            }
            if (AllyMDSwitch.IsOn)
            {
                DrawPaths(UnitManager.AllyChampions.Where(x => !x.IsMe).ToList<GameObjectBase>());
            }
            if (SelfMDSwitch.IsOn)
            {
                DrawPaths(UnitManager.AllyChampions.Where(x => x.IsMe).ToList<GameObjectBase>());
            }
        }

        internal void DrawMovePositions(List<GameObjectBase> targets)
        {
            foreach (GameObjectBase target in targets)
            {
                if (target.Position.IsOnScreen() && target.AIManager.NavEndPosition.IsOnScreen() && target.AIManager.IsMoving)
                {
                    RenderFactory.DrawNativeCircle(target.AIManager.NavEndPosition, 25, (target.OnMyTeam)? (target.IsMe)? new Color(23, 255, 58) : new Color(52, 128, 199) : new Color(252, 23, 65), 2, false);
                }
            }
        }

        private void DrawMovePosition()
        {
            if (EnemiesMPSwitch.IsOn)
            {
                DrawMovePositions(UnitManager.EnemyChampions.ToList<GameObjectBase>());
            }
            if (AllyMPSwitch.IsOn)
            {
                DrawMovePositions(UnitManager.AllyChampions.Where(x => !x.IsMe).ToList<GameObjectBase>());
            }
            if (SelfMPSwitch.IsOn)
            {
                DrawMovePositions(UnitManager.AllyChampions.Where(x => x.IsMe).ToList<GameObjectBase>());
            }
        }
    }
}
