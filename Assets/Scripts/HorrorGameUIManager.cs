using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Complete Horror Game UI Manager for Slender Man-style gameplay
/// Manages: Pages collected, Battery level, Stamina, Flashlight UI, Hints
/// </summary>
public class HorrorGameUIManager : MonoBehaviour
{
    [Header("Page Collection UI")]
    [SerializeField] private TextMeshProUGUI pageCounterText;
    [SerializeField] private Image[] pageIcons; // Visual page slots
    [SerializeField] private Sprite pageEmptySprite;
    [SerializeField] private Sprite pageCollectedSprite;
    [SerializeField] private int totalPages = 8;
    private int pagesCollected = 0;

    [Header("Battery UI")]
    [SerializeField] private Image batteryFillBar;
    [SerializeField] private Image batteryIconFlash; // Flashes when low
    [SerializeField] private TextMeshProUGUI batteryPercentText;
    [SerializeField] private Color batteryFullColor = new Color(0.2f, 1f, 0.2f); // Green
    [SerializeField] private Color batteryMediumColor = new Color(1f, 1f, 0.2f); // Yellow
    [SerializeField] private Color batteryLowColor = new Color(1f, 0.2f, 0.2f); // Red
    [SerializeField] private float batteryWarningThreshold = 20f;
    private float currentBattery = 100f;
    private bool batteryFlashing = false;

    [Header("Stamina UI")]
    [SerializeField] private Image staminaFillBar;
    [SerializeField] private CanvasGroup staminaPanel; // Fades when full
    [SerializeField] private Color staminaFullColor = new Color(0.3f, 0.8f, 1f); // Cyan
    [SerializeField] private Color staminaLowColor = new Color(1f, 0.3f, 0.3f); // Red
    [SerializeField] private float staminaFadeSpeed = 2f;
    private float currentStamina = 100f;
    private bool staminaDepleted = false;

    [Header("Flashlight UI")]
    [SerializeField] private Image flashlightIndicator;
    [SerializeField] private TextMeshProUGUI flashlightStatusText;
    [SerializeField] private Color flashlightOnColor = new Color(1f, 1f, 0.5f);
    [SerializeField] private Color flashlightOffColor = new Color(0.3f, 0.3f, 0.3f);
    private bool flashlightOn = true;

    [Header("Interaction Prompt")]
    [SerializeField] private CanvasGroup interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private float promptFadeSpeed = 5f;
    private bool showingPrompt = false;

    [Header("Warning Messages")]
    [SerializeField] private CanvasGroup warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningDuration = 3f;

    [Header("Breathing/Heartbeat Effect")]
    [SerializeField] private Image vignetteOverlay;
    [SerializeField] private float maxVignetteAlpha = 0.7f;
    [SerializeField] private float heartbeatSpeed = 1.5f;
    private bool heartbeatActive = false;

    [Header("Static/Horror Effect")]
    [SerializeField] private Image staticOverlay;
    [SerializeField] private float staticIntensity = 0f;
    private Coroutine staticCoroutine;

    [Header("Page Collection Animation")]
    [SerializeField] private CanvasGroup pageCollectedPanel;
    [SerializeField] private TextMeshProUGUI pageCollectedText;
    [SerializeField] private float pageAnimDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip pageCollectSound;
    [SerializeField] private AudioClip batteryWarningSound;
    [SerializeField] private AudioClip flashlightToggleSound;

    private void Start()
    {
        InitializeUI();
    }

    private void Update()
    {
        UpdateStaminaVisuals();
        UpdateHeartbeatEffect();
        
        // Test controls (remove in production)
        if (Input.GetKeyDown(KeyCode.P)) CollectPage();
        if (Input.GetKeyDown(KeyCode.B)) DrainBattery(10f);
        if (Input.GetKeyDown(KeyCode.T)) ToggleFlashlight();
    }

    private void InitializeUI()
    {
        // Initialize page icons
        if (pageIcons != null && pageIcons.Length > 0)
        {
            for (int i = 0; i < pageIcons.Length; i++)
            {
                if (pageIcons[i] != null && pageEmptySprite != null)
                {
                    pageIcons[i].sprite = pageEmptySprite;
                    pageIcons[i].color = new Color(1f, 1f, 1f, 0.3f);
                }
            }
        }

        UpdatePageCounter();
        UpdateBatteryUI();
        UpdateStaminaUI();
        UpdateFlashlightUI();

        // Hide panels initially
        if (interactionPrompt) interactionPrompt.alpha = 0f;
        if (warningPanel) warningPanel.alpha = 0f;
        if (pageCollectedPanel) pageCollectedPanel.alpha = 0f;
        if (staticOverlay) staticOverlay.color = new Color(1, 1, 1, 0);
    }

    #region Page Collection

