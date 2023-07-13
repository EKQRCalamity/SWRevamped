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
        internal static EffectCalc Empty => new EmptyEffect();
        internal abstract float GetValue(GameObjectBase target);
        internal bool CanKill(GameObjectBase target)
        {
            return (target.Health - GetValue(target)) < 0;
        }
    }

    internal sealed class EmptyEffect : EffectCalc
    {
        internal override float GetValue(GameObjectBase target)
        {
            return 0F;
        }
    }
}
