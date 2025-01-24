
using UnityEngine;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolUnmarkEvent
    {
        public Vector3Int Coordinates { get; }
        public bool PlaceHive { get; }

        public PlantBeehiveToolUnmarkEvent(Vector3Int coordinates, bool placeHive)
        {
            this.Coordinates = coordinates;
            this.PlaceHive = placeHive;
        }

    }
}
