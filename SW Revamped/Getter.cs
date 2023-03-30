using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients.ExtendedInstances;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.Menu;
using Oasys.SDK;
using Oasys.SDK.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped
{
    internal static class Getter
    {

        internal static Tab MainTab => MenuManager.GetTab($"SW - {Me().ModelName}");

        internal static GameObjectBase Me() => UnitManager.MyChampion;

        internal static int Level => Me().Level;

        internal static bool IsDead(GameObjectBase obj) => !obj.IsAlive;

        internal static SpellBook GetSpellBook(GameObjectBase obj = null) => (obj == null) ? ((obj = Me()) != null) ? obj.GetSpellBook() : Me().GetSpellBook() : obj.GetSpellBook();

        internal static SpellClass QSpell => GetSpellBook(Me()).GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q);

        internal static int QLevel => QSpell.Level;

        internal static bool QReady => QSpell.IsSpellReady && !QSpell.IsToggled && !QSpell.IsActivated;

        internal static bool QLooseReady => QSpell.IsSpellReady;

        internal static SpellClass WSpell => GetSpellBook(Me()).GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W);

        internal static int WLevel => WSpell.Level;

        internal static bool WReady => WSpell.IsSpellReady && !WSpell.IsToggled && !WSpell.IsActivated;

        internal static bool WLooseReady => WSpell.IsSpellReady;

        internal static SpellClass ESpell => GetSpellBook(Me()).GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E);

        internal static int ELevel => ESpell.Level;

        internal static bool EReady => ESpell.IsSpellReady && !ESpell.IsToggled && !ESpell.IsActivated;

        internal static bool ELooseReady => ESpell.IsSpellReady;

        internal static SpellClass RSpell => GetSpellBook(Me()).GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R);

        internal static int RLevel => RSpell.Level;

        internal static bool RReady => RSpell.IsSpellReady && !RSpell.IsToggled && !RSpell.IsActivated;

        internal static bool RLooseReady => RSpell.IsSpellReady;

        internal static UnitStats Stats => Me().UnitStats;

        internal static float TotalAP => Stats.TotalAbilityPower;

        internal static float BaseAP => Stats.BaseAbilityDamage;

        internal static float TotalAD => Stats.TotalAttackDamage;

        internal static float BaseAD => Stats.BaseAttackDamage;

        internal static float BonusAD => Stats.BonusAttackDamage;

        internal static float Armor => Stats.Armor;

        internal static float MagicResist => Stats.MagicResist;

        internal static float MoveSpeed => Stats.MoveSpeed;

        internal static float AARange => Stats.AttackRange;
    }
}
