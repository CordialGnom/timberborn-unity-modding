using Bindito.Core;
using Timberborn.EntityPanelSystem;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    [Context("Game")]
    public class GrowthFertilizationUIConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<GrowthFertilizationAreaService>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationBuildingFragment>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationGrowableFragment>().AsSingleton();
            containerDefinition.Bind<UiFactory>().AsSingleton();
            containerDefinition.Bind<PanelFragment>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<GrowthFertilizationUIConfigurator.EntityPanelModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            readonly GrowthFertilizationBuildingFragment _growthFertilizationBuildingFragment;
            readonly GrowthFertilizationGrowableFragment _growthFertilizationGrowableFragment;

            public EntityPanelModuleProvider(   GrowthFertilizationBuildingFragment growthFertilizationBuildingFragment,
                                                GrowthFertilizationGrowableFragment growthFertilizationGrowableFragment)
            {
                _growthFertilizationBuildingFragment = growthFertilizationBuildingFragment;
                _growthFertilizationGrowableFragment = growthFertilizationGrowableFragment;
            }

            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddBottomFragment(_growthFertilizationBuildingFragment);
                builder.AddBottomFragment(_growthFertilizationGrowableFragment);
                return builder.Build();
            }
        }
    }

}
