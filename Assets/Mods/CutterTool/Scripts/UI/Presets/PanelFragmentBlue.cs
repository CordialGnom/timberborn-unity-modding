// Timberborn Utils
// Author: igor.zavoychinskiy@gmail.com
// Reworked by cordialgnom@gmail.com for personal use
// License: Public Domain

using TimberApi.UIBuilderSystem.ElementBuilders;
using TimberApi.UIBuilderSystem.StyleSheetSystem;
using TimberApi.UIBuilderSystem.StylingElements;
using Timberborn.CoreUI;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{

    /// <summary>Fragment for the UI panel.</summary>
    public class PanelFragmentBlue : BaseElementBuilder<PanelFragmentBlue, NineSliceVisualElement>
    {
        protected override PanelFragmentBlue BuilderInstance => this;

        VisualElementBuilder _visualElementBuilder;

        const string BackgroundClass = nameof(PanelFragmentBlue);

        protected override NineSliceVisualElement InitializeRoot()
        {
            _visualElementBuilder = UIBuilder.Create<VisualElementBuilder>();
            _visualElementBuilder.AddClass(BackgroundClass);
            _visualElementBuilder.SetPadding(new Padding(new Length(12f, LengthUnit.Pixel), // top
                                                            new Length(20f, LengthUnit.Pixel), // right
                                                            new Length(12f, LengthUnit.Pixel), // bottom
                                                            new Length(20f, LengthUnit.Pixel) // left
                                                            ));
            return _visualElementBuilder.Build();
        }

        /// <inheritdoc />
        protected override void InitializeStyleSheet(StyleSheetBuilder styleSheetBuilder)
        {
            styleSheetBuilder.AddNineSlicedBackgroundClass(BackgroundClass, "ui/images/backgrounds/bg-7", 9f, 0.5f);
        }
    }


}
