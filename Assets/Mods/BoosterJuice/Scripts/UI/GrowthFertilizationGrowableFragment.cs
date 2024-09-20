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
        readonly UiFactory _uiFactory;

        private TreeComponent _growthFertilizationTreeComponent;
        private GrowthFertilizationAreaService _growthFertilizationAreaService;
        private Vector3Int _growableCoordinates;

        VisualElement _root = new();
        Label _title = new();
        Label _growthDailyInfo = new();
        Label _growthAvgInfo = new();

        public GrowthFertilizationGrowableFragment(UiFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public VisualElement InitializeFragment()
        {
            this._growthFertilizationAreaService = DependencyContainer.GetInstance<GrowthFertilizationAreaService>();
            this._growableCoordinates = Vector3Int.zero;
            // var presets = _builder.Presets();
            // _title = presets.Labels().Label(color: Color.cyan);
            // _growthDailyInfo = presets.Labels().GameText();
            //
            // UIFragmentBuilder uIFragmentBuilder = _builder.CreateFragmentBuilder()
            //     .AddComponent(_title)
            //     .AddComponent(_growthDailyInfo);
            // _root = uIFragmentBuilder.BuildAndInitialize();
            // _root.ToggleDisplayStyle(visible: false);
            // return _root;
            _title = _uiFactory.CreateLabel();
            _growthDailyInfo = _uiFactory.CreateLabel();
            _growthAvgInfo = _uiFactory.CreateLabel();

            _root = _uiFactory.CreateCenteredPanelFragmentBuilder()
                .AddComponent(_title)
                .AddComponent(_growthDailyInfo)
                .AddComponent(_growthAvgInfo)
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
                            this._growableCoordinates = blockObject.Coordinates;

                            _title.text = "Growth influenced by fertilizer.";

                            if (!growable.IsGrown)
                            {
                                _growthDailyInfo.visible = true;
                                _growthAvgInfo.visible = true;

                                _growthDailyInfo.text = "Daily growth increased by: " + (this._growthFertilizationAreaService.GetGrowthProgessDaily(blockObject.Coordinates).ToString("0.0") + " %");

                                // calculate average growth reduction
                                float growthTimeDescrease = (growable.GrowthTimeInDays * this._growthFertilizationAreaService.GetGrowthFactor() * (this._growthFertilizationAreaService.GetGrowthProgessAverage(blockObject.Coordinates) / 100.0f));

                                _growthAvgInfo.text = ("Average growth decreased by: " + growthTimeDescrease.ToString("0.0") + " days");
                            }
                            else
                            {
                                _growthDailyInfo.visible = false;
                                _growthAvgInfo.visible = false;
                            }
                            _root.ToggleDisplayStyle((bool)(Object)this._growthFertilizationTreeComponent);
                            return;
                        }
                        else
                        {
                            this._growableCoordinates = Vector3Int.zero;
                        }
                    }
                }
            }
            _root.ToggleDisplayStyle(false);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(visible: false);
        }

        public void UpdateFragment()
        {
            if ((this._growthDailyInfo != null)
                && (this._growthFertilizationTreeComponent != null)
                && (this._growthFertilizationAreaService != null)
                && (this._growableCoordinates != Vector3Int.zero)
                )
            {
                this._growthDailyInfo.text = "Daily growth increased by: " + (this._growthFertilizationAreaService.GetGrowthProgessDaily(this._growableCoordinates).ToString("0.0") + " %");
                _root.ToggleDisplayStyle((bool)(Object)this._growthFertilizationTreeComponent);
            }
        }

        private void UpdateGrowableState(Growable growable)
        {
            if (!(bool)(Object)this._growthFertilizationTreeComponent)
                return;

            if (growable != null)
            {
                // get building from area service
                this._growthDailyInfo.text = "Growth Progress: " + growable.GrowthProgress;
            }
        }
    }
}