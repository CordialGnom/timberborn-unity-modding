using Cordial.Mods.PlantingOverride.Scripts.Common;
using NUnit.Framework;
using System;
using System.Threading;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Forestry;
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

            if (true == shouldLock)
            {
                BuildingSpec foresterSpec = new BuildingSpec();
                bool specFound = false;

                // get a list of all buildings
                foreach (BuildingSpec buildingspec in _buildingService.Buildings)
                {
                    if (buildingspec.name.Contains("Forester"))
                    {
                        foresterSpec = buildingspec;
                        specFound = true;
                        break;
                    }
                }

                if (specFound)
                {
                    return (!_buildingUnlockingService.Unlocked(foresterSpec));
                }
                else
                {
                    return true;    // should lock
                }
            }
            else
            {
                return false;
            }
        }

        public void TryToUnlock(Tool tool, Action successCallback, Action failCallback)
        {
            BuildingSpec foresterSpec = new BuildingSpec();
            bool specFound = false;

            // get a list of all buildings and search for the beehive
            foreach (BuildingSpec buildingspec in _buildingService.Buildings)
            {
                if (buildingspec.name.Contains("Forester"))
                {
                    foresterSpec = buildingspec;
                    specFound = true;
                    break;
                }
            }

            // if specification was found...
            if (specFound)
            {
                if (foresterSpec == null)
                {
                    this.ShowWrongBuildingMessage("Forester", failCallback);
                }
                else if (!_buildingUnlockingService.Unlocked(foresterSpec))
                {
                    // building is not unlocked: 
                    this.ShowLockedBuildingMessage(foresterSpec, failCallback);
                }
                else
                {
                    // building is unlocked, tool may be used
                    successCallback();
                }
            }
            else
            {
                this.ShowWrongBuildingMessage("Forester", failCallback);
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
