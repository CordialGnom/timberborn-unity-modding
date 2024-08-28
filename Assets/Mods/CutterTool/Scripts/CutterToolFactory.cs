using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Mods.CutterTool.Scripts
{
    public class CutterToolFactory : IToolFactory
    {
        private readonly ICutterTool _CutterTool;
        public string Id => "CutterTool";
        public CutterToolFactory(ICutterTool CutterTool)
        {
            _CutterTool = CutterTool;
        }

        public Tool Create(ToolSpecification toolSpecification, ToolGroup toolGroup = null)
        {
            _CutterTool.SetToolGroup(toolGroup);
            return (Tool)_CutterTool;
        }

    }
}
