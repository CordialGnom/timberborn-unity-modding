using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Common;
using Timberborn.DropdownSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;

namespace Cordial.Mods.PlantingOverrideTool.Scripts.UI
{
    public class PlantingOverrideDropDownProvider : IDropdownProvider, ILoadableSingleton
    {
        private readonly PlantingOverridePrefabSpecService _specService;
        private readonly EventBus _eventBus;
        private readonly List<string> _items = new();
        private readonly ILoc _loc;

        private string plantable = "Birch";

        public string PlantName => plantable;

        public PlantingOverrideDropDownProvider( PlantingOverridePrefabSpecService specService,
                                                        EventBus eventBus, 
                                                        ILoc loc )
        {
            _specService = specService;
            _eventBus = eventBus;
            _loc = loc;
        }

        public IReadOnlyList<string> Items
        {
            get => (IReadOnlyList<string>)this._items.AsReadOnlyList<string>();
        }

        public void Load()
        {
            if (this._specService != null)
            {

                ImmutableArray<string> allowedPlantables = this._specService.GetAllTrees();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                }
            }
        }

        public void ReloadAsTree()
        {
            this._items.Clear();

            if (this._specService != null)
            {

                ImmutableArray<string> allowedPlantables = this._specService.GetAllTrees();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                    plantable = PlantableLocKey(plant);
                }
            }
        }

        public void ReloadAsCrop()
        {
            this._items.Clear();

            if (this._specService != null)
            {
                ImmutableArray<string> allowedPlantables = this._specService.GetAllCrops();

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                    plantable = PlantableLocKey(plant);
                }
            }
        }


        public string GetValue()
        {
            return PlantableLocKey(plantable);
        }

        public void SetValue(string value)
        {
            plantable = GetPlantFromLocKey(value);

            bool isTree = this._specService.CheckIsTree(plantable);
            bool isCrop = this._specService.CheckIsCrop(plantable);

            if (isTree ^ isCrop)
            {
                this._eventBus.Post((object)new PlantingOverrideConfigChangeEvent(plantable, isTree));
            }
        }

        private string PlantableLocKey(string plantname)
        {
            string newName = plantname.Replace(" ", "");
            return _loc.T("NaturalResource." + newName + ".DisplayName");
        }

        private static string GetPlantFromLocKey(string locKey)
        {
            locKey.Replace("NaturalResource.", "");
            locKey.Replace(".DisplayName", "");
            return locKey;
        }
    }
}
