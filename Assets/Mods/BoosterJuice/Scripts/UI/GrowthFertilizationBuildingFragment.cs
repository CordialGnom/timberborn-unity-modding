// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    sealed class GrowthFertilizationBuildingFragment : IEntityPanelFragment
    {
        private readonly UiFactory _uiFactory;

        private GrowthFertilizationBuilding _growthFertilizationBuilding;

        VisualElement _root = new();
        Label _growthStateText = new();
        Label _consumptionText = new ();
        Label _capacityStateText = new();

        // localiations
        private readonly ILoc _loc;
        private static readonly string ConsumptionLocKey = "Cordial.Building.FertilizerDump.Consumption";
        private static readonly string TreeCountLocKey = "Cordial.Building.FertilizerDump.TreeCount";
        private static readonly string FertilizerNameLocKey = "Cordial.Good.Fertilizer.DisplayName";
        private static readonly string UnitPerHourLocKey = "Cordial.Unit.PerHour";


        public GrowthFertilizationBuildingFragment( UiFactory uiFactory,
                                                    ILoc loc)
        {
            _uiFactory = uiFactory;
            _loc = loc;
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
            if (null != _growthFertilizationBuilding)
            {
                this.UpdateGrowthState();
                this.UpdateConsumptionState();
                this.UpdateInventoryState();
                _root.ToggleDisplayStyle((bool)(Object)this._growthFertilizationBuilding);
            }
        }

        private void UpdateInventoryState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._capacityStateText.text = _loc.T(FertilizerNameLocKey) + ": " + (this._growthFertilizationBuilding.SupplyLeft) + "/" +  (this._growthFertilizationBuilding.Capacity);
        }
        private void UpdateConsumptionState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._consumptionText.text = _loc.T(ConsumptionLocKey) + " " + _growthFertilizationBuilding.ConsumptionPerHour + _loc.T(UnitPerHourLocKey);
        }

        private void UpdateGrowthState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._growthStateText.text = _loc.T(TreeCountLocKey) + ": " + (this._growthFertilizationBuilding.TreesGrowCount) + "/" + (this._growthFertilizationBuilding.TreesTotalCount);
        }
    }
}