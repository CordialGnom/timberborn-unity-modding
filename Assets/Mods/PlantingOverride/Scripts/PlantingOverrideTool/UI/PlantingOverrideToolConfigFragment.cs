using System.Collections.Generic;
using System.Collections.Immutable;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.CustomElements;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using TimberApi.UIPresets.Dropdowns;
using Timberborn.DropdownSystem;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;
using UnityEngine.UIElements;
using Cordial.Mods.ForesterUpdate.Scripts.UI;
using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;

        // UI components
        private readonly VisualElement _root = new();
        private Label _labelTitle = new();
        private Label _labelDescription = new();
        private Dropdown _plantingOverrideDropDown = new();

        // faction / crop configuration
        private readonly DropdownItemsSetter _dropdownItemsSetter;
        private readonly PlantingOverrideDropDownProvider _dropDownProvider;

        private readonly EventBus _eventBus;

        public PlantingOverrideToolConfigFragment(UIBuilder uiBuilder,
                                                 VisualElementLoader visualElementLoader,
                                                 DropdownItemsSetter dropdownItemsSetter,
                                                 PlantingOverrideDropDownProvider dropDownProvider,
                                                 EventBus eventBus)
        {
            _eventBus = eventBus;
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;

            _dropdownItemsSetter = dropdownItemsSetter;
            _dropDownProvider = dropDownProvider;
        }

        public VisualElement InitializeFragment()
        {
            _eventBus.Register((object)this);

            // create title label
            _labelTitle = _uiBuilder.Create<GameLabel>()
                                    .SetLocKey("Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Title")
                                    .Heading()
                                    .Build();

            // create description
            _labelDescription = _uiBuilder.Create<GameLabel>().SetLocKey("Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Description").Small().Build();


            _root.Add(CreatePanelFragmentRedBuilder()
                             .AddComponent(_labelTitle)
                            .BuildAndInitialize());

            _root.Add(CreatePanelFragmentBlueBuilder()
                             .AddComponent(_labelDescription)
                             .BuildAndInitialize());

            // create drop down
            _plantingOverrideDropDown = _uiBuilder.Build<GameDropdown, Dropdown>();

            // add dropdown to panel
            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_plantingOverrideDropDown)
                            .BuildAndInitialize());
            
            // register items to dropdown
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);

            _root.ToggleDisplayStyle(false);

            //this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(_dropDownProvider.PlantName, _dropDownProvider.ItemSetIsTree()));

            return _root;
        }

        public PanelFragment CreateCenteredPanelFragmentBuilder()
        {
            return _uiBuilder.Create<PanelFragment>()
                .SetFlexDirection(FlexDirection.Column)
                .SetWidth(new Length(100f, LengthUnit.Percent))
                //.SetWidth(new Length(325f, LengthUnit.Pixel))
                //.SetHeight(new Length(111f, LengthUnit.Pixel))
                .SetJustifyContent(Justify.Center);
        }
        public PanelFragmentRed CreatePanelFragmentRedBuilder()
        {
            return _uiBuilder.Create<PanelFragmentRed>()
                .SetAlignContent(Align.Center)  // alignment --> horizontal center
                .SetJustifyContent(Justify.Center) // justify --> vertical center
                ;
        }
        public PanelFragmentBlue CreatePanelFragmentBlueBuilder()
        {
            return _uiBuilder.Create<PanelFragmentBlue>()
                .SetAlignContent(Align.Center)  // alignment --> horizontal center
                .SetJustifyContent(Justify.Center) // justify --> vertical center
                ;
        }

        public void SetTreeFragmentState()
        {
            _dropDownProvider.ReloadAsTree();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);
            _plantingOverrideDropDown.RefreshContent();

            this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(_dropDownProvider.PlantName, _dropDownProvider.ItemSetIsTree()));
        }

        public void SetCropFragmentState()
        {
            _dropDownProvider.ReloadAsCrop();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);
            _plantingOverrideDropDown.RefreshContent();

            this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(_dropDownProvider.PlantName, _dropDownProvider.ItemSetIsTree()));
        }
    }
}
