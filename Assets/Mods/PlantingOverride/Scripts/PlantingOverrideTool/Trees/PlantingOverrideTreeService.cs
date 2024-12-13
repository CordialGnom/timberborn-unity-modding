using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using NUnit.Framework;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Forestry;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    public class PlantingOverrideTreeService : Tool, ISaveableSingleton, IPostLoadableSingleton, IPlantingOverrideTreeTool
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
        private readonly PlantingOverridePrefabSpecService _specService;
        private bool _removeCuttingArea;

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private readonly PlantingService _plantingService;
        private readonly BlockService _blockService;
        private readonly TreeCuttingArea _treeCuttingArea;

        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private static Dictionary<Vector3Int, string> _treeRegistry = new();
        private static List<Vector3Int> _areaRegistry = new();

        private static readonly SingletonKey PlantingOverrideTreeServiceKey = new SingletonKey(nameof(PlantingOverrideTreeService));
        private static readonly ListKey<Vector3Int> PlantingOverrideTreeCoordKey = new ListKey<Vector3Int>("Cordial.PlantingOverrideTreeCoordKey");
        private static readonly ListKey<string> PlantingOverrideTreeTypeKey = new ListKey<string>("Cordial.PlantingOverrideTreeTypeKey");
        private static readonly ListKey<Vector3Int> PlantingOverrideAreaCoordKey = new ListKey<Vector3Int>("Cordial.PlantingOverrideAreaCoordKey");



        public PlantingOverrideTreeService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            AreaHighlightingService areaHighlightingService,
                                            PlantingOverridePrefabSpecService specService,
                                            ToolUnlockingService toolUnlockingService,
                                            TerrainAreaService terrainAreaService,
                                            ISingletonLoader singletonLoader,
                                            TreeCuttingArea treeCuttingArea,
                                            PlantingService plantingService,
                                            BlockService blockService,
                                            EventBus eventBus,
                                            Colors colors,
                                            ILoc loc ) 
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
            _singletonLoader = singletonLoader;
            _treeCuttingArea = treeCuttingArea;
            _blockService = blockService;
            _specService = specService;
            _eventBus = eventBus;
            _colors = colors;
            _loc = loc; 

        }

        public void PostLoad()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
            this._eventBus.Register((object)this);

            if (this._singletonLoader.HasSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey))
            {
                
                if (_singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Has(PlantingOverrideTreeService.PlantingOverrideAreaCoordKey)) 
                {
                    _areaRegistry = _singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Get(PlantingOverrideTreeService.PlantingOverrideAreaCoordKey);
                }

                if ((_singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Has(PlantingOverrideTreeService.PlantingOverrideTreeTypeKey))
                    && (_singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Has(PlantingOverrideTreeService.PlantingOverrideTreeCoordKey)))
                {
                    List<string> forestryTypes = _singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Get(PlantingOverrideTreeService.PlantingOverrideTreeTypeKey);
                    List<Vector3Int> treeCoordinates = _singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Get(PlantingOverrideTreeService.PlantingOverrideTreeCoordKey);

                    if (treeCoordinates.Count != forestryTypes.Count)
                    {
                        Debug.Log("PO: Did not load planting override tree configuration");
                    }
                    else
                    {
                        for (int i = 0; i < forestryTypes.Count; i++)
                        {
                            if (!_treeRegistry.TryAdd(treeCoordinates[i], forestryTypes[i]))
                            {
                                _treeRegistry[treeCoordinates[i]] = forestryTypes[i];
                            }
                        }

                        foreach (var kvp in _treeRegistry.ToList())
                        {
                            TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(kvp.Key);
                            Bush bushComponentAt = this._blockService.GetBottomObjectComponentAt<Bush>(kvp.Key);

                            if ((objectComponentAt != null)
                                 || (bushComponentAt != null))
                            {
                                if (_specService.VerifyPrefabName(kvp.Value))
                                {
                                    _plantingService.SetPlantingCoordinates(kvp.Key, kvp.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Set(PlantingOverrideTreeCoordKey, _treeRegistry.Keys);
            singletonSaver.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Set(PlantingOverrideTreeTypeKey, _treeRegistry.Values);
            singletonSaver.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Set(PlantingOverrideAreaCoordKey, _areaRegistry);
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
                Bush bushComponentAt = this._blockService.GetBottomObjectComponentAt<Bush>(block);

                if (objectComponentAt != null)
                {
                   this._areaHighlightingService.AddForHighlight((BaseComponent)objectComponentAt);
                   this._areaHighlightingService.DrawTile(block, this._colors.PlantingToolTile);
                }
                else if (bushComponentAt != null)
                {
                   this._areaHighlightingService.AddForHighlight((BaseComponent)bushComponentAt);
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
                _specService.VerifyPrefabName(_treeTypesActive[0]);

                foreach (Vector3Int block in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
                {                
                    TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(block);
                    Bush bushComponentAt = this._blockService.GetBottomObjectComponentAt<Bush>(block);

                    if ((objectComponentAt != null)
                        || (bushComponentAt != null))
                    {
                        if ((_treeTypesActive.Count == 1)
                            && (_specService.VerifyPrefabName(_treeTypesActive[0])))
                        {
                            _plantingService.SetPlantingCoordinates(block, _treeTypesActive[0]);

                            // add to cutting area remove registry if so set, and not already added
                            if ((_removeCuttingArea)
                                && (!_areaRegistry.Contains(block)))
                            {
                                _areaRegistry.Add(block);
                            }
                            else if ((!_removeCuttingArea)
                                && (_areaRegistry.Contains(block)))
                            {
                                _areaRegistry.Remove(block);
                            }
                            else
                            {
                                // keep as is
                            }

                            if (_treeRegistry.ContainsKey(block))
                            {
                                _treeRegistry[block] = _treeTypesActive[0];
                            }
                            else
                            {
                                _treeRegistry.Add(block, _treeTypesActive[0]);
                            }
                        }
                        else
                        {
                            Debug.Log("PO: Unknown Tree");
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
        [OnEvent]
        public void OnPlantingOverrideAreaRemoveEvent(PlantingOverrideAreaRemoveEvent PlantingOverrideAreaRemoveEvent)
        {
            if (null == PlantingOverrideAreaRemoveEvent)
                return;

            _removeCuttingArea = PlantingOverrideAreaRemoveEvent.RemoveCuttingArea;
        }

        public void RemoveEntryAtCoord( Vector3Int coord)
        {
            // remove coordinate from both registries
            _treeRegistry.Remove(coord);
            _areaRegistry.Remove(coord);
        }

        public void RemoveEntryInCutArea(Vector3Int coord)
        {
            if (_areaRegistry.Contains(coord))
            {
                List<Vector3Int> list = new();
                list.Add(coord);

                // remove from cutting area --> requires cutting area service
                this._treeCuttingArea.RemoveCoordinates(list.AsEnumerable<Vector3Int>());
                _areaRegistry.Remove(coord);
            }
        }

        public bool HasEntryAtCoord( Vector3Int coord)
        {
            return _treeRegistry.TryGetValue(coord, out string treeName);
        }

        [OnEvent]
        public void OnPlantingOverridePlantingEvent(PlantingOverridePlantingEvent PlantingOverridePlantingEvent)
        {
            if (null == PlantingOverridePlantingEvent)
                return;

            if (HasEntryAtCoord(PlantingOverridePlantingEvent.Coordinates))
            {
                RemoveEntryInCutArea(PlantingOverridePlantingEvent.Coordinates);
                RemoveEntryAtCoord(PlantingOverridePlantingEvent.Coordinates);
            }
        }

        [OnEvent]
        public void OnPlantingOverrideRemoveEvent(PlantingOverrideRemoveEvent PlantingOverrideRemoveEvent)
        {
            if (null == PlantingOverrideRemoveEvent)
                return;

            RemoveEntryAtCoord(PlantingOverrideRemoveEvent.Coordinates);
        }
    }
}
