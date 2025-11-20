using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Resets scene lighting to Unity's default URP settings
/// Add this to any object and run "Reset to Default Lighting" from context menu
/// </summary>
public class ResetToDefaultLighting : MonoBehaviour
{
    [ContextMenu("Reset to Default Lighting")]
    public void ResetLighting()
    {
        // Reset to default skybox ambient mode
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        
        // Reset ambient colors to Unity defaults
        RenderSettings.ambientSkyColor = new Color(0.212f, 0.227f, 0.259f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.047f, 0.043f, 0.035f, 1f);
        
        // Default skybox (Unity default)
        Material defaultSkybox = RenderSettings.skybox;
        if (defaultSkybox != null)
        {
            Debug.Log("Skybox retained: " + defaultSkybox.name);
        }
        
        // Disable fog by default
        RenderSettings.fog = false;
        RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.01f;
        
        // Reset any flashlights to reasonable defaults
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Spot)
            {
                light.intensity = 1.0f; // Unity default for spot lights
                light.range = 10f;
                light.spotAngle = 30f;
                light.innerSpotAngle = 21.8f;
                light.shadows = LightShadows.None;
                Debug.Log($"Reset light: {light.name}");
            }
            else if (light.type == LightType.Directional)
            {
                light.intensity = 1.0f;
                light.shadows = LightShadows.Soft;
                Debug.Log($"Reset directional light: {light.name}");
            }
        }
        
        Debug.Log("<color=green>✅ Scene lighting reset to Unity defaults!</color>");
        Debug.Log("<color=yellow>⚠️ Make sure your URP Asset has:</color>");
        Debug.Log("<color=yellow>   - HDR: Enabled</color>");
        Debug.Log("<color=yellow>   - Main Light: Enabled</color>");
        Debug.Log("<color=yellow>   - Additional Lights: Per Pixel</color>");
    }
}
