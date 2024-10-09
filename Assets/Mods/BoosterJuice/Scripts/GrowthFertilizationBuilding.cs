using Bindito.Core;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BuildingRange;
using Timberborn.Forestry;
using Timberborn.PrefabSystem;
using Timberborn.Yielding;
using Timberborn.Goods;
using UnityEngine;
using System;
using Timberborn.TimeSystem;
using Timberborn.Growing;
using Timberborn.BuildingsBlocking;
using Timberborn.Persistence;
using Timberborn.GoodConsumingBuildingSystem;
using Timberborn.InventorySystem;
using Timberborn.Common;
using Cordial.Mods.BoosterJuice.Scripts.UI;
using Timberborn.WorkSystem;
using Timberborn.Workshops;
using Timberborn.SingletonSystem;
using Timberborn.Gathering;

namespace Cordial.Mods.BoosterJuice.Scripts {
  public class GrowthFertilizationBuilding : BaseComponent, 
        IBuildingWithRange,
        IFinishedStateListener,
        IPausableComponent,
        IPersistentEntity
    {

        [SerializeField]
        private float _growthFactor;

        [SerializeField]
        private float _yieldFactor;

        [SerializeField]
        private int _growthFertilizationRadius;

        [SerializeField]
        private int _capacity;

        [SerializeField]
        private string _supply;

        [SerializeField]
        private float _consumptionFactor;

        private static readonly ComponentKey GrowthFertilizationBuildingKey = new ComponentKey(nameof(GrowthFertilizationBuilding));
        private static readonly PropertyKey<float> SupplyLeftKey = new PropertyKey<float>("SupplyLeft");
        private static readonly PropertyKey<float> DailyGrowthKey = new PropertyKey<float>("DailyGrowth");
        private static readonly PropertyKey<float> AverageGrowthKey = new PropertyKey<float>("AverageGrowth");
        private static readonly PropertyKey<int> WorkHoursPassed = new PropertyKey<int>("WorkHoursPassed");

        // from GoodConsumingBuilding
        private BlockableBuilding _blockableBuilding;
        private readonly List<GoodConsumingToggle> _toggles = new List<GoodConsumingToggle>();
        private float _supplyLeft;

        // tree handling
        private readonly List<Growable> _nearbyGrowingTrees = new List<Growable>();
        private readonly List<TreeComponent> _nearbyYieldTrees = new List<TreeComponent>();

        public Inventory Inventory { get; private set; }

        public bool IsConsuming { get; private set; }

        public bool ConsumptionPaused { get; private set; }

        public int TreesTotalCount => this._treesInRangeCount;
        public int TreesGrowCount => this._nearbyGrowingTrees.Count;
        public int TreesYieldCount => this._nearbyYieldTrees.Count;

        public IEnumerable<Growable> GrowingTrees => this._nearbyGrowingTrees;

        public int ConsumptionPerHour => Mathf.CeilToInt(this._consumptionPerHour);
        public string Supply => this._supply;

        public int Capacity => this._capacity;

        public int SupplyLeft => this.Inventory.UnreservedAmountInStock(this._supply);

        public float SupplyAmount
        {
            get => (float)this.Inventory.UnreservedAmountInStock(this._supply) + this._supplyLeft;
        }
        public float DailyGrowth => (this._dailyGrowth * 100.0f);

        public float AverageGrowth => (this._averageGrowth * 100.0f);

        public float GrowthFactor => (this._growthFactor);

        public bool IsReadyToFertilize => (double)this.SupplyAmount > 0.0;

        // productivity calculation and worker access
        private WorkplaceWorkingHours _workplaceWorkingHours;
        private Workshop _workshop;
        private EventBus _eventBus;

        private int _treesInRangeCount = 0;
        private int _workHoursPassed = 0;
        private float _consumptionPerHour = 0.0f;
        private float _dailyGrowth = 0.0f;
        private float _averageGrowth = 0.0f;
        private float _dailyYieldInc = 0.0f;
        private float _averageYieldInc = 0.0f;

        // fertilization handling
        private readonly float _timeTriggerCallCountPerDay = 1/24f;  // call the trigger every hour;

        private int _buildingId = 0;

        private TreeCuttingArea _treeCuttingArea;
        private GrowthFertilizationAreaService _growthFertilizationAreaService;
        private BlockService _blockService;
        private BlockObjectRange _blockObjectRange;


        private List<Yielder> _yieldersInArea = new();



        private ITimeTriggerFactory _timeTriggerFactory;
        private ITimeTrigger _timeTrigger;

        // unknown
        private Prefab _prefab;


        [Inject]
        public void InjectDependencies( BlockService blockService,
                                        TreeCuttingArea treeCuttingArea,
                                        ITimeTriggerFactory timeTriggerFactory,
                                        GrowthFertilizationAreaService growthFertilizationAreaService,
                                        EventBus eventBus )
        {
            this._treeCuttingArea = treeCuttingArea;
            this._blockService = blockService;
            this._growthFertilizationAreaService = growthFertilizationAreaService;
            this._timeTriggerFactory = timeTriggerFactory;
            this._eventBus = eventBus;
        }
        public void Awake()
        {
            this._blockableBuilding = this.GetComponentFast<BlockableBuilding>();
            this._blockObjectRange = this.GetComponentFast<BlockObjectRange>();
            this._prefab = this.GetComponentFast<Prefab>();
            // set up time triggering response
            this._timeTrigger = this._timeTriggerFactory.Create(new Action(this.FertilizeNearbyGrowingTrees), _timeTriggerCallCountPerDay);

            // add building to area service for other UIs/Classes to access range of all GrowthFertilization
            _buildingId =   this._growthFertilizationAreaService.AddBuilding(this);

            // access working hours / productivity
            this._workplaceWorkingHours = this.GetComponentFast<WorkplaceWorkingHours>();
            this._workshop = this.GetComponentFast<Workshop>();


            this._eventBus.Register((object)this);

            this.enabled = false;
        }
        public void InitializeInventory(Inventory inventory)
        {
            Asserts.FieldIsNull<GrowthFertilizationBuilding>(this, (object)this.Inventory, "Inventory");
            this.Inventory = inventory;
        }

        [OnEvent]
        public void OnDaytimeStart(DaytimeStartEvent daytimeStartEvent) => this.StartNextDay();

        private void StartNextDay()
        {
            // at each day start, reset counter for growth calculation
            _workHoursPassed = 0;

            // calculate average growth of each day
            // handle situation where average growth has never been set before (after update or new game)
            if (_averageGrowth == 0)
            {
                if (_dailyGrowth > 1)
                {
                    _averageGrowth = 1;
                }
                else
                {
                    _averageGrowth = _dailyGrowth;
                }
            }

            _averageGrowth = (_averageGrowth + _dailyGrowth) / 2.0f;

            if (_averageYieldInc == 0)
            {
                if (_dailyYieldInc > 1)
                {
                    _averageYieldInc = 1;
                }
                else
                {
                    _averageYieldInc = _dailyYieldInc;
                }
            }

            _averageYieldInc = (_averageYieldInc + _dailyYieldInc) / 2.0f;

            // reset daily growth
            _dailyGrowth = 0.0f;
            _dailyYieldInc = 0.0f;
        }

        public IEnumerable<BaseComponent> GetObjectsInRange()
        {
            foreach (Vector3Int coordinates in this.GetBlocksInRange())
            {
                TreeComponent treeComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(coordinates);

                if (treeComponentAt != null)
                {
                    yield return (BaseComponent)treeComponentAt;
                }
            }
        }


        public IEnumerable<Vector3Int> GetBlocksInRange()
        {
            return this._blockObjectRange.GetBlocksOnTerrainInRectangularRadius(this._growthFertilizationRadius);
        }

        public string RangeName => this._prefab.PrefabName;

        public IEnumerable<Yielder> YieldersInArea()
        {
            return this._yieldersInArea;
        }

        public void OnEnterFinishedState()
        {
            // register building area
            this._growthFertilizationAreaService.UpdateBuildingArea(_buildingId);

            this._timeTrigger.Resume();
            this.Inventory.Enable();
            this.enabled = true;
        }

        public void OnExitFinishedState()
        {
            // un-register building area
            this._growthFertilizationAreaService.RemoveBuildingArea(_buildingId);

            this._timeTrigger.Pause();
            this.Inventory.Disable();
            this.enabled = false;
        }

        public void Save(IEntitySaver entitySaver)
        {
            entitySaver.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey)
                .Set(GrowthFertilizationBuilding.SupplyLeftKey, this._supplyLeft);

            entitySaver.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey)
                .Set(GrowthFertilizationBuilding.DailyGrowthKey, this._dailyGrowth);

