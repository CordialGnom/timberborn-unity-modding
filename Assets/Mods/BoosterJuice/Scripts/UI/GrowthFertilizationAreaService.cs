using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    public class GrowthFertilizationAreaService : ILoadableSingleton
    {
        private int _buildingCount;

        private readonly Dictionary<Vector3Int, int> _coordinateRegistry = new Dictionary<Vector3Int, int>();
        private readonly Dictionary<int, GrowthFertilizationBuilding> _buildingRegistry = new Dictionary<int, GrowthFertilizationBuilding>();

        public GrowthFertilizationAreaService()
        {
            _coordinateRegistry = new();
            _buildingRegistry = new();
            _buildingCount = 0;
        }


        public void Load()
        {
            _coordinateRegistry.Clear();
            _buildingRegistry.Clear();
        }


        public int AddBuilding(GrowthFertilizationBuilding building)
        {
            if (building == null)
                return 0;

            _buildingCount++;
            _buildingRegistry.Add(_buildingCount, building);

            return _buildingCount;
        }

        public void UpdateBuildingArea(int _buildingCount)
        {
            // get registry before adding new building, to check if range is to be added
            if (_buildingRegistry.TryGetValue(_buildingCount, out GrowthFertilizationBuilding building))
            {
                foreach (Vector3Int coordinate in building.GetBlocksInRange())
                {
                    // check if coordinate is already in list
                    if (!CheckCoordinateFertilizationArea(coordinate))
                    {
                        _coordinateRegistry.Add(coordinate, _buildingCount);
                    }
                    else
                    {
                        // do not add coordinate, already in list
                    }
                }
            }
        }

        public void RemoveBuildingArea(int _buildingCount)
        {
            // get registry before adding new building, to check if range is to be added
            if (_buildingRegistry.TryGetValue(_buildingCount, out GrowthFertilizationBuilding building))
            {
                // remove the building from the registry
                _buildingRegistry.Remove(_buildingCount);

                // clear coordinate registry
                _coordinateRegistry.Clear();

                // after removing building, re-register the coordinates of the still available buildings (to ensure overlapped ranges are kept)
                foreach (int index in _buildingRegistry.Keys)
                {
                    UpdateBuildingArea(index);
                }
            }
        }

        public IEnumerable<Vector3Int> GetFertilizationArea()
        {
            List<Vector3Int> coordinateList = new();

            foreach (KeyValuePair<int, GrowthFertilizationBuilding> kvp in _buildingRegistry)
            {
                coordinateList.AddRange(kvp.Value.GetBlocksInRange());
            }
            return coordinateList.AsEnumerable();
        }

        public bool CheckCoordinateFertilizationArea(Vector3Int coordinates)
        {
            foreach (KeyValuePair<Vector3Int, int> kvp in _coordinateRegistry)
                if (kvp.Key == coordinates)
                    return true;

            return false;
        }

        public IEnumerable<Vector3Int> GetRegisteredFertilizationArea(int buildingId)
        {
            List<Vector3Int> coordinateList = new();

            foreach (KeyValuePair<Vector3Int, int> kvp in _coordinateRegistry)
                if (kvp.Value == buildingId)
                {
                    coordinateList.Add(kvp.Key);
                }

            return coordinateList.AsEnumerable();
        }

        public float GetGrowthProgessDaily( Vector3Int coordinate)
        {
            // get building ID referenced to the coordinate
            if (!_coordinateRegistry.TryGetValue(coordinate, out int buildingID))
            {
                return 0f;
            }
            else
            {
                if (buildingID == 0)
                {
                    return 0.0f;
                }
                else
                {
                    if (!_buildingRegistry.TryGetValue(buildingID, out GrowthFertilizationBuilding building))
                    {
                        return 0f;
                    }
                    else
                    {
                        return building.DailyGrowth;
                    }
                }
            }
        }

        public float GetGrowthProgessAverage(Vector3Int coordinate)
        {
            // get building ID referenced to the coordinate
            if (!_coordinateRegistry.TryGetValue(coordinate, out int buildingID))
            {
                return 0f;
            }
            else
            {
                if (buildingID == 0)
                {
                    return 0.0f;
                }
                else
                {
                    if (!_buildingRegistry.TryGetValue(buildingID, out GrowthFertilizationBuilding building))
                    {
                        return 0f;
                    }
                    else
                    {
                        return building.AverageGrowth;
                    }
                }
            }
        }
        
        public float GetYieldProgessAverage(Vector3Int coordinate)
        {
            // get building ID referenced to the coordinate
            if (!_coordinateRegistry.TryGetValue(coordinate, out int buildingID))
            {
                return 0f;
            }
            else
            {
                if (buildingID == 0)
                {
                    return 0.0f;
                }
                else
                {
                    if (!_buildingRegistry.TryGetValue(buildingID, out GrowthFertilizationBuilding building))
                    {
                        return 0f;
                    }
                    else
                    {
                        return building.AverageYieldInc;
                    }
                }
            }
        }
        public float GetGrowthFactor()
            if (_buildingRegistry.Count > 0)
            {
                KeyValuePair<int, GrowthFertilizationBuilding> kvp = _buildingRegistry.First();

                if (kvp.Value != null)
                {
                    return kvp.Value.GrowthFactor;
                }
            }
            return 0f;
        }
        public float GetYieldFactor()
        {
            // get first building, as all should have the same reference
            if (_buildingRegistry.Count > 0)
            {
                KeyValuePair<int, GrowthFertilizationBuilding> kvp = _buildingRegistry.First();

                if (kvp.Value != null)
                {
                    return kvp.Value.YieldFactor;
                }
            }
            return 0.0f;
        }
    }
}
