using Oasys.Common.Menu;
using Oasys.SDK.Tools;
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
            new CSHelper(),
            new WardHelper(),
            new PositionDebugger()
        };
        internal static void InitAll()
        {
            MenuManagerProvider.AddTab(MainTab);
            Logger.Log("MainTab for Utils added!");
            foreach (UtilityModule module in utilityModules)
            {
                module.Init();
            }
        }
    }
}
