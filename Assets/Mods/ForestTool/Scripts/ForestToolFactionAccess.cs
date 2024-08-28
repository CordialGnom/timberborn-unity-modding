using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Reflection;
using Timberborn.Buildings;
using Timberborn.FactionSystem;
using Timberborn.GameFactionSystem;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.PlantingUI;
using Timberborn.ScienceSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Timberborn.Persistence;
using Timberborn.PrefabGroupSystem;
using Timberborn.PrefabSystem;
using System.Linq;
using TimberApi.Tools.ToolSystem;
using Timberborn.NaturalResources;


namespace Mods.ForestTool.Scripts
{
    public class ForestToolFactionSpecService : ILoadableSingleton
    {


        // to update configuration based on active faction
        private static FactionSpecificationService _factionSpecService;
        private static FactionService _factionService;
        private static FactionSpecification _factionSpecification;
        private static string _factionId;

        // access to specs
        private static ISpecificationService _specificationService;
        private static PrefabGroupService _prefabGroupService;
        private static PrefabNameMapper _prefabNameMapper;

        // availability 
        private BuildingUnlockingService _buildingUnlockingService;
        private BuildingService _buildingService;

        public ForestToolFactionSpecService( BuildingService buildingService,
                                             BuildingUnlockingService buildingUnlockingService,
                                             FactionSpecificationService factionSpecificationService, 
                                             FactionService factionService,
                                             ISpecificationService specificationService,
                                             PrefabGroupService prefabGroupService,
                                             PrefabNameMapper prefabNameMapper)
        {
            _buildingService = buildingService;
            _buildingUnlockingService = buildingUnlockingService;
            _factionSpecService = factionSpecificationService;
            _factionService = factionService;
            _specificationService = specificationService;
            _prefabGroupService = prefabGroupService;   
            _prefabNameMapper = prefabNameMapper;
        }

        public void Load()
        {

            if (null == _factionService)
            {
                Debug.LogError("ForestTool: No Faction Service");
            }

            if (null == _factionSpecService)
            {
                Debug.LogError("ForestTool: No Faction Specification Service");
            }

            _factionId = GetFactionName();

            if (_factionId == "")
            {
                Debug.LogError("ForestTool: No faction found");
            }

            // this._specificationService.GetSpecifications<PrefabGroupSpecification>((IObjectSerializer<PrefabGroupSpecification>)this._prefabGroupSpecificationDeserializer).Where<PrefabGroupSpecification>((Func<PrefabGroupSpecification, bool>)(spec => spec.Id == prefabGroup)).SelectMany<PrefabGroupSpecification, string>((Func<PrefabGroupSpecification, IEnumerable<string>>)(spec => (IEnumerable<string>)spec.Paths)).Select<string, GameObject>((Func<string, GameObject>)(path => this._assetLoader.Load<GameObject>(path))));

            // did not achieve natural resource access through timberapi
            // ForestToolSpecificationService.GetAll();
            // ForestToolSpecificationService.GetDefaultTreeNames();

            // only call parameter init once
            if (false == ForestToolParam.ParamInitDone)
            {
                ForestToolParam.ForestToolFactionSpecService = this;
                ForestToolParam.UpdateFromConfig();
            }
        }
        public static string FactionId
        {
            get { return _factionId; }
        }

        public ImmutableArray<string> GetFactionTrees()
        {
            string prefabFound = "";
            ImmutableArray<string> prefabGroups;
            List<string> treeTypes = new() { ForestToolParam.NameEmpty };

            if (null != _factionSpecService)
            {
                _factionSpecification = _factionSpecService.GetFaction(_factionId);

                prefabGroups = _factionSpecification.PrefabGroups;

                foreach (string prefabGroup in prefabGroups)
                {
                    string search = "aturalResource";       // "NaturalResources, lose the 'N', pos > 0
                    int pos = prefabGroup.IndexOf(search);

                    if (0 < pos)
                    {
                        Debug.Log("String Found: " +  prefabGroup);
                        prefabFound = prefabGroup;
                    }
                    else
                    {
                        // ignore entry
                    }
                }

                if (prefabFound == "NaturalResources.Folktails")
                {
                    treeTypes.Clear();
                    treeTypes.Add("ChestnutTree");
                    treeTypes.Add("Maple");
                }
                else if (prefabFound == "NaturalResources.IronTeeth")
                {
                    treeTypes.Clear();
                    treeTypes.Add("Mangrove");
                }
                else
                { 
                    Debug.LogError("ForestTool: integration error: " + prefabFound);
                }

                //_specificationService.GetSpecifications<NaturalResource>();

                // NaturalResources.Folktails

                //gameObjects = _prefabGroupService.AllPrefabs;

                
                //foreach (GameObject gameObject in gameObjects)
                //{
                //    Debug.Log("GO: " + gameObject.name);
                //}

                    // todo Cordial: Load a prefab group
                    //foreach (GameObject gameObject in gameObjects)
                    //{
                    //    string search = "Trees";

                    //    gameObject.

                    //    int pos = resource.IndexOf(search);

                    //    if (0 < pos)
                    //    {
                    //        // shorten whole string
                    //        string temp = resource.Substring(pos + search.Length).Trim();
                    //        string[] parts = temp.Split('/');
                    //        treeTypes.Add(parts[parts.Length - 1]);
                    //    }
                    //    else
                    //    {
                    //        // ignore entry
                    //    }

                    //}


                // returns more resources than trees: 
//                  [Info: Tree Tool] Found: NaturalResources / Bushes / Dandelion / Dandelion
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Carrot / Carrot
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Cattail / Cattail
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Potato / Potato
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Spadderdock / Spadderdock
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Sunflower / Sunflower
//                  [Info: Tree Tool] Found: NaturalResources / Crops / Wheat / Wheat
//                  [Info: Tree Tool] Found: NaturalResources / Trees / ChestnutTree / ChestnutTree
//                  [Info: Tree Tool] Found: NaturalResources / Trees / Maple / Maple
            }
            else
            {
                Debug.LogError("ForestTool: Faction Service NA");
            }
            return treeTypes.ToImmutableArray<string>();
        }

        public static bool VerifyPrefabName(string prefabNameInp)
        {
         
            string prefabNameOut = "";
            bool prefabValid = false;

            if (null != _prefabNameMapper)
            {
                if (_prefabNameMapper.TryGetPrefabName(prefabNameInp, out prefabNameOut))
                {
                    Debug.Log("Found: " + prefabNameOut);
                    prefabValid = true;
                }
            }

            return prefabValid;
        }
        private static string GetFactionName()
        {
            string factionId = "";
            FactionSpecification _activeFaction;

            if (null != _factionService)
            {
                _activeFaction = _factionService.Current;
                factionId = _activeFaction.Id;
            }

            return factionId;
        }
    }
}
