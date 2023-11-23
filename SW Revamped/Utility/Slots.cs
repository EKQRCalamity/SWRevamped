using Oasys.Common.Enums.GameEnums;
using Oasys.SDK.SpellCasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    internal static class Slots
    {
        internal static SpellSlot CastSlotToSpellSlot(CastSlot slot)
        {
            return slot switch
            {
                CastSlot.Q => SpellSlot.Q,
                CastSlot.W => SpellSlot.W,
                CastSlot.E => SpellSlot.E,
                CastSlot.R => SpellSlot.R,
                CastSlot.Summoner1 => SpellSlot.Summoner1,
                CastSlot.Summoner2 => SpellSlot.Summoner2,
                CastSlot.Item1 => SpellSlot.Item1,
                CastSlot.Item2 => SpellSlot.Item2,
                CastSlot.Item3 => SpellSlot.Item3,
                CastSlot.Item4 => SpellSlot.Trinket,
                CastSlot.Item5 => SpellSlot.Item4,
                CastSlot.Item6 => SpellSlot.Item5,
                CastSlot.Item7 => SpellSlot.Item6,
                _ => SpellSlot.NullSpell,
            };
        }

        internal static CastSlot SpellSlotToCastSlot(SpellSlot slot)
        {
            return slot switch
            {
                SpellSlot.Q => CastSlot.Q,
                SpellSlot.W => CastSlot.W,
                SpellSlot.E => CastSlot.E,
                SpellSlot.R => CastSlot.R,
                SpellSlot.Summoner1 => CastSlot.Summoner1,
                SpellSlot.Summoner2 => CastSlot.Summoner2,
                SpellSlot.Item1 => CastSlot.Item1,
                SpellSlot.Item2 => CastSlot.Item2,
                SpellSlot.Item3 => CastSlot.Item3,
                SpellSlot.Trinket => CastSlot.Item4,
                SpellSlot.Item4 => CastSlot.Item5,
                SpellSlot.Item5 => CastSlot.Item6,
                SpellSlot.Item6 => CastSlot.Item7,
                _ => 0,
            };
        }

        internal static SpellCastSlot SpellSlotToSpellCastSlot(SpellSlot slot)
        {
            return slot switch
            {
                SpellSlot.Q => SpellCastSlot.Q,
                SpellSlot.W => SpellCastSlot.W,
                SpellSlot.E => SpellCastSlot.E,
                SpellSlot.R => SpellCastSlot.R,
                SpellSlot.Summoner1 => SpellCastSlot.Summoner1,
                SpellSlot.Summoner2 => SpellCastSlot.Summoner2,
                SpellSlot.Item1 => SpellCastSlot.Item1,
                SpellSlot.Item2 => SpellCastSlot.Item2,
                SpellSlot.Item3 => SpellCastSlot.Item3,
                SpellSlot.Item4 => SpellCastSlot.Item4,
                SpellSlot.Item5 => SpellCastSlot.Item5,
                SpellSlot.Item6 => SpellCastSlot.Item6,
                _ => 0
            };
        }
    }
}
