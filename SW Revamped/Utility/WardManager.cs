using Oasys.Common.Enums.GameEnums;
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

        internal MapIDFlag Map = MapIDFlag.SummonersRift;
        internal Vector3 MovePosition { get; }
        internal Vector3 ClickPosition { get; }
        internal Vector3 WardPosition { get; }

        internal Ward(string name, Vector3 movePosition, Vector3 clickPosition, Vector3 wardPosition, MapIDFlag map = MapIDFlag.SummonersRift)
        {
            Name = name;
            MovePosition = movePosition;
            ClickPosition = clickPosition;
            WardPosition = wardPosition;
            Map = map;
        }
    }
    internal class WardManager
    {
        private List<Ward> KnownWards  = new List<Ward>();

        private void ConstructWards()
        {
            KnownWards = new List<Ward>()
            {
            new Ward(
                "Wolvesbush Mid Blue",
                new Vector3(5749.25F, 51.65F, 7282.75F),
                new Vector3(5174.83F, 50.57F, 7119.81F),
                new Vector3(4909.10F, 50.65F, 7110.90F)
                ),
            new Ward(
                "Wolvesbush Mid Red",
                new Vector3(9122, 52.60F, 7606),
                new Vector3(9647.62F, 51.31F, 7889.96F),
                new Vector3(9874.42F, 51.50F, 7969.29F)
                ),
            new Ward(
                "Tower-Wolvesbush Blue",
                new Vector3(5574F, 51.74F, 6458F),
                new Vector3(5239.21F, 50.67F, 6944.90F),
                new Vector3(4909.10F, 50.65F, 7110.90F)
                ),
            new Ward(
                "Tower-Wolvesbush Red",
                new Vector3(9122, 53.74F, 8356),
                new Vector3(9586.57F, 59.62F, 8020.29F),
                new Vector3(9871.77F, 51.47F, 8014.44F)
                ),
            new Ward(
                "Redbush Blue",
                new Vector3(8022F, 53.72F, 4258F),
                new Vector3(8463.64F, 50.60F, 4658.71F),
                new Vector3(8512.29F, 51.30F, 4745.90F)
                ),
            new Ward(
                "Redbush Red",
                new Vector3(6824, 56, 10656),
                new Vector3(6360.12F, 52.61F, 10362.71F),
                new Vector3(6269.35F, 53.72F, 10306.69F)
                ),
            new Ward(
                "Riverbush Top Blue",
                new Vector3(1774F, 52.84F, 10856F),
                new Vector3(2380.09F, -71.24F, 11004.69F),
                new Vector3(2826.47F, -71.02F, 11221.34F)
                ),
            new Ward(
                "Riverbush Bot Red",
                new Vector3(13022F, 51.37F, 3808F),
                new Vector3(12427.00F, -35.46F, 3984.26F),
                new Vector3(11975.34F, 66.37F, 3927.68F)
                ),
            new Ward(
                "Dragon-Tribush",
                new Vector3(10072, -71.24F, 3908),
                new Vector3(10301.03F, 49.03F, 3333.20F),
                new Vector3(10322.94F, 49.03F, 3244.38F)
                ),
            new Ward(
                "Baron-Tribush",
                new Vector3(4824, -71.24F, 10906),
                new Vector3(4633.83F, 50.51F, 11354.40F),
                new Vector3(4524.69F, 53.25F, 11515.21F)),
            new Ward(
                "Dragon-Redbush",
                new Vector3(9322, -71.2406F, 4408),
                new Vector3(8710.373F, 53.111084F, 4524.969F),
                new Vector3(8691.065F, 52.784424F, 4530.796F)),
            new Ward(
                "Baron-Redbush",
                new Vector3(5490, -72.75593F, 10506),
                new Vector3(6099.6284F, 55.38684F, 10474.643F),
                new Vector3(6099.6284F, 55.50769F,10474.643F))

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
            return GetClosestWard(target.Position);
        }

        internal Ward? GetClosestWard(Vector3 position)
        {
            return GetKnownWards().OrderBy(x => x.MovePosition.Distance(position)).FirstOrDefault();
        }

        internal bool WardInRange(GameObjectBase target, int range)
        {
            return WardInRange(target.Position, range);
        }

        internal bool WardInRange(Vector3 position, int range)
        {
            return GetClosestWard(position)?.MovePosition.Distance(position) < range;
        }

        internal bool StandsOnWard(GameObjectBase target, Ward ward) 
        {
            return target.DistanceTo(ward.MovePosition) < 25;
        }
    }
}
