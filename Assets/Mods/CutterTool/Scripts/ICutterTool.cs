using Timberborn.ToolSystem;

namespace Mods.CutterTool.Scripts
{
    public interface ICutterTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();

    }
}
