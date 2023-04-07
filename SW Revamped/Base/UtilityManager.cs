using Oasys.Common.Menu;
using SWRevamped.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal static class UtilityManager
    {
        internal static Tab MainTab = new Tab("SW - Utility");

        internal static List<UtilityModule> utilityModules = new List<UtilityModule>()
        {
            new WardHelper()
        };
        internal static void InitAll()
        {
            MenuManagerProvider.AddTab(MainTab);
            foreach (UtilityModule module in utilityModules)
            {
                module.Init();
            }
        }
    }
}
