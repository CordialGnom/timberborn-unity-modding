using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Cordial.Mods.ForestTool.Scripts.UI;

namespace Cordial.Mods.ForestTool.Scripts
{
    [Context("Game")]
    public class ForestToolConfigurator : IConfigurator
    {

        public void Configure(IContainerDefinition containerDefinition)
        {

            containerDefinition.Bind<ForestToolInitializer>().AsSingleton();
            containerDefinition.Bind<PanelFragment>().AsSingleton();
            containerDefinition.Bind<PanelFragmentBlue>().AsSingleton();
            containerDefinition.Bind<PanelFragmentRed>().AsSingleton();
            containerDefinition.Bind<ForestToolConfigFragment>().AsSingleton();
            containerDefinition.Bind<ForestToolErrorFragment>().AsSingleton();

            containerDefinition.Bind<ForestToolPrefabSpecService>().AsSingleton();

            containerDefinition.Bind<IForestTool>().To<ForestToolService>().AsSingleton();
            containerDefinition.Bind<ForestToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<ForestToolFactory>().AsSingleton();
        }
    }
}