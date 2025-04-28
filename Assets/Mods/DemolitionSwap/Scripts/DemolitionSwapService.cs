using System;
using System.Collections.Generic;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.InputSystem;
using Timberborn.AreaSelectionSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.PathSystem;
using UnityEngine;
using System.Linq;
using Timberborn.Buildings;
using Timberborn.Demolishing;
using Timberborn.BaseComponentSystem;
using Timberborn.EntitySystem;
using Timberborn.Planting;

namespace Cordial.Mods.DemolitionSwap.Scripts
{
    public class DemolitionSwapService : ILoadableSingleton, IInputProcessor, IPriorityInputProcessor
    {
        private static readonly string TitleLocKey = "Cordial.DemolitionSwap.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.DemolitionSwap.Description";
        public static readonly string DemolitionSwapShortcutKey = "Cordial.DemolitionSwap.KeyBinding.DemolitionSwap";

        private static readonly string CursorKey = "DeleteBuildingCursor";

        public static readonly string DemolitionSelectKey = "MouseLeft";

        public static readonly string ToolPromptLocKey = "Cordial.DemolitionSwap.Prompt.Confirm";


        // input handling
        private readonly InputService _inputService;


        // tool handling and storage
        private readonly ToolManager _toolManager;
        private static Tool _storeTool;


        private bool _modeActive;

        // Prompts/UI
        private readonly DialogBoxShower _dialogBoxShower;

        // visualization
        private readonly AreaBlockObjectPickerFactory _areaBlockObjectPickerFactory;
        private readonly BlockObjectSelectionDrawerFactory _blockObjectSelectionDrawerFactory;
        private BlockObjectSelectionDrawer _blockObjectSelectionDrawer;
        private AreaBlockObjectPicker _areaBlockObjectPicker;
        private readonly CursorService _cursorService;

        // TB demolition 
        private readonly PlantingService _plantingService;
        private readonly EntityService _entityService;

        private readonly List<BlockObject> _deleteObjects = new();
        private readonly List<BlockObject> _demolishObjects = new();


        public DemolitionSwapService(ToolManager toolManager,
                                        InputService inputService,
                                        CursorService cursorService,
                                        EntityService entityService,
                                        PlantingService plantingService,
                                        DialogBoxShower dialogBoxShower,
                                        AreaBlockObjectPickerFactory areaBlockObjectPickerFactory,
                                        BlockObjectSelectionDrawerFactory blockObjectSelectionDrawerFactory)
        {
            _toolManager = toolManager;
            _inputService = inputService;
            _cursorService = cursorService;
            _entityService = entityService;
            _plantingService = plantingService;
            _dialogBoxShower = dialogBoxShower;

            this._areaBlockObjectPickerFactory = areaBlockObjectPickerFactory;
            this._blockObjectSelectionDrawerFactory = blockObjectSelectionDrawerFactory;

        }

        public void Load()
        {
            Debug.Log("DS: Load");
            _inputService.AddInputProcessor((IPriorityInputProcessor)this);

            this._blockObjectSelectionDrawer = this._blockObjectSelectionDrawerFactory.Create(Color.red,
                                                                                                Color.red,
                                                                                                Color.white);
        }
        void IPriorityInputProcessor.ProcessInput()
        {
            // check for keybinding, while mouse is up
            if ((_inputService.IsKeyHeld(DemolitionSwapShortcutKey))
                && (_inputService.MainMouseButtonUp))
            {
                // enter mode
                if (!_modeActive)
                {
                    // enter Demolition Mode
                    this.Enter();
                }
                else
                {
                    // keep stored config unchanged
                }

            }
            else if (_inputService.IsKeyHeld(DemolitionSwapShortcutKey))
            {
                // button down, check if demolition mode is active
                if (_modeActive)
                {
                    // all ok, ignore
                }
                else
                {
                    // do not activate mode, ongoing tool activity or whatever
                }
            }
            else if ((_modeActive)
                    && (_storeTool != null))
            {
                // must've just finished
                this.Exit();
            }
            else
            {
                // do nothing
            }
        }

        bool IInputProcessor.ProcessInput()
        {
            // mode is no longer active, shouldn't be here much longer
            if ((!_modeActive)
                || (!_inputService.IsKeyHeld(DemolitionSwapShortcutKey)))
            {
                return false;
            }

            // Enter Demolition Mode
            return _areaBlockObjectPicker.PickBlockObjects<BlockObject>(
                                                    new AreaBlockObjectPicker.Callback(this.PreviewCallback),
                                                    new AreaBlockObjectPicker.Callback(this.ActionCallback),
                                                    new Action(this.ShowNoneCallback));
        }

        private void Enter()
        {
            _modeActive = true;
            _cursorService.SetCursor(CursorKey);
            _inputService.AddInputProcessor((IInputProcessor)this);
            _areaBlockObjectPicker = _areaBlockObjectPickerFactory.CreatePickingUpwards();
            _storeTool = _toolManager.ActiveTool;
            _deleteObjects.Clear();
            _demolishObjects.Clear();

            Debug.Log("DS: Enter");
        }

        private void Exit()
        {
            _modeActive = false;
            _cursorService.ResetCursor();
            _inputService.RemoveInputProcessor(this);
            _toolManager.SwitchTool(_storeTool);
            _blockObjectSelectionDrawer.StopDrawing();
            _deleteObjects.Clear();
            _demolishObjects.Clear();

            Debug.Log("DS: Exit");
        }
        private void PreviewCallback(IEnumerable<BlockObject> blockObjects,
                                      Vector3Int start,
                                      Vector3Int end,
                                      bool selectionStarted,
                                      bool selectingArea)
        {
            this._blockObjectSelectionDrawer.Draw(blockObjects, start, end, selectingArea);


        }

        private void ActionCallback(IEnumerable<BlockObject> blockObjects,
                                      Vector3Int start,
                                      Vector3Int end,
                                      bool selectionStarted,
                                      bool selectingArea)
        {
            Debug.Log("DS: Action");

            foreach (var blockObject in blockObjects)
            {

                blockObject.TryGetComponentFast<Demolishable>(out var demolishable);
                blockObject.TryGetComponentFast<BuildingSpec>(out var building);
                blockObject.TryGetComponentFast<PathSpec>(out var pathSpec);

                if ((building != null) && (pathSpec != null))
                {
                    _deleteObjects.Add(blockObject);
                }
                else if (demolishable != null)
                {

                    _demolishObjects.Add(blockObject);
                }
                else
                {
                    // nothing relevant marked
                    Debug.Log("CS: Nope: " + blockObject.name);
                }
            }

            RemoveAllObjects();
        }
        private void ShowNoneCallback()
        {
            Debug.Log("DS: None");
        }

        private void RemoveAllObjects()
        {

            // iterate through distinct list (removes duplicates)
            IEnumerable<BlockObject> selectedObjects = _demolishObjects.Distinct();

            // remove all demolishables
            foreach (var blockObject in selectedObjects)
            {
                if (blockObject != null)
                {
                    // all in this list should be demolishables
                    blockObject.TryGetComponentFast<Demolishable>(out var demolishable);

                    if (demolishable != null)
                    {
                        demolishable.Mark();
                        this._plantingService.UnsetPlantingCoordinates(blockObject.Coordinates);
                    }
                }
            }

            // iterate through distinct list (removes duplicates)
            selectedObjects = _deleteObjects.Distinct();

            // remove all buildings/paths
            foreach (var blockObject in selectedObjects)
            {
                if (blockObject != null)
                {
                    this._entityService.Delete((BaseComponent)blockObject);
                }
            }
        }
    }
}
