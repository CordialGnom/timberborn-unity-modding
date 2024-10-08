using System.Collections.Generic;
using Timberborn.BlockSystem;
using Timberborn.Cutting;
using Timberborn.Goods;
using Timberborn.GoodStackSystem;
using Timberborn.Forestry;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.Yielding;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public class YieldFertilizationService : ILoadableSingleton //, ISaveableSingleton
    {
        // data collection
        private static Dictionary<Vector3Int, TreeComponent> m_GrownTreeDict = new();


        // saving/loading
        private readonly ISingletonLoader _singletonLoader;
        private static readonly SingletonKey YieldFertilizationServiceKey = new SingletonKey("Cordial.YieldFertilizationService");
        private static readonly ListKey<Vector3Int> GrownPlantCoordKey = new ListKey<Vector3Int>("Cordial.GrownPlantCoordKey");
        private static readonly ListKey<TreeComponent> TreeComponentKey = new ListKey<TreeComponent>("Cordial.TreeComponentKey");

        // reaction
        private readonly EventBus _eventBus;


        public YieldFertilizationService(   ISingletonLoader singletonLoader, 
                                            EventBus eventBus)
        {
            _singletonLoader = singletonLoader;
            _eventBus = eventBus;
        }

        //public void Save(ISingletonSaver singletonSaver)
        //{
        //    Debug.Log("Yield Saved");
        //    singletonSaver.GetSingleton(YieldFertilizationService.YieldFertilizationServiceKey).Set(GrownPlantCoordKey, m_GrownTreeDict.Keys);
        //    singletonSaver.GetSingleton(YieldFertilizationService.YieldFertilizationServiceKey).Set(TreeComponentKey, m_GrownTreeDict.Values);
        //}

        public void Load()
        {
            this._eventBus.Register((object)this);

            if (this._singletonLoader.HasSingleton(YieldFertilizationService.YieldFertilizationServiceKey))
            {
                List<TreeComponent> yieldInfluence = _singletonLoader.GetSingleton(YieldFertilizationService.YieldFertilizationServiceKey).Get(YieldFertilizationService.TreeComponentKey);
                List<Vector3Int> plantCoordinates = _singletonLoader.GetSingleton(YieldFertilizationService.YieldFertilizationServiceKey).Get(YieldFertilizationService.GrownPlantCoordKey);

                if (plantCoordinates.Count != yieldInfluence.Count)
                {
                    Debug.Log("PO: Did not load planting override crop configuration");
                }
                else
                {
                    for (int i = 0; i < yieldInfluence.Count; i++)
                    {
                        if (!m_GrownTreeDict.TryAdd(plantCoordinates[i], yieldInfluence[i]))
                        {
                            m_GrownTreeDict[plantCoordinates[i]] = yieldInfluence[i];
                        }
                    }
                }
            }
        }

        public void UpdateRegisteredYielders(float yieldProgress)
        {
            foreach (TreeComponent treeComponent in m_GrownTreeDict.Values)
            {
                treeComponent.TryGetComponentFast<Cuttable>( out Cuttable cuttable);

                if ((cuttable != null))
                {
                    Debug.Log("Got TreeComponent: " + treeComponent.GetComponentFast<BlockObject>().Coordinates );
                    cuttable.TryGetComponentFast<GoodStack>(out GoodStack goodstack);
                    cuttable.TryGetComponentFast<Yielder>(out Yielder yielder);

                    if ((goodstack != null)
                     && (yielder != null))
                    {
                        string goodId = yielder.Yield.GoodId;

                        GoodAmount goodAmount = new(goodId, 5);

                        goodstack.Inventory.GiveIgnoringCapacityReservation(goodAmount);

                        Debug.Log("Gave Goods: " + goodAmount + " - Y: " + yielder.Yield.Amount);

                    }
                }
            }
        }

        public void RegisterGrownTreeComponent(TreeComponent treeComponent)
        {
            treeComponent.TryGetComponentFast<BlockObject>(out BlockObject component);

            if (component != null)
            {
                Vector3Int coordinates = component.Coordinates;

                if (!m_GrownTreeDict.ContainsKey(coordinates))
                {
                    m_GrownTreeDict.TryAdd(coordinates, treeComponent);
                }
                else
                {
                    m_GrownTreeDict[coordinates] = treeComponent;
                }
            }
        }
    }
}
