// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;
using Cordial.Mods.PlantingOverride.Scripts.Common.UI;
using TimberApi.DependencyContainerSystem;
using TimberApi.UIBuilderSystem;
using TimberApi.UIPresets.Dropdowns;
using TimberApi.UIPresets.Labels;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.DropdownSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.Forestry;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.ForesterUpdate.Scripts.UI
{
    sealed class ForesterUpdateFragment : IEntityPanelFragment
    {
        UIBuilder _builder;
        Forester _forester;


        VisualElement _root = new();
        VisualElement _fragment = new();
        Label _foresterDescriptionLabel = new();
        Dropdown _foresterTreeDropDown = new();
        //Toggle _foresterStateToggle = new ();

        // store coordinates to check if "updateFragment" changes source
        Vector3Int _foresterCoordOld = Vector3Int.zero;

        DropdownItemsSetter _dropdownItemsSetter;
        DropdownListDrawer _dropdownListDrawer;
        ForesterUpdateTreeDropDownProvider _dropDownProvider;
        private readonly EventBus _eventBus;

        // localiations
        private readonly ILoc _loc;
        private static readonly string ForesterDescriptionLocKey = "Cordial.Building.Forester.Description";


        public ForesterUpdateFragment(UIBuilder uiBuilder,
                                                    ILoc loc, 
                                                    EventBus eventBus,
                                                    DropdownItemsSetter dropdownItemsSetter,
                                                    ForesterUpdateTreeDropDownProvider dropDownProvider )
        {
            _builder = uiBuilder;
            _loc = loc;

            _eventBus = eventBus;
            _dropdownItemsSetter = dropdownItemsSetter;
            _dropDownProvider = dropDownProvider;
        }

        public VisualElement InitializeFragment()
        {
            _eventBus.Register((object)this);

            _foresterDescriptionLabel =  _builder.Create<GameLabel>()
                                            .SetLocKey(ForesterDescriptionLocKey)
                                            .Small()
                                            .Build();

            _foresterTreeDropDown = _builder.Build<GameDropdown, Dropdown>();

            //_root = _builder.Create<VisualElementBuilder>()
            //                    .AddComponent<FragmentBuilder>(builder => builder.AddComponent<GameLabel>(button => button.SetLocKey(ForesterDescriptionLocKey).Small()))
            //                    //.AddComponent(_foresterDescriptionLabel)
            //                    .AddComponent(gameDropdown)
            //                    .BuildAndInitialize();

            _root.Add(CreateCenteredPanelFragmentBuilder()
                    .AddComponent(_foresterDescriptionLabel)
                    .AddComponent(_foresterTreeDropDown)
                    .BuildAndInitialize());

            _dropdownItemsSetter.SetItems(_foresterTreeDropDown, _dropDownProvider);

            _root.ToggleDisplayStyle(visible: false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            Forester forester =     entity.GetComponentFast<Forester>();

            if (null != forester)
            {
                this._forester = forester;

                _foresterCoordOld = _forester.GetComponentFast<BlockObject>().Coordinates;
                _dropDownProvider.SetValue(GetForesterState());
                _foresterTreeDropDown.RefreshContent();

                _root.ToggleDisplayStyle((bool)(Object)this._forester);
            }
            else
            {
                this._forester = null;
                _root.ToggleDisplayStyle((bool)false);
            }
            
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(visible: false);
        }

        public void UpdateFragment()
        {
            if (null != _forester)
            {
                if (_foresterCoordOld != _forester.GetComponentFast<BlockObject>().Coordinates)
                {
                    _dropDownProvider.SetValue(GetForesterState());
                    _foresterTreeDropDown.RefreshContent();
                }

                _root.ToggleDisplayStyle((bool)(Object)this._forester);
            }
        }

        public PanelFragment CreateCenteredPanelFragmentBuilder()
        {
            return _builder.Create<PanelFragment>()
                .SetFlexDirection(FlexDirection.Column)
                .SetWidth(new Length(100f, LengthUnit.Percent))
                //.SetWidth(new Length(325f, LengthUnit.Pixel))
                //.SetHeight(new Length(111f, LengthUnit.Pixel))
                .SetAlignContent(Align.Center)  // alignment --> horizontal center
                .SetJustifyContent(Justify.Center);
        }
        private void UpdateForesterState(string plantName)
        {
            if (null != _forester)
            {
                ForesterUpdateStateService updateService = DependencyContainer.GetInstance<ForesterUpdateStateService>();

                if (null != updateService)
                {
                    updateService.UpdateForester(_forester.GetComponentFast<BlockObject>().Coordinates, plantName);
                }
            }
            else
            {
                Debug.Log("No forester found to update");
            }
        }

        private string GetForesterState()
        {
            string plantname = string.Empty;

            if (null != _forester)
            {
                ForesterUpdateStateService updateService = DependencyContainer.GetInstance<ForesterUpdateStateService>();

                if (null != updateService)
                {
                    plantname = updateService.GetForesterState(_forester.GetComponentFast<BlockObject>().Coordinates);
                }
            }
            return plantname;
        }

        [OnEvent]
        public void OnForesterUpdateConfigChangeEvent(ForesterUpdateConfigChangeEvent forestUpdateConfigChangeEvent)
        {
            if (null == forestUpdateConfigChangeEvent)
                return;
            UpdateForesterState(forestUpdateConfigChangeEvent.PlantName);

        }

    }
}