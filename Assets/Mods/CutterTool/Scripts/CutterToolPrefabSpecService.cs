using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.SingletonSystem;
using UnityEngine;
using Timberborn.PrefabSystem;
using Timberborn.Forestry;


namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolPrefabSpecService : ILoadableSingleton
    {

        // access to specs
        private static PrefabService _prefabService;

        public CutterToolPrefabSpecService( PrefabService prefabService )
        {
            _prefabService = prefabService;
        }

        public void Load()
        {
            if (null == _prefabService)
            {
                Debug.LogError("CutterTool: No Prefab Service");
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
            else
            {
                Debug.LogError("CutterTool: Prefabservice NA");
            }
            return treeTypes.ToImmutableArray<string>();
        }

    }
}
