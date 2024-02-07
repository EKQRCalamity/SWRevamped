
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Base
{
    internal abstract class PermaShowItem
    {
        internal int index = -1;
        internal String name = String.Empty;
        internal PermaShowItem? Parent = null;
        internal List<PermaShowItem> Children = new List<PermaShowItem>();
        internal abstract TabItem MenuItem { get; }


        internal static bool CanHaveChildren()
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            // TODO: Add allowed types to switch 
            return currentType switch
            {
                _ => false
            };
        }


    }

    /*internal override class PermaShowString : PermaShowItem
    {
        internal override TabItem MenuItem => throw new NotImplementedException();
        internal String Value => throw new NotImplementedException();
    }*/

    internal static class PermaShowManager
    {
        static Dictionary<int, PermaShowItem> NameItemPair = new Dictionary<int, PermaShowItem>(); 
    }

    internal class PermaShow
    {
    }
}
