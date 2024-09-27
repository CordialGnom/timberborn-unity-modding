using System.Collections.Generic;
using Timberborn.MapStateSystem;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.WorldSerialization;
using UnityEngine;

namespace Cordial.Mods.ForesterUpdate.Scripts
{
    internal class ForesterUpdateStateService : ISaveableSingleton, ILoadableSingleton
    {
        private static Dictionary<Vector3Int, string> _foresterRegistry = new();

        private static readonly SingletonKey PlantingOverrideServiceKey = new SingletonKey(nameof(ForesterUpdateStateService));
        private static readonly ListKey<Vector3Int> ForesterCoordinateKey = new ListKey<Vector3Int>("Cordial.ForesterCoordinateKey");
        private static readonly ListKey<string> ForesterTypeKey = new ListKey<string>("Cordial.ForesterTypeKey");

        // avoid storing in map mode
        private readonly MapEditorMode _mapEditorMode;
        private readonly ISingletonLoader _singletonLoader;


        public ForesterUpdateStateService( ISingletonLoader singletonLoader,
                                           MapEditorMode mapEditorMode )
        {
            this._singletonLoader = singletonLoader;
            this._mapEditorMode = mapEditorMode;

        }

        public void Load()
        {
            if (this._singletonLoader.HasSingleton(ForesterUpdateStateService.PlantingOverrideServiceKey))
            {
                List<string> foresterTypes = _singletonLoader.GetSingleton(ForesterUpdateStateService.PlantingOverrideServiceKey).Get(ForesterUpdateStateService.ForesterTypeKey);
                List<Vector3Int> foresterCoordinates = _singletonLoader.GetSingleton(ForesterUpdateStateService.PlantingOverrideServiceKey).Get(ForesterUpdateStateService.ForesterCoordinateKey);

                if (foresterCoordinates.Count != foresterTypes.Count)
                {
                    Debug.Log("Did not load forester States");
                }
                else
                {
                    for (int i = 0; i < foresterTypes.Count; i++)
                    {
                        if (!_foresterRegistry.TryAdd(foresterCoordinates[i], foresterTypes[i]))
                        {
                            Debug.Log("OP: FUS: " + foresterCoordinates[i] + " - " + foresterTypes[i] + " - " + _foresterRegistry[foresterCoordinates[i]]);
                            _foresterRegistry[foresterCoordinates[i]] = foresterTypes[i];
                        }
                    }
                    Debug.Log("Loaded Forester States: " + foresterCoordinates.Count);
                }
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            if (this._mapEditorMode.IsMapEditor)
                return;
            singletonSaver.GetSingleton(ForesterUpdateStateService.PlantingOverrideServiceKey).Set(ForesterCoordinateKey, _foresterRegistry.Keys);
            singletonSaver.GetSingleton(ForesterUpdateStateService.PlantingOverrideServiceKey).Set(ForesterTypeKey, _foresterRegistry.Values);
        }

        public void RegisterForester(Vector3Int coordinates, string treeType )
        {
            if (_foresterRegistry.TryAdd(coordinates, treeType))
            {
                //Debug.Log("OP: Forester Registered");
            }
            else
            {
                //Debug.Log("OP: Forester already loaded");
            }
        }

        public void UpdateForester(Vector3Int coordinates, string treeType)
        {
            if (_foresterRegistry.ContainsKey(coordinates))
            {
                _foresterRegistry[coordinates] = treeType;
            }
            else
            {
                RegisterForester(coordinates, treeType );
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
