using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.AreaSelectionSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BlueprintSystem;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.Cutting;
using Timberborn.Demolishing;
using Timberborn.DemolishingUI;
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

namespace Cordial.Mods.DemolitionTool.Scripts
{
    public class DemolitionToolService : Tool, ILoadableSingleton, IDemolitionTool, IInputProcessor
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.DeleteThatThing.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.DeleteThatThing.Description";
        private static readonly string CursorKey = "DeleteBuildingCursor";
        private readonly ILoc _loc;
        public static readonly string DeletionSelectKey = "MouseLeft";
        public static readonly string ToolPromptLocKey = "DeletionTool.Prompt.Objects";
        public static readonly string AbortPromptLocKey = "Cordial.DeleteThatThing.Prompt.Abort";
        public static readonly string StumpPromptLocKey = "Cordial.DeleteThatThing.Prompt.Stump";
        public static readonly string DeadTreePromptLocKey = "Cordial.DeleteThatThing.Prompt.DeadTree";

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
        private readonly BlockService _blockService;
        private BlockObject _startObject;
        private string _startObjectName = String.Empty;
        private bool _pathSelectActive;
        private readonly List<BlockObject> _selectedObjects = new();
        private readonly HashSet<BlockObject> _stackedObjects = new();

        // TB demolotion 
        private readonly AreaBlockObjectPickerFactory _areaBlockObjectPickerFactory;
        private readonly BlockObjectSelectionDrawerFactory _blockObjectSelectionDrawerFactory;
        private BlockObjectSelectionDrawer _blockObjectSelectionDrawer;
        private AreaBlockObjectPicker _areaBlockObjectPicker;

        private readonly CursorService _cursorService;
        private readonly PlantingService _plantingService;
        private readonly EntityService _entityService;


        public DemolitionToolService( ToolUnlockingService toolUnlockingService,
                                    ILoc loc,
                                    ISpecService specService,
                                    PlantingService plantingService,
                                    BlockService blockService,
                                    InputService inputService,
                                    CursorService cursorService,
                                    EntityService entityService,
                                    AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
                                    BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory,
                                    DialogBoxShower dialogBoxShower)
        {
            _toolUnlockingService = toolUnlockingService;
            _dialogBoxShower = dialogBoxShower;
            _plantingService = plantingService;
            _cursorService = cursorService;
            _entityService = entityService;
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
            if ((true == selectionStarted)
                && (false == _selectionActive)
                && (0 < blockObjects.Count()))
            {
                _selectionActive =    true;
                _startObject =        blockObjects.First();
                _startObjectName =    _startObject.GetComponentFast<PrefabSpec>().PrefabName;

                if (_startObject != null)
                {
                    _startObject.TryGetComponentFast<Demolishable>(out var demolishable);
                    _startObject.TryGetComponentFast<BuildingSpec>(out var building);

                    _selectionIsDemolishable = (demolishable != null);
                    _selectionIsBuilding = (building != null);
                    _selectionIsPath = (_startObjectName.Contains("Path"));
                }

                if ((!_selectionIsDemolishable) && (!_selectionIsBuilding) && (!_selectionIsPath))
                {
                    // if the selected object is neither of these, then it should not be deleted
                    _selectionActive = false;
                    _startObject = null;
                    _startObjectName = String.Empty;
                }
            }

            if (_selectionActive)
            {
                foreach (var blockObject in blockObjects)
                {
                    if (blockObject != null)
                    {
                        var blockBaseName = blockObject.GetComponentFast<PrefabSpec>().PrefabName;

                        if (_startObjectName == blockBaseName)
                        {
                            _selectedObjects.Add(blockObject);
                        }
                    }

                }

                // iterate through distinct list (removes duplicates)
                IEnumerable<BlockObject> selectedObjects = _selectedObjects.Distinct();

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
                if (_startObjectName != String.Empty)
                {
                    foreach (var blockObject in blockObjects)
                    {
                        if (blockObject != null)
                        {
                            var blockBaseName = blockObject.GetComponentFast<PrefabSpec>().PrefabName;

                            if (_startObjectName == blockBaseName)
                            {
                                _selectedObjects.Add(blockObject);
                            }
                        }
                    }

                    // check if demolishable is a stump/dead tree, or a live one, to set a different dialog box
                    if (_selectionIsDemolishable)
                    {
                        TreeComponentSpec treeComponent = _startObject.GetComponentFast<TreeComponentSpec>();
                        if (treeComponent != null)
                        {
                            if (CheckTreeIsStump(_startObject))
                            { 
                                ShowDeadStumpDialog();
                            }
                            else if (CheckTreeIsDead(_startObject))
                            {
                                // dialog box for dead trees
                                ShowDeadTreeDialog();
                            }
                            else
                            {
                                // live tree, set all trees for demolishing -> no dialog
                                DemolishAllObjects();
                            }
                        }
                        else
                        {
                            // non-tree demolishable, continue as normal
                            DemolishAllObjects();
                        }
                    }
                    else
                    {
                        ShowStandardDialog();
                    }
                }
                else
                {
                    // no valid objects selected
                    ResetSelection();
                }
            }
        }

