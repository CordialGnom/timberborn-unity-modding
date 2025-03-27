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
