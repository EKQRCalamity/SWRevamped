using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
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
                for (int i = 0; i < path.Count - 1; i++)
                {
                    if (i + 1 == path.Count) break;
                    
                    RenderFactory.DrawLine(LeagueNativeRendererManager.WorldToScreenSpell(path[i]).X, LeagueNativeRendererManager.WorldToScreenSpell(path[i]).Y, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1]).X, LeagueNativeRendererManager.WorldToScreenSpell(path[i + 1]).Y, 2, (target.OnMyTeam) ? (target.IsMe) ? new Color(23, 255, 58) : new Color(52, 128, 199) : new Color(252, 23, 65));
                    

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
