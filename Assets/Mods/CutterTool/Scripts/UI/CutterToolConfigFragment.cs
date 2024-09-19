using Amazon.Runtime.Internal.Transform;
using System.Collections.Generic;
using System.Collections.Immutable;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.CustomElements;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;

        private VisualElement _root = new();

        private bool _togglePanelUpdate = false;

        Label _labelTitle = new();
        Label _labelDescription = new();

        Toggle _toggleArea01 = new();
        Toggle _toggleArea02 = new();
        Toggle _toggleArea03 = new();
        Toggle _toggleArea04 = new();
        Toggle _toggleTreeAll = new();

        List<Toggle> _toggleTreeList = new();
        List<string> _toggleNameList = new();
        private readonly Dictionary<string, Toggle> _toggleTreeDict = new();

        private CutterPatterns _cutterPatterns;

        // faction / tree configuration
        CutterToolFactionSpecService _cutterToolFactionSpecService;
        private readonly EventBus _eventBus;

        public CutterPatterns CutterPatterns => _cutterPatterns;



        public CutterToolConfigFragment (UIBuilder uiBuilder,
                                         CutterToolFactionSpecService cutterToolFactionSpecService,
                                         VisualElementLoader visualElementLoader,
                                         EventBus eventBus)
        {
            _eventBus = eventBus;
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;
            _cutterToolFactionSpecService = cutterToolFactionSpecService;

        }

        public VisualElement InitializeFragment()
        {

            _toggleArea01 = _uiBuilder.Create<GameToggle>()
                .SetName("Pattern01")
                .SetLocKey("Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern01")
                .Build();

            _toggleArea02 = _uiBuilder.Create<GameToggle>().SetName("Pattern02").SetLocKey("Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern02").Build();
            _toggleArea03 = _uiBuilder.Create<GameToggle>().SetName("Pattern03").SetLocKey("Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern03").Build();
            _toggleArea04 = _uiBuilder.Create<GameToggle>().SetName("Pattern04").SetLocKey("Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern04").Build();

            // add toggle for all tree types
            _toggleTreeAll = _uiBuilder.Create<GameToggle>()
                .SetName("TreeAll")
                .SetLocKey("Cordial.CutterTool.CutterToolPanel.TreeConfig.TreeAll")
                .Build();

            // create toggle elements for all available tree types
            ImmutableArray<string> treeList = _cutterToolFactionSpecService.GetAllTrees();

            for (int index = 0; index < treeList.Length; ++index )
            {
                _toggleTreeList.Add(_uiBuilder.Create<GameTextToggle>()
                    .SetName(treeList[index])
                    .SetText(treeList[index])
                    //.SetLocKey(treeList[index-1])
                    .Build());

                _toggleTreeDict.Add(treeList[index], _toggleTreeList[index]);
            }

            // create title label
            _labelTitle = _uiBuilder.Create<GameLabel>()
                                    .SetLocKey("Cordial.CutterTool.CutterToolPanel.Title")
                                    .Title()
                                    .Build();
            _labelDescription = _uiBuilder.Create<GameLabel>().SetLocKey("Cordial.CutterTool.CutterToolPanel.Description").Small().Build();

            _root.Add(CreatePanelFragmentRedBuilder()
                             .AddComponent(_labelTitle)
                            .BuildAndInitialize());

            _root.Add(CreatePanelFragmentBlueBuilder()
                             .AddComponent(_labelDescription)
                             .BuildAndInitialize());

            _root.Add(CreateCenteredPanelFragmentBuilder()
                            .AddComponent(_toggleArea01)
                            .AddComponent(_toggleArea02)
                            .AddComponent(_toggleArea03)
                            .AddComponent(_toggleArea04)
                            .BuildAndInitialize());

            VisualElement toggleList = new();

            foreach (Toggle toggle in _toggleTreeList)
            {
                toggleList.Add(toggle);
            }

            _root.Add(CreateCenteredPanelFragmentBuilder()
                    .AddComponent(_toggleTreeAll)
                    .AddComponent(toggleList)
                    .BuildAndInitialize());

            // reset toggle name list
            _toggleNameList.Clear();

            // register all toggles to name list and callbacks
            RegisterToggleCallback(_root);

            // set default values of toggles 
            foreach (Toggle toggle in _toggleTreeList)
            {
                SendToggleUpdateEvent(toggle.name, true);
            }

            SendToggleUpdateEvent("TreeAll", true);
            SendToggleUpdateEvent("Pattern01", true);

            _root.ToggleDisplayStyle(false);

            return _root;
        }

        private void RegisterToggleCallback(VisualElement visualElement )
        {
            foreach (var child in visualElement.Children())
            {
                if ((child.GetType() == typeof(LocalizableToggle))
                    || (child.GetType() == typeof(Toggle)))
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

            switch (resourceName)
            {
                case "Pattern01":
                    _cutterPatterns = CutterPatterns.All;
                    UpdateTogglePattern(true, false, false, false);
                    break;
                case "Pattern02":
                    _cutterPatterns = CutterPatterns.Checkered;
                    UpdateTogglePattern(false, true, false, false);
                    break;
                case "Pattern03":
                    _cutterPatterns = CutterPatterns.LinesX;
                    UpdateTogglePattern(false, false, true, false);
                    break;
                case "Pattern04":
                    _cutterPatterns = CutterPatterns.LinesY;
                    UpdateTogglePattern(false, false, false, true);
                    break;
                default:    // tree type configuration
                    UpdateToggleTreeType(resourceName, value);
                    break;
            }

            // after a toggle value has changed, set event to update 
            // patter and/or tree type usage elsewhere
            this._eventBus.Post((object)new CutterToolConfigChangeEvent(this));

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
        private void UpdateTogglePattern(bool pattern01, bool pattern02, bool pattern03, bool pattern04 )
        {
            SendToggleUpdateEventWithoutNotify("Pattern01", pattern01);
            SendToggleUpdateEventWithoutNotify("Pattern02", pattern02);
            SendToggleUpdateEventWithoutNotify("Pattern03", pattern03);
            SendToggleUpdateEventWithoutNotify("Pattern04", pattern04);
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
