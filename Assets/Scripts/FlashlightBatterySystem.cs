using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Battery system for flashlight - drains when on, adds tension to gameplay
/// </summary>
public class FlashlightBatterySystem : MonoBehaviour
{
    [Header("Battery Settings")]
    [Tooltip("Maximum battery life in seconds")]
    public float maxBatteryLife = 300f; // 5 minutes
    
    [Tooltip("Current battery charge (0-100)")]
    [Range(0f, 100f)]
    public float currentBattery = 100f;
    
    [Tooltip("Battery drain rate per second when flashlight is on")]
    public float drainRate = 5f; // 5% per second = 20 seconds of use
    
    [Tooltip("Flicker when battery is below this percentage")]
    public float lowBatteryThreshold = 25f;
    
    [Tooltip("Critical battery threshold")]
    public float criticalBatteryThreshold = 10f;
    
    [Header("References")]
    public SimpleFlashlightController flashlightController;
    public Light flashlight;
    
    [Header("Low Battery Effects")]
    public bool enableLowBatteryFlicker = true;
    public float lowBatteryFlickerSpeed = 5f;
    public float criticalFlickerSpeed = 15f;
    
    [Header("Audio")]
    public AudioClip lowBatteryWarning;
    public AudioClip criticalBatteryWarning;
    public AudioClip batteryDepleted;
    
    [Range(0f, 1f)]
    public float warningVolume = 0.7f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private float originalIntensity;
    private bool hasPlayedLowWarning = false;
    private bool hasPlayedCriticalWarning = false;
    private AudioSource audioSource;
    private bool isDead = false;
    
    void Start()
    {
        // Find flashlight controller if not assigned
        if (flashlightController == null)
        {
            flashlightController = GetComponent<SimpleFlashlightController>();
        }
        
        // Find flashlight light if not assigned
        if (flashlight == null && flashlightController != null)
        {
            flashlight = flashlightController.flashlight;
        }
        
        if (flashlight != null)
        {
            originalIntensity = flashlight.intensity;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("<color=cyan>🔋 Battery System initialized</color>");
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Drain battery when flashlight is on
        if (flashlightController != null && flashlightController.IsOn() && currentBattery > 0)
        {
            float drainAmount = (drainRate / maxBatteryLife) * 100f * Time.deltaTime;
            currentBattery -= drainAmount;
            currentBattery = Mathf.Max(0f, currentBattery);
            
            // Check battery levels
            CheckBatteryLevel();
        }
        
        // Apply low battery effects
        if (currentBattery > 0 && currentBattery < lowBatteryThreshold)
        {
            ApplyLowBatteryEffects();
        }
        
        // Battery depleted
        if (currentBattery <= 0 && !isDead)
        {
            BatteryDepleted();
        }
        
        // Debug: Press B to add 25% battery
        if (showDebugInfo && Keyboard.current != null && Keyboard.current[Key.B].wasPressedThisFrame)
        {
            AddBattery(25f);
        }
    }
    
    void CheckBatteryLevel()
    {
        // Low battery warning
        if (currentBattery <= lowBatteryThreshold && currentBattery > criticalBatteryThreshold && !hasPlayedLowWarning)
        {
            PlayWarningSound(lowBatteryWarning);
            hasPlayedLowWarning = true;
            
            if (showDebugInfo)
            {
                Debug.LogWarning("⚠️ Low Battery!");
            }
        }
        
        // Critical battery warning
        if (currentBattery <= criticalBatteryThreshold && !hasPlayedCriticalWarning)
        {
            PlayWarningSound(criticalBatteryWarning);
            hasPlayedCriticalWarning = true;
            
            if (showDebugInfo)
            {
                Debug.LogWarning("🚨 CRITICAL BATTERY!");
            }
        }
    }
    
    void ApplyLowBatteryEffects()
    {
        if (!enableLowBatteryFlicker || flashlight == null) return;
        
        float flickerSpeed = currentBattery < criticalBatteryThreshold ? criticalFlickerSpeed : lowBatteryFlickerSpeed;
        
        // Random flicker based on battery level
        if (Random.value < (1f - (currentBattery / 100f)) * Time.deltaTime * flickerSpeed)
        {
            flashlight.intensity = originalIntensity * Random.Range(0.3f, 1f);
        }
        else
        {
            flashlight.intensity = originalIntensity;
        }
    }
    
    void BatteryDepleted()
    {
        isDead = true;
        
        if (flashlightController != null)
        {
            flashlightController.TurnOff();
        }
        
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
        
        PlayWarningSound(batteryDepleted);
        
        if (showDebugInfo)
        {
            Debug.LogError("💀 BATTERY DEPLETED - Flashlight disabled!");
        }
    }
    
    /// <summary>
    /// Add battery charge (for pickups)
    /// </summary>
    public void AddBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Min(100f, currentBattery);
        
        // Reset warnings
        if (currentBattery > lowBatteryThreshold)
        {
            hasPlayedLowWarning = false;
            hasPlayedCriticalWarning = false;
            isDead = false;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>🔋 Battery recharged +{amount}% (Total: {currentBattery:F1}%)</color>");
        }
    }
    
    /// <summary>
    /// Get current battery percentage
    /// </summary>
    public float GetBatteryPercentage()
    {
        return currentBattery;
    }
    
    /// <summary>
    /// Check if battery is depleted
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    void PlayWarningSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, warningVolume);
        }
    }
}
