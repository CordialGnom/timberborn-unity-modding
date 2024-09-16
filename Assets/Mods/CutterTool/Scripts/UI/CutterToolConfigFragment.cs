using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Toggles;
using Timberborn.CoreUI;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private VisualElement _root = new();

        Toggle _toggle01 = new();
        Toggle _toggle02 = new();
        Toggle _toggle03 = new();


        public CutterToolConfigFragment (UIBuilder uiBuilder)
        {
            _uiBuilder = uiBuilder;
        }

        public VisualElement InitializeFragment()
        {
            _toggle01 = _uiBuilder.Create<GameToggle>().SetLocKey("Toggle01").Build();
            _toggle02 = _uiBuilder.Create<GameToggle>().SetLocKey("Toggle02").Build();
            _toggle03 = _uiBuilder.Create<GameToggle>().SetLocKey("Toggle03").Build();

            _root = CreateCenteredPanelFragmentBuilder()
                .AddComponent(_toggle01)
                .AddComponent(_toggle02)
                .AddComponent(_toggle03)
                .BuildAndInitialize();


            _root.ToggleDisplayStyle(false);

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
    }
}
