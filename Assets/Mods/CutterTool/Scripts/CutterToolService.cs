using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.CutterTool.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Forestry;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts
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
        //private CutterToolConfigPanel _cutterToolConfigPanel;
        //private CutterToolSettings _cutterToolSettings;

        // configuration
        private Dictionary<string, bool> _toggleTreeDict = new();
        private CutterPatterns _cutterPatterns;
        private List<string> _treeTypesActive = new();
        private bool _treeMarkOnly = false;

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
            this._eventBus.Register((object)this);
        }
        public override void Enter()
        {
            // activate tool
            this._selectionToolProcessor.Enter();
            this._eventBus.Post((object)new CutterToolSelectedEvent(this) );
        }
        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            this._eventBus.Post((object)new CutterToolUnselectedEvent(this));
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
            IEnumerable<Vector3Int> patternBlocks = GetPatternCoordinates(inputBlocks, ray);

            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in patternBlocks)
            {
                TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                if (objectComponentAt != null)
                {
                    string treeName = objectComponentAt.name;
                    treeName = treeName.Replace("(Clone)", "");
                    treeName = treeName.Replace(" ", "");

                    if (_treeTypesActive.Contains(treeName))
                    {
                        this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                        this._areaHighlightingService.DrawTile(block, this._colors.SelectionToolHighlight);
                    }
                    else
                    {
                        // ignore entry, not contained
                        this._areaHighlightingService.DrawTile(block, this._colors.PriorityTileColor);
                    }
                }
                // no tree, yet marking enabled
                else if (_treeMarkOnly == false)
                {
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

                IEnumerable<Vector3Int> patternBlocks = GetPatternCoordinates(inputBlocks, ray);

                // iterate over all input blocks -> toggle boolean flag for it
                foreach (Vector3Int block in patternBlocks)
                {
                    TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                    if (objectComponentAt != null)
                    {
                        string treeName = objectComponentAt.name;
                        treeName = treeName.Replace("(Clone)", "");
                        treeName = treeName.Replace(" ", "");

                        if (_treeTypesActive.Contains(treeName))
                        {
                            coordinatesList.Add(block);
                        }
                        else
                        {
                            // ignore entry, not contained
                        }
                    }
                    // no tree, yet marking enabled
                    else if (_treeMarkOnly == false)
                    {
                        coordinatesList.Add(block);
                    }
                    else
                    {
                        // no tree component here, ignore
                    }
                }

                this._treeCuttingArea.AddCoordinates(coordinatesList.AsEnumerable());
            }
        }

        private void ShowNoneCallback()
        {
            this._areaHighlightingService.UnhighlightAll();
        }


        private IEnumerable<Vector3Int> GetPatternCoordinates(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            Vector3Int blockMaxMax = Vector3Int.zero;
            Vector3Int blockMinMin = Vector3Int.zero;
            Vector3Int blockMinMax = Vector3Int.zero;
            Vector3Int blockMaxMin = Vector3Int.zero;

            List<Vector3Int> singleBlockList = new();
            List<Vector3Int> blockList = new();

            // iterate over all input blocks to get area
            foreach (Vector3Int block in inputBlocks)
            {
                // get the min/max positions of the area
                if (blockMaxMax == Vector3Int.zero)
                {
                    blockMaxMax = block;
                }
                else
                {
                    blockMaxMax = Vector3Int.Max(blockMaxMax, block);
                }

                if (blockMinMin == Vector3Int.zero)
                {
                    blockMinMin = block;
                }
                else
                {
                    blockMinMin = Vector3Int.Min(blockMinMin, block);
                }

                // get the 4 corner positions of the area
                blockMinMax = new Vector3Int(blockMinMin.x, blockMaxMax.y, blockMaxMax.z);
                blockMaxMin = new Vector3Int(blockMaxMax.x, blockMinMin.y, blockMaxMax.z);
            }

            // get the distance X and Y. It is one less than the marked area, as the start position is not counted.
            // e.g. area 4 x 8, dist is 3 x 7.
            float distX = Vector3Int.Distance(blockMinMin, blockMaxMin);
            float distY = Vector3Int.Distance(blockMinMin, blockMinMax);

            foreach (Vector3Int block in inputBlocks)
            {
                // run for X pattern, meaning Y is toggled, X stays the same
                if (CutterPatterns.LinesX == _cutterPatterns)
                {
                    if ((blockMinMin.y - block.y) % 2 == 0)
                    {
                        // add block to list for conversion to IEnumerable
                        singleBlockList.Add(block);
                        IEnumerable<Vector3Int> blocks = singleBlockList;

                        // check if the block is on the same leveled coordinates. 
                        foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                        {
                            if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                            {
                                blockList.Add(leveledCoordinate);
                            }
                        }
                    }
                }
                else if (CutterPatterns.LinesY == _cutterPatterns)
                {
                    if ((blockMinMin.x - block.x) % 2 == 0)
                    {
                        // add block to list for conversion to IEnumerable
                        singleBlockList.Add(block);
                        IEnumerable<Vector3Int> blocks = singleBlockList;

                        // check if the block is on the same leveled coordinates. 
                        foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                        {
                            if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                            {
                                blockList.Add(leveledCoordinate);
                            }
                        }
                    }
                }
                else if (CutterPatterns.Checkered == _cutterPatterns)
                {
                    if ((((blockMinMin.x - block.x) % 2 == 0)
                        && ((blockMinMin.y - block.y) % 2 == 0))
                        ||
                        ((((blockMinMin + Vector3Int.right).x - block.x) % 2 == 0)
                        && (((blockMinMin + Vector3Int.up).y - block.y) % 2 == 0))
                        )

                    {
                        // add block to list for conversion to IEnumerable
                        singleBlockList.Add(block);
                        IEnumerable<Vector3Int> blocks = singleBlockList;

                        // check if the block is on the same leveled coordinates. 
                        foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                        {
                            if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                            {
                                blockList.Add(leveledCoordinate);
                            }
                        }
                    }
                }
                else // ALL
                {
                    // add block to list for conversion to IEnumerable
                    singleBlockList.Add(block);
                    IEnumerable<Vector3Int> blocks = singleBlockList;

                    foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(blocks, ray))
                    {
                        if (!this._treeCuttingArea.IsInCuttingArea(leveledCoordinate))
                        {
                            blockList.Add(leveledCoordinate);
                        }
                    }
                }
                // empty list for next block
                singleBlockList.Clear();
            }
            return blockList.AsEnumerable();
        }

        [OnEvent]
        public void OnCutterToolConfigChangeEvent(CutterToolConfigChangeEvent cutterToolConfigChangeEvent)
        {
            if (null == cutterToolConfigChangeEvent)
                return;

            _toggleTreeDict = cutterToolConfigChangeEvent.CutterToolConfig.GetTreeDict();
            _cutterPatterns = cutterToolConfigChangeEvent.CutterToolConfig.CutterPatterns;
            _treeMarkOnly = cutterToolConfigChangeEvent.CutterToolConfig.TreeMarkOnly;

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
