using System;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using Timberborn.ScienceSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolLocker : IToolLocker
    {
        private static readonly string BuildingLockKey = "Cordial.ForestTool.BuildingLock";
        private static readonly string MissingSpecLockKey = "Cordial.ForestTool.SpecLock";

        private readonly ForestToolPrefabSpecService _prefabSpecService;
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;

        private readonly ILoc _loc;
        private readonly DialogBoxShower _dialogBoxShower;

        private string _buildingSpecName = String.Empty;

        public ForestToolLocker(    ILoc loc,
                                    DialogBoxShower dialogBoxShower,
                                    BuildingUnlockingService buildingUnlockingService,
                                    BuildingService buildingService,
                                    ForestToolPrefabSpecService prefabSpecService)
        {
            this._buildingService = buildingService;
            this._buildingUnlockingService = buildingUnlockingService;
            this._loc = loc;
            this._dialogBoxShower = dialogBoxShower;
            this._prefabSpecService = prefabSpecService;
        }

        public bool ShouldLock(Tool tool)
        {
            bool shouldLock = IsForestTool(tool, out ForestToolService overrideTool);

            if (true == shouldLock)
            {
                // get faction ID to create forester buildingspec
                _buildingSpecName = "Forester." + _prefabSpecService.FactionId;

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
        public static bool IsForestTool(Tool tool, out ForestToolService overrideTool)
        {
            overrideTool = tool as ForestToolService;
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
