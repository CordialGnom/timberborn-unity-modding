
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverridePlantingEvent
    {
        public Vector3Int Coordinates { get; }

        public PlantingOverridePlantingEvent(Vector3Int coordinates)
        {
            this.Coordinates = coordinates;
        }

    }
}
