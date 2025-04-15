using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.DemolitionTool.Scripts
{
    public class DemolitionToolFactory : IToolFactory
    {
        private readonly IDemolitionTool _DemolitionTool;
        public string Id => "DeleteThatThing";
        public DemolitionToolFactory(IDemolitionTool DemolitionTool)
        {
            _DemolitionTool = DemolitionTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _DemolitionTool.SetToolGroup(toolGroup);
            return (Tool)_DemolitionTool;
        }

    }
}
