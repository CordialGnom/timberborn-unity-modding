using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    internal class GrowthFertilizationInventoryService :
    IDedicatedDecoratorInitializer<GrowthFertilizationBuilding, Inventory>
    {
        private static readonly string InventoryComponentName = "GrowthFertilizationBuilding";
        private readonly InventoryInitializerFactory _inventoryInitializerFactory;

        public GrowthFertilizationInventoryService(
          InventoryInitializerFactory inventoryInitializerFactory)
        {
            this._inventoryInitializerFactory = inventoryInitializerFactory;
        }

        public void Initialize(GrowthFertilizationBuilding subject, Inventory decorator)
        {
            InventoryInitializer inventoryInitializer = this._inventoryInitializerFactory.Create(decorator, subject.Capacity, GrowthFertilizationInventoryService.InventoryComponentName);
            inventoryInitializer.AddAllowedGoods(Enumerables.One<StorableGoodAmount>(new StorableGoodAmount(StorableGood.CreateAsGivable(subject.Supply), subject.Capacity)));
            inventoryInitializer.HasPublicOutput();
            inventoryInitializer.Initialize();
            subject.InitializeInventory(decorator);
        }
    }
}
