using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverrideTool.Scripts
{
    public interface IPlantingOverrideCropTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();
    }
}
