using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 类名 : 工具类 - RenderSettings
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-12-22 17:23
/// 功能 : 
/// </summary>
public static class RenderSettingsEx
{
    static public void SetFog(bool fog, FogMode mode,Color fogColor, float fogDensity,float fogStartDistance, float fogEndDistance)
    {
        RenderSettings.fog = fog;
        RenderSettings.fogMode = mode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

    static public void SetFog(bool fog, int mode, Color fogColor, float fogDensity, float fogStartDistance, float fogEndDistance)
    {
        SetFog(fog, (FogMode)mode, fogColor, fogDensity, fogStartDistance, fogEndDistance);
    }

    static public void CloseFog()
    {
        RenderSettings.fog = false;
    }

    static public void SetFogLinear(Color fogColor, float fogStartDistance, float fogEndDistance)
    {
        SetFog(true, FogMode.Linear, fogColor, 0, fogStartDistance, fogEndDistance);
    }

    static public void SetFogExponential(Color fogColor, float fogDensity)
    {
        SetFog(true, FogMode.Exponential, fogColor, fogDensity, 0, 1);
    }

    static public void SetFogExponentialSquared(Color fogColor, float fogDensity)
    {
        SetFog(true, FogMode.ExponentialSquared, fogColor, fogDensity, 0, 1);
    }

    static public void SetAmbientSkybox(float intensity = 1)
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.reflectionIntensity = intensity;
    }

    static public Color ReColorIntensity(Color org,float intensity = 0)
    {
        if (intensity > 0)
        {
            // float factor = Mathf.Pow(2, intensity);
            float in1 = (org.r + org.g + org.b) / 3f;
            float factor = intensity / in1;
            org = new Color(org.r * factor, org.g * factor, org.b * factor, org.a);
        }
        return org;
    }

    static public void SetAmbientGradient(Color skyColor, Color eqColor, Color gdColor, float intensity = 0)
    {
        RenderSettings.ambientIntensity = intensity;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ReColorIntensity(skyColor, intensity);
        RenderSettings.ambientEquatorColor = ReColorIntensity(eqColor, intensity);
        RenderSettings.ambientGroundColor = ReColorIntensity(gdColor, intensity);
    }

    static public void SetAmbientColor(Color skyColor, float intensity = 0)
    {
        RenderSettings.ambientIntensity = intensity;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ReColorIntensity(skyColor, intensity);
    }
}