    /// <summary>
    /// Call this when player collects a page
    /// </summary>
    public void CollectPage()
    {
        if (pagesCollected >= totalPages) return;

        pagesCollected++;
        UpdatePageCounter();
        AnimatePageCollection();
        
        if (uiAudioSource && pageCollectSound)
            uiAudioSource.PlayOneShot(pageCollectSound);

        // Update page icon
        if (pageIcons != null && pagesCollected <= pageIcons.Length)
        {
            if (pageCollectedSprite != null)
            {
                pageIcons[pagesCollected - 1].sprite = pageCollectedSprite;
                pageIcons[pagesCollected - 1].color = Color.white;
                StartCoroutine(PulsePageIcon(pagesCollected - 1));
            }
        }

        // Check if all pages collected
        if (pagesCollected >= totalPages)
        {
            ShowWarning("ALL PAGES COLLECTED! FIND THE EXIT!", 5f);
        }
    }

    private void UpdatePageCounter()
    {
        if (pageCounterText)
            pageCounterText.text = $"{pagesCollected}/{totalPages}";
    }

    private void AnimatePageCollection()
    {
        if (pageCollectedPanel && pageCollectedText)
        {
            pageCollectedText.text = $"PAGE {pagesCollected}/{totalPages} COLLECTED";
            StartCoroutine(FadePanel(pageCollectedPanel, true, 0.3f));
            StartCoroutine(FadePanel(pageCollectedPanel, false, pageAnimDuration));
        }
    }

