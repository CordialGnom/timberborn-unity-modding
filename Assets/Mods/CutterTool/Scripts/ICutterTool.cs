using Timberborn.ToolSystem;

namespace Cordial.Mods.CutterTool.Scripts
{
    public interface ICutterTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();

    }
}
