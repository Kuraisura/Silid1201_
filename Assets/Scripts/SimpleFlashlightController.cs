using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple Flashlight Controller compatible with both Input Systems
/// Press F to toggle flashlight on/off
/// </summary>
public class SimpleFlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("The Light component for the flashlight")]
    public Light flashlight;
    
    [Tooltip("Camera transform (auto-detected if not assigned)")]
    public Transform cameraTransform;
    
    [Tooltip("Starting state of the flashlight")]
    public bool startOn = true;
    
    [Header("Light Properties")]
    [Tooltip("Intensity when flashlight is on (URP optimized: 10-20 is bright)")]
    [Range(0f, 50f)]
    public float intensity = 15f;
    
    [Tooltip("Range of the flashlight beam (realistic: 30-50m)")]
    [Range(10f, 100f)]
    public float range = 40f;
    
    [Tooltip("Spot angle of the flashlight")]
    [Range(10f, 120f)]
    public float spotAngle = 50f;
    
    [Tooltip("Inner spot angle for smooth falloff")]
    [Range(0f, 90f)]
    public float innerSpotAngle = 25f;
    
    [Header("Light Color")]
    [Tooltip("Color of the flashlight (default: bright white)")]
    public Color lightColor = new Color(1f, 1f, 0.95f, 1f);
    
    [Header("Toggle Key")]
    [Tooltip("Key to toggle flashlight (F key is hardcoded for New Input System)")]
    public string toggleKeyInfo = "Press F to toggle";
    
    [Header("Visual Effects")]
    [Tooltip("Enable smooth fade on/off transitions")]
    public bool smoothTransition = true;
    
    [Tooltip("Transition speed")]
    [Range(1f, 20f)]
    public float transitionSpeed = 10f;
    
    [Tooltip("Enable flashlight flickering effect")]
    public bool enableFlicker = true;
    
    [Tooltip("Flicker intensity variation")]
    [Range(0f, 0.5f)]
    public float flickerAmount = 0.05f;
    
    [Tooltip("Flicker speed")]
    [Range(1f, 20f)]
    public float flickerSpeed = 10f;
    
    [Header("Audio")]
    [Tooltip("Sound effect for toggling flashlight")]
    public AudioClip toggleSound;
    
    [Tooltip("Volume of toggle sound")]
    [Range(0f, 1f)]
    public float toggleVolume = 0.5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool isOn;
    private float targetIntensity;
    private float currentIntensity;
    private AudioSource audioSource;
    
    void Start()
    {
        // Setup flashlight
        SetupFlashlight();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && toggleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Set initial state
        isOn = startOn;
        targetIntensity = isOn ? intensity : 0f;
        currentIntensity = targetIntensity;
        
        if (flashlight != null)
        {
            flashlight.intensity = currentIntensity;
            flashlight.enabled = isOn;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>🔦 SimpleFlashlight initialized - Press F to toggle</color>");
        }
    }
    
    void SetupFlashlight()
    {
        // Find camera if not assigned
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
            else
            {
                // Try to find main camera
                cam = Camera.main;
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
                else
                {
                    Debug.LogError("❌ No camera found! Flashlight needs a camera to attach to.");
                    return;
                }
            }
        }
        
        // If no flashlight assigned, try to find or create one
        if (flashlight == null)
        {
            // Look for existing light in camera children
            flashlight = cameraTransform.GetComponentInChildren<Light>();
            
            // Create new light if none exists
            if (flashlight == null)
            {
                GameObject lightObj = new GameObject("Flashlight");
                lightObj.transform.SetParent(cameraTransform); // ATTACH TO CAMERA!
                lightObj.transform.localPosition = Vector3.zero; // Exact camera position
                lightObj.transform.localRotation = Quaternion.identity; // Points forward with camera
                
                flashlight = lightObj.AddComponent<Light>();
                
                if (showDebugInfo)
                {
                    Debug.Log("<color=green>✓ Created new flashlight on Camera</color>");
                    Debug.Log($"<color=cyan>Flashlight Position: {lightObj.transform.position}</color>");
                    Debug.Log($"<color=cyan>Camera Position: {cameraTransform.position}</color>");
                    Debug.Log($"<color=cyan>Flashlight Forward: {lightObj.transform.forward}</color>");
                }
            }
        }
        
        // Configure flashlight properties
        if (flashlight != null)
        {
            flashlight.type = LightType.Spot;
            flashlight.range = range;
            flashlight.spotAngle = spotAngle;
            flashlight.innerSpotAngle = innerSpotAngle;
            flashlight.intensity = intensity;
            flashlight.color = lightColor;
            flashlight.shadows = LightShadows.Soft; // Enable soft shadows for URP
            flashlight.renderMode = LightRenderMode.ForcePixel;
            flashlight.cullingMask = -1; // Render all layers
            flashlight.bounceIntensity = 2f; // Reasonable indirect lighting
            
            // IMPORTANT: Make sure light is actually rendering
            flashlight.lightmapBakeType = LightmapBakeType.Realtime;
            flashlight.useColorTemperature = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=green>✓ Flashlight configured - Range: {range}, Angle: {spotAngle}°, Intensity: {intensity}</color>");
                Debug.Log($"<color=cyan>📹 Attached to: {cameraTransform.name}</color>");
            }
        }
        else
        {
            Debug.LogError("❌ Flashlight could not be created or found!");
        }
    }
    
    void Update()
    {
        if (flashlight == null) return;
        
        // Check for toggle key press using NEW Input System (Keyboard.current)
        if (Keyboard.current != null && Keyboard.current[Key.F].wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
        
        // Handle smooth transitions
        if (smoothTransition)
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
            flashlight.intensity = currentIntensity;
            
            // Enable/disable light based on intensity
            if (currentIntensity < 0.01f && flashlight.enabled)
            {
                flashlight.enabled = false;
            }
            else if (currentIntensity > 0.01f && !flashlight.enabled)
            {
                flashlight.enabled = true;
            }
        }
        
        // Add flickering effect when on
        if (isOn && enableFlicker && currentIntensity > 0.1f)
        {
            float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * flickerAmount;
            flashlight.intensity = currentIntensity + flicker;
        }
    }
    
    /// <summary>
    /// Toggle flashlight on/off
    /// </summary>
    public void ToggleFlashlight()
    {
        isOn = !isOn;
        targetIntensity = isOn ? intensity : 0f;
        
        if (!smoothTransition)
        {
            currentIntensity = targetIntensity;
            flashlight.intensity = currentIntensity;
            flashlight.enabled = isOn;
        }
        
        // Play toggle sound
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound, toggleVolume);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>🔦 Flashlight: {(isOn ? "ON" : "OFF")}</color>");
        }
    }
    
    /// <summary>
    /// Turn flashlight on
    /// </summary>
    public void TurnOn()
    {
        if (!isOn)
        {
            ToggleFlashlight();
        }
    }
    
    /// <summary>
    /// Turn flashlight off
    /// </summary>
    public void TurnOff()
    {
        if (isOn)
        {
            ToggleFlashlight();
        }
    }
    
    /// <summary>
    /// Check if flashlight is currently on
    /// </summary>
    public bool IsOn()
    {
        return isOn;
    }
    
    // Debug GUI
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            
            string info = $"FLASHLIGHT DEBUG\n" +
                         $"Status: {(isOn ? "ON" : "OFF")}\n" +
                         $"Intensity: {currentIntensity:F2}\n" +
                         $"Range: {range}m\n" +
                         $"Spot Angle: {spotAngle}°\n" +
                         $"Light Enabled: {(flashlight != null ? flashlight.enabled.ToString() : "NULL")}\n" +
                         $"Press F to toggle!";
            
            GUI.Box(new Rect(10, 10, 250, 150), info, style);
            
            // Draw a visual indicator
            if (isOn && flashlight != null)
            {
                GUI.color = Color.yellow;
                GUI.Box(new Rect(270, 10, 50, 50), "LIGHT\nON", style);
                GUI.color = Color.white;
            }
        }
    }
    
    // Draw debug rays in Scene view
    void OnDrawGizmos()
    {
        if (flashlight != null && flashlight.enabled)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(flashlight.transform.position, 0.2f);
            Gizmos.DrawRay(flashlight.transform.position, flashlight.transform.forward * range);
        }
    }
}
