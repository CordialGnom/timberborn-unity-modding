using System;
using System.Collections.Generic;
using Cordial.Mods.PlantingOverrideTool.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Forestry;
using Timberborn.InputSystem;
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
    public class PlantingOverrideTreeService : Tool, ILoadableSingleton, IPlantingOverrideTreeTool
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

        // configuration
        private Dictionary<string, bool> _toggleTreeDict = new();
        private List<string> _treeTypesActive = new();

        // input handling
        private readonly InputService _inputService;        // to check keybinding and mouse state

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private PlantingService _plantingService;

        // cutting area
        //private readonly TreeCuttingArea _treeCuttingArea;
        private readonly BlockService _blockService;


        public PlantingOverrideTreeService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            PlantingOverrideToolInitializer PlantingOverrideToolInitializer,
                                            //PlantingOverrideToolSettings PlantingOverrideToolSettings,
                                            PlantingService plantingService,
                                            ToolUnlockingService toolUnlockingService,
                                            Colors colors,
                                            ILoc loc,
                                            AreaHighlightingService areaHighlightingService,
                                            TerrainAreaService terrainAreaService,
                                            //TreeCuttingArea treeCuttingArea,
                                            BlockService blockService,
                                            InputService inputService,
                                            EventBus eventBus ) 
        {
            _selectionToolProcessor = selectionToolProcessorFactory.Create(new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.PreviewCallback),
                                                                                    new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.ActionCallback),
                                                                                    new Action(ShowNoneCallback),
                                                                                    CursorKey);

            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService = toolUnlockingService;
            //_PlantingOverrideToolSettings = PlantingOverrideToolSettings;
            _terrainAreaService = terrainAreaService;
            //_treeCuttingArea =  treeCuttingArea;
            _plantingService = plantingService;
            _blockService = blockService;
            _inputService = inputService;
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
            this._eventBus.Post((object)new PlantingOverrideTreeSelectedEvent(this) );
        }
        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            this._eventBus.Post((object)new PlantingOverrideTreeUnselectedEvent(this));
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
                TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

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
                    TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                    if (objectComponentAt != null)
                    {
                        if (_treeTypesActive.Count == 1)
                        {
                            _plantingService.SetPlantingCoordinates(block, _treeTypesActive[0]);
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

            _toggleTreeDict = PlantingOverrideToolConfigChangeEvent.PlantingOverrideToolConfig.GetTreeDict();

            _treeTypesActive.Clear();

            foreach (KeyValuePair<string, bool> kvp in _toggleTreeDict)
            {
                if (kvp.Value)
                {
                    _treeTypesActive.Add(kvp.Key);
                }
            }
        }
    }
}
