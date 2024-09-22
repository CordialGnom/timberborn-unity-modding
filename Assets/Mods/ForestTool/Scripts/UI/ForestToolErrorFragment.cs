using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Builders;
using TimberApi.UIPresets.Labels;
using TimberApi.UIPresets.TextFields;
using Timberborn.Beavers;
using Timberborn.CoreUI;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolErrorFragment
    {
        readonly UIBuilder _uiBuilder;
        private readonly VisualElement _root = new();
        private readonly string ErrorLocKey = "Cordial.ForestTool.ForestToolError.Description";

        // UI elements
        Label _labelTitle = new();

        // localizations
        private readonly ILoc _loc;

        public ForestToolErrorFragment(UIBuilder uiBuilder,
                                                    ILoc loc)
        {
            _uiBuilder = uiBuilder;
            _loc = loc;
        }

        public VisualElement InitializeFragment()
        {
            _root.Add(_uiBuilder.Create<BoxBuilder>()
                        .AddCloseButton("ButtonName")
                        .SetWidth(250)
                        //.AddComponent<GameLabel>("Description")
                        .BuildAndInitialize());

            // create title label
            //_root.Q<Label>("Description").text = _loc.T(ErrorLocKey);

            _root.ToggleDisplayStyle(false);

            return _root;
        }

        public PanelFragment CreateCenteredPanelFragmentBuilder()
        {
            return _uiBuilder.Create<PanelFragment>()
                .SetFlexDirection(FlexDirection.Column)
                .SetWidth(new Length(250f, LengthUnit.Pixel))
                //.SetWidth(new Length(325f, LengthUnit.Pixel))
                //.SetHeight(new Length(111f, LengthUnit.Pixel))
                .SetJustifyContent(Justify.Center);
        }
    }
}
