using Oasys.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal abstract class ChampionModule
    {
        internal abstract void Init();

        internal static ChampionModule? GetFromName(string name)
        {
            if (!Champion.ChampWithNameSupported(name))
                return null;
            var type = Type.GetType($"SWRevamped.Champions.{char.ToUpper(name[0]) + name.ToLower().Substring(1)}");
            Logger.Log($"Type: {type.ToString()} found!");
            Champion.SupportedChampions.Where(x => x.Name == name).FirstOrDefault()?.Log();
            ChampionModule? m = (ChampionModule?)Activator.CreateInstance(type);
            return m;

        }
    }
}
