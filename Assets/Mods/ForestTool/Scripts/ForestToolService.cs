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
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.DefaultControls;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Timberborn.BaseComponentSystem;
using Cordial.Mods.ForestTool.Scripts.UI.Events;
using System.Security;
using Timberborn.CoreUI;
using Timberborn.SelectionSystem;
using Timberborn.BlockSystem;
using Moq;

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

        private static readonly string _defaultResource = "Pine";

        private readonly ToolManager _toolManager;
        private readonly ILoc _loc;
        private EventBus _eventBus;
        private static VisualElement _root;

        // area selection 
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;

        // planting
        private PlantingAreaValidator _plantingAreaValidator;
        private readonly PlantingSelectionService _plantingSelectionService;
        private PlantingService _plantingService;
        private TerrainAreaService _terrainAreaService;

        // highlighting
        private readonly Colors _colors;
        private readonly AreaHighlightingService _areaHighlightingService;

        // availability 
        private BuildingUnlockingService _buildingUnlockingService;
        private BuildingService _buildingService;

        private ForestToolPrefabSpecService _forestToolPrefabSpecService;

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
                            Colors colors,
                            PlantingService plantingService,
                            TerrainAreaService terrainAreaService,
                            ToolUnlockingService toolUnlockingService,
                            ILoc loc,
                            ToolManager toolManager,
                            ToolButtonService toolButtonService,
                            EventBus eventBus,
                            BuildingService buildingService,
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


            _areaHighlightingService = areaHighlightingService;
            _colors = colors;

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
                Building _forester = _buildingService.GetBuildingPrefab(prefabName);

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
            //string resourceName = GetRandomPlantableName();
            //Debug.Log("RN: " + resourceName);

            foreach (Vector3Int leveledCoordinate in this._terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                this._areaHighlightingService.DrawTile(leveledCoordinate, this._colors.PlantingToolTile);
            }

            //    // workaround so that highlighting doesn't toggle at a high cycle, 
            //    // add preview of empty spots as pine trees. available to both default factions
            //    if ((resourceName.Equals(ForestToolParam.NameEmpty, StringComparison.OrdinalIgnoreCase))
            //    || (resourceName.Equals(string.Empty,StringComparison.OrdinalIgnoreCase))
            //    )
            //{
            //    Debug.Log("RS: " + resourceName+ " - " + _defaultResource);
            //    resourceName = _defaultResource;
            //}

            //this._areaHighlightingService.DrawTile(inputBlocks, this._colors.SelectionToolHighlight);

            //this._plantingSelectionService.HighlightMarkableArea(inputBlocks, ray, resourceName);

            // highlight everything added to the service above
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
