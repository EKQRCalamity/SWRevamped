using Oasys.Common.Extensions;
using Oasys.Common.GameObject;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Utility
{
    internal class Ward
    {
        internal string Name { get; }

        internal Vector3 MovePosition { get; }
        internal Vector3 ClickPosition { get; }
        internal Vector3 WardPosition { get; }

        internal Ward(string name, Vector3 movePosition, Vector3 clickPosition, Vector3 wardPosition)
        {
            Name = name;
            MovePosition = movePosition;
            ClickPosition = clickPosition;
            WardPosition = wardPosition;
        }
    }
    internal class WardManager
    {
        private List<Ward> KnownWards  = new List<Ward>();

        private void ConstructWards()
        {
            KnownWards = new List<Ward>()
            {


            };
        }

        internal WardManager() 
        {
            ConstructWards();    
        }
        
        internal List<Ward> GetKnownWards()
        {
            return KnownWards;
        }

        internal Ward? GetClosestWard(GameObjectBase target)
        {
            return GetKnownWards().OrderBy(x => x.MovePosition.DistanceToPlayer()).FirstOrDefault();
        }

    }
}
