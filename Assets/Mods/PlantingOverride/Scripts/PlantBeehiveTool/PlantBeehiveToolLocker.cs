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

namespace Cordial.Mods.PlantBeehive.Scripts
{
    internal class PlantBeehiveToolLocker : IToolLocker
    {
        private static readonly string FactionLockKey = "Cordial.PlantBeehiveTool.FactionLock";
        private static readonly string BuildingLockKey = "Cordial.PlantBeehiveTool.BuildingLock";
        private static readonly string UnLockKey = "Cordial.PlantBeehiveTool.Unlock";
        private readonly InputService _inputService;
        private readonly PlantBeehivePrefabSpecService _prefabSpecService;
        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService _buildingService;

        private readonly DialogBoxShower _dialogBoxShower;
        private readonly ILoc _loc;

        public PlantBeehiveToolLocker(  InputService inputService,
                                        DialogBoxShower dialogBoxShower,
                                        PlantBeehivePrefabSpecService prefabSpecService,
                                        BuildingUnlockingService buildingUnlockingService,
                                        BuildingService buildingService,
                                        ILoc loc)
        {
            this._inputService = inputService;
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
                // get faction: this is only applicable for folktails (beehive is available)
                if (_prefabSpecService.FactionId.Contains("Folktails"))
                {
                    // check if beehive is unlocked
                    string prefabName = "Beehive.Folktails";

                    // create a beehive to check if system is unlocked
                    BuildingSpec _beehive = _buildingService.GetBuildingPrefab(prefabName);

                    return (!_buildingUnlockingService.Unlocked(_beehive));
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
            // get faction
            if (_prefabSpecService.FactionId.Contains("Folktails"))
            {
                // check if beehive is unlocked
                string prefabName = "Beehive.Folktails";

                // create a beehive to check if system is unlocked
                BuildingSpec _beehive = _buildingService.GetBuildingPrefab(prefabName);

                Debug.Log("PBTL: TTU: " + _buildingUnlockingService.Unlocked(_beehive));

                if (_beehive == null)
                {
                    this.ShowWrongFactionMessage(_prefabSpecService.FactionId, failCallback);
                }
                else if (!_buildingUnlockingService.Unlocked(_beehive))
                {
                    // building is not unlocked: 
                    this.ShowLockedBuildingMessage(_buildingService.GetPrefabName(_beehive), failCallback);
                }
                else
                {
                    // building is unlocked, tool may be used

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


        private void ShowUnlockedMessage(Action successCallback)
        {
            this._dialogBoxShower.Create().SetMessage(this.GetMessage("", UnLockKey)).SetConfirmButton(successCallback).Show();
        }
        private void ShowLockedBuildingMessage(string buildingName, Action failCallback)
        {
            this._dialogBoxShower.Create().SetMessage(this.GetMessage(buildingName, BuildingLockKey)).SetConfirmButton(failCallback).Show();
        }
        private void ShowWrongFactionMessage(string factionId, Action failCallback)
        {
            this._dialogBoxShower.Create().SetMessage(this.GetMessage(factionId, FactionLockKey)).SetConfirmButton(failCallback).Show();
        }
        private string GetMessage(string target, string key)
        {
            string str = this._loc.T(key);
            return str + " " + target;
        }
    }
}
