
namespace Cordial.Mods.ForestTool.Scripts.UI.Events
{
    public class ForestToolConfigChangeEvent
    {
        public ForestToolConfigFragment ForestToolConfig { get; }

        public ForestToolConfigChangeEvent(ForestToolConfigFragment forestToolConfig)
        {
            this.ForestToolConfig = forestToolConfig;
        }

    }
}
