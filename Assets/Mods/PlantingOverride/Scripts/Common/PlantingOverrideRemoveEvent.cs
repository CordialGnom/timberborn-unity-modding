
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverrideRemoveEvent
    {
        public Vector3Int Coordinates { get; }

        public PlantingOverrideRemoveEvent(Vector3Int coordinates)
        {
            this.Coordinates = coordinates;
        }

    }
}
