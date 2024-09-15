using Cordial.Mods.CutterTool.Scripts;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolUnselectedEvent
    {
        public CutterToolService CutterToolService { get; }

        public CutterToolUnselectedEvent(CutterToolService cutterToolService)
        {
            this.CutterToolService = cutterToolService;
        }

    }
}
