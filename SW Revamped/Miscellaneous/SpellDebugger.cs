using Oasys.Common.EventsProvider;
using Oasys.Common.Extensions;
using Oasys.Common.GameObject.Clients;
using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SWRevamped.Miscellaneous
{
    internal class SpellDebugger : UtilityModule
    {

        internal Switch IsOn = new Switch("Spell Debug", false);

        public override string Name => "Spell Debugger";

        public override string Version => "1.0.0.0";

        public override string Description => "Debugs Spells of hovered object.";

        public override string Author => "EKQR Kotlin";

        internal override void Init()
        {
            UtilityManager.DebuggerGroup.AddItem(IsOn);
            CoreEvents.OnCoreRender += OnDraw;
        }
        internal static Vector2 MousePosOnScreen => new Vector2(Cursor.Position.X, Cursor.Position.Y);

        private void OnDraw()
        {
            if (IsOn.IsOn)
            {
                AIBaseClient gameobj = GameEngine.HoveredGameObjectUnderMouse;
                if (gameobj != null)
                {
                    SpellBook spellBook = gameobj.GetSpellBook();
                    if (spellBook != null)
                    {
                        Logger.Log($"Q: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.MissileName}");
                        Logger.Log($"W: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.MissileName}");
                        Logger.Log($"E: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.MissileName}");
                        Logger.Log($"R: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.MissileName}");
                        Vector2 pos = MousePosOnScreen;
                        RenderFactory.DrawText($"Q: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.Q).SpellData.MissileName}", pos, Color.Blue);
                        pos.Y += 12;
                        RenderFactory.DrawText($"W: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.W).SpellData.MissileName}", pos, Color.Blue);
                        pos.Y += 12;
                        RenderFactory.DrawText($"E: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.E).SpellData.MissileName}", pos, Color.Blue);
                        pos.Y += 12;
                        RenderFactory.DrawText($"R: Spell Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.SpellName} | Missile Name: {spellBook.GetSpellClass(Oasys.Common.Enums.GameEnums.SpellSlot.R).SpellData.MissileName}", pos, Color.Blue);
                    }
                }
            }
        }
    }
}
