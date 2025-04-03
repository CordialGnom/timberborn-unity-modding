using System.Collections.Generic;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.BuilderPrioritySystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.Demolishing;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.Pollination;
using Timberborn.ScienceSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using Timberborn.WorldPersistence;
using UnityEngine;
using Timberborn.Common;
using System.Linq;
using Timberborn.SelectionToolSystem;
using System;
using Timberborn.Growing;
using Timberborn.Planting;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolService : Tool, ILoadableSingleton, ISaveableSingleton, IPostLoadableSingleton, IPlantBeehiveTool
    {
        private const int _cBeehiveRadius = 3;

        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.PlantBeehiveTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantBeehiveTool.Description";
        private static readonly string CursorKey = "PickDestinationCursor";

        // tool setup
        private readonly ILoc _loc;
        private ToolDescription _toolDescription;               // is used
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly EventBus _eventBus;
        //public ISpecService _specService;

        // highlighting
        private static AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;
        public Color _toolActionTileColor;

        // selection
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly IBlockService _blockService;
        private static Vector3Int _cursorPosPrev = new Vector3Int(0, 0, 0);

        // building placement
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;
        private BuildingSpec _beehive;
        private readonly PlantingService _plantingService;

        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private List<Vector3Int> _hiveCoordsNew = new();
        private List<Hive> _hiveRegistry = new();

        private static readonly SingletonKey PlantBeehiveToolServiceKey = new SingletonKey("Cordial.PlantBeehiveToolService");
        private static readonly ListKey<Vector3Int> PlantBeehiveToolCoordKey = new ListKey<Vector3Int>("Cordial.PlantBeehiveToolCoordKey");

        public PlantBeehiveToolService(SelectionToolProcessorFactory selectionToolProcessorFactory,
                                            BuildingUnlockingService buildingUnlockingService,
                                            AreaHighlightingService areaHighlightingService,
                                            ToolUnlockingService toolUnlockingService,
                                            TerrainAreaService terrainAreaService,
                                            ISingletonLoader singletonLoader,
                                            BuildingService buildingService,
                                            PlantingService plantingService,
                                            IBlockService blockService,
                                            //ISpecService specService,
                                            EventBus eventBus,
                                            ILoc loc )
        {
            _buildingUnlockingService = buildingUnlockingService;
            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService = toolUnlockingService;
            _terrainAreaService = terrainAreaService;
            _singletonLoader = singletonLoader;
            _buildingService = buildingService;
            _plantingService = plantingService;
            _blockService = blockService;
            //_specService = specService;
            _eventBus = eventBus;
            _loc = loc; 
            
            _selectionToolProcessor = selectionToolProcessorFactory.Create(new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.PreviewCallback),
                                                                                    new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.ActionCallback),
                                                                                    new Action(ShowNoneCallback),
                                                                                    CursorKey);

            // todo add service that the tool is locked / requires beehive
        }

        public void Load()
        {
            this._toolActionTileColor = new Color(1, 0.6f, 0, 1); // from böuprint/configurations/rangedeffectbuildingcolors.json
            // couldn't get the damn spec to load

            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();

            this._eventBus.Register((object)this);

            Debug.Log("Overriding limits...");

        }

        public void PostLoad()
        {
            if (this._singletonLoader.HasSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey))
            {
                // reload coordinates where a hive is to be planted
                _hiveCoordsNew = _singletonLoader.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Get(PlantBeehiveToolService.PlantBeehiveToolCoordKey);
                
                // replace hivecoords with a distinct (no duplicate) copy of itself
                _hiveCoordsNew = _hiveCoordsNew.Distinct().ToList();
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Set(PlantBeehiveToolCoordKey, _hiveCoordsNew);
        }

        public override void Enter()
        {
            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                // activate tool
                this._selectionToolProcessor.Enter();

                // create a beehive to be placed by the tool
                string prefabName = "Beehive.Folktails";

                _beehive = _buildingService.GetBuildingPrefab(prefabName);

                if (_beehive == null)
                {
                    this.Exit();
                }
            }
        }

        public override void Exit()
        {
            this._selectionToolProcessor.Exit();
            _areaHighlightingService.UnhighlightAll();
        }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                // only take first input block
                Vector3Int startCoord = inputBlocks.FirstOrDefault();

                List<Vector3Int> newList = new();
                List<Vector2Int> coordList = new();

                if (startCoord != Vector3Int.zero)
                {
                    coordList.Add(startCoord.XY());

                    newList.AddRange(_terrainAreaService.InMapCoordinates(coordList));

                    startCoord = newList.First();

                    newList.Clear();

                    // evaluate start coord range
                    newList.AddRange(GetBlocksInRectangularRadius(startCoord, _cBeehiveRadius));

                    // get all areas
                    foreach (Hive hive in _hiveRegistry)
                    {
                        newList.AddRange(hive.GetBlocksInRange());
                    }

                    _hiveCoordsNew = _hiveCoordsNew.Distinct().ToList();

                    foreach (Vector3Int coord in _hiveCoordsNew)
                    {
                        newList.AddRange(GetBlocksInRectangularRadius(coord, _cBeehiveRadius));
                    }

                    foreach (Vector3Int coord in newList)
                    {
                        _areaHighlightingService.DrawTile(coord, this._toolActionTileColor);
                    }

                    var tgtCoord = this._blockService.GetBottomObjectAt(startCoord);

                    if (tgtCoord != null)
                    {
                        _areaHighlightingService.AddForHighlight((BaseComponent)tgtCoord);
                    }
                }

                // highlight everything added to the service above
                _areaHighlightingService.Highlight();
            }
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                // only take first input block
                Vector3Int startCoord = inputBlocks.FirstOrDefault();

                List<Vector3Int> newList = new();
                List<Vector2Int> coordList = new();

 
                if (startCoord != Vector3Int.zero)
                {
                    coordList.Add(startCoord.XY());

                    newList.AddRange(_terrainAreaService.InMapCoordinates(coordList));

                    startCoord = newList.First();

                    newList.Clear();

                    // get all areas
                    newList.AddRange(GetBlocksInRectangularRadius(startCoord, _cBeehiveRadius));


                    foreach (Hive hive in _hiveRegistry)
                    {
                        newList.AddRange(hive.GetBlocksInRange());
                    }

                    foreach (Vector3Int coord in _hiveCoordsNew)
                    {
                        newList.AddRange(GetBlocksInRectangularRadius(coord, _cBeehiveRadius));
                    }

                    foreach (Vector3Int coord in newList)
                    {
                        _areaHighlightingService.DrawTile(coord, this._toolActionTileColor);
                    }

                    PrepareBeehivePlacement(startCoord);

                }

                // highlight everything added to the service above
                _areaHighlightingService.Highlight();
            }
        }

        private void ShowNoneCallback()
        {
            //_areaHighlightingService.UnhighlightAll();
        }

        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }
        public override ToolDescription Description() => _toolDescription;

       
        private void HighlightExistingHiveArea()
        {
            // iterate through all hives
            List<Vector3Int> hiveArea = new();

            foreach (Hive hive in _hiveRegistry)
            {
                hiveArea.AddRange(hive.GetBlocksInRange());
            }
            HighlightPassedBlocks(hiveArea);
        }

        private void HighlightReservedHiveArea()
        {
            // iterate through all hives
            List<Vector3Int> hiveArea = new();

            foreach (Vector3Int coord in _hiveCoordsNew)
            {
                hiveArea.AddRange(GetBlocksInRectangularRadius(coord, _cBeehiveRadius));
            }
            HighlightPassedBlocks(hiveArea);
        }

        private void HighlightCursorArea(Vector3Int cursorPos, int radiusRect )
        {
           if (_cursorPosPrev != cursorPos)
            {
                // highligh cursor area
                HighlightPassedBlocks(GetBlocksInRectangularRadius(cursorPos, radiusRect));

                // highlight Hive Area
                HighlightExistingHiveArea();

                // copy cursor
                _cursorPosPrev = cursorPos;
            }
        }

        private IEnumerable<Vector3Int> GetBlocksInRectangularRadius(Vector3Int cursorPos, int radiusRect)
        {
            List<Vector3Int> area = new List<Vector3Int>();
            List<Vector2Int> blocks = new List<Vector2Int>();

            for (int x = (cursorPos.x - radiusRect); x <= (cursorPos.x + (radiusRect)); ++x)
            {
                for (int y = (cursorPos.y - radiusRect); y <= (cursorPos.y + (radiusRect)); ++y)
                {
                    Vector2Int v2 = new Vector2Int(x, y);

                    blocks.Add(v2);
                }
            }

            area.AddRange(_terrainAreaService.InMapCoordinates(blocks));

            return area.AsEnumerable<Vector3Int>();
        }

        private void HighlightPassedBlocks(IEnumerable<Vector3Int> blocks)
        {
            // iterate over all input blocks -> toggle boolean flag for it
            foreach (Vector3Int block in blocks)
            {
                var bottomObject = this._blockService.GetBottomObjectAt(block);

                if (bottomObject != null)
                {
                    _areaHighlightingService.AddForHighlight((BaseComponent)bottomObject);
                }

                _areaHighlightingService.DrawTile(block, this._toolActionTileColor);
            }
        }

        public void PrepareBeehivePlacement(Vector3Int coord)
        {

            if (_blockService.AnyObjectAt(coord))
            {
                if (1 == _blockService.GetObjectsAt(coord).Count)
                {
                    foreach (var block in _blockService.GetObjectsAt(coord))
                    {
                        if (block != null)
                        {
                            // check what kind of object has been found
                            block.TryGetComponentFast<Demolishable>(out var demolishable);
                            block.TryGetComponentFast<GrowableSpec>(out var growable);
                            block.TryGetComponentFast<BuildingSpec>(out var building);

                            if ((building != null)
                                || (block.name.Contains("Path")))
                            {
                                // do nothing, object cannot be replaced
                                break;
                            }
                            else if ((demolishable != null)
                                     && (growable != null))
                            {
                                // check that it is not added twice
                                if (!_hiveCoordsNew.Contains(coord))
                                {
                                    _hiveCoordsNew.Add(coord);
                                    demolishable.Mark();
                                    _plantingService.UnsetPlantingCoordinates(coord);
                                    demolishable.TryGetComponentFast<BuilderPrioritizable>(out BuilderPrioritizable prioritizable);

                                    // increase the priority of the plant which is to be deleted
                                    if (prioritizable != null)
                                    {
                                        prioritizable.SetPriority(Timberborn.PrioritySystem.Priority.High);
                                    }
                                }
                                
                            }
                            else
                            {
                                // nothing of interest here, do nothing
                            }
                        }
                    }
                }
                else
                {
                    // multiple objects do nothing
                }
            }
            else
            {
                // no object to be deleted
                // place immediately
                _hiveCoordsNew.Add(coord);
                PlaceBeehiveObject(coord);
            }

            // replace hivecoords with a distinct (no duplicate) copy of itself
            _hiveCoordsNew = _hiveCoordsNew.Distinct().ToList();
        }


        //private IEnumerable<Vector3Int> GetBlocksInRange()
        //{
        //    return this._blockObjectRange.GetBlocksOnTerrainInRectangularRadius(_cBeehiveRadius);
        //}

        // harmony is to check the deletion of the registered plant at these coordinates

        // harmony can set the event / service to build a beehive there. 


        //[OnEvent]
        //public void OnPlantingOverrideConfigChangeEvent(PlantingOverrideConfigChangeEvent PlantingOverrideConfigChangeEvent)
        //{
        //    if (null == PlantingOverrideConfigChangeEvent)
        //        return;

        //    if (!PlantingOverrideConfigChangeEvent.IsTree)
        //    {
        //        _hiveTypesActive.Clear();

        //        string plantName = PlantingOverrideConfigChangeEvent.PlantName.Replace(" ", "");
        //        _hiveTypesActive.Add(plantName);
        //    }
        //}

        //public void RemoveEntryAtCoord( Vector3Int coord)
        //{
        //    _hiveCoordsNew.Remove(coord);
        //}
        //public bool HasEntryAtCoord( Vector3Int coord)
        //{
        //    return _hiveCoordsNew.TryGetValue(coord, out string hiveName);
        //}

        //[OnEvent]
        //public void OnPlantingOverridePlantingEvent(PlantingOverridePlantingEvent PlantingOverridePlantingEvent)
        //{
        //    if (null == PlantingOverridePlantingEvent)
        //        return;


        //    if (HasEntryAtCoord(PlantingOverridePlantingEvent.Coordinates))
        //    {
        //        // remove entry in any case. Possibly the planting was reset by using the "standard" tool
        //        RemoveEntryAtCoord(PlantingOverridePlantingEvent.Coordinates);
        //    }
        //}


        [OnEvent]
        public void OnPlantBeehiveToolUnmarkEvent(PlantBeehiveToolUnmarkEvent PlantBeehiveToolUnmarkEvent)
        {
            if (null == PlantBeehiveToolUnmarkEvent)
            {
                return;
            }
            else // event exists
            {
                if (_hiveCoordsNew.Contains(PlantBeehiveToolUnmarkEvent.Coordinates))
                {
                    // remove coordinates
                    _hiveCoordsNew.Remove(PlantBeehiveToolUnmarkEvent.Coordinates);

                    if (PlantBeehiveToolUnmarkEvent.PlaceHive)
                    {
                        // place beehive
                        this.PlaceBeehiveObject(PlantBeehiveToolUnmarkEvent.Coordinates);
                    }
                }
            }
        }

        private void PlaceBeehiveObject( Vector3Int coordinates)
        {
            // create a new placement for the building
            Placement placement = new Placement(coordinates);

            if (_beehive != null)
            {
                BlockObjectPlacerService buildingPlacer = DependencyContainer.GetInstance<BlockObjectPlacerService>();
                BlockObjectSpec block = this._beehive.GetComponentFast<BlockObjectSpec>();

                if (null != buildingPlacer)
                {
                    IBlockObjectPlacer placer= buildingPlacer.GetMatchingPlacer(block);

                    if (null != placer)
                    {
                        try
                        {
                            placer.Place(block, placement);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("PlantingOverride: Beehive failed with: " + e.Message + " at " + coordinates);
                        }

                        _hiveCoordsNew.Remove(coordinates);
                    }
                }
            }
        }

        [OnEvent]
        public void OnPlantBeehiveToolRegisterHiveEvent(PlantBeehiveToolRegisterHiveEvent PlantBeehiveToolRegisterHiveEvent)
        {
            if ((null == PlantBeehiveToolRegisterHiveEvent)
                    || (null == PlantBeehiveToolRegisterHiveEvent.Hive))
            {
                return;
            }
            else // event exists
            {
                if (!_hiveRegistry.Contains(PlantBeehiveToolRegisterHiveEvent.Hive))
                {
                    _hiveRegistry.Add(PlantBeehiveToolRegisterHiveEvent.Hive);
                    _hiveCoordsNew.Remove(PlantBeehiveToolRegisterHiveEvent.Hive.GetComponentFast<BlockObject>().Coordinates);
                }
            }
        }

        [OnEvent]
        public void OnPlantBeehiveToolUnregisterHiveEvent(PlantBeehiveToolUnregisterHiveEvent PlantBeehiveToolUnregisterHiveEvent)
        {
            if ((null == PlantBeehiveToolUnregisterHiveEvent)
                    || (null == PlantBeehiveToolUnregisterHiveEvent.Hive))
            {
                return;
            }
            else // event exists
            {
                _hiveRegistry.Remove(PlantBeehiveToolUnregisterHiveEvent.Hive);
                _hiveCoordsNew.Remove(PlantBeehiveToolUnregisterHiveEvent.Hive.GetComponentFast<BlockObject>().Coordinates);
            }
        }

        [OnEvent]
        public void OnBuildingUnlockedEvent (BuildingUnlockedEvent BuildingUnlockedEvent)
        {
            if ((null == BuildingUnlockedEvent)
                    || (null == BuildingUnlockedEvent.BuildingSpec))
            {
                return;
            }
            else // event exists, and building does as well. 
            {                
                if (BuildingUnlockedEvent.BuildingSpec.name.Contains("Beehive"))
                {
                    if (this.Locker != null)
                    {
                        this._toolUnlockingService.TryToUnlock((Tool)this);
                    }
                }
            }
        }
    }
}
