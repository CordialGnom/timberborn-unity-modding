
using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Modding;
using UnityEngine;

// runtime parameter storage (initialized from Config file)
// can be manually changed through the UI at runtime

namespace Mods.ForestTool.Scripts
{
    public static class ForestToolParam
    {
        // todo Corial: link following to an external configuration file / or store in player data
        public static List<string> DefaultTreeTypesAllFactions = new() { "Birch", "Pine", "Oak" };
        private static bool _DefaultActiveMangrove = false;
        private static bool _DefaultActiveEmptySpots = true;

        private static ForestToolFactionSpecService _forestToolFactionSpecService;


        private static List<ForestToolTypeConfig> _ForestToolTypeConfig = new();
        private static int _resourceCount = 0;
        private static int _randomTotal = 0;
        private static bool _isInit = false;
        private static readonly string _NameEmpty = "Empty Spot";


        private static readonly System.Random _random = new System.Random();


        public static void ResourceEnable(string resourceName)
        {
            SetResourceState(resourceName, true);
        }
        public static void ResourceDisable(string resourceName)
        {
            SetResourceState(resourceName, false);
        }

        public static void SetResourceState(string resourceName, bool setting )
        {
            foreach (ForestToolTypeConfig treeConfig in _ForestToolTypeConfig)
            {

                if (resourceName == treeConfig.TreeName)
                {
                    // disable resource
                    treeConfig.TreeEnabled = setting;

                    // update randomization total
                    RandomTotalUpdate();

                    return;
                }
            }
        }
        public static bool GetResourceState(string resourceName)
        {
            foreach (ForestToolTypeConfig treeConfig in _ForestToolTypeConfig)
            {
                if (resourceName == treeConfig.TreeName)
                {
                    return treeConfig.TreeEnabled;
                }
            }
            return false;
        }

        public static List<string> GetResourceNames()
        {
            List<string> resourceNames = new();

            foreach (ForestToolTypeConfig treeConfig in _ForestToolTypeConfig)
            {
                resourceNames.Add(treeConfig.TreeName);
            }
            return resourceNames;
        }

        public static int ResourceCount
        {
            get
            {
                return _resourceCount;
            }
            set
            {
                _resourceCount = value;
            }
        }

        public static ForestToolFactionSpecService ForestToolFactionSpecService
        {
            get
            {
                return _forestToolFactionSpecService;
            }
            set
            {
                _forestToolFactionSpecService = value;
            }
        }

        public static int RandomTotal
        {
            get
            {
                return _randomTotal;
            }
            set
            {
                _randomTotal = value;
            }
        }

        public static bool ParamInitDone
        { 
            get
            {
                return _isInit;
            }
            set
            {
                _isInit = value;
            }
        }

        public static string NameEmpty
        {
            get { return _NameEmpty; }
        }

        public static int RandomTotalUpdate()
        {
            // reset total before new update
            RandomTotal = 0;

            // calculate amount of numbers to be base of randomization
            foreach (ForestToolTypeConfig treeConfig in _ForestToolTypeConfig)
            {
                Debug.Log("ForestTool config: " + treeConfig.TreeName);

                if (true == treeConfig.TreeEnabled)
                {
                    // increment randomtotal with the used value
                    RandomTotal += treeConfig.TreeValue;
                }
                else
                {
                    // keep last total and set index anyway (will be skipped on iteration)
                }

                // apply reference value (for iteration)
                treeConfig.TreeValueRef = RandomTotal;
            }
            return RandomTotal;
        }

        public static string GetNextRandomResourceName()
        {
            int rand = _random.Next(0, RandomTotal);

            // calculate amount of numbers to be base of randomization
            foreach (ForestToolTypeConfig treeConfig in _ForestToolTypeConfig)
            {
                if ((rand < treeConfig.TreeValueRef)
                    && (true == treeConfig.TreeEnabled))
                {
                    return treeConfig.TreeName;         
                }
            }

            // return empty string if no corresponding entry was found // should not happen
            return string.Empty;
        }

        public static void UpdateFromConfig()
        {
            // reset existing lists and referenced variables
            _ForestToolTypeConfig.Clear();

            // update count of resources
            ResourceCount = DefaultTreeTypesAllFactions.Count;

            for (int index = 0; index < ResourceCount; ++index)
            {
                string treeName =   DefaultTreeTypesAllFactions[index];

                if (ForestToolSpecificationService.VerifyPrefabName(treeName))
                {
                    _ForestToolTypeConfig.Add(new ForestToolTypeConfig
                    {
                        TreeName = treeName,
                        TreeEnabled = true,
                        TreeValue = 10,
                        TreeValueRef = 10
                    }) ;
                }
            }

            if (null == _forestToolFactionSpecService)
            {
                Debug.LogError("ForestTool: No Faction Spec Service in parameters");
            }
            else
            {
                ImmutableArray<string> factionTreeNames = ForestToolFactionSpecService.GetFactionTrees();

                foreach (string treeName in factionTreeNames)
                {
                    Debug.Log("UFC Found: " + treeName);

                    if (ForestToolFactionSpecService.VerifyPrefabName(treeName))
                    {
                        _ForestToolTypeConfig.Add(new ForestToolTypeConfig
                        {
                            TreeName = treeName,
                            TreeEnabled = (treeName == "Mangrove") ? _DefaultActiveMangrove : true,
                            TreeValue = 10,
                            TreeValueRef = 10
                        });
                    }
                }
            }

            // link if empty spots are also to be handled
            _ForestToolTypeConfig.Add(new ForestToolTypeConfig
            {
                TreeName = _NameEmpty,
                TreeEnabled = _DefaultActiveEmptySpots,
                TreeValue = 10,
                TreeValueRef = 10
            });

            // calculate Random Total
            RandomTotalUpdate();

            // set flag that init is complete
            ParamInitDone = true;
        }
    }
}
