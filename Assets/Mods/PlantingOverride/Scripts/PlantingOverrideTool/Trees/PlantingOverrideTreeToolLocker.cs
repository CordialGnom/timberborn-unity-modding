using Cordial.Mods.PlantingOverride.Scripts.Common;
using System;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.ScienceSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    internal class PlantingOverrideTreeToolLocker : IToolLocker
    {
        private static readonly string BuildingLockKey = "Cordial.PlantingOverrideTool.BuildingLock";
        private static readonly string MissingSpecLockKey = "Cordial.PlantingOverrideTool.SpecLock";

        private readonly PlantingOverridePrefabSpecService _prefabSpecService;
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;

        private readonly DialogBoxShower _dialogBoxShower;
        private readonly ILoc _loc;

        private string _buildingSpecName = String.Empty; 

        public PlantingOverrideTreeToolLocker(  DialogBoxShower dialogBoxShower,
                                                PlantingOverridePrefabSpecService prefabSpecService,
                                                BuildingUnlockingService buildingUnlockingService,
                                                BuildingService buildingService,
                                                ILoc loc)
        {

            this._buildingService = buildingService;
            this._buildingUnlockingService = buildingUnlockingService;
            this._dialogBoxShower = dialogBoxShower;
            this._prefabSpecService = prefabSpecService;
            this._loc = loc;

        }


        public bool ShouldLock(Tool tool)
        {
            bool shouldLock = IsPlantingOverrideTreeTool(tool, out PlantingOverrideTreeService overrideTool);

            Debug.Log("POTTL: SL: " + tool.ToString() );

            if (true == shouldLock)
            {
                // get faction ID to create forester buildingspec
                _buildingSpecName = "Forester." + _prefabSpecService.FactionId;

                Debug.Log("POTTL: SL: " + _buildingSpecName);

                // create the building to check if system is unlocked
                BuildingSpec _buildingSpec = _buildingService.GetBuildingPrefab(_buildingSpecName);

                if (_buildingSpec != null)
                {
                    return (!_buildingUnlockingService.Unlocked(_buildingSpec));
                }
                else
                {
                    return true;
                }

            }
            else
            {
                return false;
            }
        }

        public void TryToUnlock(Tool tool, Action successCallback, Action failCallback)
        {
            if (string.Empty != _buildingSpecName)
            {
                // string exists, try to get it. 
                // create the building to check if system is unlocked
                BuildingSpec _buildingSpec = _buildingService.GetBuildingPrefab(_buildingSpecName);

                if (_buildingSpec == null)
                {
                    this.ShowWrongBuildingMessage(_buildingSpecName, failCallback);
                }
                else if (!_buildingUnlockingService.Unlocked(_buildingSpec))
                {
                    // building is not unlocked: 
                    this.ShowLockedBuildingMessage(_buildingSpec, failCallback);
                }
                else
                {
                    // building is unlocked, tool may be used
                    successCallback();
                }
            }
        }
        public static bool IsPlantingOverrideTreeTool(Tool tool, out PlantingOverrideTreeService overrideTool)
        {
            overrideTool = tool as PlantingOverrideTreeService;
            return overrideTool != null;
        }
        private void ShowLockedBuildingMessage(BuildingSpec building, Action failCallback)
        {
            this._dialogBoxShower.Create().SetMessage(this.GetMessageBuild(building, BuildingLockKey)).SetConfirmButton(failCallback).Show();
        }
        private void ShowWrongBuildingMessage(string buildingSpec, Action failCallback)
        {
 
            this._dialogBoxShower.Create().SetMessage(this.GetMessage(buildingSpec, MissingSpecLockKey)).SetConfirmButton(failCallback).Show();
        }
        private string GetMessageBuild(BuildingSpec building, string key)
        {
            string tgt = this._loc.T(building.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey);
            string str = this._loc.T(key);
            return str + " " + tgt;
        }
        private string GetMessage(string target, string key)
        {
            string str = this._loc.T(key);
            return str + " " + target;
        }
    }
}
