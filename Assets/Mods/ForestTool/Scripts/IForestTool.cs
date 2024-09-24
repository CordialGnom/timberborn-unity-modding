using Timberborn.ToolSystem;

namespace Cordial.Mods.ForestTool.Scripts
{
    public interface IForestTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();

    }
}
