using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Assets.Mods.PlantingOverride.Scripts.Common;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    [Context("Game")]
    public class PlantingOverrideConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<PlantingOverrideToolInitializer>().AsSingleton();
            containerDefinition.Bind<PanelFragment>().AsSingleton();
            containerDefinition.Bind<PanelFragmentRed>().AsSingleton();
            containerDefinition.Bind<PanelFragmentBlue>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideToolConfigFragment>().AsSingleton();

            containerDefinition.Bind<PlantingOverrideDropDownProvider>().AsSingleton();
            containerDefinition.Bind<PlantingOverridePrefabSpecService>().AsSingleton();
            //containerDefinition.Bind<PlantingOverrideHarmony>().AsSingleton();

            containerDefinition.Bind<IPlantingOverrideTreeTool>().To<PlantingOverrideTreeService>().AsSingleton();
            containerDefinition.Bind<IPlantingOverrideCropTool>().To<PlantingOverrideCropService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PlantingOverrideTreeToolFactory>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PlantingOverrideCropToolFactory>().AsSingleton();
            //containerDefinition.Bind<PlantingOverrideTreeService>().AsSingleton();
            //containerDefinition.Bind<PlantingOverrideCropService>().AsSingleton();
        }
    }
}