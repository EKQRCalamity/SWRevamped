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
    }
}
