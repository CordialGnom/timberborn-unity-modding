using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Forestry;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
using Timberborn.WorkSystem;
using UnityEngine;
using Cordial.Mods.PlantingOverride.Scripts.Common;

namespace Cordial.Mods.ForesterUpdate.Scripts 
{
    internal class ForesterUpdateLogger : IModStarter 
    {
        public void StartMod()
        {
            var harmony = new Harmony("Cordial.Mods.ForesterUpdate");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(PlantBehavior), "Plant")]
        public static class PlantBehaviourPatch
        {
            static bool Prefix(ref Decision __result, ref Vector3Int coordinates, PlantBehavior __instance)
            {
                // the __result are the coordinates to be planted on . 

                // need to get access to class/object as well. 
                if (null != __instance)
                {
                    Worker worker = __instance.GetComponentFast<Worker>();

                    if (null != worker)
                    {
                        var building = worker.Workplace.GetComponentFast<Forester>();

                        if (null != building)
                        {
                            ForesterUpdateStateService updateService = DependencyContainer.GetInstance<ForesterUpdateStateService>();

                            if (null != updateService)
                            {
                                PlantingService plantingService = DependencyContainer.GetInstance<PlantingService>();
                                PlantingOverridePrefabSpecService specService = DependencyContainer.GetInstance<PlantingOverridePrefabSpecService>();

                                if ((null != plantingService)
                                     && (null != specService))
                                {
                                    // before replacing plant, ensure it is a tree (not a bush)
                                    string oldResource = plantingService.GetResourceAt(coordinates.XY());

                                    if (specService.CheckIsTree(oldResource))
                                    {
                                        string state = updateService.GetForesterState(building.GetComponentFast<BlockObject>().Coordinates).Replace(" ", "");

                                        if (specService.VerifyPrefabName(state))
                                        {
                                            plantingService.SetPlantingCoordinates(coordinates, state);
                                        }
                                        else if (state == string.Empty)
                                        {
                                            // ignore
                                        }
                                        else
                                        {
                                            Debug.Log("PO: Invalid prefab: " + state);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // always return true, as planting should take place
                return true;
            }
        }

        [HarmonyPatch(typeof(Workplace), "OnEnterFinishedState")]
        public static class WorkplaceEnterFinishedPatch
        {
            static bool Prefix(Workplace __instance)
            {
                string defaultTreeType = string.Empty;

                // need to get access to class/object as well. 
                if (null != __instance)
                {
                    var building = __instance.GetComponentFast<Forester>();

                    if (null != building)
                    { 
                        // register building / workplace with forester service
                        ForesterUpdateStateService updateService = DependencyContainer.GetInstance<ForesterUpdateStateService>();

                        if (null != updateService)
                        {
                            updateService.RegisterForester(building.GetComponentFast<BlockObject>().Coordinates, defaultTreeType);
                        }
                    }
                }
                // always return true, as function should execute
                return true;
            }
        }
    }
}