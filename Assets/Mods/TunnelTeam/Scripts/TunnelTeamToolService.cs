using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.AreaSelectionSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.BlueprintSystem;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.Coordinates;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.Forestry;
using Timberborn.GoodStackSystem;
using Timberborn.Growing;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.PrefabSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Timberborn.TerrainSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ScienceSystem;
using TimberApi.DependencyContainerSystem;

namespace Cordial.Mods.TunnelTeam.Scripts
{
    public class TunnelTeamToolService : Tool, ILoadableSingleton, ITunnelTeamTool, IInputProcessor
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.TunnelTeam.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.TunnelTeam.Description";
        private static readonly string CursorKey = "DeleteBuildingCursor";
        private readonly ILoc _loc;
        public static readonly string DeletionSelectKey = "MouseLeft";

        // tool setup
        private ToolDescription _toolDescription;      // is used
        private readonly ToolUnlockingService _toolUnlockingService;

        // selection
        private readonly ISpecService _specService;
        private readonly InputService _inputService;
        private bool _selectionActive;
        private bool _selectionIsDemolishable;
        private bool _selectionIsBuilding;
        private bool _selectionIsPath;
        private bool _paused;

        // highlighting
        private readonly DialogBoxShower _dialogBoxShower;

        // deletion area / component(s)
        private readonly IBlockService _blockService;
        private BlockObject _startObject;
        private string _startObjectName = String.Empty;
        private bool _pathSelectActive;
        private readonly List<BlockObject> _selectedObjects = new();
        private readonly HashSet<BlockObject> _stackedObjects = new();

        // terrain
        public readonly ITerrainService _terrainService;
        public readonly TerrainPicker _terrainPicker;

        // building placement
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;
        private BuildingSpec _tunnelBuilding;

        // TB demolition 
        private readonly AreaBlockObjectPickerFactory _areaBlockObjectPickerFactory;
        private readonly BlockObjectSelectionDrawerFactory _blockObjectSelectionDrawerFactory;
        private BlockObjectSelectionDrawer _blockObjectSelectionDrawer;
        private AreaBlockObjectPicker _areaBlockObjectPicker;

        private readonly CursorService _cursorService;
        private readonly PlantingService _plantingService;
        private readonly EntityService _entityService;


        public TunnelTeamToolService( ToolUnlockingService toolUnlockingService,
                                            BuildingUnlockingService buildingUnlockingService,
                                    ILoc loc,
                                    ISpecService specService,
                                    ITerrainService terrainService,
                                    TerrainPicker terrainPicker,
                                    IBlockService blockService,
                                            BuildingService buildingService,
                                    InputService inputService,
                                    CursorService cursorService,
                                    EntityService entityService,
                                    AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
                                    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
                                    DialogBoxShower dialogBoxShower)
        {
            _buildingUnlockingService = buildingUnlockingService;
            _toolUnlockingService = toolUnlockingService;
            _dialogBoxShower = dialogBoxShower;
            _terrainService = terrainService;
            _terrainPicker = terrainPicker;
            _cursorService = cursorService;
            _entityService = entityService;
            _buildingService = buildingService;
            _inputService = inputService;
            _blockService = blockService;
            _specService = specService;
            _loc = loc;

            this._areaBlockObjectPickerFactory = areaBlockObjectPickerFactory;
            this._blockObjectSelectionDrawerFactory = blockObjectSelectionDrawerFactory;
        }

        public void Load()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();

