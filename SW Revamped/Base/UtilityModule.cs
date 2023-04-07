using SWRevamped.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal abstract class UtilityModule
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        internal WardManager WardManager { get; } = new WardManager();

        internal abstract void Init();

    }
}
