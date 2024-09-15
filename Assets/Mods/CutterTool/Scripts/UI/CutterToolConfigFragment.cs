using TimberApi.UIBuilderSystem;
using Timberborn.CoreUI;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolConfigFragment
    {
        readonly UIBuilder _uiBuilder;
        private VisualElement _root = new();

        public CutterToolConfigFragment (UIBuilder uiBuilder)
        {
            _uiBuilder = uiBuilder;
        }

        public VisualElement InitializeFragment()
        {

            _root = CreateCenteredPanelFragmentBuilder().BuildAndInitialize();

            _root.ToggleDisplayStyle(false);

            return _root;
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
