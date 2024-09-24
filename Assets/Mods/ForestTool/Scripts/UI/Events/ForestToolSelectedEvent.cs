
namespace Cordial.Mods.ForestTool.Scripts.UI.Events
{
    public class ForestToolSelectedEvent
    {
        public ForestToolService ForestToolService { get; }

        public ForestToolSelectedEvent(ForestToolService forestToolService)
        {
            this.ForestToolService = forestToolService;
        }

    }
}
