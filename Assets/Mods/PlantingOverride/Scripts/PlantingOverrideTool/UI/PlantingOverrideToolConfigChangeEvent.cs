
namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolConfigChangeEvent
    {
        public PlantingOverrideToolConfigFragment PlantingOverrideToolConfig { get; }

        public PlantingOverrideToolConfigChangeEvent(PlantingOverrideToolConfigFragment PlantingOverrideToolConfig)
        {
            this.PlantingOverrideToolConfig = PlantingOverrideToolConfig;
        }

    }
}
