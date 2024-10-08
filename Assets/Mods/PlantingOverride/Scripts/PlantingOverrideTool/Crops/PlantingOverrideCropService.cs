using System;
using System.Collections.Generic;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.Fields;
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
    public class PlantingOverrideCropService : Tool, ISaveableSingleton, IPostLoadableSingleton, IPlantingOverrideCropTool
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

        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private static Dictionary<Vector3Int, string> _cropRegistry = new();

        private static readonly SingletonKey PlantingOverrideCropServiceKey = new SingletonKey("Cordial.PlantingOverrideCropService");
        private static readonly ListKey<Vector3Int> PlantingOverrideCropCoordKey = new ListKey<Vector3Int>("Cordial.PlantingOverrideCropCoordKey");
        private static readonly ListKey<string> PlantingOverrideCropTypeKey = new ListKey<string>("Cordial.PlantingOverrideCropTypeKey");



        public PlantingOverrideCropService( SelectionToolProcessorFactory selectionToolProcessorFactory,
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

            if (this._singletonLoader.HasSingleton(PlantingOverrideCropService.PlantingOverrideCropServiceKey))
            {
                List<string> cropTypes = _singletonLoader.GetSingleton(PlantingOverrideCropService.PlantingOverrideCropServiceKey).Get(PlantingOverrideCropService.PlantingOverrideCropTypeKey);
                List<Vector3Int> cropCoordinates = _singletonLoader.GetSingleton(PlantingOverrideCropService.PlantingOverrideCropServiceKey).Get(PlantingOverrideCropService.PlantingOverrideCropCoordKey);

                if (cropCoordinates.Count != cropTypes.Count)
                {
                    Debug.Log("PO: Did not load planting override crop configuration");
                }
                else
                {
                    for (int i = 0; i < cropTypes.Count; i++)
                    {
                        if (!_cropRegistry.TryAdd(cropCoordinates[i], cropTypes[i]))
                        {
                            _cropRegistry[cropCoordinates[i]] = cropTypes[i];
                        }
                    }

                    foreach (var kvp in _cropRegistry)
                    {
                        Crop objectComponentAt = this._blockService.GetBottomObjectComponentAt<Crop>(kvp.Key);

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
            Debug.Log("Crops Saved");

            singletonSaver.GetSingleton(PlantingOverrideCropService.PlantingOverrideCropServiceKey).Set(PlantingOverrideCropCoordKey, _cropRegistry.Keys);
            singletonSaver.GetSingleton(PlantingOverrideCropService.PlantingOverrideCropServiceKey).Set(PlantingOverrideCropTypeKey, _cropRegistry.Values);
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

                            if (_cropRegistry.ContainsKey(block))
                            {
                                _cropRegistry[block] = _cropTypesActive[0];
                            }
                            else
                            {
                                _cropRegistry.Add(block, _cropTypesActive[0]);
                            }
                        }
                        else
                        {
                            Debug.Log("PO: Unknown Crop");
                        }
                    }
                    else
                    {
                        // no crop component here, ignore
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

        public void RemoveEntryAtCoord( Vector3Int coord)
        {
            _cropRegistry.Remove(coord);
        }
        public bool HasEntryAtCoord( Vector3Int coord, out string cropName)
        {
            return _cropRegistry.TryGetValue(coord, out cropName);
        }

        [OnEvent]
        public void OnPlantingOverridePlantingEvent(PlantingOverridePlantingEvent PlantingOverridePlantingEvent)
        {
            if (null == PlantingOverridePlantingEvent)
                return;


            if (HasEntryAtCoord(PlantingOverridePlantingEvent.Coordinates, out string resourceName))
            {
                // remove entry in any case. Possibly the planting was reset by using the "standard" tool
                RemoveEntryAtCoord(PlantingOverridePlantingEvent.Coordinates);

                //if (resourceName == PlantingOverridePlantingEvent.PlantName)
                //{
                //    RemoveEntryAtCoord(PlantingOverridePlantingEvent.Coordinates);
                //}
                //else
                //{
                //    Debug.Log("PO: T: Mismatch: " + resourceName + " - " + PlantingOverridePlantingEvent.PlantName);
                //}
            }
        }
    }
}
