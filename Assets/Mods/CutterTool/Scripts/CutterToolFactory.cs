using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolFactory : IToolFactory
    {
        private readonly ICutterTool _CutterTool;
        public string Id => "CutterTool";
        public CutterToolFactory(ICutterTool CutterTool)
        {
            _CutterTool = CutterTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _CutterTool.SetToolGroup(toolGroup);
            return (Tool)_CutterTool;
        }

    }
}
