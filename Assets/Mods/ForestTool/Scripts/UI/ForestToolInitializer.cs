using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;
using Cordial.Mods.ForestTool.Scripts.UI.Events;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolInitializer : ILoadableSingleton 
    {

        private readonly UILayout _uiLayout;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly EventBus _eventBus;


        readonly ForestToolConfigFragment _forestToolConfigFragment;

        private VisualElement _root;
        private VisualElement _entityroot;

        public ForestToolInitializer( UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        ForestToolConfigFragment forestToolConfigFragment,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._forestToolConfigFragment = forestToolConfigFragment;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._entityroot = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            this._entityroot.Clear();

            this._root = this._forestToolConfigFragment.InitializeFragment();

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
        public void OnForestToolSelectedEvent( ForestToolSelectedEvent forestToolSelectedEvent)
        {
            if (null == forestToolSelectedEvent)
                return;

            this.SetVisualState(true);
            this._entityroot.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnForestToolUnselectedEvent(ForestToolUnselectedEvent forestToolUnselectedEvent )
        {
            if (null == forestToolUnselectedEvent)
                return;

            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }
    }
}