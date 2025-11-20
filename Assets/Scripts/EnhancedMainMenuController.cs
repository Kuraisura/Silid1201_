using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Enhanced Main Menu Controller with beautiful transitions and animations
/// Automatically loads Cutscene on Play, shows quit confirmation dialog
/// </summary>
public class EnhancedMainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Main menu panel with Play/Settings/Quit buttons")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Quit confirmation dialog panel")]
    public GameObject quitDialogPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Quit Dialog Buttons")]
    public Button yesButton;
    public Button noButton;

    [Header("Scene Settings")]
    [Tooltip("Name of the cutscene scene to load - defaults to 'Cutscene'")]
    public string cutsceneSceneName = "Cutscene";

    [Header("Audio")]
    public AudioSource menuMusic;
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    
    [Header("Fade Transition")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    [Header("Title Animation")]
    public TextMeshProUGUI titleText;
    public float titlePulseSpeed = 1f;
    public float titleScaleAmount = 0.05f;
    public Color titleColorMin = new Color(0.8f, 0.8f, 1f);
    public Color titleColorMax = Color.white;

    [Header("Background Animation")]
    public Image backgroundImage;
    public float backgroundScrollSpeed = 0.1f;
    public float backgroundRotateSpeed = 5f;

    [Header("Particle Effects")]
    public ParticleSystem menuParticles;
    public ParticleSystem buttonClickParticles;

    private AudioSource sfxSource;
    private Vector3 titleOriginalScale;
    private Material backgroundMaterial;

    private void Start()
    {
        // Setup SFX audio source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // Store title original scale
        if (titleText != null)
        {
            titleOriginalScale = titleText.transform.localScale;
        }

        // Setup background material for animation
        if (backgroundImage != null && backgroundImage.material != null)
        {
            backgroundMaterial = new Material(backgroundImage.material);
            backgroundImage.material = backgroundMaterial;
        }

        // Setup button listeners
        SetupButton(playButton, OnPlayClicked);
        SetupButton(settingsButton, OnSettingsClicked);
        SetupButton(quitButton, OnQuitClicked);
        SetupButton(yesButton, OnQuitConfirmed);
        SetupButton(noButton, OnQuitCancelled);

        // Hide quit dialog initially
        if (quitDialogPanel != null)
        {
            quitDialogPanel.SetActive(false);
        }

        // Start menu particles
        if (menuParticles != null)
        {
            menuParticles.Play();
        }

        // Start fade in
        StartCoroutine(FadeIn());

        Debug.Log("Enhanced Main Menu initialized");
    }

    private void Update()
    {
        // Animate title with pulse effect
        if (titleText != null)
        {
            float pulse = Mathf.Sin(Time.time * titlePulseSpeed) * titleScaleAmount;
            titleText.transform.localScale = titleOriginalScale + Vector3.one * pulse;
            
            float lerp = (Mathf.Sin(Time.time * titlePulseSpeed * 0.5f) + 1f) / 2f;
            titleText.color = Color.Lerp(titleColorMin, titleColorMax, lerp);
        }

        // Animate background
        if (backgroundMaterial != null)
        {
            float offset = Time.time * backgroundScrollSpeed;
            backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(offset, offset * 0.5f));
        }

        if (backgroundImage != null)
        {
            backgroundImage.transform.Rotate(Vector3.forward, backgroundRotateSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Setup button with listeners and sound effects
    /// </summary>
    private void SetupButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;

        button.onClick.AddListener(action);
        button.onClick.AddListener(() => PlaySound(buttonClickSound));
        button.onClick.AddListener(() => PlayButtonClickEffect(button));

        // Add hover sound via EventTrigger
        UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlaySound(buttonHoverSound); });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// Play button clicked - Load cutscene scene
    /// </summary>
    public void OnPlayClicked()
    {
        Debug.Log("Play button clicked! Loading Cutscene...");
        StartCoroutine(LoadCutsceneWithTransition());
    }

    /// <summary>
    /// Settings button clicked
    /// </summary>
    public void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        StartCoroutine(AnimateButtonPress());
        // Add your settings panel logic here
    }

    /// <summary>
    /// Quit button clicked - Show confirmation dialog
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("Quit button clicked - showing confirmation");
        
        if (quitDialogPanel != null)
        {
            StartCoroutine(ShowQuitDialog());
        }
        else
        {
            // No dialog, quit directly
            QuitApplication();
        }
    }

    /// <summary>
    /// User confirmed quit
    /// </summary>
    public void OnQuitConfirmed()
    {
        Debug.Log("Quit confirmed");
        StartCoroutine(QuitWithFade());
    }

    /// <summary>
    /// User cancelled quit
    /// </summary>
    public void OnQuitCancelled()
    {
        Debug.Log("Quit cancelled");
        StartCoroutine(HideQuitDialog());
    }

    /// <summary>
    /// Show quit confirmation dialog with animation
    /// </summary>
    private IEnumerator ShowQuitDialog()
    {
        if (quitDialogPanel == null) yield break;

        quitDialogPanel.SetActive(true);
        CanvasGroup canvasGroup = quitDialogPanel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = quitDialogPanel.AddComponent<CanvasGroup>();
        }

        // Fade in and scale up
        canvasGroup.alpha = 0f;
        quitDialogPanel.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            canvasGroup.alpha = t;
            quitDialogPanel.transform.localScale = Vector3.one * EaseOutBack(t);
            
            yield return null;
        }

        canvasGroup.alpha = 1f;
        quitDialogPanel.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Hide quit confirmation dialog with animation
    /// </summary>
    private IEnumerator HideQuitDialog()
    {
        if (quitDialogPanel == null) yield break;

        CanvasGroup canvasGroup = quitDialogPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = quitDialogPanel.AddComponent<CanvasGroup>();

        // Fade out and scale down
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
            
            canvasGroup.alpha = t;
            quitDialogPanel.transform.localScale = Vector3.one * t;
            
            yield return null;
        }

        quitDialogPanel.SetActive(false);
    }

    /// <summary>
    /// Load cutscene scene with beautiful transition
    /// </summary>
    private IEnumerator LoadCutsceneWithTransition()
    {
        // Disable buttons
        if (playButton != null) playButton.interactable = false;
        if (settingsButton != null) settingsButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;

        // Fade out menu music
        if (menuMusic != null)
        {
            float startVolume = menuMusic.volume;
            float elapsed = 0f;
            
            while (elapsed < fadeDuration * 0.7f)
            {
                elapsed += Time.deltaTime;
                menuMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / (fadeDuration * 0.7f));
                yield return null;
            }
            
            menuMusic.Stop();
        }

        // Fade out screen
        yield return StartCoroutine(FadeOut());

        // Load cutscene scene
        Debug.Log($"Loading scene: {cutsceneSceneName}");
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(cutsceneSceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Quit with fade transition
    /// </summary>
    private IEnumerator QuitWithFade()
    {
        yield return StartCoroutine(FadeOut());
        QuitApplication();
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    private void QuitApplication()
    {
        Debug.Log("Quitting application...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Fade in from black
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Fade out to black
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = elapsed / fadeDuration;
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    /// <summary>
    /// Play button click particle effect
    /// </summary>
    private void PlayButtonClickEffect(Button button)
    {
        if (buttonClickParticles != null)
        {
            buttonClickParticles.transform.position = button.transform.position;
            buttonClickParticles.Play();
        }
    }

    /// <summary>
    /// Animate button press feedback
    /// </summary>
    private IEnumerator AnimateButtonPress()
    {
        // Add screen shake or other feedback here
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Ease out back animation curve
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
