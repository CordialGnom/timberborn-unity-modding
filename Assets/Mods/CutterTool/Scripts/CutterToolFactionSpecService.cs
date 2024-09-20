using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Buildings;
using Timberborn.FactionSystem;
using Timberborn.GameFactionSystem;
using Timberborn.ScienceSystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.Persistence;
using Timberborn.PrefabGroupSystem;
using Timberborn.PrefabSystem;
using Timberborn.Forestry;


namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolFactionSpecService : ILoadableSingleton
    {

        // to update configuration based on active faction
        private static FactionSpecificationService _factionSpecService;
        private static FactionService _factionService;
        private static FactionSpecification _factionSpecification;
        private static string _factionId;

        // access to specs
        private static ISpecificationService _specificationService;
        private static PrefabGroupService _prefabGroupService;
        private static PrefabService _prefabService;
        private static PrefabNameMapper _prefabNameMapper;

        // availability 
        private BuildingUnlockingService _buildingUnlockingService;
        private BuildingService _buildingService;

        public CutterToolFactionSpecService( BuildingService buildingService,
                                             BuildingUnlockingService buildingUnlockingService,
                                             FactionSpecificationService factionSpecificationService, 
                                             FactionService factionService,
                                             ISpecificationService specificationService,
                                             PrefabGroupService prefabGroupService,
                                             PrefabService prefabService,
                                             PrefabNameMapper prefabNameMapper)
        {
            _buildingService = buildingService;
            _buildingUnlockingService = buildingUnlockingService;
            _factionSpecService = factionSpecificationService;
            _factionService = factionService;
            _specificationService = specificationService;
            _prefabGroupService = prefabGroupService;   
            _prefabNameMapper = prefabNameMapper;
            _prefabService = prefabService;
        }

        public void Load()
        {
            if (null == _factionService)
            {
                Debug.LogError("CutterTool: No Faction Service");
            }

            if (null == _factionSpecService)
            {
                Debug.LogError("CutterTool: No Faction Specification Service");
            }

            _factionId = GetFactionName();

            if (_factionId == "")
            {
                Debug.LogError("CutterTool: No faction found");
            }
        }

        public static string FactionId
        {
            get { return _factionId; }
        }

        // not used, access available prefabs via prefabservice (below)
        public ImmutableArray<string> GetFactionTrees()
        {
            string prefabFound = "";
            ImmutableArray<string> prefabGroups;
            List<string> treeTypes = new();

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
                    Debug.LogError("CutterTool: integration error: " + prefabFound);
                }
            }
            else
            {
                Debug.LogError("CutterTool: Faction Service NA");
            }
            return treeTypes.ToImmutableArray<string>();
        }
        public ImmutableArray<string> GetAllTrees()
        {
            List<string> treeTypes = new();

            if (null != _prefabService)
            { 
                var treeComponents = _prefabService.GetAll<TreeComponent>();

                //todo Cordial: Load a prefab group
                foreach (var treeObject in treeComponents)
                {
                    treeTypes.Add(treeObject.name);
                }
            }
            else
            {
                Debug.LogError("CutterTool: Prefabservice NA");
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
