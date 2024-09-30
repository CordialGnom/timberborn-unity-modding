namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideTreeSelectedEvent
    {
        public PlantingOverrideTreeService PlantingOverrideTreeService { get; }

        public PlantingOverrideTreeSelectedEvent(PlantingOverrideTreeService PlantingOverrideToolService)
        {
            this.PlantingOverrideTreeService = PlantingOverrideToolService;
        }

    }
}
