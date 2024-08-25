// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using System.Collections.Generic;
using System.Text;
using TimberApi.UIBuilderSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{

    sealed class GrowthFertilizationBuildingFragment : IEntityPanelFragment
    {
        readonly UiFactory _uiFactory;

        private GrowthFertilizationBuilding _growthFertilizationBuilding;

        VisualElement _root = new();
        Label _growthStateText = new();
        Label _consumptionText = new ();
        Label _capacityStateText = new();

        public GrowthFertilizationBuildingFragment(UiFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public VisualElement InitializeFragment()
        {
            _growthStateText = _uiFactory.CreateLabel();
            _consumptionText = _uiFactory.CreateLabel();
            _capacityStateText = _uiFactory.CreateLabel();

            _root = _uiFactory.CreateCenteredPanelFragmentBuilder()
                .AddComponent(_growthStateText).AddComponent(_capacityStateText).AddComponent(_consumptionText)
                .BuildAndInitialize();
            _root.ToggleDisplayStyle(visible: false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            this._growthFertilizationBuilding =     entity.GetComponentFast<GrowthFertilizationBuilding>();
            
            _growthStateText.text = "Tree Growth Default";
            _capacityStateText.text = "Capacity Default";
            _consumptionText.text = "Consumption Default";
            UpdateGrowthState();
            UpdateInventoryState();
            UpdateConsumptionState();

            _root.ToggleDisplayStyle((bool)(Object)this._growthFertilizationBuilding);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(visible: false);
        }

        public void UpdateFragment()
        {
        }

        private void UpdateInventoryState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._capacityStateText.text = _growthFertilizationBuilding.Supply + ":  " + (this._growthFertilizationBuilding.SupplyLeft) + "/" +  (this._growthFertilizationBuilding.Capacity);
        }
        private void UpdateConsumptionState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._consumptionText.text = "Using: " + _growthFertilizationBuilding.ConsumptionPerHour + " Fertilizer / Hour";
        }


        private void UpdateGrowthState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._growthStateText.text = "Trees growing:  " + (this._growthFertilizationBuilding.TreesGrowCount) + "/" + (this._growthFertilizationBuilding.TreesTotalCount);
        }
    }
}