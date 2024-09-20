using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public enum CutterPatterns
    {
        All = 0,
        Checkered, 
        LinesX,
        LinesY
    }

    public class CutterToolInitializer : ILoadableSingleton 
    {

        private readonly UILayout _uiLayout;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly EventBus _eventBus;


        readonly CutterToolConfigFragment _cutterToolConfigFragment;

        private VisualElement _root;
        private VisualElement _entityroot;

        public CutterToolInitializer( UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        CutterToolConfigFragment cutterToolConfigFragment,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._cutterToolConfigFragment = cutterToolConfigFragment;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._entityroot = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            this._entityroot.Clear();

            this._root = this._cutterToolConfigFragment.InitializeFragment();

            this._entityroot.Add(this._root);

            this._uiLayout.AddAbsoluteItem(this._entityroot);

            this._eventBus.Register((object)this);

            this._entityroot.ToggleDisplayStyle(false);
        }

        public void SetVisualState(bool setActive)
        {
            this._root.ToggleDisplayStyle(setActive);
        }

        [OnEvent]
        public void OnCutterToolSelectedEvent( CutterToolSelectedEvent cutterToolSelectedEvent)
        {
            if (null == cutterToolSelectedEvent)
                return;

            this.SetVisualState(true);
            this._entityroot.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnCutterToolUnselectedEvent(CutterToolUnselectedEvent cutterToolUnselectedEvent )
        {
            if (null == cutterToolUnselectedEvent)
                return;

            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }
    }
}