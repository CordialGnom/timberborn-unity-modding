
using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Modding;
using UnityEngine;

// runtime parameter storage (initialized from Config file)
// can be manually changed through the UI at runtime

namespace Cordial.Mods.ForestTool.Scripts
{
    public static class ForestToolParam
    {
        private static ForestToolPrefabSpecService _forestToolPrefabSpecService;


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
                    // set resource state
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
        public static ForestToolPrefabSpecService ForestToolPrefabSpecService
        {
            get
            {
                return _forestToolPrefabSpecService;
            }
            set
            {
                _forestToolPrefabSpecService = value;
            }
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

        public static void InitConfigDefault()
        {
            // reset existing lists and referenced variables
            _ForestToolTypeConfig.Clear();

            if (null == _forestToolPrefabSpecService)
            {
                Debug.LogError("ForestTool: No Faction Spec Service in parameters");
            }
            else
            {
                ImmutableArray<string> factionTreeNames = _forestToolPrefabSpecService.GetAllTrees();

                foreach (string treeName in factionTreeNames)
                {
                    _ForestToolTypeConfig.Add(new ForestToolTypeConfig
                    {
                        TreeName = treeName,
                        TreeEnabled = true,
                        TreeValue = 10,
                        TreeValueRef = 10
                    });
                }

                // link if empty spots are also to be handled
                _ForestToolTypeConfig.Add(new ForestToolTypeConfig
                {
                    TreeName = _NameEmpty,
                    TreeEnabled = true,
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
}
