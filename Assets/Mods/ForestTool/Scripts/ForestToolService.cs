using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TimberApi.Tools;
using Timberborn.Buildings;
using Timberborn.Localization;
using Timberborn.PrefabSystem;
using Timberborn.Planting;
using Timberborn.PlantingUI;
using Timberborn.ScienceSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Cordial.Mods.ForestTool.Scripts.UI.Events;
using Timberborn.SelectionSystem;
using Moq;
using Timberborn.BlueprintSystem;
using Timberborn.ForestryUI;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolService : Tool, ILoadableSingleton, IPlantingToolGroup, IForestTool
    {
        private static readonly string TitleLocKey = "Cordial.ForestTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.ForestTool.Description";
        private static readonly string RequirementLocKey = "Coridal.ForestTool.Requirement";
        private static readonly string ToolBuildingLocKey = "Building.Forester.DisplayName";

        private static readonly string CursorKey = "PlantingCursor";

        private static bool isUnlocked; 

        private readonly ToolManager _toolManager;
        private readonly ILoc _loc;
        private EventBus _eventBus;
        private static VisualElement _root;

        // area selection 
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;
        public readonly ISpecService _specService;

        // planting
        private PlantingAreaValidator _plantingAreaValidator;
        private readonly PlantingSelectionService _plantingSelectionService;
        private PlantingService _plantingService;
        private TerrainAreaService _terrainAreaService;

        // highlighting
        private readonly AreaHighlightingService _areaHighlightingService;
        public Color _plantingToolTile;
        public Color _toolNoActionTileColor;

        // availability 
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;

        private readonly ForestToolPrefabSpecService _forestToolPrefabSpecService;

        // planting parametrization
        Dictionary<string, bool> _treeToggleDict = new();
        private bool _emptySpotsEnabled = false;


        //private static readonly string[] _asResource = { "Oak", "Birch", "ChestnutTree", "Pine", "Maple" };

        // todo check how to access generic specifications, and beaver faction specifications
        //"C:\Users\simon\TreeMix\Assets\Timberborn\Resources\specifications\prefabcollections\PrefabCollectionSpecification.NaturalResources.naturalresources"

        // services and objects
        private readonly ToolButtonService _toolButtonService;  // todo check if required
        private ToolDescription _toolDescription;       // is used

        protected readonly MethodInfo EnterPlantingModeMethod;
        protected readonly MethodInfo ExitPlantingModeMethod;

        public ForestToolService(SelectionToolProcessorFactory selectionToolProcessorFactory,
                            PlantingSelectionService plantingSelectionService,
                            PlantingAreaValidator plantingAreaValidator,
                            AreaHighlightingService areaHighlightingService,
                            PlantingService plantingService,
                            TerrainAreaService terrainAreaService,
                            ToolUnlockingService toolUnlockingService,
                            ILoc loc,
                            ToolManager toolManager,
                            ToolButtonService toolButtonService,
                            EventBus eventBus,
                            BuildingService buildingService,
                            ISpecService specService,
                            BuildingUnlockingService buildingUnlockingService,
                            ForestToolPrefabSpecService forestToolPrefabSpecService
                                )
        {

            _selectionToolProcessor = selectionToolProcessorFactory.Create(new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.PreviewCallback),
                                                                                    new Action<IEnumerable<Vector3Int>,
                                                                                    Ray>(this.ActionCallback),
                                                                                    new Action(ShowNoneCallback),
                                                                                   CursorKey);

            _plantingAreaValidator = plantingAreaValidator;
            _plantingService = plantingService;
            _plantingSelectionService = plantingSelectionService;
            _terrainAreaService = terrainAreaService;
            _toolUnlockingService = toolUnlockingService;
            _buildingService = buildingService;
            _buildingUnlockingService = buildingUnlockingService;
            _forestToolPrefabSpecService = forestToolPrefabSpecService;
            _specService = specService;


            _areaHighlightingService = areaHighlightingService;

            _eventBus = eventBus;
            _loc = loc;
            _toolManager = toolManager;
            _toolButtonService = toolButtonService;
            _root = new VisualElement();

        }

        public void Load()
        {
            string text = this._loc.T<string>(RequirementLocKey, _loc.T(ToolBuildingLocKey));
            _toolDescription = new ToolDescription.Builder(_loc.T(TitleLocKey)).AddSection(_loc.T(DescriptionLocKey)).AddSection(text).Build();
            this._eventBus.Register((object)this);

            _plantingToolTile = new Color(0, 0.8f, 0, 1);
            _toolNoActionTileColor = new Color(0.7f, 0.7f, 0, 1);
        }

        public void SetToolGroup(ToolGroup toolGroup)
        {
            ToolGroup = toolGroup;
        }


        public override ToolDescription Description() => _toolDescription;

        public override void Enter()
        {
            // check if tool can be entered (forester available)
            // require access to either "Forester" or the "Trees". Therefore check if
            // the trees can be planted...

            // get faction forester specific building
            if ("" != _forestToolPrefabSpecService.FactionId)
            {
                string prefabName = "Forester." + _forestToolPrefabSpecService.FactionId;

                // create a forester to check if system is unlocked
                BuildingSpec _forester = _buildingService.GetBuildingPrefab(prefabName);

                IsUnlocked = _buildingUnlockingService.Unlocked(_forester);

                if (true == IsUnlocked)
                {
                    // activate tool
                    this._selectionToolProcessor.Enter();
                }
            }
            else
            {
                Debug.LogError("ForestTool: Faction not found");
            }

            this._eventBus.Post((object)new ForestToolSelectedEvent(this));
        }


        public bool IsUnlocked { get { return isUnlocked; } set { isUnlocked = value; } }

        public override void Exit()
        {
            this._plantingSelectionService.UnhighlightAll();
            this._selectionToolProcessor.Exit();
            this._eventBus.Post((object)new ForestToolUnselectedEvent(this));
        }

        // Preview and Action Callbacks required for selection Tool Processor Factor
        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                this._areaHighlightingService.DrawTile(leveledCoordinate, this._plantingToolTile);
            }
            this._areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (this.Locker != null)
                this._toolUnlockingService.TryToUnlock((Tool)this);
            else
                this._areaHighlightingService.UnhighlightAll();
                this.Plant(inputBlocks, ray);
        }
        private void ShowNoneCallback()
        {
            this._areaHighlightingService.UnhighlightAll();
        }

        private void Plant(IEnumerable<Vector3Int> inputBlocks, Ray ray)        // based on PlantingTool function
        {
            string resourceName; ;

            // here randomize plant name for each entry in list

            foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                resourceName = GetRandomPlantableName();

                if ((resourceName.Equals(ForestToolParam.NameEmpty, StringComparison.OrdinalIgnoreCase))
                    || (resourceName.Equals("", StringComparison.OrdinalIgnoreCase))
                )
                {
                    // remove possible exisiting planting coordinates
                    _plantingService.UnsetPlantingCoordinates(leveledCoordinate);                    
                }
                else
                {

                    if (_plantingAreaValidator.CanPlant(leveledCoordinate, resourceName))
                    {
                        _plantingService.SetPlantingCoordinates(leveledCoordinate, resourceName);
                    }
                }

            }
            _eventBus.Post((object)new PlantingAreaMarkedEvent());
        }

        private string GetRandomPlantableName()
        {
            return ForestToolParam.GetNextRandomResourceName();
        }

        public void PostProcessInput()      // originally virtual (to be called elsewhere)
        {
            // currently no implementation, just placeholder
            return;
        }

        [OnEvent]
        public void OnForestToolConfigChangeEvent(ForestToolConfigChangeEvent forestToolConfigChangeEvent)
        {
            if (null == forestToolConfigChangeEvent)
                return;

            if (ForestToolParam.ParamInitDone)
            {
                _emptySpotsEnabled = forestToolConfigChangeEvent.ForestToolConfig.EmptySpotsEnabled;
                ForestToolParam.SetResourceState(ForestToolParam.NameEmpty, _emptySpotsEnabled);

                _treeToggleDict = forestToolConfigChangeEvent.ForestToolConfig.GetTreeDict();
                foreach (KeyValuePair<string, bool> kvp in _treeToggleDict)
                {
                    ForestToolParam.SetResourceState(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
