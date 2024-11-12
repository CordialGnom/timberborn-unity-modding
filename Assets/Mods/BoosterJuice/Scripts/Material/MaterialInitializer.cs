using UnityEngine;
using Timberborn.SingletonSystem;
using Timberborn.Timbermesh;
    

namespace Cordial.Mods.BoosterJuice.Scripts.Material
{ 
    public class MaterialInitializer : ILoadableSingleton
    {
        IMaterialRepository _materialRepository;
        Color fertilizerColor = new Color(0.58f, 0f, 0.42f, 1.0f);
        Color fertilizerEmissionColor = new Color(0.58f, 0f, 0.42f, 0.01f);

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
                fertilizer.CopyPropertiesFromMaterial(extract);
                fertilizer.shader = extract.shader;
                // disable foam, suggestion based on Tobbert/Emberpelt code
                //fertilizer.SetVector("_FoamColor", fertilizerEmissionColor);
                //fertilizer.SetFloat("_FoamIntensity", 0.0f);

                fertilizer.SetColor("_Color", fertilizerColor);
                //fertilizer.SetColor("_EmissionColor", fertilizerEmissionColor);
                fertilizer.SetVector("_FoamColor", fertilizerEmissionColor);
                fertilizer.SetFloat("_FoamIntensity", 0.005f);
            }
            else
            {
                Debug.Log("No material found.");
            }
        }

    }
}