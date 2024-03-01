using Oasys.Common.Menu;
using Oasys.SDK.Tools;
using SWRevamped.Miscellaneous;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal static class UtilityManager
    {
        internal static Tab MainTab = new Tab("SW - Utility");
        private static bool LoadDebugger = true;
        internal static Group DebuggerGroup = new Group("Debuggers");

        internal static List<UtilityModule> utilityModules = new List<UtilityModule>()
        {
            new Activator(),
            new ItemManager(),
            new CSHelper(),
            new MoveViewer(),
            new WardHelper(),
            new WardPing(),
            new CollectorDrawings(),
            new ScripterDetector(),
        };
        internal static List<UtilityModule> debuggerModules = new List<UtilityModule>()
        {
            new BuffDebugger(),
            new PositionDebugger(),
            new ObjectDebugger(),
            new SpellDebugger()
        };
        internal static void InitAll()
        {
            MenuManagerProvider.AddTab(MainTab);
            Logger.Log("MainTab for Utils added!");
            foreach (UtilityModule module in utilityModules)
            {
                module.Init();
            }
            InitDebug();
        }
            
        [Conditional("DEBUG")]
        internal static void InitDebug()
        {
            MainTab.AddGroup(DebuggerGroup);
            foreach (UtilityModule module in debuggerModules)
            {
                module.Init();
            }
        }
    }
}
