using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped
{
    internal static class Extensions
    {
        internal static List<T> deepCopy<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = new List<T>(enumerable.ToArray());
            return list;
        }

        internal static bool Equals(this Color color, Color otherColor)
        {
            return color.A == otherColor.A && color.B == otherColor.B && color.G == otherColor.G && color.A == otherColor.A;
        }

        internal static bool IsSyndraBall(this AIBaseClient obj)
        {
            return obj != null 
                && obj.Name != null 
                && obj.IsObject(Oasys.Common.Enums.GameEnums.ObjectTypeFlag.AIMinionClient) 
                && obj.IsAlly 
                && obj.Position.IsValid()
                && obj.Position.Y != float.NaN
                && obj.Name.Contains("Seed", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsSyndraBallAlt(this AIBaseClient obj) 
        {
            return obj is not null && obj.Distance <= 2000 && obj.IsAlive && obj.Position.IsValid() &&
                   obj.Name.Contains("Syndra_", StringComparison.OrdinalIgnoreCase) &&
                   obj.Name.Contains("_Q_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
