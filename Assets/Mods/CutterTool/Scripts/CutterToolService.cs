using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.CutterTool.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Cutting;
using Timberborn.Forestry;
using Timberborn.Gathering;
using Timberborn.GoodStackSystem;
using Timberborn.Growing;
using Timberborn.Localization;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolService : Tool, ILoadableSingleton, ICutterTool
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.CutterTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.CutterTool.Description";
        private static readonly string CursorKey = "CutTreeCursor";

        // tool setup
        private readonly ILoc _loc;
        private ToolDescription _toolDescription;      // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private EventBus _eventBus;

        // UI setup
        //private CutterToolInitializer _cutterToolInitializer;

        // configuration
        private Dictionary<string, bool> _toggleTreeDict = new();
        private CutterPatterns _cutterPatterns;
        private readonly List<string> _treeTypesActive = new();
        private bool _treeMarkOnly = false;
        private bool _ignoreStumps = false;

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // cutting area
        private readonly TreeCuttingArea _treeCuttingArea;
        private readonly BlockService _blockService;


        public CutterToolService(   SelectionToolProcessorFactory selectionToolProcessorFactory,
                                    ToolUnlockingService toolUnlockingService,
                                    Colors colors,
                                    ILoc loc,
                                    AreaHighlightingService areaHighlightingService,
                                    TerrainAreaService terrainAreaService,
                                    TreeCuttingArea treeCuttingArea,
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
            _treeCuttingArea =  treeCuttingArea;
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
            this._eventBus.Post((object)new CutterToolSelectedEvent(this) );
        }
        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            this._eventBus.Post((object)new CutterToolUnselectedEvent(this));
        }
        
        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }
        public override ToolDescription Description() => _toolDescription;

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
                        // check if component is only a stump
                        objectComponentAt.TryGetComponentFast<Cuttable>(out Cuttable cuttable);
                        objectComponentAt.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);
                        objectComponentAt.TryGetComponentFast<Gatherable>(out Gatherable gatherable);
                        objectComponentAt.TryGetComponentFast<Growable>(out Growable growable);

                        if ((cuttable != null)
                            && (livingResource != null)
                            && (growable != null))
                        {
                            bool gatherableEmpty = false;
                            bool invIsEmpty = livingResource.GetComponentFast<GoodStack>().Inventory.IsEmpty;
                            bool growthDone = (growable.IsGrown);

                            // must be a cuttable
                            // must be a living resource
                            // --> check if fully grown (= 1.0)
                            // --> check if not yielding (cuttable / gatherable)

                            bool cuttableEmpty = (cuttable.Yielder.Yield.Amount == 0);

                            // not all trees have gatherables
                            if (gatherable != null)
                            {
                                gatherableEmpty = (gatherable.Yielder.Yield.Amount == 0);
                            }

                            // is a stump or not
                            if (invIsEmpty && cuttableEmpty && growthDone && gatherableEmpty && _ignoreStumps)
                            {
                                // ignore stumps, do not mark as part of the selection
                                this._areaHighlightingService.DrawTile(block, this._colors.PriorityTileColor);
                            }
                            else // a tree or a markable stump
                            {
                                this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                                this._areaHighlightingService.DrawTile(block, this._colors.SelectionToolHighlight);
                            }
                        }
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
                            if (_ignoreStumps)
                            {
                                // check if component is only a stump
                                objectComponentAt.TryGetComponentFast<Cuttable>(out Cuttable cuttable);
                                objectComponentAt.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);
                                objectComponentAt.TryGetComponentFast<Gatherable>(out Gatherable gatherable);
                                objectComponentAt.TryGetComponentFast<Growable>(out Growable growable);

                                if ((cuttable != null)
                                    && (livingResource != null)
                                    && (growable != null))
                                {
                                    bool gatherableEmpty = false;
                                    bool invIsEmpty = livingResource.GetComponentFast<GoodStack>().Inventory.IsEmpty;
                                    bool growthDone = (growable.IsGrown);

                                    // must be a cuttable
                                    // must be a living resource
                                    // --> check if fully grown (= 1.0)
                                    // --> check if not yielding (cuttable / gatherable)

                                    bool cuttableEmpty = (cuttable.Yielder.Yield.Amount == 0);

                                    // not all trees have gatherables
                                    if (gatherable != null)
                                    {
                                        gatherableEmpty = (gatherable.Yielder.Yield.Amount == 0);
                                    }

                                    // is a stump or not
                                    if (invIsEmpty && cuttableEmpty && growthDone && gatherableEmpty)
                                    {
                                        // ignore stumps, do not mark as part of the selection
                                    }
                                    else // a tree or a markable stump
                                    {
                                        coordinatesList.Add(block);
                                    }
                                }
                            }
                            else // add any expected trees
                            {
                                coordinatesList.Add(block);
                            }
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
            Vector3Int blockMinMin = Vector3Int.zero;

            List<Vector3Int> singleBlockList = new();
            List<Vector3Int> blockList = new();

            // iterate over all input blocks to get area
            foreach (Vector3Int block in inputBlocks)
            {
                if (blockMinMin == Vector3Int.zero)
                {
                    blockMinMin = block;
                }
                else
                {
                    blockMinMin = Vector3Int.Min(blockMinMin, block);
                }
            }

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
            _ignoreStumps = cutterToolConfigChangeEvent.CutterToolConfig.IgnoreStumps;

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
