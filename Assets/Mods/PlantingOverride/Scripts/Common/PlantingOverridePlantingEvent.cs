
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverridePlantingEvent
    {
        public string PlantName { get; }
        public Vector3Int Coordinates { get; }

        public PlantingOverridePlantingEvent(string plantName, Vector3Int coordinates)
        {
            this.PlantName = plantName;
            this.Coordinates = coordinates;
        }

    }
}
