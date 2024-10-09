using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.Goods;
using Timberborn.Cutting;
using Timberborn.ModManagerScene;
using UnityEngine;
using Timberborn.GoodStackSystem;
using Timberborn.BlockSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public class YieldFertilizationHarmony : IModStarter
    {
        public void StartMod()
        {
            var harmony = new Harmony("Cordial.Mods.BoosterJuice");

            harmony.PatchAll();
        } 


        // following code did not make it possible to "increase" the goods in a tree. 
        // the inventory capacity itself would need to be modified, and this is not accessible. 
        //[HarmonyPatch(typeof(Cuttable), "EnableGoodStack")]
        //public static class CuttableEnableGoodStackPrefixPatch
        //{
        //    static bool Prefix(Cuttable __instance)
        //    {
        //        // need to get access to class/object as well. 
        //        if (null != __instance)
        //        {
        //            if (!__instance.Yielder.IsYielding)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                YieldFertilizationService yieldService = DependencyContainer.GetInstance<YieldFertilizationService>();

        //                if (null != yieldService)
        //                {
        //                    __instance.TryGetComponentFast<GoodStack>(out GoodStack goodStack);
        //                    __instance.TryGetComponentFast<BlockObject>(out BlockObject blockObject);

        //                    if ((null != goodStack)
        //                        && (null != blockObject))
        //                    {
        //                        // check if blockobject coordinates match covered area
        //                        if (yieldService.CheckCoordinates(blockObject.Coordinates))
        //                        {
        //                            goodStack.EnableGoodStack(new GoodAmount(__instance.Yielder.Yield.GoodId, 5));

        //                            Debug.Log("Found Cuttable: " + blockObject.Coordinates);

        //                        }

        //                    }

        //                }


        //                //__instance.Yielder.RemoveRemainingYield();

        //                //return false;       // do not execute original code
        //            }
        //        }

        //        // always return true, as planting should take place
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(Cuttable), "Cut")]
        public static class CuttableCutEvent
        {
            static void Postfix(Cuttable __instance)
            {
                // need to get access to class/object as well. 
                if (null != __instance)
                {                    
                    YieldFertilizationService yieldService = DependencyContainer.GetInstance<YieldFertilizationService>();
                    __instance.TryGetComponentFast<BlockObject>(out BlockObject blockObject);

                    if ((null != blockObject)
                        && (null != yieldService))
                    {
                        // try and remove from stored coordinates
                        yieldService.RemoveCoordinates(blockObject.Coordinates);
                    }
                }
            }
        }
    }
}
