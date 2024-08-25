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

        private Dictionary<Vector3Int, int> _coordinateRegistry = new Dictionary<Vector3Int, int>();
        private Dictionary<int, GrowthFertilizationBuilding> _buildingRegistry = new Dictionary<int, GrowthFertilizationBuilding>();

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
            int buildingID =    _coordinateRegistry.GetValueOrDefault(coordinate, 0);

            if (buildingID == 0)
            {
                return 0.0f;
            }
            else
            {
                GrowthFertilizationBuilding building =  _buildingRegistry.GetValueOrDefault(buildingID, null);

                if (building != null)
                {
                    return building.DailyGrowth;
                }
                else
                {
                    return 0.0f;
                }
            }
        }

    }


}
