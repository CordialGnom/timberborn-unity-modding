
namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantingOverrideTreeUnselectedEvent
    {
        public PlantingOverrideTreeService PlantingOverrideTreeService { get; }

        public PlantingOverrideTreeUnselectedEvent(PlantingOverrideTreeService PlantingOverrideToolService)
        {
            this.PlantingOverrideTreeService = PlantingOverrideToolService;
        }

    }
}
