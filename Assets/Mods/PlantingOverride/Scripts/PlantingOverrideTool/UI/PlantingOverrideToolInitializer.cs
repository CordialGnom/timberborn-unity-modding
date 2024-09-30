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
            this.SetTreeConfigState(false);
            this.SetCropConfigState(false);
        }

        public void SetVisualState(bool setActive)
        {
            this._root.ToggleDisplayStyle(setActive);
        }

        private void SetTreeConfigState(bool setActive)
        {
            this._PlantingOverrideToolConfigFragment.SetTreeFragmentState(setActive);
        }
        private void SetCropConfigState(bool setActive)
        {
            this._PlantingOverrideToolConfigFragment.SetCropFragmentState(setActive);
        }

        [OnEvent]
        public void OnPlantingOverrideTreeSelectedEvent( PlantingOverrideTreeSelectedEvent PlantingOverrideToolSelectedEvent)
        {
            if (null == PlantingOverrideToolSelectedEvent)
                return;
            this.SetTreeConfigState(true);
            this.SetVisualState(true);
            this._entityroot.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnPlantingOverrideTreeUnselectedEvent(PlantingOverrideTreeUnselectedEvent PlantingOverrideToolUnselectedEvent )
        {
            if (null == PlantingOverrideToolUnselectedEvent)
                return;

            this.SetTreeConfigState(false);
            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }

        [OnEvent]
        public void OnPlantingOverrideCropSelectedEvent(PlantingOverrideCropSelectedEvent PlantingOverrideToolSelectedEvent)
        {
            if (null == PlantingOverrideToolSelectedEvent)
                return;

            this.SetCropConfigState(true);
            this.SetVisualState(true);
            this._entityroot.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnPlantingOverrideCropUnselectedEvent(PlantingOverrideCropUnselectedEvent PlantingOverrideToolUnselectedEvent)
        {
            if (null == PlantingOverrideToolUnselectedEvent)
                return;

            this.SetCropConfigState(false);
            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }
    }
}