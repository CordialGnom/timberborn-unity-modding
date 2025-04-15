using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    public class PlantingOverrideCropToolFactory : IToolFactory
    {
        private readonly IPlantingOverrideCropTool _PlantingOverrideTool;
        public string Id => "PlantingOverrideToolCrops";
        public PlantingOverrideCropToolFactory(IPlantingOverrideCropTool PlantingOverrideTool)
        {
            _PlantingOverrideTool = PlantingOverrideTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _PlantingOverrideTool.SetToolGroup(toolGroup);
            return (Tool)_PlantingOverrideTool;
        }

    }
}
