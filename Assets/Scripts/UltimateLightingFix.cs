using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ULTIMATE FIX - Fixes absolutely everything for visibility
/// </summary>
[ExecuteAlways]
public class UltimateLightingFix : MonoBehaviour
{
    [Header("Run in Edit Mode")]
    [Tooltip("Check this to apply fixes immediately")]
    public bool applyFixNow = true;
    
    void OnEnable()
    {
        if (applyFixNow)
        {
            ApplyCompleteFix();
        }
    }
    
    void Update()
    {
        if (!Application.isPlaying && applyFixNow)
        {
            ApplyCompleteFix();
            applyFixNow = false; // Only run once
        }
    }
    
    [ContextMenu("APPLY COMPLETE FIX")]
    public void ApplyCompleteFix()
    {
        Debug.Log("<color=red>========================================</color>");
        Debug.Log("<color=red>🔥 ULTIMATE LIGHTING FIX STARTED 🔥</color>");
        Debug.Log("<color=red>========================================</color>");
        
        // 1. FIX AMBIENT LIGHT - Make it BRIGHT
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f, 1f); // Bright gray
        RenderSettings.ambientIntensity = 2.0f; // Double intensity
        RenderSettings.skybox = null; // Remove potentially black skybox
        RenderSettings.fog = false;
        Debug.Log("<color=green>✅ Ambient: RGB(128,128,128) @ 2.0x intensity</color>");
        
        // 2. FIX DIRECTIONAL LIGHT
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light dirLight = null;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                dirLight = light;
                dirLight.intensity = 3.0f; // VERY BRIGHT for URP
                dirLight.color = Color.white;
                dirLight.shadows = LightShadows.Soft;
                dirLight.enabled = true;
                Debug.Log($"<color=green>✅ Directional Light: {light.name} @ 3.0 intensity</color>");
                break;
            }
        }
        
        if (dirLight == null)
        {
            GameObject go = new GameObject("Directional Light");
            dirLight = go.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 3.0f;
            dirLight.color = Color.white;
            dirLight.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50, -30, 0);
            Debug.Log("<color=green>✅ Created NEW Directional Light @ 3.0 intensity</color>");
        }
        
        // 3. FIX CAMERA
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.5f, 0.5f, 0.5f); // Gray background
            
            // Fix camera's URP data if it exists
            UniversalAdditionalCameraData camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
                Debug.Log("<color=green>✅ Camera URP data configured</color>");
            }
            
            Debug.Log("<color=green>✅ Main Camera configured</color>");
        }
        else
        {
            Debug.LogError("<color=red>❌ NO MAIN CAMERA FOUND! Tag a camera as 'MainCamera'</color>");
        }
        
        // 4. FIX/ADD EXPOSURE IN GLOBAL VOLUME
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Add or fix ColorAdjustments for exposure
            ColorAdjustments colorAdj;
            if (!volume.profile.TryGet(out colorAdj))
            {
                colorAdj = volume.profile.Add<ColorAdjustments>();
            }
            
            colorAdj.active = true;
            colorAdj.postExposure.overrideState = true;
            colorAdj.postExposure.value = 2.0f; // BRIGHT exposure
            colorAdj.contrast.overrideState = true;
            colorAdj.contrast.value = 0f;
            colorAdj.colorFilter.overrideState = true;
            colorAdj.colorFilter.value = Color.white;
            colorAdj.saturation.overrideState = true;
            colorAdj.saturation.value = 0f;
            
            Debug.Log("<color=green>✅ Global Volume: Exposure +2.0 (BRIGHT)</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>⚠️ No Global Volume found (this is OK)</color>");
        }
        
        // 5. BOOST ALL SPOT LIGHTS (flashlights)
        foreach (Light light in lights)
        {
            if (light.type == LightType.Spot)
            {
                light.intensity = 15f; // Very bright
                light.range = 100f;
                light.enabled = true;
                Debug.Log($"<color=green>✅ Spot Light boosted: {light.name}</color>");
            }
        }
        
        Debug.Log("<color=red>========================================</color>");
        Debug.Log("<color=yellow>⚡⚡⚡ ALL FIXES APPLIED! ⚡⚡⚡</color>");
        Debug.Log("<color=cyan>Scene should now be VERY BRIGHT!</color>");
        Debug.Log("<color=cyan>If still dark, check:</color>");
        Debug.Log("<color=cyan>  1. Press PLAY button ▶️</color>");
        Debug.Log("<color=cyan>  2. Camera not inside solid object</color>");
        Debug.Log("<color=cyan>  3. Objects have visible materials</color>");
        Debug.Log("<color=red>========================================</color>");
    }
}
