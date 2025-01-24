using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolFactory : IToolFactory
    {
        private readonly IPlantBeehiveTool _PlantBeehiveTool;
        public string Id => "PlantBeehiveTool";
        public PlantBeehiveToolFactory(IPlantBeehiveTool PlantBeehiveTool)
        {
            _PlantBeehiveTool = PlantBeehiveTool;
        }

        public Tool Create(ToolSpecification toolSpecification, ToolGroup toolGroup = null)
        {
            _PlantBeehiveTool.SetToolGroup(toolGroup);
            return (Tool)_PlantBeehiveTool;
        }

    }
}
