using System.Collections.Generic;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Cordial.Mods.ForesterUpdate.Scripts
{
    internal class ForesterUpdateStateService : ILoadableSingleton, IPersistentEntity
    {
        private static Dictionary<Vector3Int, string> _foresterRegistry = new();

        private static readonly ComponentKey ForesterUpdateKey = new ComponentKey(nameof(ForesterUpdateStateService));
        private static readonly ListKey<Vector3Int> ForesterCoordinateKey = new ListKey<Vector3Int>("Cordial.ForesterCoordinateKey");
        private static readonly ListKey<string> ForesterTypeKey = new ListKey<string>("Cordial.ForesterTypeKey");

        public ForesterUpdateStateService()
        {

        }

        public void Load()
        {

        }

        public void Save(IEntitySaver entitySaver)
        {
            entitySaver.GetComponent(ForesterUpdateStateService.ForesterUpdateKey).Set(ForesterCoordinateKey, _foresterRegistry.Keys);
            entitySaver.GetComponent(ForesterUpdateStateService.ForesterUpdateKey).Set(ForesterTypeKey, _foresterRegistry.Values);

            Debug.Log("FUSS: Saved");
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(ForesterUpdateStateService.ForesterUpdateKey))
                return;
            List<string> foresterTypes = entityLoader.GetComponent(ForesterUpdateStateService.ForesterUpdateKey).Get(ForesterUpdateStateService.ForesterTypeKey);
            List<Vector3Int> foresterCoordinates = entityLoader.GetComponent(ForesterUpdateStateService.ForesterUpdateKey).Get(ForesterUpdateStateService.ForesterCoordinateKey);

            if (foresterCoordinates.Count != foresterTypes.Count)
            {
                Debug.Log("Did not load forester States");
            }
            else
            {
                for (int i = 0; i < foresterTypes.Count; i++)
                {
                    _foresterRegistry.Add(foresterCoordinates[i], foresterTypes[i]);
                }
                Debug.Log("Loaded Forester States");
            }

            Debug.Log("FUSS: Loaded Save");
        }

        public void RegisterForester(Vector3Int coordinates, string treeType )
        {
            _foresterRegistry.Add(coordinates, treeType);
        }

        public void UpdateForester(Vector3Int coordinates, string treeType)
        {
            if (_foresterRegistry.ContainsKey(coordinates))
            {
                _foresterRegistry[coordinates] = treeType;

                Debug.Log("UpdateF: " + treeType);
            }
            else
            {
                RegisterForester(coordinates, treeType );
                Debug.Log("UpdateF: New Register");
            }
        }


        public string GetForesterState(Vector3Int coordinates)
        {
            foreach (var kvp in _foresterRegistry)
            {
                if (kvp.Key == coordinates)
                {
                    return kvp.Value;
                }
            }
            return string.Empty;
        }

    }
}
