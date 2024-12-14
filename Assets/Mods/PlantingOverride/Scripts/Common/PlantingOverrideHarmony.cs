using Cordial.Mods.PlantingOverride.Scripts.Common;
using HarmonyLib;
using System.Collections.Generic;
using TimberApi.DependencyContainerSystem;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.DemolishingUI;
using Timberborn.Forestry;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
using Timberborn.PlantingUI;
using Timberborn.ReservableSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Assets.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverrideHarmony : IModStarter
    {
        public void StartMod()
        {
            var harmony = new Harmony("Cordial.Mods.PlantingOverride");
            harmony.PatchAll();
        } 

        [HarmonyPatch(typeof(PlantBehavior), "Plant")]
        public static class PlantBehaviourPatch
        {
            static bool Prefix(ref Decision __result, ref Vector3Int coordinates, PlantBehavior __instance)
            {
                // need to get access to class/object as well. 
                if (null != __instance)
                {
                    PlantingOverridePrefabSpecService specService = DependencyContainer.GetInstance<PlantingOverridePrefabSpecService>();
                    PlantingService plantingService = DependencyContainer.GetInstance<PlantingService>();
                    EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                    if ((null != plantingService)
                            && (null != specService)
                            && (null != eventBus))
                    {
                        string oldResource = plantingService.GetResourceAt(coordinates.XY());
                        eventBus.Post((object)new PlantingOverridePlantingEvent(coordinates, oldResource));
                    }
                }

                // always return true, as planting should take place
                return true;
            }
        }

        /*
         * 
         * 
         * Unset Tool: Timberborn.DemolishingUI.DemolishableSelectionTool - Timberborn.ToolSystem.ToolDescription
TR: False AR: False
Unset Tool: Timberborn.PlantingUI.CancelPlantingTool - Timberborn.ToolSystem.ToolDescription
TR: False AR: False
UnmarkArea Tool: Timberborn.PlantingUI.CancelPlantingTool - Timberborn.ToolSystem.ToolDescription
TR: False AR: False
Unset Tool: Cordial.Mods.PlantingOverride.Scripts.PlantingOverrideTreeService - Timberborn.ToolSystem.ToolDescription
TR: False AR: False
Unset Tool: Timberborn.PlantingUI.PlantingTool - Timberborn.ToolSystem.ToolDescription
TR: False AR: False

         * 
         * 
*/

        //CancelPlantingTool: PlantingSelectionService -> Unmark Area --> UnsetPlantingCoordinates
        //Demolition: DemoishableSelectionTool ->UnsetPlantingCoordinates
        //Planting: PlantingTool Plant


        // planting service SetPlanting / UnsetPlanting
        // for when the "standard" tools override the planting override service

        [HarmonyPatch(typeof(PlantingService), "UnsetPlantingCoordinates")]
        public static class PlantingServiceUnsetCoordinatePatch
        {
            static void Postfix(ref Vector3Int coordinates)
            {
                EventBus eventBus = DependencyContainer.GetInstance<EventBus>();
                ToolManager toolManager = DependencyContainer.GetInstance<ToolManager>();

                if (null != toolManager)
                {
                    if (null != toolManager.ActiveTool)
                    {
                        // when the active tool is the override service, don't call entry removal,
                        // any planting override changes are handled directly through the tool. 

                        // for all other tools, remove the coordinates from the override service
                        // this may be called by DemolishableSelectionTool, CancelPlantingTool or the standard PlantingTool.
                        if (toolManager.ActiveTool.ToString().Contains("PlantingOverride"))
                        {
                            // ignore removal
                        }
                        else
                        {
                            eventBus.Post((object)new PlantingOverrideRemoveEvent(coordinates));
                        }
                    }
                    else
                    {
                        // no active tool, ignore situation (likely save game startup validation)
                    }
                }


            }
        }
    }
}
