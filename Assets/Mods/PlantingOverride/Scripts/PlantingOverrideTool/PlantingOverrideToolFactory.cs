using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverrideTool.Scripts
{
    public class PlantingOverrideToolFactory : IToolFactory
    {
        private readonly IPlantingOverrideTool _PlantingOverrideTool;
        public string Id => "PlantingOverrideTool";
        public PlantingOverrideToolFactory(IPlantingOverrideTool PlantingOverrideTool)
        {
            _PlantingOverrideTool = PlantingOverrideTool;
        }

        public Tool Create(ToolSpecification toolSpecification, ToolGroup toolGroup = null)
        {
            _PlantingOverrideTool.SetToolGroup(toolGroup);
            return (Tool)_PlantingOverrideTool;
        }

    }
}
