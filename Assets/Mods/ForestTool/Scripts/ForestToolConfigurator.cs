using Bindito.Core;
using HarmonyLib;
using TimberApi.Tools.ToolSystem;
using Timberborn.PrefabSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mods.ForestTool.Scripts {
    [Context("Game")]
    public class ForestToolConfigurator : IConfigurator
    {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ForestToolInitializer>().AsSingleton();
            containerDefinition.Bind<ForestToolFactionSpecService>().AsSingleton();
            containerDefinition.Bind<ForestToolPanel>().AsSingleton();
            containerDefinition.Bind<IForestTool>().To<ForestTool>().AsSingleton();
            //containerDefinition.Bind<ForestToolErrorUIPanel>().AsSingleton();
            containerDefinition.Bind<ForestTool>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<ForestToolFactory>().AsSingleton();
            containerDefinition.MultiBind<PrefabNameMapper>().AsSingleton();
        }


        [HarmonyPatch(typeof(ForestTool), "EnterTool")]
        public static class ForestToolEnterPatch
        {
            [HarmonyPostfix]
            private static void Postfix(ref ForestToolPanel __result, ref ForestTool __instance)
            {
                //ForestToolPanel _treePanel = DependencyContainer.GetInstance<ForestToolPanel>();
                //ForestToolErrorUIPanel _errorUIPanel = DependencyContainer.GetInstance<ForestToolErrorUIPanel>();
                //ForestTool _ForestTool = DependencyContainer.GetInstance<ForestTool>();

                VisualElement root = new();

                if ((null == root)
                    || (null == __result))
                {
                    Debug.LogError("ForestTool: Patch - No UIs");
                }
                else
                {
                    Debug.Log("ForestTool: Patch");

                    if (__instance.IsUnlocked)
                    {
                        if (__result.GetUIEnabledByKey())
                        {
                            root.Insert(0, __result.GetPanelConfigUi());
                            __result.OnUIConfirmed();
                        }
                        else
                        {
                            // no panel, continue to standard tool execution
                        }
                    }
                    else
                    {
                        root.Insert(0, __result.GetPanelErrorUi());
                        __result.OnUIConfirmed();
                    }
                }
            }
        }
    }
}