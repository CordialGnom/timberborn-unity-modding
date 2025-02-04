using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    [Context("Game")]
    public class PlantingBeehiveConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<PlantBeehivePrefabSpecService>().AsSingleton();

            containerDefinition.Bind<IPlantBeehiveTool>().To<PlantBeehiveToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PlantBeehiveToolFactory>().AsSingleton();
            containerDefinition.MultiBind<IToolLocker>().To<PlantBeehiveToolLocker>().AsSingleton();
        }
    }
}