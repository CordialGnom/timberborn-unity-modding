// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using System;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIPresets.Builders;
using TimberApi.UIPresets.Dropdowns;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.DropdownSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.Forestry;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.ForesterUpdate.Scripts.UI
{
    sealed class ForesterUpdateFragment : IEntityPanelFragment
    {
        UIBuilder _builder;
        Forester _forester;


        VisualElement _root = new();
        Label _foresterDescriptionLabel = new();
        Toggle _foresterStateToggle = new ();
        DropDownBuilder _capacityStateText = new();

        // localiations
        private readonly ILoc _loc;
        private static readonly string ForesterDescriptionLocKey = "Cordial.Building.FertilizerDump.Consumption";
        private static readonly string TreeCountLocKey = "Cordial.Building.FertilizerDump.TreeCount";
        private static readonly string FertilizerNameLocKey = "Cordial.Good.Fertilizer.DisplayName";
        private static readonly string UnitPerHourLocKey = "Cordial.Unit.PerHour";


        public ForesterUpdateFragment(UIBuilder uiBuilder,
                                                    ILoc loc)
        {
            _uiBuilder = uiBuilder;
            _loc = loc;
        }

        public VisualElement InitializeFragment()
        {
            _foresterDescriptionLabel =  _builder.Create<GameLabel>()
                                            .SetLocKey(ForesterDescriptionLocKey)
                                            .Build();


            _foresterStateToggle =  _builder.Create<GameToggle>()
                                            .SetLocKey(ForesterDescriptionLocKey).Build();

            var gameDropdown = _builder.Build<GameDropdown, Dropdown>();

            _dropdownItemsSetter.SetItems(gameDropdown, new TestDropdownProvider());



            _root =      _builder.Create<VisualElementBuilder>()
                                .AddComponent<FragmentBuilder>()
                                .AddComponent(_foresterDescriptionLabel)
                                .AddComponent(_foresterStateToggle).Build();



            // label, toggle, dropdown? 
            _foresterDescriptionLabel = _uiBuilder.Create<GameLabel>().Build();

            _foresterStateToggle = _uiBuilder.Create<GameLabel>().Build();
            _capacityStateText = _uiBuilder.Create<GameLabel>().Build();

            _root = _uiBuilder.Create<FragmentBuilder>()
                .AddComponent(_foresterDescriptionLabel).AddComponent(_capacityStateText).AddComponent(_foresterStateToggle)
                .BuildAndInitialize();
            _root.ToggleDisplayStyle(visible: false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            this._forester =     entity.GetComponentFast<Forester>();
            
            _foresterDescriptionLabel.text = "Tree Growth Default";
            _capacityStateText.text = "Capacity Default";
            _foresterStateToggle.text = "Consumption Default";
            UpdateGrowthState();
            UpdateInventoryState();
            UpdateConsumptionState();

            _root.ToggleDisplayStyle((bool)(Object)this._forester);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(visible: false);
        }

        public void UpdateFragment()
        {
            if (null != _forester)
            {
                this.UpdateGrowthState();
                this.UpdateConsumptionState();
                this.UpdateInventoryState();
                _root.ToggleDisplayStyle((bool)(Object)this._forester);
            }
        }

        private void UpdateInventoryState()
        {
            if (!(bool)(Object)this._forester)
                return;
            this._capacityStateText.text = _loc.T(FertilizerNameLocKey) + ": " + (this._growthFertilizationBuilding.SupplyLeft) + "/" +  (this._growthFertilizationBuilding.Capacity);
        }
        private void UpdateConsumptionState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._foresterStateToggle.text = _loc.T(ConsumptionLocKey) + " " + _growthFertilizationBuilding.ConsumptionPerHour + _loc.T(UnitPerHourLocKey);
        }

        private void UpdateGrowthState()
        {
            if (!(bool)(Object)this._growthFertilizationBuilding)
                return;
            this._foresterDescriptionLabel.text = _loc.T(TreeCountLocKey) + ": " + (this._growthFertilizationBuilding.TreesGrowCount) + "/" + (this._growthFertilizationBuilding.TreesTotalCount);
        }
    }
}