using Cordial.Mods.PlantingOverride.Scripts.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Common;
using Timberborn.DropdownSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;

namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantingOverrideDropDownProvider : IDropdownProvider, ILoadableSingleton
    {
        private readonly PlantingOverridePrefabSpecService _plantOverrideSpecService;
        private readonly EventBus _eventBus;
        private readonly List<string> _items = new();
        private readonly List<string> _gameItemName = new();
        private readonly ILoc _loc;

        private string plantable = "Birch";

        public string PlantName => plantable;

        public PlantingOverrideDropDownProvider( PlantingOverridePrefabSpecService plantOverrideSpecService,
                                                        EventBus eventBus, 
                                                        ILoc loc )
        {
            _plantOverrideSpecService = plantOverrideSpecService;
            _eventBus = eventBus;
            _loc = loc;
        }

        public IReadOnlyList<string> Items
        {
            get => (IReadOnlyList<string>)this._items.AsReadOnlyList<string>();
        }

        public void Load()
        {
            if (this._plantOverrideSpecService != null)
            {
                ImmutableArray<string> allowedPlantables = this._plantOverrideSpecService.GetAllForestryPlantables();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                    this._gameItemName.Add(plant);
                }
            }
        }

        public void ReloadAsTree()
        {
            this._items.Clear();
            this._gameItemName.Clear();

            if (this._plantOverrideSpecService != null)
            {

                ImmutableArray<string> allowedPlantables = this._plantOverrideSpecService.GetAllForestryPlantables();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                    this._gameItemName.Add(plant);
                    plantable = plant;
                }
            }
        }

        public void ReloadAsCrop()
        {
            this._items.Clear();
            this._gameItemName.Clear();

            if (this._plantOverrideSpecService != null)
            {
                ImmutableArray<string> allowedPlantables = this._plantOverrideSpecService.GetAllCrops();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                    this._gameItemName.Add(plant);
                    plantable = plant;
                }
            }
        }

        public string GetValue()
        {
            // return (translated) name
            return PlantableLocKey(plantable);
        }

        // value is the translated name, find and set corresponding prefab name
        public void SetValue(string value)
        {
            plantable = GetNamedPlantable(value);

            bool isTree = this._plantOverrideSpecService.CheckIsForestry(plantable);
            bool isCrop = this._plantOverrideSpecService.CheckIsCrop(plantable);

            if (isTree ^ isCrop)
            {
                this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(plantable, isTree));
            }
        }

        public bool ItemSetIsTree()
        {
            return this._plantOverrideSpecService.CheckIsForestry(plantable);
        }

        private string PlantableLocKey(string plantname)
        {
            plantname = plantname.Replace(" ", "");
            plantname = plantname.Replace("Bush", "");
            return _loc.T("NaturalResource." + plantname + ".DisplayName");
        }

        // converts string value to plantable based on the value being compared to each entry
        private string GetNamedPlantable(string value)
        {
            // value contains the translated name
            // compare each entry in game items to find which value has been set. 
            foreach (string item in this._gameItemName)
            {
                // convert string item to loc key
                if (value == PlantableLocKey((string)item))
                {
                    return item;
                }
            }
            return String.Empty;
        }
    }
}
