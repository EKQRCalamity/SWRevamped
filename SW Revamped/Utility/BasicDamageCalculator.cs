using Oasys.Common.GameObject;
using Oasys.Common.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    internal static class DamageAndOnHitCalculator
    {
        internal static float NashorsTooth(GameObjectBase target)
        {
            return NashorsTooth(Getter.Me(), target);
        }
        
        internal static float NashorsTooth(GameObjectBase caster, GameObjectBase target)
        {
            return DamageCalculator.CalculateActualDamage(caster, target, 0, (15 + (Getter.TotalAP * 0.2F)), 0);
        }

        internal static float WitsEnd(GameObjectBase target)
        {
            return WitsEnd(Getter.Me(), target);
        }
        internal static float WitsEnd(GameObjectBase caster, GameObjectBase target)
        {
            float damage = (Getter.Me().Level == 1) ? 15 : (Getter.Me().Level == 9) ? 25 : (Getter.Me().Level < 15) ? 25 + ((Getter.Me().Level - 9) * 10) : 75 + ((Getter.Me().Level - 14) * 1.25F);
            return DamageCalculator.CalculateActualDamage(caster, target, 0, damage, 0);
        }

    }
}
