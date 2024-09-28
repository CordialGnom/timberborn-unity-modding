using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverrideTool.Scripts
{
    public interface IPlantingOverrideTool
    {
        void SetToolGroup(ToolGroup toolGroup);
        void PostProcessInput();
    }
}
