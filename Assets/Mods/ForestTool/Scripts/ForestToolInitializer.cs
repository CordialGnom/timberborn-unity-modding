using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;

namespace Mods.ForestTool.Scripts {
  public class ForestToolInitializer : ILoadableSingleton{

    private readonly UILayout _uiLayout;
    private readonly VisualElementLoader _visualElementLoader;

    private VisualElement _visualElementErrorUI = null;
    private VisualElement _visualElementConfigUI = null;


    public ForestToolInitializer( UILayout uiLayout, 
                                  VisualElementLoader visualElementLoader) 
    {
        _uiLayout = uiLayout;
        _visualElementLoader = visualElementLoader;
    }

    public void Load() 
    {
            // store items
            _visualElementErrorUI = _visualElementLoader.LoadVisualElement("ForestToolErrorUi");
            _visualElementConfigUI = _visualElementLoader.LoadVisualElement("ForestToolConfigUi");
    }

    public VisualElement GetErrorUiElement()
    {
        return _visualElementErrorUI;
    }
    public VisualElement GetConfigUiElement()
    {
        return _visualElementConfigUI;
    }
    }
}