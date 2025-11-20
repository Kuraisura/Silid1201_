using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Complete scene lighting setup - fixes all lighting issues at once
/// </summary>
[ExecuteAlways]
public class CompleteSceneLightingFix : MonoBehaviour
{
    void OnEnable()
    {
        FixEverything();
    }
    
    [ContextMenu("FIX EVERYTHING NOW")]
    public void FixEverything()
    {
        Debug.Log("<color=cyan>🔧 FIXING ALL LIGHTING ISSUES...</color>");
        
        // 1. Fix Ambient Lighting
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.212f, 0.227f, 0.259f); // Default Unity gray
        RenderSettings.ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f);
        RenderSettings.ambientGroundColor = new Color(0.047f, 0.043f, 0.035f);
        RenderSettings.ambientIntensity = 1.0f;
        Debug.Log("<color=green>✅ Ambient lighting set to normal</color>");
        
        // 2. Disable Fog
        RenderSettings.fog = false;
        Debug.Log("<color=green>✅ Fog disabled</color>");
        
        // 3. Check for Directional Light
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light directionalLight = null;
        
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                directionalLight = light;
                break;
            }
        }
        
        // 4. Create Directional Light if missing
        if (directionalLight == null)
        {
            GameObject lightGO = new GameObject("Directional Light");
            directionalLight = lightGO.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("<color=green>✅ Created Directional Light</color>");
        }
        
        // 5. Configure Directional Light
        directionalLight.intensity = 1.0f;
        directionalLight.color = Color.white;
        directionalLight.shadows = LightShadows.Soft;
        directionalLight.shadowStrength = 1.0f;
        Debug.Log("<color=green>✅ Directional Light configured</color>");
        
        // 6. Fix Player Flashlight if exists
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Spot && light.gameObject.name.ToLower().Contains("flash"))
            {
                light.intensity = 2.0f; // Higher for better visibility
                light.range = 50f;
                light.spotAngle = 50f;
                light.shadows = LightShadows.Soft;
                Debug.Log($"<color=green>✅ Fixed flashlight: {light.name}</color>");
            }
        }
        
        Debug.Log("<color=yellow>⚡⚡⚡ ALL LIGHTING FIXED! Scene should be visible now! ⚡⚡⚡</color>");
    }
    
    [ContextMenu("Make Scene BRIGHT (For Testing)")]
    public void MakeBright()
    {
        FixEverything();
        
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                light.intensity = 2.0f; // Extra bright
            }
        }
        
        RenderSettings.ambientIntensity = 1.5f; // Brighter ambient
        Debug.Log("<color=cyan>☀️ Scene set to BRIGHT mode for testing!</color>");
    }
}
