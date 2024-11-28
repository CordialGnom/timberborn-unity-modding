using Bindito.Core;
using TimberApi.Tools.ToolSystem;

namespace Cordial.Mods.DeletionTool.Scripts
{
    [Context("Game")]
    public class DeletionToolConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<IDeletionTool>().To<DeletionToolService>().AsSingleton();
            containerDefinition.Bind<DeletionToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<DeletionToolFactory>().AsSingleton();
        }


    }
}