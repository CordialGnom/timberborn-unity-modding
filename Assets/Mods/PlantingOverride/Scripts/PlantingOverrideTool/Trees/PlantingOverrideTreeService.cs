using System;
using System.Collections.Generic;
using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;
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
        private static readonly string TitleLocKey = "Cordial.PlantingOverrideTool.Tree.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.Tree.Description";
        private static readonly string CursorKey = "PlantingCursor";

        // tool setup
        private readonly ILoc _loc;
        private ToolDescription _toolDescription;               // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly EventBus _eventBus;

        // configuration
        private readonly List<string> _treeTypesActive = new();

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private readonly PlantingService _plantingService;
        private readonly BlockService _blockService;


        public PlantingOverrideTreeService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            PlantingService plantingService,
                                            ToolUnlockingService toolUnlockingService,
                                            Colors colors,
                                            ILoc loc,
                                            AreaHighlightingService areaHighlightingService,
                                            TerrainAreaService terrainAreaService,
                                            BlockService blockService,
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
            _terrainAreaService = terrainAreaService;
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

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {

            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

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
                    TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                    if (objectComponentAt != null)
                    {
                        if (_treeTypesActive.Count == 1)
                        {
                            _plantingService.SetPlantingCoordinates(block, _treeTypesActive[0]);
                        }
                        else
                        {
                            Debug.Log("PO: Incorrect tree count");
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

            if (PlantingOverrideConfigChangeEvent.IsTree)
            {
                _treeTypesActive.Clear();

                string plantName = PlantingOverrideConfigChangeEvent.PlantName.Replace(" ", "");
                _treeTypesActive.Add(plantName);
            }
        }
    }
}
