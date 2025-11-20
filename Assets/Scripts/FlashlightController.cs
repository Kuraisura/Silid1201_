using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Professional Flashlight Controller with realistic lighting and toggle functionality
/// Press F to toggle flashlight on/off
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("The Light component for the flashlight")]
    public Light flashlight;
    
    [Tooltip("Starting state of the flashlight")]
    public bool startOn = true;
    
    [Header("Light Properties")]
    [Tooltip("Intensity when flashlight is on")]
    [Range(0f, 10f)]
    public float intensity = 2.5f;
    
    [Tooltip("Range of the flashlight beam")]
    [Range(1f, 100f)]
    public float range = 20f;
    
    [Tooltip("Spot angle of the flashlight")]
    [Range(10f, 120f)]
    public float spotAngle = 50f;
    
    [Tooltip("Inner spot angle for smooth falloff")]
    [Range(0f, 90f)]
    public float innerSpotAngle = 25f;
    
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
    
    [Header("Battery Effect (Optional)")]
    [Tooltip("Enable battery drain simulation")]
    public bool simulateBattery = false;
    
    [Tooltip("Battery life in seconds (0 = infinite)")]
    public float batteryLife = 300f;
    
    [Header("Audio")]
    [Tooltip("Sound effect for toggling flashlight")]
    public AudioClip toggleSound;
    
    [Tooltip("Volume of toggle sound")]
    [Range(0f, 1f)]
    public float toggleVolume = 0.5f;
    
    [Header("Cookie/Pattern (Optional)")]
    [Tooltip("Light cookie texture for realistic flashlight pattern")]
    public Texture lightCookie;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool isOn;
    private float targetIntensity;
    private float currentIntensity;
    private float batteryRemaining;
    private AudioSource audioSource;
    private float baseIntensity;
    
    void Start()
    {
        // Setup flashlight
        SetupFlashlight();
        
        // Initialize battery
        batteryRemaining = batteryLife;
        baseIntensity = intensity;
        
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
            Debug.Log($"<color=yellow>🔦 Flashlight initialized - State: {(isOn ? "ON" : "OFF")}</color>");
        }
    }
    
    void SetupFlashlight()
    {
        // If no flashlight assigned, try to find or create one
        if (flashlight == null)
        {
            // Look for existing light
            flashlight = GetComponentInChildren<Light>();
            
            // Create new light if none exists
            if (flashlight == null)
            {
                GameObject lightObj = new GameObject("Flashlight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = new Vector3(0.2f, -0.1f, 0.3f); // Slightly offset
                lightObj.transform.localRotation = Quaternion.identity;
                
                flashlight = lightObj.AddComponent<Light>();
                
                if (showDebugInfo)
                {
                    Debug.Log("<color=green>✓ Created new flashlight GameObject</color>");
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
            flashlight.color = Color.white;
            flashlight.shadows = LightShadows.Soft; // Enable soft shadows for realism
            flashlight.shadowStrength = 0.8f;
            flashlight.renderMode = LightRenderMode.ForcePixel; // Better quality
            
            // Apply cookie if provided
            if (lightCookie != null)
            {
                flashlight.cookie = lightCookie;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=green>✓ Flashlight configured - Range: {range}, Angle: {spotAngle}°</color>");
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
        
        // Battery drain simulation
        if (simulateBattery && isOn && batteryLife > 0)
        {
            batteryRemaining -= Time.deltaTime;
            
            if (batteryRemaining <= 0)
            {
                batteryRemaining = 0;
                if (isOn)
                {
                    ToggleFlashlight();
                    if (showDebugInfo)
                    {
                        Debug.Log("<color=red>🔋 Battery depleted!</color>");
                    }
                }
            }
            else if (batteryRemaining < 30f)
            {
                // Dim light when battery is low
                float batteryPercent = batteryRemaining / 30f;
                targetIntensity = intensity * batteryPercent;
            }
        }
    }
    
    /// <summary>
    /// Toggle flashlight on/off
    /// </summary>
    public void ToggleFlashlight()
    {
        // Check battery
        if (simulateBattery && batteryRemaining <= 0)
        {
            if (showDebugInfo)
            {
                Debug.Log("<color=red>Cannot turn on flashlight - Battery depleted!</color>");
            }
            return;
        }
        
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
            if (simulateBattery)
            {
                Debug.Log($"<color=cyan>🔋 Battery: {(batteryRemaining / batteryLife * 100f):F1}%</color>");
            }
        }
    }
    
    /// <summary>
    /// Input System callback for Flashlight toggle (F key)
    /// </summary>
    public void OnFlashlight(InputValue value)
    {
        if (value.isPressed)
        {
            ToggleFlashlight();
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
    
    /// <summary>
    /// Get battery percentage (0-100)
    /// </summary>
    public float GetBatteryPercent()
    {
        if (!simulateBattery || batteryLife <= 0)
        {
            return 100f;
        }
        return (batteryRemaining / batteryLife) * 100f;
    }
    
    /// <summary>
    /// Recharge battery to full
    /// </summary>
    public void RechargeBattery()
    {
        batteryRemaining = batteryLife;
        if (showDebugInfo)
        {
            Debug.Log("<color=green>🔋 Battery recharged to 100%</color>");
        }
    }
    
    // Debug GUI
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 14;
            
            string info = $"FLASHLIGHT DEBUG\n" +
                         $"Status: {(isOn ? "ON" : "OFF")}\n" +
                         $"Intensity: {currentIntensity:F2}\n" +
                         $"Range: {range}m\n" +
                         $"Spot Angle: {spotAngle}°";
            
            if (simulateBattery)
            {
                info += $"\nBattery: {GetBatteryPercent():F1}%";
            }
            
            GUI.Box(new Rect(10, 10, 200, simulateBattery ? 140 : 120), info, style);
        }
    }
}
