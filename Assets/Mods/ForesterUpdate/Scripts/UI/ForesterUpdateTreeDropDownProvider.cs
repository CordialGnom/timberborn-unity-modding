using Cordial.Mods.ForesterUpdate.Scripts.UI.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Common;
using Timberborn.DropdownSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Cordial.Mods.ForesterUpdate.Scripts.UI
{
    public class ForesterUpdateTreeDropDownProvider : IDropdownProvider, ILoadableSingleton
    {
        private static readonly string NoPriorityItemLocKey = "Cordial.ForesterUpdate.Fragment.NoUpdateOption";
        private readonly ForesterUpdatePrefabSpecService _specService;
        private readonly EventBus _eventBus;
        private readonly List<string> _items = new List<string>();
        private readonly ILoc _loc;

        private string plantable;

        public string PlantName => plantable;

        public ForesterUpdateTreeDropDownProvider(  ForesterUpdatePrefabSpecService specService,
                                                    EventBus eventBus, 
                                                    ILoc loc )
        {
            _specService = specService;
            _eventBus = eventBus;
            _loc = loc;

            Debug.Log("ModCreated");
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
                this._items.Add(_loc.T(NoPriorityItemLocKey));

                foreach (string plant in allowedPlantables)
                {
                    this._items.Add(PlantableLocKey(plant));
                }

                Debug.Log("FUT: " + this._items.Count);
            }
            else
            {
                Debug.Log("FUT: No Building");
            }
        }

        public string GetValue()
        {

            Debug.Log("FUT: GetValue");
            return _loc.T((plantable == String.Empty) ? _loc.T(NoPriorityItemLocKey) : PlantableLocKey(plantable));
        }

        public void SetValue(string value)
        {
            Debug.Log("FUT: SetValue: " + value);
            if ((value != NoPriorityItemLocKey)
                && (value != _loc.T(NoPriorityItemLocKey)))
            {
                plantable = GetPlantFromLocKey(value);
            }
            else
            {
                plantable = String.Empty;
            }

            this._eventBus.Post((object)new ForesterUpdateConfigChangeEvent(plantable));

        }

        private string PlantableLocKey(string plantname)
        {
            return _loc.T("NaturalResource." + plantname + ".DisplayName");
        }

        private static string GetPlantFromLocKey(string locKey)
        {
            locKey.Replace("NaturalResource.", "");
            locKey.Replace(".DisplayName", "");

            return locKey;
        }
    }
}
