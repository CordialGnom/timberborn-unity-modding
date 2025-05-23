﻿using UnityEngine;
using Timberborn.SingletonSystem;
using Timberborn.Timbermesh;
    

namespace Cordial.Mods.BoosterJuice.Scripts.Material
{ 
    public class MaterialInitializer : ILoadableSingleton
    {
        IMaterialRepository _materialRepository;
        Color fertilizerColor = new Color(0.6f, 0.1f, 0.42f, 1.0f);
        Color fertilizerEmissionColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        Color fertilizerEmissionMapColor = new Color(0.04f, 0f, 0.4f, 1.0f);

        public MaterialInitializer(IMaterialRepository materialRepository)
        {
            _materialRepository = materialRepository;
        }

        public void Load()
        {
            var extract = _materialRepository.GetMaterial("Extract");
            //var water = _materialRepository.GetMaterial("Water");
            var fertilizer = _materialRepository.GetMaterial("Fertilizer");

            if ((extract != null)
                && (fertilizer != null))
            {
                // todo Cord: further tests of changing color properties required. 
                fertilizer.shader = extract.shader;
                // disable foam, suggestion based on Tobbert/Emberpelt code
                //fertilizer.SetVector("_FoamColor", fertilizerEmissionColor);
                //fertilizer.SetFloat("_FoamIntensity", 0.0f);

                fertilizer.CopyPropertiesFromMaterial(extract);

                fertilizer.SetColor("_Color", fertilizerColor);
                fertilizer.SetColor("_EmissionColor", fertilizerEmissionColor);
                fertilizer.SetColor("_EmissionMapColor", fertilizerEmissionMapColor);
                fertilizer.SetVector("_FoamColor", fertilizerColor);
                fertilizer.SetFloat("_FoamIntensity", 0.005f);
            }
            else
            {
                Debug.Log("No material found.");
            }
        }

    }
}