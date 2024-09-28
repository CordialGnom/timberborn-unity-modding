using Cordial.Mods.PlantingOverrideTool.Scripts;

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolUnselectedEvent
    {
        public PlantingOverrideToolService PlantingOverrideToolService { get; }

        public PlantingOverrideToolUnselectedEvent(PlantingOverrideToolService PlantingOverrideToolService)
        {
            this.PlantingOverrideToolService = PlantingOverrideToolService;
        }

    }
}
