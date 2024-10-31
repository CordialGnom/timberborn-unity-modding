
namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantingOverrideConfigChangeEvent
    {
        public string PlantName { get; }
        public bool IsTree { get; }


        public PlantingOverrideConfigChangeEvent(string plantName, bool isTree)
        {
            this.PlantName = plantName;
            this.IsTree = isTree;
        }

    }
}
