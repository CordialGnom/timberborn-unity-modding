using System;
using System.Collections.Generic;
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
        private static readonly string TitleLocKey = "Cordial.PlantingOverrideTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.Description";
        private static readonly string CursorKey = "PlantingCursor";

        // tool setup
        private readonly ILoc _loc;
        private readonly ToolButtonService _toolButtonService;  // todo check if required
        private ToolDescription _toolDescription;               // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private EventBus _eventBus;

        // UI setup
        private PlantingOverrideToolInitializer _PlantingOverrideToolInitializer;

        // configuration
        private Dictionary<string, bool> _toggleCropDict = new();
        private List<string> _cropTypesActive = new();

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private PlantingService _plantingService;

        // cutting area
        //private readonly TreeCuttingArea _treeCuttingArea;
        private readonly BlockService _blockService;


        public PlantingOverrideCropService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            PlantingOverrideToolInitializer PlantingOverrideToolInitializer,
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

            _PlantingOverrideToolInitializer = PlantingOverrideToolInitializer;
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

        public void PostProcessInput()      // originally virtual (to be called elsewhere)
        {
            // currently no implementation, just placeholder
            return;
        }
        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {

            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                Crop objectComponentAt = this._blockService.GetBottomObjectComponentAt<Crop>(block);

                if (objectComponentAt != null)
                {
                   this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                   this._areaHighlightingService.DrawTile(block, this._colors.SelectionToolHighlight);
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
            List<Vector3Int> coordinatesList = new();

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
                        if (_cropTypesActive.Count == 1)
                        {
                            _plantingService.SetPlantingCoordinates(block, _cropTypesActive[0]);
                        }
                        else
                        {
                            Debug.Log("Incorrect tree count");
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
        public void OnPlantingOverrideToolConfigChangeEvent(PlantingOverrideToolConfigChangeEvent PlantingOverrideToolConfigChangeEvent)
        {
            if (null == PlantingOverrideToolConfigChangeEvent)
                return;

            _toggleCropDict = PlantingOverrideToolConfigChangeEvent.PlantingOverrideToolConfig.GetCropDict();
            _cropTypesActive.Clear();

            foreach (KeyValuePair<string, bool> kvp in _toggleCropDict)
            {
                if (kvp.Value)
                {
                    _cropTypesActive.Add(kvp.Key);
                }
            }
        }
    }
}
