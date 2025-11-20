using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Adds default Unity scene lighting - run once to fix black scenes
/// </summary>
public class AddDefaultSceneLighting : MonoBehaviour
{
    [ContextMenu("Add Default Scene Lighting")]
    public void AddLighting()
    {
        // Reset ambient to default skybox mode
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        
        // Check if we have a directional light (sun)
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        bool hasDirectionalLight = false;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                hasDirectionalLight = true;
                // Reset it to defaults
                light.intensity = 1.0f;
                light.color = Color.white;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 1.0f;
                Debug.Log($"<color=green>✅ Reset Directional Light: {light.name}</color>");
            }
        }
        
        // Add directional light if none exists
        if (!hasDirectionalLight)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1.0f;
            dirLight.color = Color.white;
            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowStrength = 1.0f;
            
            // Position it like Unity's default
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Debug.Log("<color=green>✅ Created Directional Light!</color>");
        }
        
        // Disable fog
        RenderSettings.fog = false;
        
        Debug.Log("<color=cyan>🌟 Default scene lighting applied!</color>");
        Debug.Log("<color=yellow>Your scene should now be visible with normal lighting.</color>");
    }
    
    [ContextMenu("Add Outdoor Lighting (Bright)")]
    public void AddOutdoorLighting()
    {
        AddLighting();
        
        // Make it brighter for outdoor scenes
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = 1.5f;
                light.color = new Color(1f, 0.95f, 0.9f); // Slightly warm
            }
        }
        
        RenderSettings.ambientIntensity = 1.2f;
        Debug.Log("<color=cyan>☀️ Outdoor lighting applied - bright and visible!</color>");
    }
}
