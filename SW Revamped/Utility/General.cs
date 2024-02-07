using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.ObjectClass;
using Oasys.SDK;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    internal static class General
    {
        internal static bool InNexusRange(GameObjectBase target)
        {
            return InNexusRange(target.Position);
        }
        internal static bool InNexusRange(Vector3 pos)
        {
            Vector3 BlueNexus = new Vector3(405, 95, 425);
            Vector3 RedNexus = new Vector3(14300, 90, 14400);
            return pos.Distance(BlueNexus) <= 1000 || pos.Distance(RedNexus) <= 1000;
        }

        internal static bool InTowerRange(GameObjectBase target)
        {
            if (target.IsAlly)
            {
                return InTowerRange(target.Position);
            } else
            {
                return InAllyTowerRange(target.Position);
            }
        }

        internal static bool InTowerRange(Vector3 pos)
        {
            return UnitManager.EnemyTowers.Any(x => x.IsAlive && x.IsTargetable && x.Health > 1 && x.DistanceTo(pos) < 750);
        }

        internal static bool InAllyTowerRange(Vector3 pos)
        {
            return UnitManager.AllyTowers.Any(x => x.IsAlive && x.IsTargetable && x.Health > 1 && x.DistanceTo(pos) < 750);
        }

        internal static bool InTowerOrNexusRange(GameObjectBase target)
        {
            return InTowerRange(target) && InNexusRange(target);
        }

        internal static bool IsTowerTarget(GameObjectBase target)
        {
            if (!target.IsAlive)
                return false;
            if (target.IsAlly)
            {
                Turret? targetTower = UnitManager.EnemyTowers.Where(x => x.IsAlive && x.IsTargetable && x.Health > 1 && x.DistanceTo(target.Position) < 750).ToList().FirstOrDefault();
                if (targetTower == null)
                    return false;
                return targetTower.GetCurrentCastingSpell().Targets.Contains(target);
            } else
            {
                Turret? targetTower = UnitManager.AllyTowers.Where(x => x.IsAlive && x.IsTargetable && x.Health > 1 && x.DistanceTo(target.Position) < 750).ToList().FirstOrDefault();
                if (targetTower == null)
                    return false;
                return targetTower.GetCurrentCastingSpell().Targets.Contains(target);
            }
        }
    }
}
