namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolSelectedEvent
    {
        public CutterToolService CutterToolService { get; }

        public CutterToolSelectedEvent(CutterToolService cutterToolService)
        {
            this.CutterToolService = cutterToolService;
        }

    }
}
