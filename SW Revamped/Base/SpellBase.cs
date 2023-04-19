using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK.SpellCasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Base
{
    internal class CollisionCheck
    {
        internal bool Collision { get; private set; }
        internal int MaxCollisionObjects { get; set; }
        internal int MinCollisionObjects => (MinCollisionObjectsCounter != null) ? MinCollisionObjectsCounter.Value : 0;
        internal Counter MinCollisionObjectsCounter { get; private set; }

        internal CollisionCheck(bool collision, int maxCollisionObjects, int minCollisionObjects = 0)
        {
            Collision = collision;
            MaxCollisionObjects = maxCollisionObjects;
            MinCollisionObjectsCounter = new Counter("Min Collisions", minCollisionObjects, 0, 5);
        }
    }

    internal abstract class SpellBase
    {
        internal Tab MainTab;
        internal Group SpellGroup;

        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal bool IsOn => IsOnSwitch.IsOn;

        internal SpellSlot Slot;

        internal CastSlot SpellCastSlot;

        internal int Range;
        internal int Width;
        internal int Speed;
        internal int CastRange;
        internal float CastTime;

        internal EffectCalc effectCalc;

        internal Func<GameObjectBase, bool> SelfCheck;
        internal Func<GameObjectBase, bool> TargetCheck;

        internal Oasys.SDK.Prediction.MenuSelected.HitChance GetHitchanceFromName(string name)
        {
            return name.ToLower() switch
            {
                "immobile" => Oasys.SDK.Prediction.MenuSelected.HitChance.Immobile,
                "veryhigh" => Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh,
                "high" => Oasys.SDK.Prediction.MenuSelected.HitChance.High,
                "medium" => Oasys.SDK.Prediction.MenuSelected.HitChance.Medium,
                "low" => Oasys.SDK.Prediction.MenuSelected.HitChance.Low,
                "dashing" => Oasys.SDK.Prediction.MenuSelected.HitChance.Dashing,
                "outofrange" => Oasys.SDK.Prediction.MenuSelected.HitChance.OutOfRange,
                "unknown" => Oasys.SDK.Prediction.MenuSelected.HitChance.Unknown,
                _ => Oasys.SDK.Prediction.MenuSelected.HitChance.Impossible
            };
        }

        internal string GetNameFromHitchance(Oasys.SDK.Prediction.MenuSelected.HitChance h)
        {
            return h switch
            {
                Oasys.SDK.Prediction.MenuSelected.HitChance.Immobile => "Immobile",
                Oasys.SDK.Prediction.MenuSelected.HitChance.VeryHigh => "VeryHigh",
                Oasys.SDK.Prediction.MenuSelected.HitChance.High => "High",
                Oasys.SDK.Prediction.MenuSelected.HitChance.Medium => "Medium",
                Oasys.SDK.Prediction.MenuSelected.HitChance.Low => "Low",
                Oasys.SDK.Prediction.MenuSelected.HitChance.Dashing => "Dashing",
                Oasys.SDK.Prediction.MenuSelected.HitChance.OutOfRange => "OutOfRange",
                Oasys.SDK.Prediction.MenuSelected.HitChance.Unknown => "Unknown",
                _ => "Impossible"
            };
        }

        internal Keys SpellSlotToKey()
        {
            if (Slot == SpellSlot.Q)
            {
                return Keys.Q;
            }
            else if (Slot == SpellSlot.W)
            {
                return Keys.W;
            }
            else if (Slot == SpellSlot.E)
            {
                return Keys.E;
            }
            else
            {
                return Keys.R;
            }
        }

        internal string SpellSlotToString()
        {
            if (Slot == SpellSlot.Q)
            {
                return "Q";
            }
            else if (Slot == SpellSlot.W)
            {
                return "W";
            }
            else if (Slot == SpellSlot.E)
            {
                return "E";
            }
            else if (Slot == SpellSlot.R)
            {
                return "R";
            } else if (Slot == SpellSlot.Passive)
            {
                return "P";
            } else
            {
                return "U";
            }
        }

        internal bool SpellIsReady()
        {
            if (Getter.Me().GetSpellBook().GetSpellClass(Slot).IsSpellReady)
            {
                return true;
            }
            return false;
        }
    }
}
