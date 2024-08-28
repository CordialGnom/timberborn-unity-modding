using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Mods.ForestTool.Scripts
{
    public class ForestToolFactory : IToolFactory
    {
        private readonly IForestTool _ForestTool;
        public string Id => "ForestTool";
        public ForestToolFactory(IForestTool ForestTool)
        {
            _ForestTool = ForestTool;
        }

        public Tool Create(ToolSpecification toolSpecification, ToolGroup toolGroup = null)
        {
            _ForestTool.SetToolGroup(toolGroup);
            return (Tool)_ForestTool;
        }

    }
}
