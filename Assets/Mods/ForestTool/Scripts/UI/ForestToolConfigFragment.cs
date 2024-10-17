using System.Collections.Generic;
using System.Collections.Immutable;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.CustomElements;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using UnityEngine.UIElements;
using Cordial.Mods.ForestTool.Scripts.UI.Events;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly VisualElement _root = new();

        // UI elements
        Label _labelTitle = new();
        Label _labelDescription = new();

        Toggle _toggleTreeAll = new();
        Toggle _toggleEmptySpots = new();

        List<Toggle> _toggleTreeList = new();
        private readonly Dictionary<string, Toggle> _toggleTreeDict = new();

        

        // faction / tree configuration
        ForestToolPrefabSpecService _forestToolPrefabSpecService;
        private readonly EventBus _eventBus;

        public bool EmptySpotsEnabled => _toggleEmptySpots.value;


        public ForestToolConfigFragment (UIBuilder uiBuilder,
                                         ForestToolPrefabSpecService forestToolPrefabSpecService,
                                         VisualElementLoader visualElementLoader,
                                         EventBus eventBus)
        {
            _eventBus = eventBus;
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;
            _forestToolPrefabSpecService = forestToolPrefabSpecService;

        }

        public VisualElement InitializeFragment()
        {

            // add toggle for to mark only tree types
            _toggleEmptySpots = _uiBuilder.Create<GameToggle>()
                .SetName("EmptySpots")
                .SetLocKey("Cordial.ForestTool.ForestToolPanel.ToggleEmpty")
                .Build();
            
            // add toggle for all tree types
            _toggleTreeAll = _uiBuilder.Create<GameToggle>()
                .SetName("TreeAll")
                .SetLocKey("Cordial.ForestTool.ForestToolPanel.ToggleTreeAll")
                .Build();

            // create toggle elements for all available tree types
            ImmutableArray<string> treeList = _forestToolPrefabSpecService.GetAllForestryPlantables();

            for (int index = 0; index < treeList.Length; ++index )
            {
                _toggleTreeList.Add(_uiBuilder.Create<GameToggle>()
                    .SetName(treeList[index])
                    .SetLocKey("NaturalResource." + treeList[index].Replace("Bush", "") + ".DisplayName")
                    .Build());

                _toggleTreeDict.Add(treeList[index], _toggleTreeList[index]);
            }

            // create title label
            _labelTitle = _uiBuilder.Create<GameLabel>()
                                    .SetLocKey("Cordial.ForestTool.ForestToolPanel.Title")
                                    .Heading()
                                    .Build();

            _labelDescription = _uiBuilder.Create<GameLabel>()
                                            .SetLocKey("Cordial.ForestTool.ForestToolPanel.Description")
                                            .Small()
                                            .Build();

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
                    .AddComponent(_toggleEmptySpots)
                    .AddComponent(_toggleTreeAll)
                    .AddComponent(toggleList)
                    .BuildAndInitialize());

            // register all toggles to name list and callbacks
            RegisterToggleCallback(_root);

            // set default values of toggles 
            foreach (Toggle toggle in _toggleTreeList)
            {
                SendToggleUpdateEventWithoutNotify(toggle.name, true);
            }

            SendToggleUpdateEventWithoutNotify("TreeAll", true);
            SendToggleUpdateEvent("EmptySpots", true);

            _root.ToggleDisplayStyle(false);

            this._eventBus.Post((object)new ForestToolConfigChangeEvent(this));

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

            switch (resourceName)
            {
                case "EmptySpots":
                    // do nothing, value handled separately, just used to trigger event
                    break;
                default:    // tree type configuration
                    UpdateToggleTreeType(resourceName, value);
                    break;
            }

            // after a toggle value has changed, set event to update 
            // pattern and/or tree type usage elsewhere
            this._eventBus.Post((object)new ForestToolConfigChangeEvent(this));
        }

        private void SendToggleUpdateEvent(string name, bool newValue)
        {
            bool oldValue = _root.Q<Toggle>(name).value;

            if (oldValue != newValue)
            {
                _root.Q<Toggle>(name).value = newValue;
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
            if (name == "TreeAll")
            {
                SendToggleUpdateEventWithoutNotify("TreeAll", value);

                if (true == value)
                {
                    foreach (var toggle in _toggleTreeList)
                    {
                        SendToggleUpdateEventWithoutNotify(toggle.name, value);
                    }
                }
            }
            else
            {
                SendToggleUpdateEventWithoutNotify("TreeAll", false);

                SendToggleUpdateEventWithoutNotify(name, value);
            }  
        }
    }
}
