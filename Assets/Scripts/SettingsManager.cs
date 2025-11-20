using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public AudioMixer audioMixer; // Optional: for advanced audio control
    
    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    
    [Header("Gameplay Settings")]
    public Slider mouseSensitivitySlider;
    public Text mouseSensitivityValue;
    
    [Header("UI References")]
    public Text masterVolumeText;
    public Text musicVolumeText;
    public Text sfxVolumeText;

    private Resolution[] resolutions;
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string QUALITY_KEY = "QualityLevel";
    private const string FULLSCREEN_KEY = "Fullscreen";
    private const string MOUSE_SENS_KEY = "MouseSensitivity";

    private void Start()
    {
        SetupResolutionDropdown();
        LoadSettings();
        ApplySettings();
    }

    /// <summary>
    /// Setup resolution dropdown with available resolutions
    /// </summary>
    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            #if UNITY_2022_2_OR_NEWER
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
            #else
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            #endif
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Load saved settings from PlayerPrefs
    /// </summary>
    private void LoadSettings()
    {
        // Audio Settings
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

        // Graphics Settings
        int qualityLevel = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        if (qualityDropdown != null) qualityDropdown.value = qualityLevel;

        int isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0);
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen == 1;

        // Mouse Sensitivity
        float mouseSens = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 2f);
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = mouseSens;

        // Update UI text displays
        UpdateVolumeDisplays();
    }

    /// <summary>
    /// Apply all current settings
    /// </summary>
    public void ApplySettings()
    {
        SaveSettings();
        
        // Apply quality settings
        if (qualityDropdown != null)
        {
            QualitySettings.SetQualityLevel(qualityDropdown.value);
        }

        // Apply fullscreen
        if (fullscreenToggle != null)
        {
            Screen.fullScreen = fullscreenToggle.isOn;
        }

        // Apply VSync
        if (vsyncToggle != null)
        {
            QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
        }

        Debug.Log("Settings applied successfully!");
    }

    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolumeSlider.value);
        
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value);
        
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider.value);
        
        if (qualityDropdown != null)
            PlayerPrefs.SetInt(QUALITY_KEY, qualityDropdown.value);
        
        if (fullscreenToggle != null)
            PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreenToggle.isOn ? 1 : 0);
        
        if (mouseSensitivitySlider != null)
            PlayerPrefs.SetFloat(MOUSE_SENS_KEY, mouseSensitivitySlider.value);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset all settings to default
    /// </summary>
    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 0.7f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 0.8f;
        if (qualityDropdown != null) qualityDropdown.value = 2; // Medium quality
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (vsyncToggle != null) vsyncToggle.isOn = true;
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = 2f;

        ApplySettings();
        UpdateVolumeDisplays();
    }

    // Called when volume sliders change
    public void OnMasterVolumeChanged()
    {
        if (masterVolumeSlider != null)
        {
            AudioListener.volume = masterVolumeSlider.value;
            UpdateVolumeDisplays();
        }
    }

    public void OnMusicVolumeChanged()
    {
        if (musicVolumeSlider != null && audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolumeSlider.value) * 20);
            UpdateVolumeDisplays();
        }
    }

    public void OnSFXVolumeChanged()
    {
        if (sfxVolumeSlider != null && audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolumeSlider.value) * 20);
            UpdateVolumeDisplays();
        }
    }

    public void OnMouseSensitivityChanged()
    {
        if (mouseSensitivitySlider != null && mouseSensitivityValue != null)
        {
            mouseSensitivityValue.text = mouseSensitivitySlider.value.ToString("F1");
        }
    }

    public void OnResolutionChanged()
    {
        if (resolutionDropdown != null && resolutions != null)
        {
            Resolution resolution = resolutions[resolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    /// <summary>
    /// Update volume percentage displays
    /// </summary>
    private void UpdateVolumeDisplays()
    {
        if (masterVolumeText != null && masterVolumeSlider != null)
            masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";
        
        if (musicVolumeText != null && musicVolumeSlider != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
        
        if (sfxVolumeText != null && sfxVolumeSlider != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
        
        if (mouseSensitivityValue != null && mouseSensitivitySlider != null)
            mouseSensitivityValue.text = mouseSensitivitySlider.value.ToString("F1");
    }

    /// <summary>
    /// Get current mouse sensitivity (call this from other scripts)
    /// </summary>
    public static float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 2f);
    }
}
