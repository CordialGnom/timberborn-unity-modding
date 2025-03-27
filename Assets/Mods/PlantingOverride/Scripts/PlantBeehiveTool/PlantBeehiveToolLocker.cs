using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.ScienceSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using System;
using Timberborn.BlockObjectTools;
using Timberborn.PlantingUI;
using Timberborn.EntitySystem;
using static System.Collections.Specialized.BitVector32;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    internal class PlantBeehiveToolLocker : IToolLocker
    {
        private static readonly string FactionLockKey = "Cordial.PlantingOverrideTool.FactionLock";
        private static readonly string BuildingLockKey = "Cordial.PlantingOverrideTool.BuildingLock";

        private readonly PlantBeehivePrefabSpecService _prefabSpecService;
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;

        private readonly DialogBoxShower _dialogBoxShower;
        private readonly ILoc _loc;

        public PlantBeehiveToolLocker(  DialogBoxShower dialogBoxShower,
                                        PlantBeehivePrefabSpecService prefabSpecService,
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
            PlantBeehiveToolService beehiveTool;

            bool shouldLock =   IsPlantBeehiveTool(tool, out beehiveTool);

            if (true == shouldLock)
            {
                BuildingSpec beehiveSpec = new BuildingSpec();
                bool specFound = false;

                // get a list of all buildings
                foreach (BuildingSpec buildingspec in _buildingService.Buildings)
                {
                    if (buildingspec.name.Contains("Beehive"))
                    {
                        beehiveSpec = buildingspec;
                        specFound = true;
                        break;
                    }
                }

                if (specFound)
                {
                    return (!_buildingUnlockingService.Unlocked(beehiveSpec));
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
            BuildingSpec beehiveSpec = new BuildingSpec();
            bool specFound = false;

            // get a list of all buildings and search for the beehive
            foreach (BuildingSpec buildingspec in _buildingService.Buildings)
            {
                if (buildingspec.name.Contains("Beehive"))
                {
                    beehiveSpec = buildingspec;
                    specFound = true;
                    break;
                }
            }



            // if specification was found...
            if (specFound)
            {
                if (beehiveSpec == null)
                {
                    this.ShowWrongFactionMessage(_prefabSpecService.FactionId, failCallback);
                }
                else if (!_buildingUnlockingService.Unlocked(beehiveSpec))
                {
                    // building is not unlocked: 
                    this.ShowLockedBuildingMessage(beehiveSpec, failCallback);
                }
                else
                {
                    // building is unlocked, tool may be used
                    successCallback();
                }
            }
            else
            {
                this.ShowWrongFactionMessage(_prefabSpecService.FactionId, failCallback);
            }
        }
        public static bool IsPlantBeehiveTool(Tool tool, out PlantBeehiveToolService beehiveTool)
        {
            beehiveTool = tool as PlantBeehiveToolService;
            return beehiveTool != null;
        }



        private void ShowLockedBuildingMessage(BuildingSpec building, Action failCallback)
        {
            this._dialogBoxShower.Create().SetMessage(this.GetMessageBuild(building, BuildingLockKey)).SetConfirmButton(failCallback).Show();
        }
        private void ShowWrongFactionMessage(string factionId, Action failCallback)
        {
            string tgt = this._loc.T("Faction." + factionId + ".DisplayName");

            // replace if modded faction and no displayname is available
            if (tgt.Contains("DisplayName"))
            {
                tgt = factionId;
            }

            this._dialogBoxShower.Create().SetMessage(this.GetMessage(tgt, FactionLockKey)).SetConfirmButton(failCallback).Show();
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
