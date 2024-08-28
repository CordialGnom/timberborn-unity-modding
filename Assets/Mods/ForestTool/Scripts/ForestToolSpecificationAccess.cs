using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Timberborn.FactionSystem;
using Timberborn.GameFactionSystem;
using Timberborn.NaturalResources;
using Timberborn.Persistence;
using Timberborn.PrefabSystem;
using Timberborn.SingletonSystem;
using System.IO;
using System.Linq;
using UnityEngine;



namespace Mods.ForestTool.Scripts
{
    public static class ForestToolSpecificationService
    {
        private static List<string> _defaultTreeNames = ForestToolParam.DefaultTreeTypesAllFactions;


        //public static void GetAll()
        //{

        //    SpecificationRepository specificationRepository = DependencyContainer.GetInstance<SpecificationRepository>();

        //    IEnumerable<ISpecification> prefabSpecs = specificationRepository.GetBySpecification("NaturalResources");
        //    IEnumerable<ISpecification> naturalResourceSpecs = specificationRepository.GetBySpecification("PrefabCollectionSpecification.NaturalResources");


        //    if (IsNullOrEmpty(prefabSpecs))
        //    {
        //        Mod.Log.LogError("no prefab collection specs");
        //    }
        //    else
        //    {
        //        foreach (ISpecification spec in prefabSpecs)
        //        {
        //            Mod.Log.LogInfo("PS: " + spec.FullName);
        //        }
        //    }

        //    if (IsNullOrEmpty(naturalResourceSpecs))
        //    {
        //        Mod.Log.LogError("no natural resource specs");
        //    }
        //    else
        //    {
        //        foreach (ISpecification spec in naturalResourceSpecs)
        //        {
        //            Mod.Log.LogInfo("PS: " + spec.FullName);
        //        }
        //    }
        //}

        //public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        //{
        //    if (source != null)
        //    {
        //        foreach (T obj in source)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        public static List<string> VerifyTreeNamesPrefab()
        {
            PrefabNameMapper prefabNameMapper = null; // ForestToolDependencyContainer.GetInstance<PrefabNameMapper>();
            List<string> treeNamesOut = new();
            treeNamesOut.Clear();       // ensure no artifacts are in new list

            string treeNameOut = "";

            if (null != prefabNameMapper)
            {
                foreach (string treeNameIn in _defaultTreeNames)
                {
                    prefabNameMapper.TryGetPrefabName(treeNameIn, out treeNameOut);

                    if ("" != treeNameOut)
                    {
                        Debug.Log("VTNP Found: " + treeNameOut);

                        treeNamesOut.Add(treeNameOut);
                    }

                }
            }
            return treeNamesOut;
        }

        //public static string VerifyPrefabName(string prefabNameInp)
        //{
        //    PrefabNameMapper prefabNameMapper = DependencyContainer.GetInstance<PrefabNameMapper>();
 
        //    string prefabNameOut = "";

        //    if (null != prefabNameMapper)
        //    {
        //        if (prefabNameMapper.TryGetPrefabName(prefabNameInp, out prefabNameOut))
        //        { 
        //            Mod.Log.LogInfo("Found: " + prefabNameOut);  
        //        }
        //    }
        //    return prefabNameOut;
        //}

        public static bool VerifyPrefabName(string prefabNameInp)
        {
            PrefabNameMapper prefabNameMapper = null; // ForestToolDependencyContainer.GetInstance<PrefabNameMapper>();

            string prefabNameOut = "";
            bool prefabValid = false;

            if (null != prefabNameMapper)
            {
                if (prefabNameMapper.TryGetPrefabName(prefabNameInp, out prefabNameOut))
                {
                    Debug.Log("Found: " + prefabNameOut);
                    prefabValid = true;
                }
            }
            else
            {
                // todo cordial: workaround to load hardcoded trees
                prefabValid= true;  
            }
            
            return prefabValid;
        }
        // how timberapi gets specifications: 
        // could be used somehow to access path to timberborn specs
        //public void Load()
        //{
        //    this.AddRange((IEnumerable<ISpecification>)FileService.GetFiles(Path.Combine(Paths.TimberApi, SpecificationRepository.SpecificationPath), "*.json").Select<string, FileSpecification>((Func<string, FileSpecification>)(filePath => new FileSpecification(filePath))));

        //    this.AddRange((IEnumerable<ISpecification>)FileService.GetFiles(ImmutableArrayExtensions.Select<IMod, string>(this._modRepository.All(), (Func<IMod, string>)(mod => Path.Combine(mod.DirectoryPath, mod.SpecificationPath))), "*.json").Select<string, FileSpecification>((Func<string, FileSpecification>)(filePath => new FileSpecification(filePath))));

        //    this.AddRange((IEnumerable<ISpecification>)((IEnumerable<TextAsset>)Resources.LoadAll<TextAsset>(SpecificationRepository.SpecificationPath)).Select<TextAsset, TimberbornSpecification>((Func<TextAsset, TimberbornSpecification>)(asset => new TimberbornSpecification(asset))));
        //}

    }
}
