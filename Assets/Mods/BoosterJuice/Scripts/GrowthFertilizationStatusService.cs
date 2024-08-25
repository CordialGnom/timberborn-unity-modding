using System;
using Timberborn.BaseComponentSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.GameDistricts;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    internal class GrowthFertilizationStatusService : BaseComponent, IFinishedStateListener
    {
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private Workplace _workplace;
        private DistrictBuilding _districtBuilding;
        private LackOfResourcesStatus _lackOfResourcesStatus;
        private NoHaulingPostStatus _noHaulingPostStatus;

        public void Awake()
        {
            this._growthFertilizationBuilding = this.GetComponentFast<GrowthFertilizationBuilding>();
            this._workplace = this.GetComponentFast<Workplace>();
            this._districtBuilding = this.GetComponentFast<DistrictBuilding>();
            this._lackOfResourcesStatus = this.GetComponentFast<LackOfResourcesStatus>();
            this._noHaulingPostStatus = this.GetComponentFast<NoHaulingPostStatus>();
        }

        public void OnEnterFinishedState()
        {
            this._lackOfResourcesStatus.Initialize(new Func<bool>(this.CheckIfSupplyIsUnavailable));
            if ((bool)(UnityEngine.Object)this._workplace)
                return;
            this._noHaulingPostStatus.Initialize((Func<bool>)(() => true));
        }

        public void OnExitFinishedState()
        {
            this._lackOfResourcesStatus.Disable();
            if ((bool)(UnityEngine.Object)this._workplace)
                return;
            this._noHaulingPostStatus.Disable();
        }

        private bool CheckIfSupplyIsUnavailable()
        {
            return (!(bool)(UnityEngine.Object)this._workplace || this._workplace.NumberOfAssignedWorkers != 0) && (bool)(UnityEngine.Object)this._districtBuilding.District && (double)this._growthFertilizationBuilding.SupplyAmount <= 0.0 && this._districtBuilding.District.GetComponentFast<DistrictInventoryRegistry>().ActiveInventoriesWithStock(this._growthFertilizationBuilding.Supply).Count == 0;
        }
    }
}
