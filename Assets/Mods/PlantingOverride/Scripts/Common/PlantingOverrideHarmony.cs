﻿using Cordial.Mods.PlantBeehive.Scripts;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
using Timberborn.Pollination;
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
                    PlantingOverridePrefabSpecService plantOverrideSpecService = DependencyContainer.GetInstance<PlantingOverridePrefabSpecService>();
                    PlantingService plantingService = DependencyContainer.GetInstance<PlantingService>();
                    EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                    if ((null != plantingService)
                            && (null != plantOverrideSpecService)
                            && (null != eventBus))
                    {
                        string oldResource = plantingService.GetResourceAt(coordinates);
                        eventBus.Post((object)new PlantingOverridePlantingEvent(coordinates, oldResource));
                        eventBus.Post((object)new PlantBeehiveToolUnmarkEvent(coordinates, false));
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


        // Plant Beehive Tool Patch for the demolishable (plant) unmark event
        // in the case of unmark, either the plant has been deleted, or effectively unmarked by the user
        [HarmonyPatch(typeof(Demolishable), "Unmark")]
        public static class DemolishableUnmarkPatch
        {
            static void Postfix(Demolishable __instance)
            {
                bool placeHive = true;
                EventBus eventBus = DependencyContainer.GetInstance<EventBus>();
                ToolManager toolManager = DependencyContainer.GetInstance<ToolManager>();

                if ((null != __instance)
                    && (null != __instance.GetComponentFast<BlockObject>()))
                {
                    if (null != toolManager)
                    {
                        if (null != toolManager.ActiveTool)
                        {
                            // there is an active tool, check if it is the demolition service

                            if ((toolManager.ActiveTool.ToString().Contains("Delet"))
                                || (toolManager.ActiveTool.ToString().Contains("Demol"))
                                || (toolManager.ActiveTool.ToString().Contains("Cancel")))
                            {
                                // only remove coordinates
                                placeHive = false;
                            }
                        }
                    }

                    eventBus.Post((object)new PlantBeehiveToolUnmarkEvent(__instance.GetComponentFast<BlockObject>().Coordinates, placeHive));
                }
            }
        }

        // register all hives as they are created
        [HarmonyPatch(typeof(Hive), "Awake")]
        public static class HiveAwakePatch
        {
            static void Postfix(Hive __instance)
            {
                EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                if (null != __instance)
                {
                    eventBus.Post((object)new PlantBeehiveToolRegisterHiveEvent(__instance));
                }
            }
        }

        // register all hives as they are created
        [HarmonyPatch(typeof(Hive), "OnEnterFinishedState")]
        public static class HiveOnEnterFinishedStatePatch
        {
            static void Postfix(Hive __instance)
            {
                EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                if (null != __instance)
                {
                    eventBus.Post((object)new PlantBeehiveToolRegisterHiveEvent(__instance));

                }
            }
        }

        // unregister any hive
        [HarmonyPatch(typeof(Hive), "OnExitFinishedState")]
        public static class HiveOnExitFinishedStatePatch
        {
            static void Postfix(Hive __instance)
            {
                EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                if (null != __instance)
                {
                    eventBus.Post((object)new PlantBeehiveToolUnregisterHiveEvent(__instance));
                }
            }
        } 
        
        // unregister any hive that has been deleted (e.g. in construction state)
        [HarmonyPatch(typeof(EntityService), "Delete")]
        public static class EntityServiceOnDeletePatch
        {
            static void Postfix(ref BaseComponent entity)
            {
                if (null != entity)
                {
                    EventBus eventBus = DependencyContainer.GetInstance<EventBus>();

                    // check if entity has a hive component
                    entity.TryGetComponentFast<Hive>(out Hive component);

                    if (null != component)
                    {
                        component.TryGetComponentFast<BlockObject>(out BlockObject blockObject);

                        if (null != blockObject)
                        {
                            Debug.Log("Delete Hive At Coordinates: " + blockObject.Coordinates);
                            eventBus.Post((object)new PlantBeehiveToolUnmarkEvent(blockObject.Coordinates, false));
                        }
                        else
                        {
                            Debug.Log("Delete Hive : " + component.name);
                            eventBus.Post((object)new PlantBeehiveToolUnregisterHiveEvent(component));
                        }
                    }
                    else
                    {
                        Debug.Log("Delete: " + entity.name);
                    }
                }
            }
        }
    }
}
