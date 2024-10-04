using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.PrefabSystem;
using Timberborn.Forestry;
using TimberApi.DependencyContainerSystem;
using Timberborn.Fields;


namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverridePrefabSpecService : ILoadableSingleton
    {

        // access to specs
        private static PrefabService _prefabService;

        public PlantingOverridePrefabSpecService( PrefabService prefabService)
        {
            _prefabService = prefabService;
        }

        public void Load()
        {
            if (null == _prefabService)
            {
                Debug.LogError("PO: Missing Service");
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

        public bool VerifyPrefabName(string prefabNameInp)
        {
            PrefabNameMapper prefabNameMapper = DependencyContainer.GetInstance<PrefabNameMapper>();

            string prefabNameOut = "";
            bool prefabValid = false;

            if (null != prefabNameMapper)
            {
                if (prefabNameMapper.TryGetPrefabName(prefabNameInp, out prefabNameOut))
                {
                    prefabValid = true;
                }
            }
            return prefabValid;
        }
        public bool CheckIsTree(string prefabNameInp)
        {
            ImmutableArray<string> treeTypes = GetAllTrees();


            return treeTypes.Contains(prefabNameInp.Replace(" ", ""));
        }

        public ImmutableArray<string> GetAllCrops()
        {
            List<string> cropTypes = new();

            if (null != _prefabService)
            {
                var cropComponents = _prefabService.GetAll<Crop>();

                //todo Cordial: Load a prefab group
                foreach (var crop in cropComponents)
                {
                    cropTypes.Add(crop.name);
                }
            }
            return cropTypes.ToImmutableArray<string>();
        }

        public bool CheckIsCrop(string prefabNameInp)
        {
            ImmutableArray<string> cropTypes = GetAllCrops();

            return cropTypes.Contains(prefabNameInp.Replace(" ", ""));
        }

    }
}
