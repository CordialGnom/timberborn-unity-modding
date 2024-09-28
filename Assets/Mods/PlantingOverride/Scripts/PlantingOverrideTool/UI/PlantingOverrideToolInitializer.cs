using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideToolInitializer : ILoadableSingleton 
    {

        private readonly UILayout _uiLayout;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly EventBus _eventBus;


        readonly PlantingOverrideToolConfigFragment _PlantingOverrideToolConfigFragment;

        private VisualElement _root;
        private VisualElement _entityroot;

        public PlantingOverrideToolInitializer( UILayout uiLayout, 
                                        VisualElementLoader visualElementLoader,
                                        PlantingOverrideToolConfigFragment PlantingOverrideToolConfigFragment,
                                        EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._PlantingOverrideToolConfigFragment = PlantingOverrideToolConfigFragment;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._entityroot = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            this._entityroot.Clear();

            this._root = this._PlantingOverrideToolConfigFragment.InitializeFragment();

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
        public void OnPlantingOverrideToolSelectedEvent( PlantingOverrideToolSelectedEvent PlantingOverrideToolSelectedEvent)
        {
            if (null == PlantingOverrideToolSelectedEvent)
                return;

            this.SetVisualState(true);
            this._entityroot.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnPlantingOverrideToolUnselectedEvent(PlantingOverrideToolUnselectedEvent PlantingOverrideToolUnselectedEvent )
        {
            if (null == PlantingOverrideToolUnselectedEvent)
                return;

            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }
    }
}