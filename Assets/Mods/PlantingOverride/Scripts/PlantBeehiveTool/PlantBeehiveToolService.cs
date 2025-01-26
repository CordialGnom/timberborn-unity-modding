using Cordial.Mods.PlantingOverride.Scripts.UI;
using System.Collections.Generic;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.BuilderPrioritySystem;
using Timberborn.Buildings;
using Timberborn.BuildingTools;
using Timberborn.Coordinates;
using Timberborn.CoreUI;
using Timberborn.Demolishing;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.Pollination;
using Timberborn.Planting;
using Timberborn.PrefabSystem;
using Timberborn.ScienceSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using Timberborn.Fields;
using Timberborn.Common;
using Moq;
using System.Drawing;
using System.Linq;
using Timberborn.SelectionToolSystem;
using System;
using Timberborn.AreaSelectionSystem;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolService : Tool, ISaveableSingleton, ILoadableSingleton, IPostLoadableSingleton, IPlantBeehiveTool, IInputProcessor
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
        private readonly CursorService _cursorService;

        // highlighting
        private readonly Colors _colors;
        private static AreaHighlightingService _areaHighlightingService;

        // selection
        //private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly BlockService _blockService;
        private static Vector3Int _cursorPosPrev = new Vector3Int(0, 0, 0);


        // building placement
        private BuildingUnlockingService _buildingUnlockingService;
        private BuildingService _buildingService;
        private Building _beehive;

        private BlockObjectRange _blockObjectRange;

        // v2 
        private readonly InputService _inputService;


        private readonly AreaPickerFactory _areaPickerFactory;
        private readonly PreviewPlacer _previewPlacer;
        private AreaPicker _areaPicker;
        private PlaceableBlockObject Prefab;


        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private static List<Vector3Int> _hiveCoordsNew = new();
        private static List<Hive> _hiveRegistry = new();

        private static readonly SingletonKey PlantBeehiveToolServiceKey = new SingletonKey("Cordial.PlantBeehiveToolService");
        private static readonly ListKey<Vector3Int> PlantBeehiveToolCoordKey = new ListKey<Vector3Int>("Cordial.PlantBeehiveToolCoordKey");

        public PlantBeehiveToolService( /*SelectionToolProcessorFactory selectionToolProcessorFactory,*/
                                            BuildingUnlockingService buildingUnlockingService,
                                            AreaHighlightingService areaHighlightingService,
                                            ToolUnlockingService toolUnlockingService,
                                            ISingletonLoader singletonLoader,
                                            BuildingService buildingService,
                                            InputService inputService,
                                            AreaPickerFactory areaPickerFactory,
      PreviewPlacer previewPlacer,
                                            CursorService cursorService,
                                            BlockService blockService,
                                            EventBus eventBus,
                                            Colors colors,
                                            ILoc loc )
        {
            _buildingUnlockingService = buildingUnlockingService;
            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService = toolUnlockingService;
            _singletonLoader = singletonLoader;
            _buildingService = buildingService;
            this._areaPickerFactory = areaPickerFactory;
            this._previewPlacer = previewPlacer;

            _inputService = inputService;
            _cursorService = cursorService;
            _blockService = blockService;
            _eventBus = eventBus;
            _colors = colors;
            _loc = loc;

            //_selectionToolProcessor = selectionToolProcessorFactory.Create(new Action<IEnumerable<Vector3Int>,
            //                                                                        Ray>(this.PreviewCallback),
            //                                                                        new Action<IEnumerable<Vector3Int>,
            //                                                                        Ray>(this.ActionCallback),
            //                                                                        new Action(ShowNoneCallback),
            //                                                                        CursorKey);

            // todo add service that the tool is locked / requires beehive
            this.Prefab = null;
        }

        public void Load()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
            this._eventBus.Register((object)this);


            // create a beehive to check if system is unlocked
            string prefabName = "Beehive.Folktails";
            _beehive = _buildingService.GetBuildingPrefab(prefabName);

            if (_beehive != null)
            {
                Debug.Log("Load Hive");
                this.Prefab = _beehive.GetComponentFast<PlaceableBlockObject>();

                if (this.Prefab != null)
                {
                    Debug.Log("Load Prefab");
                }
            }
        }

        public void PostLoad()
        {
            if (this._singletonLoader.HasSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey))
            {
                // reload coordinates where a hive is to be planted
                _hiveCoordsNew = _singletonLoader.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Get(PlantBeehiveToolService.PlantBeehiveToolCoordKey);
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Set(PlantBeehiveToolCoordKey, _hiveCoordsNew);
        }

        public override void Enter()
        {


            if (_beehive == null)
            {
                return; // tool not available for this faction
            }
            else
            {
                bool isUnlocked = _buildingUnlockingService.Unlocked(_beehive);
                this._blockObjectRange = _beehive.GetComponentFast<BlockObjectRange>();

                if (true == isUnlocked)
                {
                    // activate tool
                    this._cursorService.SetTemporaryCursor(CursorKey);
                    //this._selectionToolProcessor.Enter();
                    this._inputService.AddInputProcessor((IInputProcessor)this);
                    this._areaPicker = this._areaPickerFactory.Create();

                    // highlight area
                    //HighlightExistingHiveArea();
                    //_areaHighlightingService.Highlight();

                }
                else
                {
                    return;
                }
            }
        }
        public override void Exit()
        {
            //this._selectionToolProcessor.Exit();
            this._cursorService.ResetTemporaryCursor();
            this._inputService.RemoveInputProcessor((IInputProcessor)this);
            this._previewPlacer.HideAllPreviews();
            this._areaPicker = (AreaPicker)null;

        }
        public bool ProcessInput()
        {
            if (this.Prefab != null)
            {
                return this._areaPicker.PickBlockObjectArea(this.Prefab, Orientation.Cw0, FlipMode.Unflipped, new AreaPicker.BlockObjectAreaCallback(this.PreviewCallback), new AreaPicker.BlockObjectAreaCallback(this.ActionCallback));
            }
            else
            { 
                return false;
            }
        }

        private void PreviewCallback(IEnumerable<Placement> placements)
        {
            this.ShowPreviews(placements);
        }
        private void ActionCallback(IEnumerable<Placement> placements)
        {

            var bottomObject = this._blockService.GetBottomObjectAt(placements.First().Coordinates);

            if (bottomObject != null)
            {
                OnSelectableObjectSelected(bottomObject);
                Debug.Log("Action Ok");
            }
            else
            {

                Debug.Log("Action Failed");
            }

            this._previewPlacer.HideAllPreviews();
        }
        private void ShowPreviews(IEnumerable<Placement> placements)
        {
            this._previewPlacer.ShowPreviews(placements);
        }

        /*
        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            // only take first input block
            Vector3Int startCoord =     inputBlocks.First();

            Debug.Log("PBTS: PreV: " + startCoord);

            if (startCoord != Vector3Int.zero)
            {
                HighlightCursorArea(startCoord, _cBeehiveRadius);

                var bottomObject = this._blockService.GetBottomObjectAt(startCoord);

                if (bottomObject != null)
                {
                    Debug.Log("PBTS: PreV 3: " + startCoord);
                    _areaHighlightingService.AddForHighlight((BaseComponent)bottomObject);
                }

            }

            // highlight everything added to the service above
            _areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            // only take first input block
            Vector3Int startCoord = inputBlocks.First();

            Debug.Log("PBTS: Act: " + startCoord);

            if (startCoord != Vector3Int.zero)
            {
                HighlightCursorArea(startCoord, _cBeehiveRadius);

                var bottomObject = this._blockService.GetBottomObjectAt(startCoord);

                if (bottomObject != null)
                {
                    OnSelectableObjectSelected(bottomObject);
                }
            }

            // highlight everything added to the service above
            _areaHighlightingService.Highlight();
        }

        private void ShowNoneCallback()
        {
            _areaHighlightingService.UnhighlightAll();
        }
        */
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

            Vector2 vector2 = cursorPos.XY();
            Vector2Int vector2Int = new Vector2Int(cursorPos.y, cursorPos.x);
            (int num1, int num2) = ((int)((double)vector2.x - ((double)vector2Int.x / 2.0) - (double)radiusRect), ((int)((double)vector2.x - ((double)vector2Int.x / 2.0) + (double)radiusRect)));
            (int num3, int num4) = ((int)((double)vector2.y - ((double)vector2Int.y / 2.0) - (double)radiusRect), ((int)((double)vector2.y - ((double)vector2Int.y / 2.0) + (double)radiusRect)));

            for (int x = num1; x < num2; ++x)
            {
                for (int y = num3; y < num4; ++y)
                {
                    area.Add(new Vector3Int(x, y, cursorPos.z));
                }
            }
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

                _areaHighlightingService.DrawTile(block, this._colors.BuildingRangeTile);
            }
        }

        public void OnSelectableObjectSelected(BaseComponent hitObject)
        {
            BuilderPrioritizable prioritizable = null;  

            // check if the hit object is a plant
            hitObject.TryGetComponentFast<Plantable>(out Plantable plantable);
            hitObject.TryGetComponentFast<BlockObject>(out BlockObject blockObject);

            if ((plantable != null)
                && (blockObject != null))
            {
                // reserve the coordinates
                _hiveCoordsNew.Add(blockObject.Coordinates);

                // mark the plant to be deleted
                Demolishable demolishable = blockObject.GetComponentFast<Demolishable>();
                demolishable.Mark();
                demolishable.TryGetComponentFast<BuilderPrioritizable>(out prioritizable);

                // increase the priority of the plant which is to be deleted
                if (prioritizable != null)
                {
                    prioritizable.SetPriority(Timberborn.PrioritySystem.Priority.High);
                }

                // highlighting update
                HighlightReservedHiveArea();
                HighlightExistingHiveArea();

                //
                Debug.Log("PBTS: New Selection!");
            }
            else
            {

                Debug.Log("PBTS: No Object!");
            }
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
                BlockObject block = this._beehive.GetComponentFast<BlockObject>();

                if (null != buildingPlacer)
                {
                    IBlockObjectPlacer placer= buildingPlacer.GetMatchingPlacer(block);

                    if (null != placer)
                    {
                        placer.Place(block, placement);
                    }
                }
            }
        }

        [OnEvent]
        public void OnPlantBeehiveToolRegisterHiveEvent(PlantBeehiveToolRegisterHiveEvent PlantBeehiveToolRegisterHiveEvent)
        {
            if (null == PlantBeehiveToolRegisterHiveEvent)
            {
                return;
            }
            else // event exists
            {
                if (!_hiveRegistry.Contains(PlantBeehiveToolRegisterHiveEvent.Hive))
                {
                    _hiveRegistry.Add(PlantBeehiveToolRegisterHiveEvent.Hive);

                    Debug.Log("OPBTRHE: " + _hiveRegistry.Count);
                }
            }
        }

        [OnEvent]
        public void OnPlantBeehiveToolUnregisterHiveEvent(PlantBeehiveToolUnregisterHiveEvent PlantBeehiveToolUnregisterHiveEvent)
        {
            if (null == PlantBeehiveToolUnregisterHiveEvent)
            {
                return;
            }
            else // event exists
            {
                _hiveRegistry.Remove(PlantBeehiveToolUnregisterHiveEvent.Hive);

                Debug.Log("OPBTUHE: " + _hiveRegistry.Count);
            }
        }
    }
}
