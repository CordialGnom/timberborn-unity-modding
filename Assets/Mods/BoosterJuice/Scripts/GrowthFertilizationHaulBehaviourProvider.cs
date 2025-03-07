using Bindito.Core;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BuildingsBlocking;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public class GrowthFertilizationHaulBehaviourProvider : BaseComponent, IHaulBehaviorProvider
    {
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private InventoryFillCalculator _inventoryFillCalculator;
        private Inventories _inventories;
        private BlockableBuilding _blockableBuilding;
        private FillInputWorkplaceBehavior _fillInputWorkplaceBehavior;
        private EmptyOutputWorkplaceBehavior _emptyOutputWorkplaceBehavior;

        [Inject]
        public void InjectDependencies(InventoryFillCalculator inventoryFillCalculator)
        {
            this._inventoryFillCalculator = inventoryFillCalculator;

            Debug.Log("IHBP: Inject");
        }

        public void Awake()
        {
            this._growthFertilizationBuilding = this.GetComponentFast<GrowthFertilizationBuilding>();
            this._blockableBuilding = this.GetComponentFast<BlockableBuilding>();
            this._inventories = this.GetComponentFast<Inventories>();
            this._fillInputWorkplaceBehavior = this.GetComponentFast<FillInputWorkplaceBehavior>();
            this._emptyOutputWorkplaceBehavior = this.GetComponentFast<EmptyOutputWorkplaceBehavior>();

            Debug.Log("IHBP: Awake");

            if (null != this._growthFertilizationBuilding)
            {

                Debug.Log("IHBP: GFB");
            }
            if (null != this._inventoryFillCalculator)
            {

                Debug.Log("IHBP: _inventoryFillCalculator");
            }
            if (null != this._blockableBuilding)
            {

                Debug.Log("IHBP: _blockableBuilding");
            }
            if (null != this._fillInputWorkplaceBehavior)
            {

                Debug.Log("IHBP: _fillInputWorkplaceBehavior");
            }
            if (null != this._emptyOutputWorkplaceBehavior)
            {

                Debug.Log("IHBP: _emptyOutputWorkplaceBehavior");
            }
        }

        public void GetWeightedBehaviors(IList<WeightedBehavior> weightedBehaviors)
        {
            if (!this._growthFertilizationBuilding || !this._blockableBuilding.IsUnblocked)
                return;

            foreach (Inventory enabledInventory in this._inventories.EnabledInventories)
            {
                if (enabledInventory.IsInput)
                {
                    float weight = 1f - this._inventoryFillCalculator.GetInputFillPercentage(enabledInventory);
                    if ((double)weight > 0.0)
                        weightedBehaviors.Add(new WeightedBehavior(weight, (WorkplaceBehavior)this._fillInputWorkplaceBehavior));
                }
                if (enabledInventory.IsOutput)
                {
                    float outputFillPercentage = this._inventoryFillCalculator.GetOutputFillPercentage(enabledInventory);
                    if ((double)outputFillPercentage > 0.0)
                        weightedBehaviors.Add(new WeightedBehavior(outputFillPercentage, (WorkplaceBehavior)this._emptyOutputWorkplaceBehavior));
                }
            }
        }
    }
}
