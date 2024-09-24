using HarmonyLib;
using System.Collections.Generic;
using TimberApi.DependencyContainerSystem;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Forestry;
using Timberborn.ModManagerScene;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.WorkSystem;
using UnityEngine;

namespace Cordial.Mods.ForesterUpdate.Scripts 
{
    internal class ForesterUpdateLogger : IModStarter 
    {
        private static int foresterCount = 1;
        private static string foresterPrefix = "Forester";

        public void StartMod()
        {
            // required to start up mod


            // requires entitypanel access for Forester
            // --> see booster juice
            // requries "service" or config handler for what is set in forester


            // requires Harmony to access the planting event, or the cutting event to replace the "plantable". 
            // --> planting behaviour, get coordinate, check if coordinate is in building range, resp. which building the worker belongs to. 
            // yep. 

            var harmony = new Harmony("Cordial.Mods.ForesterUpdate");
            harmony.PatchAll();

            Debug.Log("Forester Update Started");

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
                            Debug.Log("PlantBehaviour Found: " + worker.name + " - " + building.GetComponentFast<BlockObject>().Coordinates);


                            ForesterUpdateStateService updateService = DependencyContainer.GetInstance<ForesterUpdateStateService>();

                            if (null != updateService)
                            {
                                bool state = updateService.GetForesterState(building.GetComponentFast<BlockObject>().Coordinates);

                                Debug.Log("PlantBehaviour State: " + state); 
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
            private static bool toggleState = false;
            static bool Prefix(Workplace __instance)
            {
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
                            updateService.RegisterForester(building.GetComponentFast<BlockObject>().Coordinates, toggleState);

                            toggleState = !toggleState;

                            Debug.Log("WEFP: Registered Building" + building.GetComponentFast<BlockObject>().Coordinates);
                        }
                    }
                }

                // always return true, as function should execute
                return true;

            }


        }
    }
}