using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal class Champion
    {
        internal string Name { get; }

        internal string VersionNumber { get; }

        internal bool Stable { get; }

        internal Champion(string name, string version, bool stable) 
        {
            Name = name;
            VersionNumber = version;
            Stable = stable;
        }

        internal static List<Champion> SupportedChampions = ConstructSupported();

        internal void Log()
        {
            Logger.Log($"{Name} version {VersionNumber} loaded!");
            Logger.Log($"{{{((Stable == true)? "This is a stable release!" : "Not a stable release! Beware.")}}}");
        }

        internal static List<Champion> ConstructSupported()
        {
            List<Champion> supportedChampions = new List<Champion>()
            {
                new Champion("Kalista", "1.0.0.0", true),
                new Champion("KogMaw", "1.0.0.0", false),
                new Champion("Cassiopeia", "1.0.0.0", false),
                new Champion("Brand", "1.0.0.0", false),
                new Champion("Ezreal", "1.0.0.0", false),
                new Champion("Annie", "1.0.0.0", false),
                new Champion("Blitzcrank", "1.0.0.0", false),
                new Champion("Lux", "1.0.0.0", false)
            };
            return supportedChampions;            
        }

        internal static bool ChampWithNameSupported(string name)
        {
            List<Champion> supportedChampions = SupportedChampions;
            return supportedChampions.Where(x => x.Name == name).Any();
        }
    }
}
