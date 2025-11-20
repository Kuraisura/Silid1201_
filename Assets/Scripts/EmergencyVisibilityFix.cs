using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// EMERGENCY FIX - Makes scene visible immediately
/// </summary>
[ExecuteAlways]
public class EmergencyVisibilityFix : MonoBehaviour
{
    void Start()
    {
        MakeSceneVisible();
    }
    
    void OnEnable()
    {
        MakeSceneVisible();
    }
    
    [ContextMenu("MAKE SCENE VISIBLE NOW")]
    public void MakeSceneVisible()
    {
        Debug.Log("<color=red>🚨 EMERGENCY FIX - MAKING SCENE VISIBLE!</color>");
        
        // 1. Set BRIGHT ambient lighting
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.4f, 1f); // Bright gray
        RenderSettings.ambientIntensity = 1.5f;
        Debug.Log("<color=green>✅ Ambient light: BRIGHT</color>");
        
        // 2. Remove skybox (might be black)
        RenderSettings.skybox = null;
        
        // 3. Disable fog
        RenderSettings.fog = false;
        
        // 4. Find or create directional light
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light dirLight = null;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                dirLight = light;
                break;
            }
        }
        
        if (dirLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("<color=green>✅ Created Directional Light</color>");
        }
        
        // 5. Make light VERY BRIGHT
        dirLight.intensity = 2.0f; // Extra bright
        dirLight.color = Color.white;
        dirLight.shadows = LightShadows.Soft;
        dirLight.enabled = true;
        Debug.Log($"<color=green>✅ Directional Light intensity: {dirLight.intensity}</color>");
        
        // 6. Fix camera settings
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.backgroundColor = Color.gray; // Fallback color
            Debug.Log("<color=green>✅ Camera settings fixed</color>");
        }
        
        // 7. Boost any flashlights
        foreach (Light light in lights)
        {
            if (light.type == LightType.Spot)
            {
                light.intensity = 10f; // Very bright
                light.range = 100f; // Long range
                light.enabled = true;
                Debug.Log($"<color=green>✅ Boosted spotlight: {light.name}</color>");
            }
        }
        
        Debug.Log("<color=cyan>================================================</color>");
        Debug.Log("<color=yellow>⚡ SCENE SHOULD NOW BE VISIBLE! ⚡</color>");
        Debug.Log("<color=yellow>If still dark, check:</color>");
        Debug.Log("<color=yellow>1. Your camera is not inside a solid object</color>");
        Debug.Log("<color=yellow>2. Press PLAY button to see the scene</color>");
        Debug.Log("<color=yellow>3. Click on Scene tab (not Game tab)</color>");
        Debug.Log("<color=cyan>================================================</color>");
    }
}
