using System;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Forestry;
using Timberborn.ForestryUI;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Mods.CutterTool.Scripts
{
    public class CutterToolService : Tool, ILoadableSingleton, ICutterTool, IPriorityInputProcessor, IInputProcessor
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.CutterTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.CutterTool.Description";
        private static readonly string CursorKey = "CutTreeCursor";

        // tool setup
        private readonly ILoc _loc;
        private readonly ToolButtonService _toolButtonService;  // todo check if required
        private ToolDescription _toolDescription;               // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private EventBus _eventBus;

        // UI setup
        private CutterToolInitializer _cutterToolInitializer;
        //private CutterToolSettings _cutterToolSettings;

        // input handling
        private readonly InputService _inputService;        // to check keybinding and mouse state

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // cutting area
        private readonly TreeCuttingArea _treeCuttingArea;
        private readonly BlockService _blockService;


        public CutterToolService(   SelectionToolProcessorFactory selectionToolProcessorFactory,
                                    CutterToolInitializer cutterToolInitializer,
                                    //CutterToolSettings cutterToolSettings,
                                    ToolUnlockingService toolUnlockingService,
                                    Colors colors,
                                    ILoc loc,
                                    AreaHighlightingService areaHighlightingService,
                                    TerrainAreaService terrainAreaService,
                                    TreeCuttingArea treeCuttingArea,
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
            _cutterToolInitializer = cutterToolInitializer;
            _toolUnlockingService = toolUnlockingService;
            //_cutterToolSettings = cutterToolSettings;
            _terrainAreaService = terrainAreaService;
            _treeCuttingArea =  treeCuttingArea;
            _blockService = blockService;
            _inputService = inputService;
            _eventBus = eventBus;
            _colors = colors;
            _loc = loc; 

        }

        public void Load()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
        }
        public override void Enter()
        {
            // activate tool
            this._selectionToolProcessor.Enter();
            _cutterToolInitializer.SetVisualState(true);
        }
        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            _cutterToolInitializer.SetVisualState(false);
        }
        void IPriorityInputProcessor.ProcessInput()
        {
            _inputService.AddInputProcessor((IInputProcessor)this);
        }

        bool IInputProcessor.ProcessInput()
        {
            return false;
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
            bool boToggleAdd = false;
            List<Vector3Int> singleBlockList = new();

            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in inputBlocks)
            {
                boToggleAdd = !boToggleAdd;

                if (boToggleAdd)
                {
                    // add block to list for conversion to IEnumerable
                    singleBlockList.Add(block);
                    IEnumerable<Vector3Int> blocks = singleBlockList;

                    // check if the block is on the same leveled coordinates. 
                    foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                    {
                        if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                        {
                            TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(leveledCoordinate);

                            if (objectComponentAt != null)
                            {
                                string resource = objectComponentAt.name;
                                string search = "Pine";

                                int pos = resource.IndexOf(search);

                                if (0 <= pos)
                                {

                                    this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                                    this._areaHighlightingService.DrawTile(leveledCoordinate, this._colors.SelectionToolHighlight);
                                }
                                else
                                {
                                    // ignore entry, not contained
                                    this._areaHighlightingService.DrawTile(leveledCoordinate, this._colors.PriorityTileColor);
                                }
                            }
                            else
                            {
                                this._areaHighlightingService.DrawTile(leveledCoordinate, this._colors.PriorityTileColor);

                            }
                        }
                    }
                    // empty list for next block
                    singleBlockList.Clear();
                }
            }

            // highlight everything added to the service above
            this._areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            bool boToggleAdd = false;
            List<Vector3Int> coordinatesList = new();
            List<Vector3Int> singleBlockList = new();

            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                this._areaHighlightingService.UnhighlightAll();

                // iterate over all input blocks -> toggle boolean flag for it
                foreach (Vector3Int block in inputBlocks)
                {
                    boToggleAdd = !boToggleAdd;

                    if (boToggleAdd)
                    {
                        // add block to list for conversion to IEnumerable
                        singleBlockList.Add(block);
                        IEnumerable<Vector3Int> blocks = singleBlockList;

                        // check if the block is on the same leveled coordinates. 
                        foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                        {
                            if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                            {
                                TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(leveledCoordinate);

                                if (objectComponentAt != null)
                                {
                                    // todo Cord: add again when settings view is done
                                    //string resource = objectComponentAt.name;
                                    //string search = "Pine";

                                    //int pos = resource.IndexOf(search);

                                    //if (0 <= pos)
                                    //{
                                        coordinatesList.Add(leveledCoordinate);
                                    //}
                                    //else
                                    //{
                                    //    // ignore entry, not contained
                                    //}
                                }
                                else
                                {
                                    // no tree component here, ignore
                                }
                            }
                        }

                        // empty list for next block
                        singleBlockList.Clear();
                    }
                }
                IEnumerable<Vector3Int> coordinates = coordinatesList;
                this._treeCuttingArea.AddCoordinates(coordinates);
            }
        }
        private void ShowNoneCallback()
        {
            this._areaHighlightingService.UnhighlightAll();
        }
    }
}
