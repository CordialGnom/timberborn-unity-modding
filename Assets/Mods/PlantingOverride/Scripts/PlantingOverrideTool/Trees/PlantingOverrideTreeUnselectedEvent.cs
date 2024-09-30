
namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
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
