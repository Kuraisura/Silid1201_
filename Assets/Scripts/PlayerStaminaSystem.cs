using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stamina system for player - drains when running, regenerates when walking/standing
/// </summary>
public class PlayerStaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina")]
    public float maxStamina = 100f;
    
    [Tooltip("Current stamina")]
    [Range(0f, 100f)]
    public float currentStamina = 100f;
    
    [Tooltip("Stamina drain rate per second when sprinting")]
    public float drainRate = 15f;
    
    [Tooltip("Stamina regeneration rate per second")]
    public float regenRate = 10f;
    
    [Tooltip("Delay before stamina starts regenerating after sprinting")]
    public float regenDelay = 1f;
    
    [Tooltip("Minimum stamina required to start sprinting")]
    public float minStaminaToSprint = 10f;
    
    [Header("References")]
    public BetterPlayerMovement playerMovement;
    
    [Header("Breathing Audio")]
    public AudioClip heavyBreathing;
    public AudioClip exhaustedBreathing;
    
    [Range(0f, 1f)]
    public float breathingVolume = 0.5f;
    
    [Header("Effects")]
    public bool enableLowStaminaEffects = true;
    public float lowStaminaThreshold = 25f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool isSprinting = false;
    private float timeSinceLastSprint = 0f;
    private AudioSource audioSource;
    private bool isExhausted = false;
    private float originalWalkSpeed;
    private float originalRunSpeed;
    
    void Start()
    {
        // Find player movement if not assigned
        if (playerMovement == null)
        {
            playerMovement = GetComponent<BetterPlayerMovement>();
        }
        
        if (playerMovement != null)
        {
            originalWalkSpeed = playerMovement.walkSpeed;
            originalRunSpeed = playerMovement.runSpeed;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.loop = true;
        }
        
        currentStamina = maxStamina;
        
        if (showDebugInfo)
        {
            Debug.Log("<color=green>💪 Stamina System initialized</color>");
        }
    }
    
    void Update()
    {
        // Check if player is sprinting
        bool wantToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        
        // Determine if we can sprint
        bool canSprint = currentStamina >= minStaminaToSprint && !isExhausted;
        isSprinting = wantToSprint && canSprint;
        
        // Drain stamina when sprinting
        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= drainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            timeSinceLastSprint = 0f;
            
            // Check if exhausted
            if (currentStamina <= 0)
            {
                OnExhausted();
            }
        }
        else
        {
            timeSinceLastSprint += Time.deltaTime;
            
            // Regenerate stamina after delay
            if (timeSinceLastSprint >= regenDelay && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                
                // Recover from exhaustion
                if (isExhausted && currentStamina >= minStaminaToSprint)
                {
                    isExhausted = false;
                    if (showDebugInfo)
                    {
                        Debug.Log("<color=green>💪 Recovered from exhaustion</color>");
                    }
                }
            }
        }
        
        // Apply low stamina effects
        if (enableLowStaminaEffects)
        {
            ApplyStaminaEffects();
        }
        
        // Prevent sprinting if no stamina
        if (playerMovement != null && currentStamina <= 0)
        {
            // Force walk speed
            playerMovement.runSpeed = playerMovement.walkSpeed;
        }
        else if (playerMovement != null && !isExhausted)
        {
            // Restore run speed
            playerMovement.runSpeed = originalRunSpeed;
        }
        
        // Debug: Press N to deplete stamina, M to restore
        if (showDebugInfo && Keyboard.current != null)
        {
            if (Keyboard.current[Key.N].wasPressedThisFrame)
            {
                currentStamina = 0;
                Debug.Log("⚠️ Stamina depleted!");
            }
            if (Keyboard.current[Key.M].wasPressedThisFrame)
            {
                currentStamina = maxStamina;
                isExhausted = false;
                Debug.Log("💪 Stamina restored!");
            }
        }
    }
    
    void ApplyStaminaEffects()
    {
        // Heavy breathing when low on stamina
        if (currentStamina < lowStaminaThreshold && currentStamina > 0)
        {
            if (!audioSource.isPlaying && heavyBreathing != null)
            {
                audioSource.clip = heavyBreathing;
                audioSource.volume = breathingVolume;
                audioSource.Play();
            }
        }
        // Exhausted breathing
        else if (currentStamina <= 0 && isExhausted)
        {
            if (exhaustedBreathing != null && audioSource.clip != exhaustedBreathing)
            {
                audioSource.clip = exhaustedBreathing;
                audioSource.volume = breathingVolume * 1.5f;
                audioSource.Play();
            }
        }
        // Stop breathing sound when recovered
        else if (currentStamina > lowStaminaThreshold && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    void OnExhausted()
    {
        isExhausted = true;
        
        if (showDebugInfo)
        {
            Debug.LogWarning("😰 EXHAUSTED! Cannot sprint!");
        }
    }
    
    /// <summary>
    /// Get current stamina percentage
    /// </summary>
    public float GetStaminaPercentage()
    {
        return (currentStamina / maxStamina) * 100f;
    }
    
    /// <summary>
    /// Check if player is currently sprinting
    /// </summary>
    public bool IsSprinting()
    {
        return isSprinting;
    }
    
    /// <summary>
    /// Check if player is exhausted
    /// </summary>
    public bool IsExhausted()
    {
        return isExhausted;
    }
    
    /// <summary>
    /// Can player sprint right now?
    /// </summary>
    public bool CanSprint()
    {
        return currentStamina >= minStaminaToSprint && !isExhausted;
    }
}