            entitySaver.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey)
                .Set(GrowthFertilizationBuilding.AverageGrowthKey, this._averageGrowth);

            entitySaver.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey)
                .Set(GrowthFertilizationBuilding.WorkHoursPassed, this._workHoursPassed);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey))
                return;

            // update supply from save if available
            float? valueOrNullable = entityLoader.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey).GetValueOrNullable(GrowthFertilizationBuilding.SupplyLeftKey);
            this._supplyLeft = valueOrNullable.GetValueOrDefault();

            // update growth from save if available
            valueOrNullable = entityLoader.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey).GetValueOrNullable(GrowthFertilizationBuilding.DailyGrowthKey);
            this._dailyGrowth = valueOrNullable.GetValueOrDefault();

            // update average growth from save if available
            valueOrNullable = entityLoader.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey).GetValueOrNullable(GrowthFertilizationBuilding.AverageGrowthKey);
            this._averageGrowth = valueOrNullable.GetValueOrDefault();

            // update passed work hours from save if available
            int? intValueOrNullable = entityLoader.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey).GetValueOrNullable(GrowthFertilizationBuilding.WorkHoursPassed);
            this._workHoursPassed = intValueOrNullable.GetValueOrDefault();
        }

        public GoodConsumingToggle GetGoodConsumingToggle()
        {
            GoodConsumingToggle goodConsumingToggle = new GoodConsumingToggle();
            this._toggles.Add(goodConsumingToggle);
            goodConsumingToggle.StateChanged += (EventHandler)((_1, _2) => this.UpdateConsumptionState());
            return goodConsumingToggle;
        }
        private void UpdateConsumptionState()
        {
            this.ConsumptionPaused = this._toggles.FastAny<GoodConsumingToggle>((Predicate<GoodConsumingToggle>)(toggle => toggle.Paused));
        }

        private bool UpdateConsumption(float goodConsume)
        {
            this.IsConsuming = !this.ConsumptionPaused && this._blockableBuilding.IsUnblocked && this.ConsumeSupplies(goodConsume);

            return this.IsConsuming;
        }
        private bool ConsumeSupplies(float goodConsume)
        {
            if ((double)this._supplyLeft > 0.0)
            {
                this._supplyLeft -= goodConsume;
                return true;
            }
            if ((double)this._supplyLeft > 0.0 || this.Inventory.UnreservedAmountInStock(this._supply) <= 0)
            {
                return false;
            }


            this.Inventory.Take(new GoodAmount(this._supply, 1));
            this._supplyLeft = 1f;
            return true;
        }

        private void FertilizeNearbyGrowingTrees()
        {
            this.UpdateNearbyGrowingTrees();

            //this._yieldFertilizationService.UpdateRegisteredYielders(0.1f);

            // check if working day has finished
            if (this._workplaceWorkingHours.AreWorkingHours && (0 < this._workshop.NumberOfWorkersWorking))
            {
                _workHoursPassed++; // increment for each hour worker is actively there
                _consumptionPerHour = 0;
                float growthTimeOffsetCycle = 0.0f;
                float growthTimeTotal_d =   0.0f;
                float growthTimeTgt_d = 0.0f;
                float growthTimeOffset = 0.0f;
                float growthFertilizerConsumption = 0.0f;

                foreach (Growable growable in this._nearbyGrowingTrees)
                {
                    // get original growth time
                    growthTimeTotal_d = growable.GrowthTimeInDays;

                    // calculate targeted growth time 
                    growthTimeTgt_d = growthTimeTotal_d * _growthFactor;

                    // calculate offset to be applied to growable each day
                    growthTimeOffset = (((1.0f / growthTimeTgt_d) - (1.0f / growthTimeTotal_d)) / 100.0f);

                    // add proportional effect that growth effect diminishes during the day
                    // x =  ((hoursInDay+1) - workHour) * (growthTimeOffset * !HoursOfDay)
                    // x =  ((24+1) - t) * (e.g. 3.33 * 300)
                    growthTimeOffsetCycle = ((25.0f - (float)_workHoursPassed) * (growthTimeOffset / 300.0f));

                    // calculate consumption
                    growthFertilizerConsumption = _consumptionFactor * growthTimeOffset * _timeTriggerCallCountPerDay;

                    _consumptionPerHour += growthFertilizerConsumption;

                    if (UpdateConsumption(growthFertilizerConsumption))
                    {
                        // update growth
                        growable.IncreaseGrowthProgress(growthTimeOffsetCycle);
                    }
                }

                // growth increase in this cycle, reference as percentage of the whole growth time
                _dailyGrowth += ((growthTimeOffsetCycle / (1 / growthTimeTotal_d)) * 100.0f);

                foreach (TreeComponent treeComponent in this._nearbyYieldTrees)
                {
                    // get original yield growth time
                    treeComponent.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower );
                    treeComponent.TryGetComponentFast<Gatherable>(out Gatherable gatherable);

                    if ((null != gatherable)
                        && (null != yieldGrower))
                    {
                        growthTimeTotal_d = gatherable.YieldGrowthTimeInDays;

                        growthTimeTgt_d = growthTimeTotal_d * _yieldFactor;

                        growthTimeOffset = (((1.0f / growthTimeTgt_d) - (1.0f / growthTimeTotal_d)) / 100.0f);

                        growthFertilizerConsumption = _consumptionFactor * growthTimeOffset;

                        _consumptionPerHour += growthFertilizerConsumption;

                        float growthProgress = yieldGrower.GrowthProgress;

                        if (UpdateConsumption(growthFertilizerConsumption))
                        {
                            // update growth
                            yieldGrower.FastForwardGrowth(growthTimeOffset);
                        }

                        Debug.Log("Yield: " + growthProgress + " --> " + yieldGrower.GrowthProgress);

                    }
                }

                // yield growth increase in this cycle, reference as percentage of the whole growth time
                _dailyYieldInc += ((growthTimeOffset / (1 / growthTimeTotal_d)) * 100.0f);


            }
            this._timeTrigger.Reset();
            this._timeTrigger.Resume();
        }

        private void UpdateNearbyGrowingTrees()
        {
            this._nearbyGrowingTrees.Clear();
            this._nearbyYieldTrees.Clear();

            _treesInRangeCount = 0;

            foreach (Vector3Int coordinates in this._growthFertilizationAreaService.GetRegisteredFertilizationArea(_buildingId))
            {
                TreeComponent treeComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponent>(coordinates);

                if (treeComponentAt != null)
                {
                    treeComponentAt.TryGetComponentFast<Growable>(out Growable growable);

                    if (growable != null)
                    {
                        // check if still growing
                        if (growable.GrowthInProgress)
                        {
                            this._nearbyGrowingTrees.Add(growable);
                        }
                        else if (growable.IsGrown)
                        {
                            treeComponentAt.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower);

                            if (yieldGrower != null)
                            {
                                if (yieldGrower.GrowthProgress < 1.0f)
                                {
                                    this._nearbyYieldTrees.Add(treeComponentAt);
                                }
                            }

                        }
                    }
                    _treesInRangeCount++;
                }
            }
        }
    }
}