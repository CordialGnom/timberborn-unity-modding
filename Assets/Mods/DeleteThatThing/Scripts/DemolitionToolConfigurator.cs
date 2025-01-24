using Bindito.Core;
using TimberApi.Tools.ToolSystem;

namespace Cordial.Mods.DemolitionTool.Scripts
{
    [Context("Game")]
    public class DemolitionToolConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<IDemolitionTool>().To<DemolitionToolService>().AsSingleton();
            containerDefinition.Bind<DemolitionToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<DemolitionToolFactory>().AsSingleton();
        }


    }
}