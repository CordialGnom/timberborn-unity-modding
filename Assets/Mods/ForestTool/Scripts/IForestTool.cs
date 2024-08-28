using Timberborn.ToolSystem;

namespace Mods.ForestTool.Scripts
{
    public interface IForestTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();

    }
}
