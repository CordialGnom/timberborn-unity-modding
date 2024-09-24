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
        readonly ForestToolErrorPrompt _forestToolErrorPrompt;

        private VisualElement _rootConfig;
        private VisualElement _rootEntity;

        public ForestToolInitializer( UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        ForestToolConfigFragment forestToolConfigFragment,
                                        ForestToolErrorPrompt forestToolErrorPrompt,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._forestToolConfigFragment = forestToolConfigFragment;
            this._forestToolErrorPrompt = forestToolErrorPrompt;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._rootEntity = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");
            this._rootEntity.Clear();

            this._rootConfig = this._forestToolConfigFragment.InitializeFragment();
            this._rootEntity.Add(this._rootConfig);
            this._uiLayout.AddAbsoluteItem(this._rootEntity);

            this._rootEntity.ToggleDisplayStyle(false);

            this._eventBus.Register((object)this);
        }

        public void SetVisualState(bool setActive)
        {
            this._rootConfig.ToggleDisplayStyle(setActive);
        }

        [OnEvent]
        public void OnForestToolSelectedEvent( ForestToolSelectedEvent forestToolSelectedEvent)
        {
            if (null == forestToolSelectedEvent)
                return;

            if (forestToolSelectedEvent.ForestToolService.IsUnlocked)
            {
                this.SetVisualState(true);
                this._rootEntity.ToggleDisplayStyle(true);
            }
            else
            {
                _forestToolErrorPrompt.ShowLockedMessage();
            }
        }

        [OnEvent]
        public void OnForestToolUnselectedEvent(ForestToolUnselectedEvent forestToolUnselectedEvent )
        {
            if (null == forestToolUnselectedEvent)
                return;

            this.SetVisualState(false);
            this._rootEntity.ToggleDisplayStyle(false);
        }
    }
}