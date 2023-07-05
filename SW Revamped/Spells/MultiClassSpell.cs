using Oasys.Common.GameObject;
using Oasys.SDK.Tools;
using SharpDX.Win32;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Spells
{
    internal class MultiClassSpell
    {
        internal SpellBase[] Bases;
        internal Func<GameObjectBase, bool>[] Checkers;
        public MultiClassSpell(SpellBase[] spellBases, Func<GameObjectBase, bool>[] checkers) 
        {
            Bases = spellBases;
            Checkers = checkers;
            
            Oasys.SDK.Events.CoreEvents.OnCoreMainTick += CheckBases;
        }

        private Task CheckBases()
        {
            if (!(Bases.Length == Checkers.Length))
            {
                Logger.Log("MultiSpell Checkers and Bases Length is not equal. Contact the developer.");
            } else
            {
                for (int i = 0; i <= Bases.Length;)
                {
                    if (Checkers[i](Getter.Me()))
                    {
                        Bases[i].isOn = true;
                    } else
                    {
                        Bases[i].isOn = false;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
