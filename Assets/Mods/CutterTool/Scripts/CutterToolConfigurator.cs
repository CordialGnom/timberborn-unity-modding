using Bindito.Core;
using TimberApi.Tools.ToolSystem;

namespace Mods.CutterTool.Scripts {
  [Context("Game")]
  public class CutterToolConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<CutterToolSettings>().AsSingleton();
        containerDefinition.Bind<CutterToolInitializer>().AsSingleton();
        containerDefinition.Bind<ICutterTool>().To<CutterToolService>().AsSingleton();
        containerDefinition.Bind<CutterToolService>().AsSingleton();
        containerDefinition.MultiBind<IToolFactory>().To<CutterToolFactory>().AsSingleton();
    }

  }
}