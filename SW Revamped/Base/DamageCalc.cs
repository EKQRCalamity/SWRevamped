using Oasys.Common.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Base
{
    internal abstract class EffectCalc
    {
        internal abstract float GetValue(GameObjectBase target);
    }
}
