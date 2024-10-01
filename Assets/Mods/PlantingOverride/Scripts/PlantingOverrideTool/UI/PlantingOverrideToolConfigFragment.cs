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

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;

        private VisualElement _root = new();

        Label _labelTitle = new();
        Label _labelDescription = new();

        Dropdown _plantingOverrideDropDown = new();

        private readonly List<Toggle> _toggleTreeList = new();
        private readonly List<Toggle> _toggleCropList = new();
        private readonly Dictionary<string, Toggle> _toggleTreeDict = new();
        private readonly Dictionary<string, Toggle> _toggleCropDict = new();


        // faction / crop configuration
        private readonly PlantingOverridePrefabSpecService _plantingOverridePrefabSpecService;
        private readonly EventBus _eventBus;


        private readonly DropdownItemsSetter _dropdownItemsSetter;
        private readonly PlantingOverrideDropDownProvider _dropDownProvider;

        public PlantingOverrideToolConfigFragment(UIBuilder uiBuilder,
                                                 PlantingOverridePrefabSpecService plantingOverridePrefabSpecService,
                                                 VisualElementLoader visualElementLoader,
                                                 DropdownItemsSetter dropdownItemsSetter,
                                                 PlantingOverrideDropDownProvider dropDownProvider,
                                                 EventBus eventBus)
        {
            _eventBus = eventBus;
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;
            _plantingOverridePrefabSpecService = plantingOverridePrefabSpecService;

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


            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_plantingOverrideDropDown)
                            .BuildAndInitialize());

            CreateTreeFragment(_root);

            CreateCropFragment(_root);

            // register all  list and callbacks
            RegisterToggleCallback(_root);

            InitializeTreeFragment();
            InitializeCropFragment();

            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);


            _root.ToggleDisplayStyle(false);

            this._eventBus.Post((object)new PlantingOverrideToolConfigChangeEvent(this));

            return _root;
        }

        
        private void RegisterToggleCallback(VisualElement visualElement )
        {
            foreach (var child in visualElement.Children())
            {
                if ((child.GetType() == typeof(LocalizableToggle))
                    //|| (child.GetType() == typeof(Toggle))     // if GameTextToggle is used
                    )
                {
                    if (child.name.Contains("Crop"))
                    {
                        _root.Q<Toggle>(child.name).RegisterValueChangedCallback(value => UpdateToggleCropType(child.name, value.newValue));
                    }
                    else // assume Tree
                    {
                        _root.Q<Toggle>(child.name).RegisterValueChangedCallback(value => UpdateToggleTreeType(child.name, value.newValue));
                    }
                }
                else
                {
                    RegisterToggleCallback(child);
                }
            }
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

        public void SetTreeFragmentState(bool state)
        {
            _dropDownProvider.ReloadAsTree();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);

            _root.Q<VisualElement>("TreePanel").ToggleDisplayStyle(state);
        }

        public void SetCropFragmentState(bool state)
        {
            _dropDownProvider.ReloadAsCrop();
            _dropdownItemsSetter.SetItems(_plantingOverrideDropDown, _dropDownProvider);
            
            
            _root.Q<VisualElement>("CropPanel").ToggleDisplayStyle(state);
        }

        public Dictionary<string, bool> GetTreeDict()
        {
            Dictionary<string, bool> dict = new();

            foreach (KeyValuePair<string, Toggle> kvp in _toggleTreeDict)
            {
                dict.Add(kvp.Key, kvp.Value.value);
            }
            return dict;
        }
        public Dictionary<string, bool> GetCropDict()
        {
            Dictionary<string, bool> dict = new();

            foreach (KeyValuePair<string, Toggle> kvp in _toggleCropDict)
            {
                dict.Add(kvp.Key, kvp.Value.value);
            }
            return dict;
        }

        private void SendToggleUpdateEvent(string name, bool newValue)
        {
            bool oldValue = _root.Q<Toggle>(name).value;

            if (oldValue != newValue)
            {
                _root.Q<Toggle>(name).value = newValue;
                //_root.Q<Toggle>(name).SendEvent(ChangeEvent<bool>.GetPooled(oldValue, newValue));
            }
        }

        private void SendToggleUpdateEventWithoutNotify(string name, bool newValue)
        {
            bool oldValue = _root.Q<Toggle>(name).value;

            if (oldValue != newValue)
            {
                _root.Q<Toggle>(name).SetValueWithoutNotify(newValue);
            }
        }
        private void CreateTreeFragment(VisualElement root)
        {
            VisualElement toggleList = new();

            // create toggle elements for all available tree types
            ImmutableArray<string> treeList = _plantingOverridePrefabSpecService.GetAllTrees();

            for (int index = 0; index < treeList.Length; ++index)
            {
                _toggleTreeList.Add(_uiBuilder.Create<GameToggle>()
                    .SetName(treeList[index])
                    //.SetText(treeList[index])
                    .SetLocKey("NaturalResource." + treeList[index] + ".DisplayName")
                    .Build());

                _toggleTreeDict.Add(treeList[index], _toggleTreeList[index]);
            }


            foreach (Toggle toggle in _toggleTreeList)
            {
                toggleList.Add(toggle);
            }

            root.Add(CreateCenteredPanelFragmentBuilder()
                    .AddComponent(toggleList)
                    .SetName("TreePanel")
                    .BuildAndInitialize());
        }

        private void InitializeTreeFragment()
        {
            // set one element as true
            SendToggleUpdateEvent(_toggleTreeList[0].name, true);
        }

        private void UpdateToggleTreeType(string name, bool value)
        {
            if (true == value)
            {
                foreach (var toggle in _toggleTreeList)
                {
                    if (toggle.name != name)
                    { 
                        SendToggleUpdateEventWithoutNotify(toggle.name, false);
                    }
                }

                // after a toggle value has changed, set event to update
                this._eventBus.Post((object)new PlantingOverrideToolConfigChangeEvent(this));
            }
            else // keep true, can't deactivate
            {
                SendToggleUpdateEventWithoutNotify(name, true);
            }
        }

        private void CreateCropFragment(VisualElement root)
        {
            VisualElement toggleList = new();

            // create toggle elements for all available crop types
            ImmutableArray<string> cropList = _plantingOverridePrefabSpecService.GetAllCrops();

            for (int index = 0; index < cropList.Length; ++index)
            {
                _toggleCropList.Add(_uiBuilder.Create<GameToggle>()
                    .SetName("Crop." + cropList[index])
                    //.SetText(cropList[index])
                    .SetLocKey("NaturalResource." + cropList[index] + ".DisplayName")
                    .Build());

                _toggleCropDict.Add(cropList[index], _toggleCropList[index]);
            }


            foreach (Toggle toggle in _toggleCropList)
            {
                toggleList.Add(toggle);
            }

            root.Add(CreateCenteredPanelFragmentBuilder()
                    .AddComponent(toggleList)
                    .SetName("CropPanel")
                    .BuildAndInitialize());
        }
        private void InitializeCropFragment()
        {
            // set one element as true
            SendToggleUpdateEvent(_toggleCropList[0].name, true);
        }

        private void UpdateToggleCropType(string name, bool value)
        {
            if (true == value)
            {
                foreach (var toggle in _toggleCropList)
                {
                    if (toggle.name != name)
                    {
                        SendToggleUpdateEventWithoutNotify(toggle.name, false);
                    }
                }
                // after a toggle value has changed, set event to update
                this._eventBus.Post((object)new PlantingOverrideToolConfigChangeEvent(this));
            }
            else // keep true, can't deactivate
            {
                SendToggleUpdateEventWithoutNotify(name, true);
            }
        }
    }
}
