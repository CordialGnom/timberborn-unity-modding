﻿
namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantingOverrideCropUnselectedEvent
    {
        public PlantingOverrideCropService PlantingOverrideCropService { get; }

        public PlantingOverrideCropUnselectedEvent(PlantingOverrideCropService PlantingOverrideToolService)
        {
            this.PlantingOverrideCropService = PlantingOverrideToolService;
        }

    }
}
