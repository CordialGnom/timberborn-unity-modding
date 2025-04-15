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
using Cordial.Mods.BoosterJuice.Scripts.Events;
using Timberborn.Cutting;
using Timberborn.GoodStackSystem;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.WorldPersistence;

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
        private float _growthConsumptionFactor;

        [SerializeField]
        private float _yieldFactor;

        [SerializeField]
        private float _yieldConsumptionFactor;

        [SerializeField]
        private int _growthFertilizationRadius;

        [SerializeField]
        private int _capacity;

        [SerializeField]
        private string _supply;

        private static readonly ComponentKey GrowthFertilizationBuildingKey = new ComponentKey(nameof(GrowthFertilizationBuilding));
        private static readonly PropertyKey<float> SupplyLeftKey = new PropertyKey<float>("SupplyLeft");
        private static readonly PropertyKey<float> DailyGrowthKey = new PropertyKey<float>("DailyGrowth");
        private static readonly PropertyKey<float> AverageGrowthKey = new PropertyKey<float>("AverageGrowth");
        private static readonly PropertyKey<int> WorkHoursPassed = new PropertyKey<int>("WorkHoursPassed");
        private static readonly PropertyKey<float> DailyYieldKey = new PropertyKey<float>("DailyYield");
        private static readonly PropertyKey<float> AverageYieldKey = new PropertyKey<float>("AverageYield");
        private static readonly PropertyKey<bool> FertilizeYieldKey = new PropertyKey<bool>("FertilizeYieldKey");

        // from GoodConsumingBuilding
        private BlockableBuilding _blockableBuilding;
        private readonly List<GoodConsumingToggle> _toggles = new List<GoodConsumingToggle>();
        private float _supplyLeft;

        // tree handling
        private readonly List<Growable> _nearbyGrowingTrees = new List<Growable>();
        private readonly List<TreeComponentSpec> _nearbyYieldTrees = new List<TreeComponentSpec>();

        public Inventory Inventory { get; private set; }

        public bool IsConsuming { get; private set; }

        public bool ConsumptionPaused { get; private set; }

        public int TreesTotalCount => this._treesInRangeCount;
        public int TreesGrowCount => (FertilizeYieldActive) ? (this._nearbyGrowingTrees.Count + this._nearbyYieldTrees.Count) : this._nearbyGrowingTrees.Count;
        public int TreesYieldCount => this._nearbyYieldTrees.Count;
        public bool FertilizeYieldActive { get; set; }  

        public IEnumerable<Growable> GrowingTrees => this._nearbyGrowingTrees;

        public float ConsumptionPerHour => this._consumptionPerHour;
        public string Supply => this._supply;

        public int Capacity => this._capacity;

        public int SupplyLeft => this.Inventory.UnreservedAmountInStock(this._supply);

        public float SupplyAmount
        {
            get => (float)this.Inventory.UnreservedAmountInStock(this._supply) + this._supplyLeft;
        }
        public float DailyGrowth => (this._dailyGrowth *100.0f);

        public float AverageGrowth => (this._averageGrowth);

        public float GrowthFactor => (this._growthFactor);
        public float YieldFactor => (this._yieldFactor);
        public float AverageYieldInc => (this._averageYieldInc);

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
        private IBlockService _blockService;
        private BlockObjectRange _blockObjectRange;

        private readonly List<Yielder> _yieldersInArea = new();

        private ITimeTriggerFactory _timeTriggerFactory;
        private ITimeTrigger _timeTrigger;

        // unknown
        private PrefabSpec _prefab;


        [Inject]
        public void InjectDependencies( IBlockService blockService,
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
            this._prefab = this.GetComponentFast<PrefabSpec>();
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
                TreeComponentSpec treeComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponentSpec>(coordinates);

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
            IObjectSaver saver = entitySaver.GetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey);

            if (saver != null)
            {
                saver.Set(GrowthFertilizationBuilding.SupplyLeftKey, this._supplyLeft);
                saver.Set(GrowthFertilizationBuilding.DailyGrowthKey, this._dailyGrowth);
                saver.Set(GrowthFertilizationBuilding.AverageGrowthKey, this._averageGrowth);
                saver.Set(GrowthFertilizationBuilding.WorkHoursPassed, this._workHoursPassed);
                saver.Set(GrowthFertilizationBuilding.DailyYieldKey, this._dailyYieldInc);
                saver.Set(GrowthFertilizationBuilding.AverageYieldKey, this._averageYieldInc);
                saver.Set(GrowthFertilizationBuilding.FertilizeYieldKey, this.FertilizeYieldActive);
            }
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.TryGetComponent(GrowthFertilizationBuilding.GrowthFertilizationBuildingKey, out IObjectLoader loader))
                return;

            if (loader != null)
            {
                // update supply from save if available
                float? valueOrNullable = loader.Get(GrowthFertilizationBuilding.SupplyLeftKey);
                this._supplyLeft = valueOrNullable.GetValueOrDefault();

                // update growth from save if available
                valueOrNullable = loader.Get(GrowthFertilizationBuilding.DailyGrowthKey);
                this._dailyGrowth = valueOrNullable.GetValueOrDefault();

                // update average growth from save if available
                valueOrNullable = loader.Get(GrowthFertilizationBuilding.AverageGrowthKey);
                this._averageGrowth = valueOrNullable.GetValueOrDefault();

                // update passed work hours from save if available
                int? intValueOrNullable = loader.Get(GrowthFertilizationBuilding.WorkHoursPassed);
                this._workHoursPassed = intValueOrNullable.GetValueOrDefault();

                // update yield from save if available
                valueOrNullable = loader.Get(GrowthFertilizationBuilding.DailyYieldKey);
                this._dailyYieldInc = valueOrNullable.GetValueOrDefault();

                // update average yield from save if available
                valueOrNullable = loader.Get(GrowthFertilizationBuilding.AverageYieldKey);
                this._averageYieldInc = valueOrNullable.GetValueOrDefault();

                // update passed work hours from save if available
                bool? boolValueOrNullable = loader.Get(GrowthFertilizationBuilding.FertilizeYieldKey);
                this.FertilizeYieldActive = boolValueOrNullable.GetValueOrDefault();
            }
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

            // check if working day is in progress
            if (this._workplaceWorkingHours.AreWorkingHours && (0 < this._workshop.NumberOfWorkersWorking))
            {
                _workHoursPassed++; // increment for each hour worker is actively there
                _consumptionPerHour = 0;
                float growthTimeOffsetCycle = 0.0f;
                float growthTimeTotal_d =   0.0f;
                float growthTimeTgt_d = 0.0f;
                float growthTimeOffset = 0.0f;
                float growthFertilizerConsumption = 0.0f;

                // apply fertilizer to growing trees
                foreach (Growable growable in this._nearbyGrowingTrees)
                {
                    // get original growth time
                    growthTimeTotal_d = growable.GrowthTimeInDays;

                    // calculate targeted growth time 
                    growthTimeTgt_d = growthTimeTotal_d * _growthFactor;

                    // calculate offset to be applied to growable each day
                    growthTimeOffset = (((1.0f / growthTimeTgt_d) - (1.0f / growthTimeTotal_d)));

                    // add proportional effect that growth effect diminishes during the day
                    // x =  ((hoursInDay+1) - workHour) * (growthTimeOffset * !HoursOfDay)
                    // x =  ((24+1) - t) * (e.g. 3.33 * 300)
                    growthTimeOffsetCycle = ((25.0f - (float)_workHoursPassed) * (growthTimeOffset / 300.0f));


                    // calculate consumption
                    // 10.11.2024: changed from a relative consumption based on tree growth, set to a fixed value to 
                    // instead for clarity. 
                    //growthFertilizerConsumption = _growthConsumptionFactor * growthTimeOffset * _timeTriggerCallCountPerDay;
                    // the growth time offset here is based on the original calculation referencing oak. 
                    growthFertilizerConsumption = _growthConsumptionFactor * 0.333f * _timeTriggerCallCountPerDay;

                    _consumptionPerHour += growthFertilizerConsumption;

                    // store current state to compare
                    float growthTimeDone = growable.GrowthProgress;

                    if (UpdateConsumption(growthFertilizerConsumption))
                    {
                        // update growth
                        growable.IncreaseGrowthProgress(growthTimeOffsetCycle);
                    }
                }

                // growth increase in this cycle, reference as percentage of the whole growth time
                _dailyGrowth += ((growthTimeOffsetCycle / (1 / growthTimeTotal_d)));

                if (FertilizeYieldActive)
                {
                    // apply fertilizer to "yielding" trees
                    foreach (TreeComponentSpec treeComponent in this._nearbyYieldTrees)
                    {
                        // get original yield growth time
                        treeComponent.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower);
                        treeComponent.TryGetComponentFast<Gatherable>(out Gatherable gatherable);

                        if ((null != gatherable)
                            && (null != yieldGrower))
                        {
                            growthTimeTotal_d = gatherable.YieldGrowthTimeInDays;

                            float growthTimeTotal_h = growthTimeTotal_d * 24.0f;
                            float growthTimePerHourDflt_pc = 1.0f / growthTimeTotal_h;
                            float growthTimePerHourComb_pc = growthTimePerHourDflt_pc / _yieldFactor;

                            growthTimeOffset = growthTimePerHourComb_pc - growthTimePerHourDflt_pc;

                            growthFertilizerConsumption = (_yieldConsumptionFactor/100.0f);

                            _consumptionPerHour += growthFertilizerConsumption;

                            float growthProgress = yieldGrower.GrowthProgress;

                            if (UpdateConsumption(growthFertilizerConsumption))
                            {
                                // update growth
                                yieldGrower.FastForwardGrowth(growthTimeOffset);
                            }
                        }
                    }

                    // yield growth increase in this cycle, reference as percentage of the whole growth time
                    _dailyYieldInc += ((growthTimeOffset / (1 / growthTimeTotal_d)) * 100.0f);
                }
                else
                {
                    _dailyYieldInc = 0.0f;
                }

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
                TreeComponentSpec treeComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponentSpec>(coordinates);

                if (treeComponentAt != null)
                {
                    treeComponentAt.TryGetComponentFast<Growable>(out Growable growable);
                    treeComponentAt.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);

                    if ((growable != null)
                        && (livingResource != null))
                    {
                        if (!livingResource.IsDead)
                        { 
                            // check if still growing (and planted --> growth progress must have started
                            if ((0.0f < growable.GrowthProgress)
                                && (growable.GrowthProgress < 1.0f))
                            {
                                this._nearbyGrowingTrees.Add(growable);
                            }
                            // check if it is grown, and still alive!
                            else if (growable.IsGrown)
                            {
                                // check if component is only a stump
                                treeComponentAt.TryGetComponentFast<Cuttable>(out Cuttable cuttable);
                                treeComponentAt.TryGetComponentFast<Gatherable>(out Gatherable gatherable);
                                treeComponentAt.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower);

                                // growable is already known
                                if ((cuttable != null)
                                    && (yieldGrower != null))
                                {
                                    bool gatherableEmpty = false;
                                    bool invIsEmpty = livingResource.GetComponentFast<GoodStack>().Inventory.IsEmpty;

                                    // must be a cuttable
                                    // must be a living resource
                                    // --> check if fully grown (= 1.0)
                                    // --> check if not yielding (cuttable / gatherable)

                                    bool cuttableEmpty = (cuttable.Yielder.Yield.Amount == 0);

                                    // not all trees have gatherables
                                    if (gatherable != null)
                                    {
                                        gatherableEmpty = (gatherable.Yielder.Yield.Amount == 0);
                                    }

                                    // is a stump or not: nothing to yield, and growth is done (checked before)
                                    if (invIsEmpty && cuttableEmpty && gatherableEmpty)
                                    {
                                        // ignore stumps, do not mark as part of the selection
                                    }
                                    else // a tree or a markable stump
                                    {
                                        if (yieldGrower.GrowthProgress < 1.0f)
                                        {
                                            this._nearbyYieldTrees.Add(treeComponentAt);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _treesInRangeCount++;
                }
            }
        }

        private void UpdateNearbyGrowingBushes()
        {
            this._nearbyGrowingTrees.Clear();
            this._nearbyYieldTrees.Clear();

            _treesInRangeCount = 0;

            foreach (Vector3Int coordinates in this._growthFertilizationAreaService.GetRegisteredFertilizationArea(_buildingId))
            {
                TreeComponentSpec treeComponentAt = this._blockService.GetBottomObjectComponentAt<TreeComponentSpec>(coordinates);

                if (treeComponentAt != null)
                {
                    treeComponentAt.TryGetComponentFast<Growable>(out Growable growable);
                    treeComponentAt.TryGetComponentFast<LivingNaturalResource>(out LivingNaturalResource livingResource);

                    if ((growable != null)
                        && (livingResource != null))
                    {
                        if (!livingResource.IsDead)
                        {
                            // check if still growing (and planted --> growth progress must have started
                            if ((0.0f < growable.GrowthProgress)
                                && (growable.GrowthProgress < 1.0f))
                            {
                                this._nearbyGrowingTrees.Add(growable);
                            }
                            // check if it is grown, and still alive!
                            else if (growable.IsGrown)
                            {
                                // check if component is only a stump
                                treeComponentAt.TryGetComponentFast<Cuttable>(out Cuttable cuttable);
                                treeComponentAt.TryGetComponentFast<Gatherable>(out Gatherable gatherable);
                                treeComponentAt.TryGetComponentFast<GatherableYieldGrower>(out GatherableYieldGrower yieldGrower);

                                // growable is already known
                                if ((cuttable != null)
                                    && (yieldGrower != null))
                                {
                                    bool gatherableEmpty = false;
                                    bool invIsEmpty = livingResource.GetComponentFast<GoodStack>().Inventory.IsEmpty;

                                    // must be a cuttable
                                    // must be a living resource
                                    // --> check if fully grown (= 1.0)
                                    // --> check if not yielding (cuttable / gatherable)

                                    bool cuttableEmpty = (cuttable.Yielder.Yield.Amount == 0);

                                    // not all trees have gatherables
                                    if (gatherable != null)
                                    {
                                        gatherableEmpty = (gatherable.Yielder.Yield.Amount == 0);
                                    }

                                    // is a stump or not: nothing to yield, and growth is done (checked before)
                                    if (invIsEmpty && cuttableEmpty && gatherableEmpty)
                                    {
                                        // ignore stumps, do not mark as part of the selection
                                    }
                                    else // a tree or a markable stump
                                    {
                                        if (yieldGrower.GrowthProgress < 1.0f)
                                        {
                                            this._nearbyYieldTrees.Add(treeComponentAt);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _treesInRangeCount++;
                }
            }
        }

        [OnEvent]
        public void OnGrowthFertilizationConfigChangeEvent(GrowthFertilizationConfigChangeEvent configChangeEvent)
        {
            if (null == configChangeEvent)
                return;

            FertilizeYieldActive = configChangeEvent.FertilizeYield;
        }
    }
}