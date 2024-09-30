using Bindito.Core;
using TimberApi.Tools.ToolSystem;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;
using Cordial.Mods.ForesterUpdate.Scripts.UI;
using Cordial.Mods.ForesterUpdate.Scripts;
using Cordial.Mods.PlantingOverrideTool.Scripts;
using Cordial.Mods.PlantingOverrideTool.Scripts.UI;
using Timberborn.EntityPanelSystem;

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

            containerDefinition.Bind<PlantingOverridePrefabSpecService>().AsSingleton();

            containerDefinition.Bind<IPlantingOverrideTreeTool>().To<PlantingOverrideTreeService>().AsSingleton();
            containerDefinition.Bind<IPlantingOverrideCropTool>().To<PlantingOverrideCropService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PlantingOverrideTreeToolFactory>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PlantingOverrideCropToolFactory>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideTreeService>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideCropService>().AsSingleton();


            containerDefinition.Bind<ForesterUpdateStateService>().AsSingleton();
            containerDefinition.Bind<ForesterUpdateFragment>().AsSingleton();
            containerDefinition.Bind<ForesterUpdateTreeDropDownProvider>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<PlantingOverrideConfigurator.EntityPanelModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            readonly ForesterUpdateFragment _foresterUpdateFragment;

            public EntityPanelModuleProvider(ForesterUpdateFragment foresterUpdateFragment)
            {
                _foresterUpdateFragment = foresterUpdateFragment;
            }

            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddBottomFragment(_foresterUpdateFragment);
                return builder.Build();
            }
        }
    }
}