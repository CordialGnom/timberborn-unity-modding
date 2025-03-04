using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Cordial.Mods.ForestTool.Scripts.UI;
using Timberborn.ToolSystem;

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
            containerDefinition.Bind<ForestToolErrorPrompt>().AsSingleton();

            containerDefinition.Bind<ForestToolPrefabSpecService>().AsSingleton();

            containerDefinition.Bind<IForestTool>().To<ForestToolService>().AsSingleton();
            containerDefinition.Bind<ForestToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<ForestToolFactory>().AsSingleton();
            containerDefinition.MultiBind<IToolLocker>().To<ForestToolLocker>().AsSingleton();
        }
    }
}