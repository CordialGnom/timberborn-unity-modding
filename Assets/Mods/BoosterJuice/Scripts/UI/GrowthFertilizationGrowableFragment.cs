// Based on the: 
// Timberborn Mod: Automation
// Author: igor.zavoychinskiy@gmail.com
// License: Public Domain

using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Forestry;
using Timberborn.Gathering;
using Timberborn.Growing;
using Timberborn.Localization;
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


        private VisualElement _root = new();
        private VisualElement _growthElement = new();
        private VisualElement _yieldElement = new();

        private Label _title = new();
        private Label _growthDailyInfo = new();
        private Label _growthAvgInfo = new();
        private Label _yieldAvgInfo = new();

        // localiations
        // _loc.T(DescriptionLocKey)
        private readonly ILoc _loc;
        private static readonly string TitleLocKey = "Cordial.TreeFragment.Title";
        private static readonly string GrowthDailyLocKey = "Cordial.TreeFragment.GrowthDaily";
        private static readonly string GrowthAvgLocKey = "Cordial.TreeFragment.GrowthAverage";
        private static readonly string YieldAvgLocKey = "Cordial.TreeFragment.YieldAverage";
        private static readonly string UnitDayLocKey = "Cordial.Unit.Days";

        public GrowthFertilizationGrowableFragment( UiFactory uiFactory,
                                                    ILoc loc)
        {
            _uiFactory = uiFactory;
            _loc = loc;
        }

        public VisualElement InitializeFragment()
        {
            this._growthFertilizationAreaService = DependencyContainer.GetInstance<GrowthFertilizationAreaService>();
            this._growableCoordinates = Vector3Int.zero;


            _title = _uiFactory.CreateLabel();
            _growthDailyInfo = _uiFactory.CreateLabel();
            _growthAvgInfo = _uiFactory.CreateLabel();
            _yieldAvgInfo = _uiFactory.CreateLabel();

            _growthElement.Add(_growthDailyInfo);
            _growthElement.Add(_growthAvgInfo);

            _yieldElement.Add(_yieldAvgInfo);

            _root = _uiFactory.CreateCenteredPanelFragmentBuilder()
                                .AddComponent(_title)
                                .AddComponent(_growthElement)
                                .AddComponent(_yieldElement)
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
                    this._growthFertilizationTreeComponent.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower);

                    if ((blockObject != null)
                        && (growable != null))
                    {
                        if( this._growthFertilizationAreaService.CheckCoordinateFertilizationArea(blockObject.Coordinates) )
                        {
                            this._growableCoordinates = blockObject.Coordinates;

                            _title.text = _loc.T(TitleLocKey);

                            if (!growable.IsGrown)
                            {
                                _growthElement.ToggleDisplayStyle(true);
                                _yieldElement.ToggleDisplayStyle(false);

                                _growthDailyInfo.text = _loc.T(GrowthDailyLocKey) + " " + (this._growthFertilizationAreaService.GetGrowthProgessDaily(blockObject.Coordinates).ToString("0.0") + " %");

                                // calculate average growth reduction
                                float growthTimeDecrease = (growable.GrowthTimeInDays * this._growthFertilizationAreaService.GetGrowthFactor() * this._growthFertilizationAreaService.GetGrowthProgessAverage(blockObject.Coordinates));

                                _growthAvgInfo.text = (_loc.T(GrowthAvgLocKey) + " " + growthTimeDecrease.ToString("0.0") + " " + _loc.T(UnitDayLocKey));
                            }
                            else
                            {
                                if (yieldGrower != null)
                                {
                                    this._growthFertilizationTreeComponent.TryGetComponentFast<Gatherable>(out Gatherable gatherable);

                                    float yieldTimeDecrease = (gatherable.YieldGrowthTimeInDays * this._growthFertilizationAreaService.GetYieldFactor() * (this._growthFertilizationAreaService.GetYieldProgessAverage(blockObject.Coordinates)/100.0f));

                                    _yieldAvgInfo.text = (_loc.T(YieldAvgLocKey) + " " + yieldTimeDecrease.ToString("0.0") + " " + _loc.T(UnitDayLocKey));

                                    _yieldElement.ToggleDisplayStyle(true);
                                }

                                _growthElement.ToggleDisplayStyle(false);
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
                this._growthDailyInfo.text = _loc.T(GrowthDailyLocKey) + " " + (this._growthFertilizationAreaService.GetGrowthProgessDaily(this._growableCoordinates).ToString("0.0") + " %");
                _root.ToggleDisplayStyle((bool)(Object)this._growthFertilizationTreeComponent);
            }
        }
    }
}