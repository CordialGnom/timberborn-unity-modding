namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideCropSelectedEvent
    {
        public PlantingOverrideCropService PlantingOverrideCropService { get; }

        public PlantingOverrideCropSelectedEvent(PlantingOverrideCropService PlantingOverrideToolService)
        {
            this.PlantingOverrideCropService = PlantingOverrideToolService;
        }

    }
}