            this._blockObjectSelectionDrawer = this._blockObjectSelectionDrawerFactory.Create(Color.red,
                                                                                                Color.red,
                                                                                                Color.white);
        }
        public override void Enter()
        {
            // activate tool
            this._inputService.AddInputProcessor((IInputProcessor)this);
            this._cursorService.SetCursor(CursorKey);
            this._areaBlockObjectPicker = this._areaBlockObjectPickerFactory.CreatePickingUpwards();


            // create a beehive to be placed by the tool
            string prefabName = "Platform.Folktails";

            _tunnelBuilding = _buildingService.GetBuildingPrefab(prefabName);

            if (_tunnelBuilding == null)
            {
                this.Exit();
            }
        }
        public override void Exit()
        {
            this._blockObjectSelectionDrawer.StopDrawing();
            this._inputService.RemoveInputProcessor((IInputProcessor)this);
            this._cursorService.ResetCursor();
        }
        private VisualElement GetDialogBoxContent(IEnumerable<BlockObject> blockObjects)
        {
            return (VisualElement)null;
        }

        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }
        public override ToolDescription Description() => _toolDescription;

        bool IInputProcessor.ProcessInput()
        {
            if (this._paused)
                return false;

            return this._areaBlockObjectPicker.PickBlockObjects<BlockObject>(new AreaBlockObjectPicker.Callback(this.PreviewCallback), new AreaBlockObjectPicker.Callback(this.ActionCallback), new Action(this.ShowNoneCallback));

        }

        private void PreviewCallback( IEnumerable<BlockObject> blockObjects,
                                      Vector3Int start,
                                      Vector3Int end,
                                      bool selectionStarted,
                                      bool selectingArea)
        {
            // find "expected" height. 

            // unset terrain to this height.

            // place a platform

            // set terrain on top again

            if ((true == selectionStarted)
                && (false == _selectionActive)
                && (0 < blockObjects.Count()))
            {
                _selectionActive =    true;
            }

            if (_selectionActive)
            {
                // iterate through distinct list (removes duplicates)
                IEnumerable<BlockObject> selectedObjects = blockObjects.Distinct();

                this._blockObjectSelectionDrawer.Draw(selectedObjects, start, end, selectingArea);

                _selectedObjects.Clear();
            }
            else
            {
                // create empty list so nothing is marked
                IEnumerable<BlockObject> selectedObjects = new List<BlockObject>();
                this._blockObjectSelectionDrawer.Draw(selectedObjects, start, end, selectingArea);
            }
        }


        private void ActionCallback(IEnumerable<BlockObject> blockObjects,
                                      Vector3Int start,
                                      Vector3Int end,
                                      bool selectionStarted,
                                      bool selectingArea)
        {


            if (this.Locker != null)
            {
                this._toolUnlockingService.TryToUnlock((Tool)this);
            }
            else
            {
                // generate iteratable area
                Vector3Int v3Min = Vector3Int.Min(start, end);
                Vector3Int v3Max = Vector3Int.Max(start, end);

                // iterate through each coordinate, the z axis is constant
                for (int i = v3Min.x; i <= v3Max.x; i++)
                {
                    for (int j = v3Min.y; j <= v3Max.y; j++)
                    {
                        // new coordinate to check
                        Vector3Int local = new Vector3Int(i, j, v3Min.z);

                        // check if valid to unset terrain: 
                        // -> no immediate block object above, 
                        // -> no blockobject above that requires ground to be all the way through (e.g. underground storage)

                        if (_blockService.AllBlocksAtCoordinatesAllowAirBelow(local))
                        {
                            _terrainService.UnsetTerrain(local, 1);

                            // place new "platform" at cleared coordinates
                            PlaceTunnelBuilding(local);

                        }
                        else
                        {
                            Debug.Log("No Air allowed below: " + local);
                        }


                    }
                }
            }
        }

        private void PlaceTunnelBuilding(Vector3Int coordinates)
        {
            // create a new placement for the building
            Placement placement = new Placement(coordinates);

            if (_tunnelBuilding != null)
            {
                BlockObjectPlacerService buildingPlacer = DependencyContainer.GetInstance<BlockObjectPlacerService>();
                BlockObjectSpec block = this._tunnelBuilding.GetComponentFast<BlockObjectSpec>();

                if (null != buildingPlacer)
                {
                    IBlockObjectPlacer placer = buildingPlacer.GetMatchingPlacer(block);

                    if (null != placer)
                    {
                        try
                        {
                            placer.Place(block, placement);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("TunnelTeam: Building failed with: " + e.Message + " at " + coordinates);
                        }
                    }
                }
            }
        }


        private void ShowNoneCallback()
        {
            this.ResetSelection();
            this._blockObjectSelectionDrawer.StopDrawing();
        }

        private void ResetSelection()
        {
            _startObject = null;
            _startObjectName = String.Empty;
            _selectedObjects.Clear();
            _selectionActive = false;
            _selectionIsDemolishable = false;
            _selectionIsPath = false;
            _selectionIsBuilding = false;
        }

        private void Pause()
        {
            this._paused = true;
            this._cursorService.ResetCursor();
        }

        private void Unpause()
        {
            this._paused = false;
            this._cursorService.SetCursor(CursorKey);
        }

        private void AddBlockObjectsRecursively( BlockObject blockObject)
        {
            // only add block once
            if (this._stackedObjects.Add(blockObject))
            {
                this.AddConnectedBlockObjects(blockObject);
            }
        }

        private void AddConnectedBlockObjects( BlockObject blockObject)
        {
            foreach (Block block in blockObject.PositionedBlocks.GetAllBlocks().Where<Block>((Func<Block, bool>)(block => block.Stackable.IsStackable())))
            {
                this.AddValidBlockObjectsStackedWithBlock(block);
            }
        }
        private void AddValidBlockObjectsStackedWithBlock( Block block)
        {
            // forward is z + 1, not up!
            Vector3Int coordinates = (block.Coordinates + Vector3Int.forward);

            foreach (BlockObject blockObject in this._blockService.GetObjectsAt(coordinates))
            {
                this.AddBlockObjectsRecursively(blockObject);
            }
        }
    }
}
