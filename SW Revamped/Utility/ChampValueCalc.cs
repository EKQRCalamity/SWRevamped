using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    internal static class ChampValueCalc
    {
        internal static int GetEnemyPoints(GameObjectBase target, EffectCalc Calculator, float Range, Func<GameObjectBase, Vector3> SourcePosition)
        {
            if (target == null || (!target.IsAlive || !target.IsZombie)) return 0;
            int points = 0;
            if (Calculator.CanKill(target)) points += 50;
            points += (int)(Math.Floor(Calculator.GetValue(target) / target.Health) * 25);
            points += (int)(Math.Min(25, Math.Floor(Range / (SourcePosition(Getter.Me()).Distance(target.Position)) * 15)));
            return points;
        }

        internal static int GetEnemyPointsExtended(GameObjectBase target, EffectCalc Calculator, float Range, Func<GameObjectBase, Vector3> SourcePosition)
        {
            if (target == null || (!target.IsAlive || !target.IsZombie)) return 0;
            int points = 0;
            if (Calculator.CanKill(target)) points += 50;
            points += (int)(Math.Min(30,Math.Floor(Calculator.GetValue(target) / target.Health) * 25));
            points += (int)(Math.Min(25, Math.Floor(Range / (SourcePosition(Getter.Me()).Distance(target.Position)) * 15)));
            points += (int)(Math.Min(25, Math.Floor(target.Armor / Math.Max(Getter.Me().Armor, 80)) * 15));
            return points;
        }
    }
}
