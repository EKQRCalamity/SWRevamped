using Oasys.Common.Enums.GameEnums;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
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
    internal enum SpellCastMode
    {
        Spam,
        AfterAutoAttack,
        BeforeAutoAttack
    }

    internal enum CollisionModes
    {
        Hero,
        Minion,
        HeroMinion,
        None
    }

    internal enum TargetModes
    {
        Hero,
        Minion,
        HeroMinion
    }

    internal enum CollLogic
    {
        Min,
        Max
    }

    internal class PredOut
    {
        internal Prediction.MenuSelected.PredictionOutput Prediction;
        internal GameObjectBase Target;
        internal bool Failed;

        public PredOut(Prediction.MenuSelected.PredictionOutput pred, GameObjectBase target, bool failed = false)
        {
            Prediction = pred;
            Target = target;
            Failed = failed;
        }
    }

    internal class Coll
    {
        internal int Num => (CollisionCounter != null) ? CollisionCounter.Value : 0;
        internal int _initn { get; private set; }
        internal CollisionModes Mode { get; set; }
        internal CollLogic Logic { get; set; }
        internal Counter CollisionCounter { get; set; }
        public Coll(int n, CollisionModes mode, CollLogic logic)
        {
            _initn = n;
            Mode = mode;
            Logic = logic;
        }
    }

    internal class CollisionCheck
    {
        internal bool Collision { get; private set; }
        internal List<Coll> CollisionObjects { get; set; }
        internal Counter MinCollisionObjectsCounter { get; private set; }

        internal CollisionCheck(bool collision, List<Coll> collisions)
        {
            Collision = collision;
            CollisionObjects = collisions;
            Coll? minColl = collisions.Where(x => x.Logic == CollLogic.Min).FirstOrDefault();
            MinCollisionObjectsCounter = new Counter("Min Collisions", (minColl != null)? minColl._initn : 0, 0, 5);
        }
    }

    internal abstract class SpellBase
    {
        internal Tab MainTab;
        internal Group SpellGroup;

        internal Switch IsOnSwitch = new Switch("Enabled", true);
        internal bool isOn = true;
        internal bool IsOn { get => (isOn) ? IsOnSwitch.IsOn : isOn; }

        internal SpellSlot Slot;

        internal CastSlot SpellCastSlot;

        internal int Range;
        internal int Width;
        internal int Speed;
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
