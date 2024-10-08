using Bindito.Core;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.LaborSystem;
using Timberborn.TemplateSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
  
{
    [Context("Game")]
    public class GrowthFertilizationConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<GrowthFertilizationInventoryService>().AsSingleton();
            containerDefinition.Bind<YieldFertilizationService>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider<GrowthFertilizationConfigurator.TemplateModuleProvider>().AsSingleton();
        }

        private class TemplateModuleProvider : IProvider<TemplateModule>
        {
            private readonly GrowthFertilizationInventoryService _growthFertilizationInventoryService;

            public TemplateModuleProvider(
              GrowthFertilizationInventoryService growthFertilizationInventoryService)
            {
                this._growthFertilizationInventoryService = growthFertilizationInventoryService;
            }

            public TemplateModule Get()
            {
                TemplateModule.Builder builder = new TemplateModule.Builder();
                builder.AddDecorator<GrowthFertilizationBuilding, AutoEmptiable>();
                builder.AddDecorator<GrowthFertilizationBuilding, Emptiable>();
                builder.AddDecorator<GrowthFertilizationBuilding, FillInputHaulBehaviorProvider>();
                builder.AddDecorator<GrowthFertilizationBuilding, GrowthFertilizationStatusService>();
                builder.AddDecorator<GrowthFertilizationBuilding, WorkshopProductivityCounter>();
                builder.AddDecorator<Worker, WorkplaceWorkStarter>();
                builder.AddDecorator<GrowthFertilizationStatusService, LackOfResourcesStatus>();
                builder.AddDecorator<GrowthFertilizationStatusService, NoHaulingPostStatus>();
                builder.AddDedicatedDecorator<GrowthFertilizationBuilding, Inventory>((IDedicatedDecoratorInitializer<GrowthFertilizationBuilding, Inventory>)this._growthFertilizationInventoryService);
                GrowthFertilizationConfigurator.TemplateModuleProvider.InitializeBehaviors(builder);
                return builder.Build();
            }

            private static void InitializeBehaviors(TemplateModule.Builder builder)
            {
                builder.AddDecorator<GrowthFertilizationBuilding, EmptyInventoriesWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuilding, FillInputWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuilding, RemoveUnwantedStockWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuilding, GrowthFertilizationWorkplaceBehaviour>();
                builder.AddDecorator<GrowthFertilizationBuilding, LaborWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuilding, WaitInsideIdlyWorkplaceBehavior>();
            }
        }
    }
}
