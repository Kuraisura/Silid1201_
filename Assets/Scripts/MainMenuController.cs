using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Main Menu Manager for Demo1 scene
/// Handles Play, Settings, Quit buttons and scene transitions
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Main menu panel with Play/Settings/Quit buttons")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Settings panel (optional)")]
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton; // For settings panel

    [Header("Scene Settings")]
    [Tooltip("Name of the cutscene scene to load")]
    public string cutsceneSceneName = "Cutscene";
    
    [Tooltip("Show loading screen when transitioning")]
    public bool useLoadingScreen = true;

    [Header("Audio (Optional)")]
    public AudioSource menuMusic;
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    
    [Header("Fade Transition")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    [Header("Title Animation (Optional)")]
    public Text titleText;
    public float titlePulseSpeed = 1f;
    public Color titleColorMin = new Color(1f, 0.8f, 0.8f);
    public Color titleColorMax = Color.white;

    private AudioSource sfxSource;

    private void Start()
    {
        // Setup SFX audio source
        if (buttonClickSound != null || buttonHoverSound != null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
            AddButtonSounds(playButton);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonSounds(settingsButton);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
            AddButtonSounds(quitButton);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            AddButtonSounds(backButton);
        }

        // Hide settings panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Start fade in
        StartCoroutine(FadeIn());

        Debug.Log("Main Menu initialized");
    }

    private void Update()
    {
        // Animate title if assigned
        if (titleText != null)
        {
            float lerp = (Mathf.Sin(Time.time * titlePulseSpeed) + 1f) / 2f;
            titleText.color = Color.Lerp(titleColorMin, titleColorMax, lerp);
        }
    }

    /// <summary>
    /// Play button clicked - Load cutscene
    /// </summary>
    public void OnPlayClicked()
    {
        Debug.Log("Play button clicked! Loading cutscene...");
        StartCoroutine(LoadCutsceneScene());
    }

    /// <summary>
    /// Settings button clicked
    /// </summary>
    public void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Back button clicked (from settings)
    /// </summary>
    public void OnBackClicked()
    {
        Debug.Log("Back button clicked");
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Quit button clicked
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("Quit button clicked");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Load the cutscene scene with fade transition
    /// </summary>
    private IEnumerator LoadCutsceneScene()
    {
        // Fade out menu music
        if (menuMusic != null)
        {
            float startVolume = menuMusic.volume;
            float elapsed = 0f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                menuMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }
            
            menuMusic.Stop();
        }

        // Fade out screen
        yield return StartCoroutine(FadeOut());

        // Load cutscene scene
        Debug.Log($"Loading scene: {cutsceneSceneName}");
        
        if (useLoadingScreen)
        {
            // Use async loading
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(cutsceneSceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            // Direct load
            SceneManager.LoadScene(cutsceneSceneName);
        }
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
    /// Add hover and click sounds to button
    /// </summary>
    private void AddButtonSounds(Button button)
    {
        if (button == null) return;

        // Click sound
        if (buttonClickSound != null)
        {
            button.onClick.AddListener(() => PlaySound(buttonClickSound));
        }

        // Hover sound (using EventTrigger would be better but this is simpler)
        var buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null && buttonHoverSound != null)
        {
            // Add event trigger for hover
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
}
