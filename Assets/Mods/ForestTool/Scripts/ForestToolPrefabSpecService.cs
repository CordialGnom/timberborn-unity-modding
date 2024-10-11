using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.PrefabSystem;
using Timberborn.Forestry;
using Timberborn.GameFactionSystem;
using Timberborn.FactionSystem;


namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolPrefabSpecService : ILoadableSingleton
    {

        // access to specs
        private static PrefabService _prefabService;

        // access to faction
        private static FactionService _factionService;

        // faction information
        private static string _factionId;

        public string FactionId => _factionId;

        public ForestToolPrefabSpecService( PrefabService prefabService,
                                             FactionService factionService)
        {
            _prefabService = prefabService;
            _factionService = factionService;
        }

        public void Load()
        {
            if ((null == _prefabService)
                || (null == _factionService))
            {
                Debug.LogError("ForestTool: Missing Service");
            }
            else
            {
                _factionId = GetFactionName();

                // only call parameter init once
                ForestToolParam.ForestToolPrefabSpecService = this;
                ForestToolParam.InitConfigDefault();
            }
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
            return treeTypes.ToImmutableArray<string>();
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
        public ImmutableArray<string> GetAllForestryPlantables()
        {
            List<string> treeTypes = new();

            if (null != _prefabService)
            {
                var treeComponents = _prefabService.GetAll<TreeComponent>();
                var bushComponents = _prefabService.GetAll<Bush>();

                foreach (var treeObject in treeComponents)
                {
                    treeTypes.Add(treeObject.name);
                }

                foreach (var bushObject in bushComponents)
                {
                    treeTypes.Add(bushObject.name);
                }

            }

            return treeTypes.ToImmutableArray<string>();
        }
    }
}
