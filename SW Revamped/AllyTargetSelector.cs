using Oasys.Common.GameObject;
using Oasys.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped
{
    internal static class AllyTargetSelector
    {
        internal static GameObjectBase GetLowestHealthTarget(Func<GameObjectBase, bool> predicate)
        {
            return UnitManager.AllyChampions.Where(predicate).OrderBy(x => x.Health).FirstOrDefault();
        }

        internal static GameObjectBase GetLowestHealthPercentTarget(Func<GameObjectBase, bool> predicate)
        {
            return UnitManager.AllyChampions.Where(predicate).OrderBy(x => x.HealthPercent).FirstOrDefault();
        }
    }
}
