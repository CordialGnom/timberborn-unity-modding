using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.PrefabSystem;
using Timberborn.Forestry;
using TimberApi.DependencyContainerSystem;
using Timberborn.Modding;


namespace Cordial.Mods.ForesterUpdate.Scripts
{
    public class ForesterUpdatePrefabSpecService : ILoadableSingleton
    {

        // access to specs
        private static PrefabService _prefabService;

        public ForesterUpdatePrefabSpecService( PrefabService prefabService)
        {
            _prefabService = prefabService;
        }

        public void Load()
        {
            if (null == _prefabService)
            {
                Debug.LogError("ForestTool: Missing Service");
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
    }
}
