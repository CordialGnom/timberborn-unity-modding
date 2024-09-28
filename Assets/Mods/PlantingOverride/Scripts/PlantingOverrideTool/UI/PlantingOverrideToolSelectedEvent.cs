namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolSelectedEvent
    {
        public PlantingOverrideToolService PlantingOverrideToolService { get; }

        public PlantingOverrideToolSelectedEvent(PlantingOverrideToolService PlantingOverrideToolService)
        {
            this.PlantingOverrideToolService = PlantingOverrideToolService;
        }

    }
}