    private IEnumerator PulsePageIcon(int index)
    {
        if (pageIcons == null || index >= pageIcons.Length) yield break;

        Vector3 originalScale = pageIcons[index].transform.localScale;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / duration * Mathf.PI) * 0.3f;
            pageIcons[index].transform.localScale = originalScale * scale;
            yield return null;
        }

        pageIcons[index].transform.localScale = originalScale;
    }

    public int GetPagesCollected() => pagesCollected;

    #endregion

    #region Battery System

    /// <summary>
    /// Update battery level (0-100)
    /// </summary>
    public void SetBattery(float percent)
    {
        currentBattery = Mathf.Clamp(percent, 0f, 100f);
        UpdateBatteryUI();

        // Start warning flash if low
        if (currentBattery <= batteryWarningThreshold && !batteryFlashing)
        {
            StartCoroutine(FlashBatteryWarning());
            if (uiAudioSource && batteryWarningSound)
                uiAudioSource.PlayOneShot(batteryWarningSound);
        }
    }

    /// <summary>
    /// Drain battery by amount
    /// </summary>
    public void DrainBattery(float amount)
    {
        SetBattery(currentBattery - amount);
    }

    private void UpdateBatteryUI()
    {
        if (batteryFillBar)
        {
            batteryFillBar.fillAmount = currentBattery / 100f;
            
            // Color based on level
            if (currentBattery > 50f)
                batteryFillBar.color = batteryFullColor;
            else if (currentBattery > 20f)
                batteryFillBar.color = Color.Lerp(batteryMediumColor, batteryFullColor, (currentBattery - 20f) / 30f);
            else
                batteryFillBar.color = batteryLowColor;
        }

        if (batteryPercentText)
            batteryPercentText.text = $"{Mathf.RoundToInt(currentBattery)}%";
    }

    private IEnumerator FlashBatteryWarning()
    {
        batteryFlashing = true;
        
        while (currentBattery <= batteryWarningThreshold && currentBattery > 0)
        {
            if (batteryIconFlash)
            {
                batteryIconFlash.color = new Color(1, 0, 0, 0.8f);
                yield return new WaitForSeconds(0.3f);
                batteryIconFlash.color = new Color(1, 0, 0, 0.2f);
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                yield return new WaitForSeconds(0.6f);
            }
        }

        if (batteryIconFlash)
            batteryIconFlash.color = new Color(1, 1, 1, 0f);
        
        batteryFlashing = false;
    }

    public float GetBattery() => currentBattery;

    #endregion

    #region Stamina System

    /// <summary>
    /// Update stamina level (0-100)
    /// </summary>
    public void SetStamina(float percent)
    {
        currentStamina = Mathf.Clamp(percent, 0f, 100f);
        UpdateStaminaUI();

        if (currentStamina <= 0f && !staminaDepleted)
        {
            staminaDepleted = true;
            StartHeartbeat();
        }
        else if (currentStamina > 20f && staminaDepleted)
        {
            staminaDepleted = false;
            StopHeartbeat();
        }
    }

    /// <summary>
    /// Drain stamina by amount
    /// </summary>
    public void DrainStamina(float amount)
    {
        SetStamina(currentStamina - amount);
    }

    /// <summary>
    /// Regenerate stamina
    /// </summary>
    public void RegenerateStamina(float amount)
    {
        SetStamina(currentStamina + amount);
    }

    private void UpdateStaminaUI()
    {
        if (staminaFillBar)
        {
            staminaFillBar.fillAmount = currentStamina / 100f;
            staminaFillBar.color = Color.Lerp(staminaLowColor, staminaFullColor, currentStamina / 100f);
        }
    }

    private void UpdateStaminaVisuals()
    {
        if (staminaPanel == null) return;

        // Fade out stamina bar when full
        float targetAlpha = currentStamina >= 95f ? 0.3f : 1f;
        staminaPanel.alpha = Mathf.Lerp(staminaPanel.alpha, targetAlpha, Time.deltaTime * staminaFadeSpeed);
    }

    public float GetStamina() => currentStamina;
    public bool IsStaminaDepleted() => staminaDepleted;

    #endregion

    #region Flashlight UI

    /// <summary>
    /// Toggle flashlight state
    /// </summary>
    public void ToggleFlashlight()
    {
        flashlightOn = !flashlightOn;
        UpdateFlashlightUI();
        
        if (uiAudioSource && flashlightToggleSound)
            uiAudioSource.PlayOneShot(flashlightToggleSound);
    }

    /// <summary>
    /// Set flashlight state
    /// </summary>
    public void SetFlashlight(bool on)
    {
        flashlightOn = on;
        UpdateFlashlightUI();
    }

    private void UpdateFlashlightUI()
    {
        if (flashlightIndicator)
            flashlightIndicator.color = flashlightOn ? flashlightOnColor : flashlightOffColor;

        if (flashlightStatusText)
            flashlightStatusText.text = flashlightOn ? "ON" : "OFF";
    }

    public bool IsFlashlightOn() => flashlightOn;

    #endregion

    #region Interaction Prompt

    /// <summary>
    /// Show interaction prompt (e.g., "Press E to collect page")
    /// </summary>
    public void ShowInteractionPrompt(string message)
    {
        if (interactionText)
            interactionText.text = message;
        
        showingPrompt = true;
        StartCoroutine(FadePanel(interactionPrompt, true, 0.2f));
    }

    /// <summary>
    /// Hide interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        showingPrompt = false;
        StartCoroutine(FadePanel(interactionPrompt, false, 0.2f));
    }

    #endregion

    #region Warning Messages

    /// <summary>
    /// Show warning message (e.g., "HE IS NEAR")
    /// </summary>
    public void ShowWarning(string message, float duration = 3f)
    {
        if (warningText)
            warningText.text = message;
        
        StopAllCoroutines();
        StartCoroutine(ShowWarningCoroutine(duration));
    }

    private IEnumerator ShowWarningCoroutine(float duration)
    {
        // Fade in
        yield return StartCoroutine(FadePanel(warningPanel, true, 0.3f));
        
        // Wait
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return StartCoroutine(FadePanel(warningPanel, false, 0.5f));
    }

    #endregion

    #region Horror Effects

    /// <summary>
    /// Start heartbeat/breathing effect (when enemy near or stamina low)
    /// </summary>
    public void StartHeartbeat()
    {
        heartbeatActive = true;
    }

    /// <summary>
    /// Stop heartbeat effect
    /// </summary>
    public void StopHeartbeat()
    {
        heartbeatActive = false;
        if (vignetteOverlay)
            vignetteOverlay.color = new Color(0, 0, 0, 0);
    }

    private void UpdateHeartbeatEffect()
    {
        if (vignetteOverlay == null) return;

        if (heartbeatActive)
        {
            float pulse = (Mathf.Sin(Time.time * heartbeatSpeed * Mathf.PI) + 1f) / 2f;
            float alpha = pulse * maxVignetteAlpha;
            vignetteOverlay.color = new Color(0, 0, 0, alpha);
        }
        else
        {
            vignetteOverlay.color = Color.Lerp(vignetteOverlay.color, new Color(0, 0, 0, 0), Time.deltaTime * 2f);
        }
    }

    /// <summary>
    /// Show static effect (when enemy very close)
    /// </summary>
    public void ShowStatic(float intensity, float duration = 1f)
    {
        if (staticCoroutine != null) StopCoroutine(staticCoroutine);
        staticCoroutine = StartCoroutine(StaticEffect(intensity, duration));
    }

    private IEnumerator StaticEffect(float intensity, float duration)
    {
        if (staticOverlay == null) yield break;

        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(intensity, 0f, elapsed / duration);
            staticOverlay.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        staticOverlay.color = new Color(1, 1, 1, 0);
    }

    #endregion

    #region Helper Methods

    private IEnumerator FadePanel(CanvasGroup panel, bool fadeIn, float duration)
    {
        if (panel == null) yield break;

        float start = panel.alpha;
        float end = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            panel.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        panel.alpha = end;
    }

    #endregion

    #region Public Getters

    public int GetTotalPages() => totalPages;
    public bool AllPagesCollected() => pagesCollected >= totalPages;

    #endregion
}
