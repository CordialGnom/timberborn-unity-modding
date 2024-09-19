
namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolConfigChangeEvent
    {
        public CutterToolConfigFragment CutterToolConfig { get; }

        public CutterToolConfigChangeEvent(CutterToolConfigFragment cutterToolConfig)
        {
            this.CutterToolConfig = cutterToolConfig;
        }

    }
}
