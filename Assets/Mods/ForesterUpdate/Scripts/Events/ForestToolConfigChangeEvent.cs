
namespace Cordial.Mods.ForesterUpdate.Scripts.UI.Events
{
    public class ForesterUpdateConfigChangeEvent
    {
        public string PlantName { get; }

        public ForesterUpdateConfigChangeEvent(string plantName)
        {
            this.PlantName = plantName;
        }

    }
}
