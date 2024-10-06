using System;
using System.Collections.Generic;
using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverrideTool.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Fields;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.PlantingOverrideTool.Scripts
{
    public class PlantingOverrideCropService : Tool, ILoadableSingleton, IPlantingOverrideCropTool
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.PlantingOverrideTool.Crop.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.Crop.Description";
        private static readonly string CursorKey = "PlantingCursor";

        // tool setup
        private readonly ILoc _loc;
        private ToolDescription _toolDescription;               // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly EventBus _eventBus;


        // configuration
        private readonly List<string> _cropTypesActive = new();
        private readonly PlantingOverridePrefabSpecService _specService;

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private readonly PlantingService _plantingService;
        private readonly BlockService _blockService;


        public PlantingOverrideCropService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            PlantingOverridePrefabSpecService specService,
                                            PlantingService plantingService,
                                            ToolUnlockingService toolUnlockingService,
                                            Colors colors,
                                            ILoc loc,
                                            AreaHighlightingService areaHighlightingService,
                                            TerrainAreaService terrainAreaService,
                                            BlockService blockService,
                                            EventBus eventBus) 
        {
            _selectionToolProcessor = selectionToolProcessorFactory.Create(new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.PreviewCallback),
                                                                                    new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.ActionCallback),
                                                                                    new Action(ShowNoneCallback),
                                                                                    CursorKey);

            _areaHighlightingService = areaHighlightingService;
            _terrainAreaService = terrainAreaService;
            _specService = specService;
            _toolUnlockingService = toolUnlockingService;

            _plantingService = plantingService;
            _blockService = blockService;
            _eventBus = eventBus;
            _colors = colors;
            _loc = loc; 
        }

        public void Load()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
            this._eventBus.Register((object)this);
        }
        public override void Enter()
        {
            // activate tool
            this._selectionToolProcessor.Enter();
            this._eventBus.Post((object)new PlantingOverrideCropSelectedEvent(this));
        }
        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            this._eventBus.Post((object)new PlantingOverrideCropUnselectedEvent(this));
        }

        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }
        public override ToolDescription Description() => _toolDescription;

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {

            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                Crop objectComponentAt = this._blockService.GetBottomObjectComponentAt<Crop>(block);

                if (objectComponentAt != null)
                {
                   this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                   this._areaHighlightingService.DrawTile(block, this._colors.PlantingToolTile);
                }
                else
                {
                    this._areaHighlightingService.DrawTile(block, this._colors.PriorityTileColor);
                }
            }
                
            // highlight everything added to the service above
            this._areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                this._areaHighlightingService.UnhighlightAll();

                foreach (Vector3Int block in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
                {                
                    Crop objectComponentAt = this._blockService.GetBottomObjectComponentAt<Crop>(block);

                    if (objectComponentAt != null)
                    {
                        if ((_cropTypesActive.Count == 1)
                           && (_specService.VerifyPrefabName(_cropTypesActive[0])))
                        {
                            _plantingService.SetPlantingCoordinates(block, _cropTypesActive[0]);
                        }
                        else
                        {
                            Debug.Log("PO: Unknown Crop");
                        }
                    }
                    else
                    {
                        // no tree component here, ignore
                    }
                }
            }
        }

        private void ShowNoneCallback()
        {
            this._areaHighlightingService.UnhighlightAll();
        }

        [OnEvent]
        public void OnPlantingOverrideConfigChangeEvent(PlantingOverrideConfigChangeEvent PlantingOverrideConfigChangeEvent)
        {
            if (null == PlantingOverrideConfigChangeEvent)
                return;

            if (!PlantingOverrideConfigChangeEvent.IsTree)
            {
                _cropTypesActive.Clear();

                string plantName = PlantingOverrideConfigChangeEvent.PlantName.Replace(" ", "");
                _cropTypesActive.Add(plantName);
            }
        }
    }
}
