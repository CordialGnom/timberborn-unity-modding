
namespace Cordial.Mods.ForestTool.Scripts.UI.Events
{
    public class ForestToolUnselectedEvent
    {
        public ForestToolService ForestToolService { get; }

        public ForestToolUnselectedEvent(ForestToolService forestToolService)
        {
            this.ForestToolService = forestToolService;
        }

    }
}
