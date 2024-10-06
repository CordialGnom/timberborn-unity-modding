namespace Cordial.Mods.PlantingOverride.Scripts.UI
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
