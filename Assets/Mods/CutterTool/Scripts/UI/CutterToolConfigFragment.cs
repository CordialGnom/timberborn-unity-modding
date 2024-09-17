using System.Collections.Generic;
using TimberApi.UIBuilderSystem;
using TimberApi.UIBuilderSystem.CustomElements;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.Toggles;
using Timberborn.Beavers;
using Timberborn.CoreUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElementLoader _visualElementLoader;

        private VisualElement _root = new();

        Label _labelTitle = new();
        Label _labelDescription = new();

        Toggle _toggleArea01 = new();
        Toggle _toggleArea02 = new();
        Toggle _toggleArea03 = new();
        Toggle _toggleArea04 = new();
        Toggle _toggleTreeAll = new();

        List<Toggle> _toggleTreeList = new();




        Toggle _toggle06 = new();


        public CutterToolConfigFragment (UIBuilder uiBuilder,
            VisualElementLoader visualElementLoader)
        {
            _uiBuilder = uiBuilder;
            _visualElementLoader = visualElementLoader;
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

            // get faction access and available tree types
            for (int i = 0; i < 4; ++i)
            {
                string resourceName = "TreeType0" + i.ToString();
                _toggleTreeList.Add(_uiBuilder.Create<GameToggle>()
                    .SetName(resourceName)
                    .SetLocKey(resourceName)
                    .Build());
            }

            // create title label
            _labelTitle = _uiBuilder.Create<GameLabel>()
                .SetLocKey("Cordial.CutterTool.CutterToolPanel.Title")
                .Title()
                .Build();
            _labelDescription = _uiBuilder.Create<GameLabel>().SetLocKey("Cordial.CutterTool.CutterToolPanel.Description").Small().Build();

            //menu.AddPreset(factory => factory.Labels().DefaultText("Cordial.TreeTool.TreeToolPanel.PanelDescription", builder: builder => builder.SetStyle(style => { style.alignSelf = Align.Center; style.marginBottom = new Length(10); })));


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

            RegisterToggleCallback(_root);

            SendToggleUpdateEvent("Pattern01", true);
            SendToggleUpdateEvent("TreeAll", true);

            //.AddComponent(_toggleTreeList)
            //.AddComponent(_toggle05)
            //.AddComponent(_toggle06)
            //.BuildAndInitialize());


            _root.ToggleDisplayStyle(false);

            return _root;
        }

        private void RegisterToggleCallback(VisualElement visualElement )
        {
            foreach (var child in visualElement.Children())
            {
                Debug.Log("RTC: " + child.name + " - " + child.GetType().Name );

                if (child.GetType() == typeof(LocalizableToggle))
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

        private void ToggleValueChange(string resourceName, bool value)
        {
            // Do some action when toggle changed value
            //for (int index = 0; index < _resourceNames.Count; ++index)
            //{
            //    if (resourceName == _resourceNames[index])
            //    {
            //        _LocalCheckbox[index] = value;
            //    }
            //}

            Debug.Log("TVC: " + resourceName + " -- " + value.ToString());
        }

        private void SendToggleUpdateEvent(string name, bool newValue)
        {
            bool oldValue = _root.Q<Toggle>(name).value;


            Debug.Log("STUE: " + name + " - " + oldValue.ToString() + " - " + newValue.ToString());

            if (oldValue != newValue)
            {
                _root.Q<Toggle>(name).value = newValue;
                _root.Q<Toggle>(name).SendEvent(ChangeEvent<bool>.GetPooled(oldValue, newValue));
            }
        }
    }
}
