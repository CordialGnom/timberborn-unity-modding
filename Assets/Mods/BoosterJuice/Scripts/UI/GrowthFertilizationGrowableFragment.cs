// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using System.Collections.Generic;
using System.Text;
using TimberApi.DependencyContainerSystem;
using TimberApi.UIBuilderSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Forestry;
using Timberborn.Growing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{

    sealed class GrowthFertilizationGrowableFragment : IEntityPanelFragment
    {
        const string RulesAreaCaptionTextLocKey = "Cordial.Growables.GrowthFertilizationGrowableFragment.RulesAreaCaptionTextLocKey";
        const string RuleTextLocKey = "Cordial.Growables.GrowthFertilizationGrowableFragment.RuleTextLocKey";

        readonly UiFactory _uiFactory;

        private TreeComponent _growthFertilizationTreeComponent;
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private GrowthFertilizationAreaService _growthFertilizationAreaService;

        VisualElement _root = new();
        Label _caption = new();
        Label _rulesList = new();

        public GrowthFertilizationGrowableFragment(UiFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public VisualElement InitializeFragment()
        {
            this._growthFertilizationAreaService = DependencyContainer.GetInstance<GrowthFertilizationAreaService>();

            // var presets = _builder.Presets();
            // _caption = presets.Labels().Label(color: Color.cyan);
            // _rulesList = presets.Labels().GameText();
            //
            // UIFragmentBuilder uIFragmentBuilder = _builder.CreateFragmentBuilder()
            //     .AddComponent(_caption)
            //     .AddComponent(_rulesList);
            // _root = uIFragmentBuilder.BuildAndInitialize();
            // _root.ToggleDisplayStyle(visible: false);
            // return _root;
            _caption = _uiFactory.CreateLabel();
            _rulesList = _uiFactory.CreateLabel();

            _root = _uiFactory.CreateCenteredPanelFragmentBuilder()
                .AddComponent(_caption).AddComponent(_rulesList)
                .BuildAndInitialize();
            _root.ToggleDisplayStyle(visible: false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            this._growthFertilizationTreeComponent =     entity.GetComponentFast<TreeComponent>();
             
            if (this._growthFertilizationTreeComponent != null)
            {
                if (this._growthFertilizationAreaService != null)
                {
                    BlockObject blockObject = new();
                    this._growthFertilizationTreeComponent.TryGetComponentFast<BlockObject>(out blockObject);
                    this._growthFertilizationTreeComponent.TryGetComponentFast<Growable>(out Growable growable);

                    if ((blockObject != null)
                        && (growable != null))
                    {
                        if( this._growthFertilizationAreaService.CheckCoordinateFertilizationArea(blockObject.Coordinates) )
                        {
                            

                                _caption.text = "Growth influenced by fertilizer.";
                                _rulesList.text = "Daily growth increased by: " + (this._growthFertilizationAreaService.GetGrowthProgessDaily(blockObject.Coordinates).ToString("0.0") + " %");
                                _root.ToggleDisplayStyle(true);
                                return;
                        }
                    }
                    else
                    {
                        _caption.text = "Tree Growth Objects";
                    }
                }
                else
                {
                    _caption.text = "Tree Growth Default";
                }
            }
            //UpdateYielderState();
            _root.ToggleDisplayStyle(false);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(visible: false);
        }

        public void UpdateFragment()
        {
        }

        private void UpdateGrowableState(Growable growable)
        {
            if (!(bool)(Object)this._growthFertilizationTreeComponent)
                return;

            if (growable != null)
            {
                // get building from area service

                this._rulesList.text = "Growth Progress: " + growable.GrowthProgress;
            }
        }

        //private void UpdateYielderState()
        //{
        //    if (!(bool)(Object)this._growthFertilizationTreeComponent)
        //        return;
        //    this._caption.text = "Trees growing:  " + (this._growthFertilizationTreeComponent.GrowthProgress) + "/" + (this._growthFertilizationTreeComponent.TreesTotalCount);
        //}
    }
}