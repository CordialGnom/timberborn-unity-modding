namespace Cordial.Mods.DeletionTool.Scripts.UI
{
    public class DeletionToolSelectedEvent
    {
        public DeletionToolService DeletionToolService { get; }

        public DeletionToolSelectedEvent(DeletionToolService DeletionToolService)
        {
            this.DeletionToolService = DeletionToolService;
        }

    }
}
