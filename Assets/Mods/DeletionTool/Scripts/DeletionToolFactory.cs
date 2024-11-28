using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.DeletionTool.Scripts
{
    public class DeletionToolFactory : IToolFactory
    {
        private readonly IDeletionTool _DeletionTool;
        public string Id => "DeletionTool";
        public DeletionToolFactory(IDeletionTool DeletionTool)
        {
            _DeletionTool = DeletionTool;
        }

        public Tool Create(ToolSpecification toolSpecification, ToolGroup toolGroup = null)
        {
            _DeletionTool.SetToolGroup(toolGroup);
            return (Tool)_DeletionTool;
        }

    }
}
