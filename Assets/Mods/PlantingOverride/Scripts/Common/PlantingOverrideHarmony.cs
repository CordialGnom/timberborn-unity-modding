using Cordial.Mods.PlantingOverride.Scripts;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
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
                // the __result are the coordinates to be planted on . 

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
                        eventBus.Post((object)new PlantingOverridePlantingEvent(oldResource, coordinates));
                    }
                }

                // always return true, as planting should take place
                return true;
            }
        }
    }
}
