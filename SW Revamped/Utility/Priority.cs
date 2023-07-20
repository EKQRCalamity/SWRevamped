using Oasys.Common.GameObject;
using Oasys.Common.Menu;
using Oasys.Common.Menu.ItemComponents;
using Oasys.SDK;
using System;
using System.Collections.Generic;

namespace SWRevamped.Utility
{
    internal sealed class Priorities
    {
        internal int GetValueFromKey(String key)
        {
            return PriorityValues.ContainsKey(key) ? PriorityValues[key] : -1; 
        }

        internal Counter? GetValueCounterFromKey(String key)
        {
            return PriorityValues.ContainsKey(key) ? PriorityCounters[key] : null;
        }

        internal Dictionary<String, int> PriorityValues
        {
            get
            {
                Dictionary<String, int> pair = new();
                foreach (KeyValuePair<String, Counter> _pair in PriorityCounters)
                {
                    pair.Add(_pair.Key, _pair.Value.Value);
                }
                return pair;
            }
        }
        internal Dictionary<String, Counter> PriorityCounters = new();
        internal Priorities(Group hookGroup)
        {
            Group group = new Group("Prios");
            hookGroup.AddItem(group);
            foreach (GameObjectBase ally in UnitManager.AllyChampions)
            {
                Counter counter = new Counter($"{ally.ModelName}", 1, 0, 5);
                PriorityCounters.Add(ally.Name, counter);
                group.AddItem(counter);
            }
            group.AddItem(new InfoDisplay() { Information = "0 = Disabled" });
        }
    }
}
