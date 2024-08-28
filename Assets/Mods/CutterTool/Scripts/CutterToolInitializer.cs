using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;

namespace Mods.CutterTool.Scripts {
  public class CutterToolInitializer : ILoadableSingleton {

    private readonly UILayout _uiLayout;
    private readonly VisualElementLoader _visualElementLoader;
    private VisualElement _visualElement;

    public CutterToolInitializer(UILayout uiLayout, 
                                 VisualElementLoader visualElementLoader) {
      _uiLayout = uiLayout;
      _visualElementLoader = visualElementLoader;
    }

    public void Load() {
            _visualElement = _visualElementLoader.LoadVisualElement("CutterTool");
            _uiLayout.AddBottomLeft(_visualElement, 0);
            _visualElement.SetEnabled(false);
        }

    public void SetVisualState(bool setActive)
    {
        _visualElement.SetEnabled(setActive);
    }


  }
}