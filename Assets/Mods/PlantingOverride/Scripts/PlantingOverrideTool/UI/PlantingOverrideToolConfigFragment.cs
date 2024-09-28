using System.Collections.Generic;
using System.Collections.Immutable;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.CustomElements;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using UnityEngine.UIElements;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;

        private VisualElement _root = new();

        Label _labelTitle = new();
        Label _labelDescription = new();

        List<Toggle> _toggleTreeList = new();
        List<string> _toggleNameList = new();
        private readonly Dictionary<string, Toggle> _toggleTreeDict = new();

        private CutterPatterns _cutterPatterns;
        

        // faction / tree configuration
        PlantingOverridePrefabSpecService _PlantingOverridePrefabSpecService;
        private readonly EventBus _eventBus;



        public PlantingOverrideToolConfigFragment (UIBuilder uiBuilder,
                                         PlantingOverridePrefabSpecService PlantingOverridePrefabSpecService,
                                         VisualElementLoader visualElementLoader,
                                         EventBus eventBus)
        {
            _eventBus = eventBus;
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;
            _PlantingOverridePrefabSpecService = PlantingOverridePrefabSpecService;

        }

        public VisualElement InitializeFragment()
        {
            // create toggle elements for all available tree types
            ImmutableArray<string> treeList = _PlantingOverridePrefabSpecService.GetAllTrees();

            for (int index = 0; index < treeList.Length; ++index )
            {
                _toggleTreeList.Add(_uiBuilder.Create<GameToggle>()
                    .SetName(treeList[index])
                    //.SetText(treeList[index])
                    .SetLocKey("NaturalResource." + treeList[index] + ".DisplayName")
                    .Build());

                _toggleTreeDict.Add(treeList[index], _toggleTreeList[index]);
            }

            // create title label
            _labelTitle = _uiBuilder.Create<GameLabel>()
                                    .SetLocKey("Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Title")
                                    .Heading()
                                    .Build();
            _labelDescription = _uiBuilder.Create<GameLabel>().SetLocKey("Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Description").Small().Build();

            _root.Add(CreatePanelFragmentRedBuilder()
                             .AddComponent(_labelTitle)
                            .BuildAndInitialize());

            _root.Add(CreatePanelFragmentBlueBuilder()
                             .AddComponent(_labelDescription)
                             .BuildAndInitialize());

            VisualElement toggleList = new();

            foreach (Toggle toggle in _toggleTreeList)
            {
                toggleList.Add(toggle);
            }

            _root.Add(CreateCenteredPanelFragmentBuilder()
                    .AddComponent(toggleList)
                    .BuildAndInitialize());

            // reset toggle name list
            _toggleNameList.Clear();

            // register all toggles to name list and callbacks
            RegisterToggleCallback(_root);

            // set one element as true
            SendToggleUpdateEvent(_toggleTreeList[0].name, true);

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
                    _root.Q<Toggle>(child.name).RegisterValueChangedCallback(value => ToggleValueChange(child.name, value.newValue));
                    _toggleNameList.Add(child.name);   
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

        public Dictionary<string, bool> GetTreeDict()
        {
            Dictionary<string, bool> dict = new();

            foreach (KeyValuePair<string, Toggle> kvp in _toggleTreeDict)
            {
                dict.Add(kvp.Key, kvp.Value.value);
            }
            return dict;
        }

        private void ToggleValueChange(string resourceName, bool value)
        {
            UpdateToggleTreeType(resourceName, value);
            
            // after a toggle value has changed, set event to update 
            // pattern and/or tree type usage elsewhere
            this._eventBus.Post((object)new PlantingOverrideToolConfigChangeEvent(this));
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
            }
            else // keep true, can't deactivate
            {
                SendToggleUpdateEventWithoutNotify(name, true);
            }
        }
    }
}
