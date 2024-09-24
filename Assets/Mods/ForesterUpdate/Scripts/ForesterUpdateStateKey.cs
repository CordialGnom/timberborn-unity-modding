using HarmonyLib;
using System;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.WorkSystem;
using UnityEngine;

namespace Cordial.Mods.ForesterUpdate.Scripts
{
    //public class ForesterUpdateStateKey : IPersistentEntity //, ILoadableSingleton
    //{

    //    private static readonly ComponentKey ForesterUpdateKey = new ComponentKey(nameof(ForesterUpdateStateKey));
    //    private static readonly PropertyKey<Vector3Int> ForesterCoordKey = new PropertyKey<Vector3Int>(nameof(Vector3Int));

    //    public Vector3Int Coordinates { get; private set; }

    //    public void Save(IEntitySaver entitySaver)
    //    {
    //        entitySaver.GetComponent(ForesterUpdateStateKey.ForesterUpdateKey).Set(ForesterUpdateStateKey.ForesterCoordKey, this.Coordinates);
    //    }

    //    public void Load(IEntityLoader entityLoader)
    //    {
    //        if (!entityLoader.HasComponent(ForesterUpdateStateKey.ForesterUpdateKey))
    //            return;
    //        this.Coordinates = entityLoader.GetComponent(ForesterUpdateStateKey.ForesterUpdateKey).Get(ForesterUpdateStateKey.ForesterCoordKey);
    //    }


        // this class is there to catch the planting behaviour event
        // then find the worker, find the building
        // if forester, check forester configuration
        // if configured, then replace plantable if required. 

        // timberborn PlantBehaviour
        /*
         *  Use harmony PreFix, and replace the plantable at the coordinates. 
         *  
         * 
         *      private Decision Plant(Vector3Int coordinates)
                {
                return !this._plantExecutor.Launch(coordinates, this._plantingService.GetResourceAt(coordinates.XY())) ? Decision.ReleaseNextTick() : Decision.ReleaseWhenFinished((IExecutor) this._plantExecutor);
                }

        this._worker.Workplace.GetComponentFast<PlantingSpotFinder>().FindClosest(position);



            private PlantingService _plantingService;
            private Planter _planter;
            private Worker _worker;
            private WalkToPositionExecutor _walkToPositionExecutor;
            private PlantExecutor _plantExecutor;

        */


        //[HarmonyPatch(typeof(PlantBehavior), "Plant")]
        //public static class PlantBehaviourPatch
        //{
        //    private static bool Prefix(ref Vector3Int __result, PlantBehavior __instance)
        //    {
        //        // the __result are the coordinates to be planted on . 

        //        // need to get access to class/object as well. 
        //        if (null != __instance)
        //        {
        //            Worker worker = __instance.GetComponentFast<Worker>();

        //            if (null != worker)
        //            {
        //                Debug.Log("Worker Found: " + worker.name);
        //            }
        //            else
        //            {
        //                Debug.Log("No worker found");
        //            }
        //            Debug.Log("No planting");

        //        }
        //        else
        //        {
        //            Debug.Log("No plantbehaviour: " + __result);
        //        }

        //        // always return true, as planting should take place
        //        return true;

        //    }
        //}

    }
//}
