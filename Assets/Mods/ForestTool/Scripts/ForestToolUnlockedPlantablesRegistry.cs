using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.Buildings;
using Timberborn.Planting;
using Timberborn.ScienceSystem;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolUnlockedPlantableRegistry : IPostLoadableSingleton
    {
        private readonly BuildingService _buildingService;
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly HashSet<string> _unlockedResourceGroups = new HashSet<string>();

        public ForestToolUnlockedPlantableRegistry(   BuildingService buildingService,
                                                      BuildingUnlockingService buildingUnlockingService)
        {
            this._buildingService = buildingService;
            this._buildingUnlockingService = buildingUnlockingService;
        }

        public void PostLoad()
        {
            foreach (BuildingSpec building in this._buildingService.Buildings)
            {
                if (this._buildingUnlockingService.Unlocked(building))
                    this.AddUnlockedPlantableGroups((BaseComponent)building);
            }
        }

        public bool IsLocked()
        {
            bool isLocked = !this._unlockedResourceGroups.Contains("Forester");
            Debug.Log("FTUPR: " + isLocked);
            return isLocked;
        }

        public void AddUnlockedPlantableGroups(BaseComponent baseComponent)
        {
            PlanterBuildingSpec componentFast = baseComponent.GetComponentFast<PlanterBuildingSpec>();
            if (componentFast == null)
                return;
            this._unlockedResourceGroups.Add(componentFast.PlantableResourceGroup);
        }
    }
}
