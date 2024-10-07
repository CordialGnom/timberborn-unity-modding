using System;
using System.Collections.Generic;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
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

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // planting area / selection
        private readonly PlantingService _plantingService;
        private readonly BlockService _blockService;

        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private static Dictionary<Vector3Int, string> _treeRegistry = new();

        private static readonly SingletonKey PlantingOverrideTreeServiceKey = new SingletonKey("Cordial.PlantingOverrideTreeService");
        private static readonly ListKey<Vector3Int> PlantingOverrideTreeCoordKey = new ListKey<Vector3Int>("Cordial.PlantingOverrideTreeCoordKey");
        private static readonly ListKey<string> PlantingOverrideTreeTypeKey = new ListKey<string>("Cordial.PlantingOverrideTreeTypeKey");



        public PlantingOverrideTreeService( SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            AreaHighlightingService areaHighlightingService,
                                            PlantingOverridePrefabSpecService specService,
                                            ToolUnlockingService toolUnlockingService,
                                            TerrainAreaService terrainAreaService,
                                            ISingletonLoader singletonLoader,
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
                List<string> treeTypes = _singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Get(PlantingOverrideTreeService.PlantingOverrideTreeTypeKey);
                List<Vector3Int> treeCoordinates = _singletonLoader.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Get(PlantingOverrideTreeService.PlantingOverrideTreeCoordKey);

                if (treeCoordinates.Count != treeTypes.Count)
                {
                    Debug.Log("PO: Did not load planting override tree configuration");
                }
                else
                {
                    for (int i = 0; i < treeTypes.Count; i++)
                    {
                        if (!_treeRegistry.TryAdd(treeCoordinates[i], treeTypes[i]))
                        {
                            _treeRegistry[treeCoordinates[i]] = treeTypes[i];
                        }
                    }

                    foreach (var kvp in _treeRegistry)
                    {
                        TreeComponent objectComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(kvp.Key);

                        if (objectComponentAt != null)
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

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Set(PlantingOverrideTreeCoordKey, _treeRegistry.Keys);
            singletonSaver.GetSingleton(PlantingOverrideTreeService.PlantingOverrideTreeServiceKey).Set(PlantingOverrideTreeTypeKey, _treeRegistry.Values);
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

                    _specService.VerifyPrefabName(_treeTypesActive[0]);

                    if (objectComponentAt != null)
                    {
                        if ((_treeTypesActive.Count == 1)
                            && (_specService.VerifyPrefabName(_treeTypesActive[0])))
                        {
                            _plantingService.SetPlantingCoordinates(block, _treeTypesActive[0]);

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

        public void RemoveEntryAtCoord( Vector3Int coord)
        {
            _treeRegistry.Remove(coord);
        }
        public bool HasEntryAtCoord( Vector3Int coord, out string treeName)
        {
            return _treeRegistry.TryGetValue(coord, out treeName);
        }
    }
}
