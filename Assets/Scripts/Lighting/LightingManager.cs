using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LightingManager : MonoBehaviour
{
    [SerializeField] private Transform DirectionalParent;
    [SerializeField] private Light SunDirectionalLight;
    [SerializeField] private Light MoonDirectionalLight;
    [SerializeField] private LightingPreset Preset;
    [SerializeField] private Material SkyboxMat;

    public void UpdateLighting(float timePercent)
    {
        if (Preset == null)
        {
            Debug.LogError("No Lighting Preset Set");
            return;
        }
        
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (SunDirectionalLight != null && MoonDirectionalLight!=null && DirectionalParent!=null)
        {
            SunDirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            MoonDirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            SunDirectionalLight.intensity = Preset.SunIntensityCurve.Evaluate(timePercent);
            MoonDirectionalLight.intensity = Preset.MoonIntensityCurve.Evaluate(timePercent);

            DirectionalParent.transform.rotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, -90f, -90f));
        }
        
        if (SkyboxMat != null)
        {
            SkyboxMat.SetColor("_SkyColor", Preset.SkyColor.Evaluate(timePercent));
            SkyboxMat.SetColor("_HorizonColor", Preset.HorizonColor.Evaluate(timePercent));
            if (timePercent <= 0.27 || timePercent >= 0.73)
            {
                SkyboxMat.SetInt("_EnableStar", 1);
            }
            else
            {
                SkyboxMat.SetInt("_EnableStar", 0);
            }
            Shader.SetGlobalVector("_SunDirection", SunDirectionalLight.transform.forward);
            Shader.SetGlobalVector("_MoonDirection", MoonDirectionalLight.transform.forward);
        }
    }
    private void OnValidate()
    {
        if (SunDirectionalLight != null)
            return;

        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            SunDirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    SunDirectionalLight = light;
                    return;
                }
            }
        }
        
    }
}
