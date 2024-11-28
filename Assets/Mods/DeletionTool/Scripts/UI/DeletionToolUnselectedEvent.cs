using Cordial.Mods.DeletionTool.Scripts;

namespace Cordial.Mods.DeletionTool.Scripts.UI
{
    public class DeletionToolUnselectedEvent
    {
        public DeletionToolService DeletionToolService { get; }

        public DeletionToolUnselectedEvent(DeletionToolService DeletionToolService)
        {
            this.DeletionToolService = DeletionToolService;
        }

    }
}
