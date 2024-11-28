using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Cordial.Mods.DeletionTool.Scripts.UI;
using NUnit;
using Timberborn.AreaSelectionSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.CoreUI;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.PrefabSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.DeletionTool.Scripts
{
    public class DeletionToolService : Tool, ILoadableSingleton, IDeletionTool, IInputProcessor
    {
        // tool descriptions
        private static readonly string TitleLocKey = "Cordial.DeletionTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.DeletionTool.Description";
        private static readonly string CursorKey = "DeleteBuildingCursor";
        public static readonly string DeletionSelectKey = "MouseLeft";
        public static readonly string ToolPromptLocKey = "DeletionTool.Prompt.Objects";
        public static readonly string AbortPromptLocKey = "Cordial.DeletionTool.Prompt.Abort";

        // tool setup
        private readonly ILoc _loc;
        private ToolDescription _toolDescription;      // is used
        private readonly ToolUnlockingService _toolUnlockingService;

        // selection
        private readonly InputService _inputService;
        private bool _selectionActive;
        private bool _selectionIsDemolishable;
        private bool _selectionIsPath;
        private bool _paused;

        // highlighting
        private readonly Colors _colors;
        private readonly DialogBoxShower _dialogBoxShower;

        // deletion area / component(s)
        private readonly BlockService _blockService;
        private BlockObject _startObject;
        private string _startObjectName;
        private bool _pathSelectActive;
        private readonly List<BlockObject> _selectedObjects = new();
        private readonly List<BlockObject> _temporaryObjects = new();
        private readonly HashSet<BlockObject> _stackedObjects = new();

        // TB demolotion 
        private readonly AreaBlockObjectPickerFactory _areaBlockObjectPickerFactory;
        private readonly BlockObjectSelectionDrawerFactory _blockObjectSelectionDrawerFactory;
        private readonly BlockObjectSelectionDrawer _blockObjectSelectionDrawer;
        private AreaBlockObjectPicker _areaBlockObjectPicker;

        private readonly CursorService _cursorService;
        private readonly PlantingService _plantingService;
        private readonly EntityService _entityService;


        public DeletionToolService( ToolUnlockingService toolUnlockingService,
                                    Colors colors,
                                    ILoc loc,
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
            _colors = colors;
            _loc = loc;

            this._areaBlockObjectPickerFactory = areaBlockObjectPickerFactory;
            this._blockObjectSelectionDrawerFactory = blockObjectSelectionDrawerFactory;

            this._blockObjectSelectionDrawer = this._blockObjectSelectionDrawerFactory.Create(this._colors.DeletedObjectHighlightColor,
                                                                                                this._colors.DeletedAreaTileColor,
                                                                                                this._colors.DeletedAreaSideColor);
        }

        public void Load()
        {
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).Build();
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
                _selectionActive =      true;
                _startObject =        blockObjects.First();
                _startObjectName =    _startObject.GetComponentFast<Prefab>().PrefabName;

                if (_startObject != null)
                {
                    _startObject.TryGetComponentFast<Demolishable>(out var demolishable);

                    _selectionIsDemolishable = (demolishable != null);

                    _selectionIsPath = (_startObjectName.Contains("Path"));
                }
            }

            if (_selectionActive)
            {

                foreach (var blockObject in blockObjects)
                {
                    if (blockObject != null)
                    {
                        var blockBaseName = blockObject.GetComponentFast<Prefab>().PrefabName;

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
                this._blockObjectSelectionDrawer.Draw(blockObjects, start, end, selectingArea);
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
                _temporaryObjects.AddRange(blockObjects);

                foreach (var blockObject in blockObjects)
                {
                    if (blockObject != null)
                    {
                        var blockBaseName = blockObject.GetComponentFast<Prefab>().PrefabName;

                        if (_startObjectName == blockBaseName)
                        {
                            _selectedObjects.Add(blockObject);
                        }
                    }

                }

                this.Pause();
                this._dialogBoxShower.Create().SetLocalizedMessage(ToolPromptLocKey).SetConfirmButton(new Action(this.OnDeleteConfirmed)).SetCancelButton(new Action(this.OnDeleteCanceled)).SetOffset(new Vector2Int(0, -200)).AddContent( this.GetDialogBoxContent((IEnumerable<BlockObject>)this._selectedObjects)).Show();

            }
        }

        private void ShowNoneCallback()
        {
            this.ResetSelection();
            this._blockObjectSelectionDrawer.StopDrawing();
        }

        private void DemolishObject( BlockObject blockObject)
        {
            blockObject.GetComponentFast<Demolishable>().Mark();
            this._plantingService.UnsetPlantingCoordinates(blockObject.Coordinates);

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

            // iterate through distinct list (removes duplicates)
            IEnumerable<BlockObject> selectedObjects = _selectedObjects.Distinct();
            foreach (BlockObject blockObject in selectedObjects)
            {
                
                if (_selectionIsDemolishable)
                {
                    DemolishObject(blockObject);
                }
                else if (_selectionIsPath)
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
                            if (stackObject.GetComponentFast<Prefab>().PrefabName != _startObjectName)
                            {
                                // different type, do not delete any at this position
                                deleteAbort =       true;   // per iteration
                                deleteAbortMsg =    true;   // for all objects --> final prompt
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

        private void ResetSelection()
        {
            _selectedObjects.Clear();
            _temporaryObjects.Clear();
            _selectionActive = false;
            _selectionIsDemolishable = false;
            _selectionIsPath = false;
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