        private void ShowDeadTreeDialog()
        {
            this.Pause();
            this._dialogBoxShower.Create()
                .SetLocalizedMessage(DeadTreePromptLocKey)
                .SetConfirmButton(new Action(this.DemolishDeadObjects))
                .SetCancelButton(new Action(this.OnDeleteCanceled))
                .SetOffset(new Vector2Int(0, -200))
                .SetMaxWidth(600)
                .AddContent(this.GetDialogBoxContent((IEnumerable<BlockObject>)this._selectedObjects)).Show();
        }
        private void ShowDeadStumpDialog()
        {
            this.Pause();
            this._dialogBoxShower.Create()
                .SetLocalizedMessage(StumpPromptLocKey)
                .SetConfirmButton(new Action(this.DemolishDeadObjects))
                .SetCancelButton(new Action(this.OnDeleteCanceled))
                .SetOffset(new Vector2Int(0, -200))
                .SetMaxWidth(600)
                .AddContent(this.GetDialogBoxContent((IEnumerable<BlockObject>)this._selectedObjects)).Show();
        }

        private void ShowStandardDialog()
        {
            this.Pause();
            this._dialogBoxShower.Create()
                .SetLocalizedMessage(ToolPromptLocKey)
                .SetConfirmButton(new Action(this.OnDeleteConfirmed))
                .SetCancelButton(new Action(this.OnDeleteCanceled))
                .SetOffset(new Vector2Int(0, -200))
                .AddContent(this.GetDialogBoxContent((IEnumerable<BlockObject>)this._selectedObjects)).Show();
        }

        private void ShowNoneCallback()
        {
            this.ResetSelection();
            this._blockObjectSelectionDrawer.StopDrawing();
        }

        private bool CheckTreeIsDead(BlockObject blockObject)
        {
            // check if it is a dead tree
            blockObject.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);

            if (livingResource != null)
            {
                if (livingResource.IsDead)
                {
                    // it is dead
                    return true;
                }
                else // a live tree
                {
                    return false;
                }
            }
            else
            {
                // different type of "demolishable"
            }
            return false;
        }
        private bool CheckTreeIsStump(BlockObject blockObject)
        {
            // check if it is a stump
            blockObject.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);
            blockObject.TryGetComponentFast<Cuttable>(out Cuttable cuttable);
            blockObject.TryGetComponentFast<Growable>(out Growable growable);

            if ((cuttable != null)
                && (livingResource != null)
                && (growable != null))
            {
                bool invIsEmpty = livingResource.GetComponentFast<GoodStack>().Inventory.IsEmpty;
                bool growthDone = (growable.IsGrown);
                bool cuttableEmpty = (cuttable.Yielder.Yield.Amount == 0);

                // is a stump or not
                if (invIsEmpty && cuttableEmpty && growthDone)
                {
                    return true;
                }
                else // a tree or a markable stump
                {
                    return false;
                }
            }
            else
            {
                // different type of "demolishable"
            }
            return false;
        }

        private void DemolishAllObjects()
        {
            IEnumerable<BlockObject> selectedObjects = _selectedObjects.Distinct();
            foreach (BlockObject blockObject in selectedObjects)
            {
                blockObject.GetComponentFast<Demolishable>().Mark();
                this._plantingService.UnsetPlantingCoordinates(blockObject.Coordinates);
            }
            this.ResetSelection();
            this.Unpause();
        }
        private void DemolishDeadObjects()
        {
            IEnumerable<BlockObject> selectedObjects = _selectedObjects.Distinct();
            foreach (BlockObject blockObject in selectedObjects)
            {
                if (CheckTreeIsDead(blockObject))
                {
                    blockObject.GetComponentFast<Demolishable>().Mark();
                    this._plantingService.UnsetPlantingCoordinates(blockObject.Coordinates);
                }
            }
            this.ResetSelection();
            this.Unpause();
        }
        private void OnDeleteConfirmed()
        {
            this.DeleteBlockObjects();
            this.ResetSelection();
            this.Unpause();
        }

        private void OnDeleteCanceled()
        {
            this.ResetSelection();
            this.Unpause();
        }
        private void OnAbortConfirmed()
        {
            this.ResetSelection();
            this.Unpause();
        }

        private void DeleteBlockObjects()
        {
            bool deleteAbortMsg = false;

            if (_selectionIsDemolishable)
            {
                DemolishAllObjects();
            }
            else
            {
                // iterate through distinct list (removes duplicates)
                IEnumerable<BlockObject> selectedObjects = _selectedObjects.Distinct();
                foreach (BlockObject blockObject in selectedObjects)
                {
                    if (_selectionIsPath)
                    {
                        // only delete objects on same Z layer
                        if (_startObject.Coordinates.z == blockObject.Coordinates.z)
                        {
                            this._entityService.Delete((BaseComponent)blockObject);
                        }
                    }
                    else
                    {
                        bool deleteAbort = false;

                        // standard object, e.g. building
                        // check if other objects are stacked on top: 
                        _stackedObjects.Clear();
                        AddBlockObjectsRecursively(blockObject);

                        if (_stackedObjects.Count > 1)
                        {
                            foreach (var stackObject in _stackedObjects)
                            {
                                // different object, same location
                                if (stackObject.GetComponentFast<PrefabSpec>().PrefabName != _startObjectName)
                                {
                                    // different type, do not delete any at this position
                                    deleteAbort = true;   // per iteration
                                    deleteAbortMsg = true;   // for all objects --> final prompt
                                    break;
                                }
                            }

                            if (!deleteAbort)
                            {
                                this._entityService.Delete((BaseComponent)blockObject);
                            }
                        }
                        else
                        {
                            this._entityService.Delete((BaseComponent)blockObject);
                        }
                    }
                }

                if (deleteAbortMsg)
                {
                    this._dialogBoxShower.Create()
                        .SetLocalizedMessage(AbortPromptLocKey)
                        .SetConfirmButton(new Action(this.OnAbortConfirmed))
                        .SetOffset(new Vector2Int(0, -200))
                        .AddContent(this.GetDialogBoxContent((IEnumerable<BlockObject>)this._selectedObjects)).Show();
                }
            }
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
