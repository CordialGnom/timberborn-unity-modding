using Cordial.Mods.PlantingOverride.Scripts;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Forestry;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
using Timberborn.ReservableSystem;
using Timberborn.SingletonSystem;
using Timberborn.WorkSystem;
using UnityEngine;
using UnityEngine.Playables;

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
                        eventBus.Post((object)new PlantingOverridePlantingEvent(coordinates));
                    }
                }

                // always return true, as planting should take place
                return true;
            }
        }

        [HarmonyPatch(typeof(Planter), "Unreserve")]
        public static class PlantUnreservePatch
        {
            static bool Prefix(ref Planter __instance)
            {
                // need to get access to object as well. 
                if (null != __instance)
                {
                    if (__instance.PlantingCoordinates.HasValue)
                    {
                        EventBus eventBus = DependencyContainer.GetInstance<EventBus>();
                        eventBus.Post((object)new PlantingOverridePlantingEvent(__instance.PlantingCoordinates.Value));
                    }
                }

                // always return true, to ensure patched function is executed
                return true;
            }
        }

        [HarmonyPatch(typeof(Reservable), "Unreserve")]
        public static class ReservableUnreservePatch
        {
            static bool Prefix(ref Reservable __instance)
            {
                // need to get access to object as well. 
                if (null != __instance)
                {
                    __instance.TryGetComponentFast<TreeComponent>(out TreeComponent component);
                    __instance.TryGetComponentFast<BlockObject>(out BlockObject block);

                    if ((null != component)
                        && (null != block))
                    {
                        EventBus eventBus = DependencyContainer.GetInstance<EventBus>();
                        eventBus.Post((object)new PlantingOverridePlantingEvent(block.Coordinates));
                    }
                }
                // always return true, to ensure patched function is executed
                return true;
            }
        }
    }
}
