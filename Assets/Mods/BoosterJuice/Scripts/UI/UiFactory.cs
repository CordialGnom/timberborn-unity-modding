// Timberborn Utils
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using System;
using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Sliders;
using TimberApi.UIPresets.Toggles;
using Timberborn.CoreUI;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{ 

/// <summary>Factory for making standard fragment panel elements.</summary>
public class UiFactory {
  readonly VisualElementLoader _visualElementLoader;
  readonly UIBuilder _uiBuilder;

  /// <summary>The localization service to use for the UI elements.</summary>
  public ILoc Loc { get; }

  UiFactory(VisualElementLoader visualElementLoader, UIBuilder uiBuilder, ILoc loc) {
    _visualElementLoader = visualElementLoader;
    _uiBuilder = uiBuilder;
    Loc = loc;
  }

  /// <summary>Creates a slider in a theme suitable for the right side panel.</summary>
  /// <remarks>
  /// TAPI offers the sliders builder, but in the recent updates it got broken. Use this factory as a quick workaround.
  /// </remarks>
  /// <param name="onValueChangedFn">
  /// A callback method that will be called on the value change. The only argument is the new value.
  /// </param>
  /// <param name="stepSize">
  /// The minimum delta for the value changes. All positions on the slider will be multiples of this value.
  /// </param>
  /// <param name="lowValue">The lowest possible value.</param>
  /// <param name="highValue">The highest possible value.</param>
  public Slider CreateSlider(
      Action<ChangeEvent<float>> onValueChangedFn, float lowValue, float highValue, float stepSize = 0) {
    var slider = _uiBuilder.Create<GameTextSlider>().Small().Build();
    slider.lowValue = lowValue;
    slider.highValue = highValue;
    slider.RegisterValueChangedCallback(
        evt => {
          if (stepSize > 0) {
            var newValue = Mathf.Round(slider.value / stepSize) * stepSize;
            slider.SetValueWithoutNotify(newValue);
            evt = ChangeEvent<float>.GetPooled(evt.previousValue, newValue);
          }
          onValueChangedFn(evt);
        });
    return slider;
  }

  /// <summary>Creates a min/max slider in a theme suitable for the right side panel.</summary>
  /// <param name="onValueChangedFn">A callback method that will be called on the value change.</param>
  /// <param name="lowValue">The minimum value limit.</param>
  /// <param name="highValue">The maximum value limit.</param>
  /// <param name="minDelta">The minimum delta between min/max values.</param>
  /// <param name="stepSize">If greater than zero, then the values are rounded to the step.</param>
  public MinMaxSlider CreateMinMaxSlider(Action<ChangeEvent<Vector2>> onValueChangedFn, float lowValue, float highValue,
                                         float minDelta, float stepSize = 0) {
    var slider = _uiBuilder.Create<GameTextMinMaxSlider>()
        .SetLowLimit(lowValue)
        .SetHighLimit(highValue)
        .Small().Build();
    slider.RegisterValueChangedCallback(
        evt => {
          var newValue = evt.newValue;
          if (stepSize > 0) {
            newValue = new Vector2(
                Mathf.Round(evt.newValue.x / stepSize) * stepSize, Mathf.Round(evt.newValue.y / stepSize) * stepSize);
          }
          if (newValue.y - newValue.x < minDelta) {
            if (Math.Abs(evt.previousValue.x - newValue.x) < float.Epsilon) {
              newValue.y = newValue.x + minDelta;
            } else {
              newValue.x = newValue.y - minDelta;
            }
          }
          slider.SetValueWithoutNotify(newValue);
          evt = ChangeEvent<Vector2>.GetPooled(evt.previousValue, newValue);
          onValueChangedFn(evt);
        });
    return slider;
  }

  /// <summary>Creates a toggle in a theme suitable for the right side panel.</summary>
  /// <param name="locKey">Loc key for the caption.</param>
  /// <param name="onValueChangedFn">
  /// A callback method that will be called on the value change. The only argument is the new value.
  /// </param>
  public Toggle CreateToggle(string locKey, Action<ChangeEvent<bool>> onValueChangedFn) {
    var toggle = _uiBuilder.Create<GameToggle>().SetLocKey(locKey).Build();
    toggle.RegisterValueChangedCallback(evt => onValueChangedFn(evt));
    return toggle;
  }

  /// <summary>Creates a label in a theme suitable for the right side panel.</summary>
  /// <param name="locKey">Optional loc key for the caption.</param>
  public Label CreateLabel(string locKey = null) {
    var label = _visualElementLoader.LoadVisualElement("Game/EntityPanel/MechanicalNodeFragment").Q<Label>("Generator");
    label.text = locKey != null ? Loc.T(locKey) : "";
    return label;
  }

  /// <summary>Creates a panel builder that can be used as a fragment in the right side panel.</summary>
  /// <remarks>
  /// This is a root element for the fragment's panel. Add controls to it via
  /// <see cref="PanelFragmentBuilder{PanelFragment}.AddComponent(UnityEngine.UIElements.VisualElement)"/>.
  /// </remarks>
  public PanelFragment CreateCenteredPanelFragmentBuilder() {
    return _uiBuilder.Create<PanelFragment>()
        .SetFlexDirection(FlexDirection.Column)
        .SetWidth(new Length(100f, LengthUnit.Percent))
        .SetJustifyContent(Justify.Center);
  }
}

}
