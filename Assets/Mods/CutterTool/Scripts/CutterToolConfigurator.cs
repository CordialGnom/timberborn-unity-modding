using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Cordial.Mods.CutterTool.Scripts.UI;

namespace Cordial.Mods.CutterTool.Scripts
{
    [Context("Game")]
    public class CutterToolConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<CutterToolSettings>().AsSingleton();
            containerDefinition.Bind<CutterToolInitializer>().AsSingleton();
            containerDefinition.Bind<PanelFragment>().AsSingleton();
            containerDefinition.Bind<CutterToolConfigFragment>().AsSingleton();
            //containerDefinition.Bind<CutterToolConfigPanel>().AsSingleton();
            containerDefinition.Bind<ICutterTool>().To<CutterToolService>().AsSingleton();
            containerDefinition.Bind<CutterToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<CutterToolFactory>().AsSingleton();
        }


    }
}