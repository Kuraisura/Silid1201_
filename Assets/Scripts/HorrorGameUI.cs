using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Slenderman-style Horror Game UI - Minimalist, atmospheric HUD
/// Shows battery, stamina, pages collected, and other vital info
/// </summary>
public class HorrorGameUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main Canvas - will be created if not assigned")]
    public Canvas mainCanvas;
    
    [Header("Battery UI")]
    public Image batteryFillBar;
    public Image batteryIcon;
    public TextMeshProUGUI batteryText;
    public Color batteryFullColor = Color.green;
    public Color batteryLowColor = Color.yellow;
    public Color batteryCriticalColor = Color.red;
    
    [Header("Stamina UI")]
    public Image staminaFillBar;
    public Image staminaIcon;
    public TextMeshProUGUI staminaText;
    public Color staminaFullColor = Color.cyan;
    public Color staminaLowColor = Color.yellow;
    public Color staminaEmptyColor = Color.red;
    
    [Header("Objectives UI")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI pagesCollectedText;
    
    [Header("Interaction UI")]
    public TextMeshProUGUI interactionPrompt;
    public string defaultInteractionText = "[E] Interact";
    
    [Header("Warning Messages")]
    public TextMeshProUGUI warningText;
    public float warningDisplayTime = 3f;
    
    [Header("Flashlight Indicator")]
    public Image flashlightIndicator;
    public Color flashlightOnColor = Color.yellow;
    public Color flashlightOffColor = Color.gray;
    
    [Header("Crosshair")]
    public Image crosshair;
    public bool showCrosshair = false;
    
    [Header("Vignette Effect")]
    public Image damageVignette;
    public float vignetteFlashSpeed = 2f;
    
    [Header("Game Systems")]
    public FlashlightBatterySystem batterySystem;
    public PlayerStaminaSystem staminaSystem;
    public SimpleFlashlightController flashlightController;
    
    [Header("UI Settings")]
    public bool fadeUIWhenNotNeeded = true;
    public float uiFadeSpeed = 2f;
    public float minUIAlpha = 0.3f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private int pagesCollected = 0;
    private int totalPages = 8; // Like Slenderman
    private float warningTimer = 0f;
    private float batteryAlpha = 1f;
    private float staminaAlpha = 1f;
    private CanvasGroup batteryGroup;
    private CanvasGroup staminaGroup;
    
    void Start()
    {
        SetupUI();
        FindGameSystems();
        
        // Hide interaction prompt by default
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
        
        // Hide warning text
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
        
        // Setup crosshair
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(showCrosshair);
        }
        
        // Setup damage vignette
        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0f;
            damageVignette.color = c;
        }
        
        UpdateObjectiveText();
        
        if (showDebugInfo)
        {
            Debug.Log("<color=cyan>🎮 Horror Game UI initialized!</color>");
        }
    }
    
    void SetupUI()
    {
        // Setup canvas groups for fading
        if (batteryFillBar != null)
        {
            batteryGroup = batteryFillBar.transform.parent.GetComponent<CanvasGroup>();
            if (batteryGroup == null)
            {
                batteryGroup = batteryFillBar.transform.parent.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (staminaFillBar != null)
        {
            staminaGroup = staminaFillBar.transform.parent.GetComponent<CanvasGroup>();
            if (staminaGroup == null)
            {
                staminaGroup = staminaFillBar.transform.parent.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    void FindGameSystems()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindFirstObjectByType<BetterPlayerMovement>()?.gameObject;
        }
        
        if (player != null)
        {
            if (batterySystem == null)
            {
                batterySystem = player.GetComponent<FlashlightBatterySystem>();
            }
            
            if (staminaSystem == null)
            {
                staminaSystem = player.GetComponent<PlayerStaminaSystem>();
            }
            
            if (flashlightController == null)
            {
                flashlightController = player.GetComponent<SimpleFlashlightController>();
            }
        }
    }
    
    void Update()
    {
        UpdateBatteryUI();
        UpdateStaminaUI();
        UpdateFlashlightIndicator();
        UpdateWarningText();
        
        // Update UI fading
        if (fadeUIWhenNotNeeded)
        {
            UpdateUIFading();
        }
    }
    
    void UpdateBatteryUI()
    {
        if (batterySystem == null) return;
        
        float batteryPercent = batterySystem.GetBatteryPercentage();
        
        // Update fill bar
        if (batteryFillBar != null)
        {
            batteryFillBar.fillAmount = batteryPercent / 100f;
            
            // Color based on battery level
            if (batteryPercent > 50f)
            {
                batteryFillBar.color = batteryFullColor;
            }
            else if (batteryPercent > 25f)
            {
                batteryFillBar.color = batteryLowColor;
            }
            else
            {
                batteryFillBar.color = batteryCriticalColor;
                
                // Pulse effect when critical
                float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                Color c = batteryFillBar.color;
                c.a = 0.5f + (pulse * 0.5f);
                batteryFillBar.color = c;
            }
        }
        
        // Update text
        if (batteryText != null)
        {
            batteryText.text = $"{batteryPercent:F0}%";
        }
    }
    
    void UpdateStaminaUI()
    {
        if (staminaSystem == null) return;
        
        float staminaPercent = staminaSystem.GetStaminaPercentage();
        
        // Update fill bar
        if (staminaFillBar != null)
        {
            staminaFillBar.fillAmount = staminaPercent / 100f;
            
            // Color based on stamina level
            if (staminaPercent > 50f)
            {
                staminaFillBar.color = staminaFullColor;
            }
            else if (staminaPercent > 25f)
            {
                staminaFillBar.color = staminaLowColor;
            }
            else
            {
                staminaFillBar.color = staminaEmptyColor;
            }
        }
        
        // Update text
        if (staminaText != null)
        {
            staminaText.text = $"{staminaPercent:F0}%";
        }
    }
    
    void UpdateFlashlightIndicator()
    {
        if (flashlightController == null || flashlightIndicator == null) return;
        
        bool isOn = flashlightController.IsOn();
        flashlightIndicator.color = isOn ? flashlightOnColor : flashlightOffColor;
        
        // Pulsing effect when on
        if (isOn)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
            Color c = flashlightOnColor;
            c.a = 0.7f + pulse;
            flashlightIndicator.color = c;
        }
    }
    
    void UpdateWarningText()
    {
        if (warningText == null) return;
        
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            
            // Fade out effect
            float alpha = Mathf.Clamp01(warningTimer / 0.5f);
            Color c = warningText.color;
            c.a = alpha;
            warningText.color = c;
            
            if (warningTimer <= 0f)
            {
                warningText.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdateUIFading()
    {
        // Fade battery UI when full
        if (batteryGroup != null && batterySystem != null)
        {
            float targetAlpha = batterySystem.GetBatteryPercentage() < 100f ? 1f : minUIAlpha;
            batteryAlpha = Mathf.Lerp(batteryAlpha, targetAlpha, uiFadeSpeed * Time.deltaTime);
            batteryGroup.alpha = batteryAlpha;
        }
        
        // Show stamina UI when depleting
        if (staminaGroup != null && staminaSystem != null)
        {
            float targetAlpha = staminaSystem.IsSprinting() || staminaSystem.GetStaminaPercentage() < 100f ? 1f : minUIAlpha;
            staminaAlpha = Mathf.Lerp(staminaAlpha, targetAlpha, uiFadeSpeed * Time.deltaTime);
            staminaGroup.alpha = staminaAlpha;
        }
    }
    
    /// <summary>
    /// Show interaction prompt
    /// </summary>
    public void ShowInteractionPrompt(string text = "")
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = string.IsNullOrEmpty(text) ? defaultInteractionText : text;
            interactionPrompt.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Display a warning message
    /// </summary>
    public void ShowWarning(string message, float duration = 3f)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            warningTimer = duration;
            
            Color c = warningText.color;
            c.a = 1f;
            warningText.color = c;
        }
    }
    
    /// <summary>
    /// Add a page to collection
    /// </summary>
    public void CollectPage()
    {
        pagesCollected++;
        UpdateObjectiveText();
        ShowWarning($"PAGE {pagesCollected}/{totalPages} COLLECTED", 2f);
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>📄 Page collected! ({pagesCollected}/{totalPages})</color>");
        }
        
        // Check win condition
        if (pagesCollected >= totalPages)
        {
            OnAllPagesCollected();
        }
    }
    
    void UpdateObjectiveText()
    {
        if (objectiveText != null)
        {
            objectiveText.text = $"COLLECT ALL {totalPages} PAGES";
        }
        
        if (pagesCollectedText != null)
        {
            pagesCollectedText.text = $"{pagesCollected}/{totalPages}";
        }
    }
    
    void OnAllPagesCollected()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "ESCAPE!";
        }
        
        ShowWarning("ALL PAGES COLLECTED! NOW ESCAPE!", 5f);
        
        if (showDebugInfo)
        {
            Debug.Log("<color=green>🎉 ALL PAGES COLLECTED!</color>");
        }
    }
    
    /// <summary>
    /// Flash damage vignette effect
    /// </summary>
    public void FlashDamageVignette()
    {
        if (damageVignette != null)
        {
            StartCoroutine(DamageVignetteEffect());
        }
    }
    
    System.Collections.IEnumerator DamageVignetteEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsed * vignetteFlashSpeed, 0.5f);
            
            Color c = damageVignette.color;
            c.a = alpha;
            damageVignette.color = c;
            
            yield return null;
        }
        
        // Fade out
        Color finalColor = damageVignette.color;
        finalColor.a = 0f;
        damageVignette.color = finalColor;
    }
    
    /// <summary>
    /// Set total pages for the game
    /// </summary>
    public void SetTotalPages(int total)
    {
        totalPages = total;
        UpdateObjectiveText();
    }
    
    /// <summary>
    /// Get current pages collected
    /// </summary>
    public int GetPagesCollected()
    {
        return pagesCollected;
    }
}
