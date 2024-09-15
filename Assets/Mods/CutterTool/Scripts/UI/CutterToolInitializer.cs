using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolInitializer : ILoadableSingleton 
    {

        private readonly UILayout _uiLayout;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly EventBus _eventBus;

        private VisualElement _root;

            public CutterToolInitializer(UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._eventBus = eventBus;
        }

        public void Load()
            {
                this._root = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            this._uiLayout.AddAbsoluteItem(this._root);
            this._eventBus.Register((object)this);

            this._root.ToggleDisplayStyle(false);

            Debug.Log("CTI: Load");
        }

        public void SetVisualState(bool setActive)
        {
            this._root.ToggleDisplayStyle(setActive);
            Debug.Log("CTI: SVS: " + setActive);
        }

        [OnEvent]
        public void OnCutterToolSelectedEvent( CutterToolSelectedEvent cutterToolSelectedEvent)
        {
            if (null == cutterToolSelectedEvent)
                return;

            Debug.Log("CTI: SE");
            this.SetVisualState(true);
        }

        [OnEvent]
        public void OnCutterToolUnselectedEvent(CutterToolUnselectedEvent cutterToolUnselectedEvent )
        {
            if (null == cutterToolUnselectedEvent)
                return;
            Debug.Log("CTI: USE");
            this.SetVisualState(false);
        }
    }
}