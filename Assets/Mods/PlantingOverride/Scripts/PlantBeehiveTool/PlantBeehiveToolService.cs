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
using Timberborn.Planting;
using Timberborn.PrefabSystem;
using Timberborn.ScienceSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolService : Tool, ISaveableSingleton, IPostLoadableSingleton, IInputProcessor, IPlantBeehiveTool
    {
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
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;

        // selection
        private readonly InputService _inputService;
        private readonly BlockService _blockService;
        private readonly SelectableObjectRaycaster _selectableObjectRaycaster;
        private bool _plantBeehiveToolActive = true;

        // building placement
        private BuildingUnlockingService _buildingUnlockingService;
        private BuildingService _buildingService;
        private Building _beehive;

        // configuration storage
        private readonly ISingletonLoader _singletonLoader;
        private static List<Vector3Int> _hiveRegistry = new();

        private static readonly SingletonKey PlantBeehiveToolServiceKey = new SingletonKey("Cordial.PlantBeehiveToolService");
        private static readonly ListKey<Vector3Int> PlantBeehiveToolCoordKey = new ListKey<Vector3Int>("Cordial.PlantBeehiveToolCoordKey");

        public PlantBeehiveToolService( SelectableObjectRaycaster selectableObjectRaycaster,
                                            BuildingUnlockingService buildingUnlockingService,
                                            AreaHighlightingService areaHighlightingService,
                                            ToolUnlockingService toolUnlockingService,
                                            TerrainAreaService terrainAreaService,
                                            ISingletonLoader singletonLoader,
                                            BuildingService buildingService,
                                            CursorService cursorService,
                                            BlockService blockService,
                                            InputService inputService,
                                            EventBus eventBus,
                                            Colors colors,
                                            ILoc loc )
        {
            _selectableObjectRaycaster = selectableObjectRaycaster;
            _buildingUnlockingService = buildingUnlockingService;
            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService = toolUnlockingService;
            _terrainAreaService = terrainAreaService;
            _singletonLoader = singletonLoader;
            _buildingService = buildingService;
            _cursorService = cursorService;
            _blockService = blockService;
            _inputService = inputService;
            _eventBus = eventBus;
            _colors = colors;
            _loc = loc;

            // todo add service that the tool is locked / requires beehive


        }

        public void PostLoad()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
            this._eventBus.Register((object)this);

            _inputService.AddInputProcessor((IInputProcessor)this);

            if (this._singletonLoader.HasSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey))
            {
                // reload coordinates where a hive is to be planted
                _hiveRegistry = _singletonLoader.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Get(PlantBeehiveToolService.PlantBeehiveToolCoordKey);
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(PlantBeehiveToolService.PlantBeehiveToolServiceKey).Set(PlantBeehiveToolCoordKey, _hiveRegistry);
        }

        public override void Enter()
        {
            string prefabName = "Beehive.Folktails";

            // create a beehive to check if system is unlocked
            _beehive = _buildingService.GetBuildingPrefab(prefabName);

            if (_beehive == null)
            {
                return; // tool not available for this faction
            }
            else
            {
                bool isUnlocked = _buildingUnlockingService.Unlocked(_beehive);

                if (true == isUnlocked)
                {
                    // activate tool
                    this._cursorService.SetTemporaryCursor(CursorKey);
                    _plantBeehiveToolActive = true;
                }
                else
                {
                    return;
                }
            }
        }
        public override void Exit()
        {
            this._cursorService.ResetTemporaryCursor();
            _plantBeehiveToolActive = false;
            //this._eventBus.Post((object)new PlantBeehiveToolUnselectedEvent(this));

        }

        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }
        public override ToolDescription Description() => _toolDescription;

       
        bool IInputProcessor.ProcessInput()
        {
            if (!_plantBeehiveToolActive)
            {
                return false;
            }

            if (_inputService.MouseOverUI || !_inputService.MainMouseButtonDown)
            {
                return false;
            }

            if (_selectableObjectRaycaster.TryHitSelectableObject(out var hitObject))
            {
                OnSelectableObjectSelected(hitObject);
                return true;
            }

            return false;
        }

        public void OnSelectableObjectSelected(BaseComponent hitObject)
        {
            var selectableObjectName = hitObject.GetComponentFast<Prefab>().PrefabName;

            BuilderPrioritizable prioritizable = null;  

            // check if the hit object is a plant
            hitObject.TryGetComponentFast<Plantable>(out Plantable plantable);
            hitObject.TryGetComponentFast<BlockObject>(out BlockObject blockObject);

            if ((plantable != null)
                && (blockObject != null))
            {
                // reserve the coordinates
                _hiveRegistry.Add(blockObject.Coordinates);

                // mark the plant to be deleted
                Demolishable demolishable = blockObject.GetComponentFast<Demolishable>();
                demolishable.Mark();
                demolishable.TryGetComponentFast<BuilderPrioritizable>(out prioritizable);

                // increase the priority of the plant which is to be deleted
                if (prioritizable != null)
                {
                    prioritizable.SetPriority(Timberborn.PrioritySystem.Priority.High);
                }
            }
        }



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
        //    _hiveRegistry.Remove(coord);
        //}
        //public bool HasEntryAtCoord( Vector3Int coord)
        //{
        //    return _hiveRegistry.TryGetValue(coord, out string hiveName);
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
                if (_hiveRegistry.Contains(PlantBeehiveToolUnmarkEvent.Coordinates))
                {
                    // remove coordinates
                    _hiveRegistry.Remove(PlantBeehiveToolUnmarkEvent.Coordinates);

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
    }
}
