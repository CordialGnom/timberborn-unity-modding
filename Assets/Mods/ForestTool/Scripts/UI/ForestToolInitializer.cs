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
        readonly ForestToolErrorFragment _forestToolErrorFragment;

        private VisualElement _rootConfig;
        private VisualElement _rootEntity;
        private VisualElement _rootError;

        public ForestToolInitializer( UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        ForestToolConfigFragment forestToolConfigFragment,
                                        ForestToolErrorFragment forestToolErrorFragment,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._forestToolConfigFragment = forestToolConfigFragment;
            this._forestToolErrorFragment = forestToolErrorFragment;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._rootEntity = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            this._rootEntity.Clear();

            this._rootConfig = this._forestToolConfigFragment.InitializeFragment();

            this._rootError = this._forestToolErrorFragment.InitializeFragment();

            this._rootEntity.Add(this._rootConfig);

            this._uiLayout.AddAbsoluteItem(this._rootEntity);
            this._uiLayout.AddAbsoluteItem(this._rootError);

            this._eventBus.Register((object)this);

            this._rootEntity.ToggleDisplayStyle(false);
            this._rootError.ToggleDisplayStyle(false);
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
                this._rootError.ToggleDisplayStyle(true);
            }
        }

        [OnEvent]
        public void OnForestToolUnselectedEvent(ForestToolUnselectedEvent forestToolUnselectedEvent )
        {
            if (null == forestToolUnselectedEvent)
                return;

            this.SetVisualState(false);
            this._rootEntity.ToggleDisplayStyle(false);
            this._rootError.ToggleDisplayStyle(false);
        }
    }
}