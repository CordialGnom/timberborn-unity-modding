using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Dropdowns;
using Timberborn.DropdownSystem;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;
using UnityEngine.UIElements;
using TimberApi.UIPresets.Toggles;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.UI
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
        private VisualElement _treeElement = new();
        private Toggle _treeCuttingAreaRemove = new();

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

            // add a toggle for cutting area
            _treeCuttingAreaRemove = _uiBuilder.Create<GameToggle>()
                                        .SetName("CuttingAreaRemove")
                                        .SetLocKey("Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.CuttingAreaRemove")
                                        .Build();

            _treeElement.Add(_treeCuttingAreaRemove);

            // create drop down
            _plantingOverrideDropDown = _uiBuilder.Build<GameDropdown, Dropdown>();

            // add dropdown to panel
            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_plantingOverrideDropDown)
                            .AddComponent(_treeElement)
                            .BuildAndInitialize());
            
            // register items to dropdown
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);

            _root.ToggleDisplayStyle(false);

            // register event
            _root.Q<Toggle>("CuttingAreaRemove").RegisterValueChangedCallback(value => ToggleValueChange(value.newValue));

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
            _treeElement.ToggleDisplayStyle(true);
            _dropDownProvider.ReloadAsTree();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);
            _plantingOverrideDropDown.RefreshContent();
            _treeCuttingAreaRemove.value = false;


            this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(_dropDownProvider.PlantName, _dropDownProvider.ItemSetIsTree() ));
        }

        public void SetCropFragmentState()
        {
            _treeElement.ToggleDisplayStyle(false);
            _dropDownProvider.ReloadAsCrop();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);
            _plantingOverrideDropDown.RefreshContent();

            this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(_dropDownProvider.PlantName, _dropDownProvider.ItemSetIsTree() ));
        }

        public void ToggleValueChange(bool value)
        {
            this._eventBus.Post((object)new PlantingOverrideAreaRemoveEvent(value));

        }
    }
}
