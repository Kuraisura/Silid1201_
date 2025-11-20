using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Audio")]
    public AudioSource menuMusicSource;
    public AudioClip buttonClickSound;
    private AudioSource buttonAudioSource;

    [Header("Scene Management")]
    public string cutsceneSceneName = "IntroCutscene"; // Name of cutscene scene
    public string gameSceneName = "Main"; // Name of your main game scene
    public bool playCutsceneOnStart = true; // Set false to skip cutscene

    [Header("UI Animation")]
    public Animator titleAnimator; // Optional: for title animations
    public float fadeInDuration = 1f;

    private void Start()
    {
        // Setup audio source for button clicks
        buttonAudioSource = gameObject.AddComponent<AudioSource>();
        buttonAudioSource.playOnAwake = false;

        // Make sure we start with the main menu visible
        ShowMainMenu();

        // Optional: Play background music
        if (menuMusicSource != null && !menuMusicSource.isPlaying)
        {
            menuMusicSource.Play();
        }

        // Prevent the screen from sleeping
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    /// <summary>
    /// Called when Start button is clicked
    /// </summary>
    public void OnStartButtonClicked()
    {
        PlayButtonSound();
        
        // Check if we should play cutscene first
        if (playCutsceneOnStart && !string.IsNullOrEmpty(cutsceneSceneName))
        {
            LoadCutsceneScene();
        }
        else
        {
            LoadGameScene();
        }
    }

    /// <summary>
    /// Load the cutscene scene
    /// </summary>
    private void LoadCutsceneScene()
    {
        // Fade out music if needed
        if (menuMusicSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }

        Debug.Log($"Loading cutscene: {cutsceneSceneName}");
        SceneManager.LoadScene(cutsceneSceneName);
    }

    /// <summary>
    /// Called when Settings button is clicked
    /// </summary>
    public void OnSettingsButtonClicked()
    {
        PlayButtonSound();
        ShowSettings();
    }

    /// <summary>
    /// Called when Back button in settings is clicked
    /// </summary>
    public void OnBackButtonClicked()
    {
        PlayButtonSound();
        ShowMainMenu();
    }

    /// <summary>
    /// Called when Quit button is clicked (optional)
    /// </summary>
    public void OnQuitButtonClicked()
    {
        PlayButtonSound();
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Show main menu panel
    /// </summary>
    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Show settings panel
    /// </summary>
    private void ShowSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Load the main game scene
    /// </summary>
    private void LoadGameScene()
    {
        // Fade out music if needed
        if (menuMusicSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Play button click sound
    /// </summary>
    private void PlayButtonSound()
    {
        if (buttonClickSound != null && buttonAudioSource != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// Fade out background music
    /// </summary>
    private System.Collections.IEnumerator FadeOutMusic()
    {
        float startVolume = menuMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            menuMusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        menuMusicSource.volume = 0f;
        menuMusicSource.Stop();
    }

    /// <summary>
    /// Method for button hover effects (call from EventTrigger)
    /// </summary>
    public void OnButtonHover()
    {
        // You can add hover sound effect here
        // buttonAudioSource.PlayOneShot(hoverSound);
    }
}
