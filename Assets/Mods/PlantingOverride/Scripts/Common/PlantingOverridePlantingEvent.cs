
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverridePlantingEvent
    {
        public Vector3Int Coordinates { get; }
        public string plantableName {  get; }

        public PlantingOverridePlantingEvent(Vector3Int coordinates, string plantableName = "")
        {
            this.Coordinates = coordinates;
            this.plantableName = plantableName;
        }

    }
}
