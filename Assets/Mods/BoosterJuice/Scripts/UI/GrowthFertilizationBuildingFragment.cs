// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Toggles;
using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Cordial.Mods.BoosterJuice.Scripts.Events;
using TimberApi.UIPresets.Labels;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    sealed class GrowthFertilizationBuildingFragment : IEntityPanelFragment
    {
        private readonly UiFactory _uiFactory;
        private readonly UIBuilder _uiBuilder;

        private GrowthFertilizationBuilding _growthFertilizationBuilding;

        private VisualElement _root = new();
        private Label _growthStateText = new();
        private Label _consumptionText = new();
        private Label _capacityStateText = new();
        private Label _yieldStateText = new();
        private Toggle _yieldFertilize = new();

        // localizations
        private readonly ILoc _loc;
        private static readonly string ConsumptionLocKey = "Cordial.Building.FertilizerDump.Consumption";
        private static readonly string TreeCountLocKey = "Cordial.Building.FertilizerDump.TreeCount";
        private static readonly string FertilizerNameLocKey = "Cordial.Good.Fertilizer.DisplayName";
        private static readonly string UnitPerHourLocKey = "Cordial.Unit.PerHour";

        // event system
        private readonly EventBus _eventBus;


        public GrowthFertilizationBuildingFragment(UiFactory uiFactory,
                                                    UIBuilder uiBuilder,
                                                    EventBus eventBus,
                                                    ILoc loc)
        {
            _uiFactory = uiFactory;
            _uiBuilder = uiBuilder;
            _eventBus = eventBus;
            _loc = loc;
        }

        public VisualElement InitializeFragment()
        {

            _growthStateText =  _uiBuilder.Create<GameLabel>()
                                    .Small()
                                    .Build();

            _consumptionText =  _uiBuilder.Create<GameLabel>()
                                    .Small()
                                    .Build();

            _capacityStateText = _uiBuilder.Create<GameLabel>()
                                    .Small()
                                    .Build();

            _yieldFertilize = _uiBuilder.Create<GameToggle>()
                                    .SetName("FertilizeYield")
                                    .SetLocKey("Cordial.Building.FertilizerDump.FertilizeYield")
                                    .Build();

            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_yieldFertilize)
                            .BuildAndInitialize());

            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_growthStateText)
                            .AddComponent(_capacityStateText)
                            .AddComponent(_consumptionText)
                            .BuildAndInitialize());

            _root.ToggleDisplayStyle(visible: false);

            // register event
            _root.Q<Toggle>("FertilizeYield").RegisterValueChangedCallback(value => ToggleValueChange(value.newValue));

            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            this._growthFertilizationBuilding = entity.GetComponentFast<GrowthFertilizationBuilding>();

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
            this._capacityStateText.text = _loc.T(FertilizerNameLocKey) + ": " + (this._growthFertilizationBuilding.SupplyLeft) + "/" + (this._growthFertilizationBuilding.Capacity);
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

        private void ToggleValueChange(bool value)
        {
            this._eventBus.Post((object)new GrowthFertilizationConfigChangeEvent(value));
        }

        public PanelFragment CreateCenteredPanelFragmentBuilder()
        {
            return _uiBuilder.Create<PanelFragment>()
                .SetFlexDirection(FlexDirection.Column)
                .SetWidth(new Length(100f, LengthUnit.Percent))
                .SetJustifyContent(Justify.Center);
        }
    }
}