using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Automated Main Menu Generator for Silid 12:01
/// Creates complete main menu UI structure with all scripts attached
/// </summary>
public class MainMenuGenerator : EditorWindow
{
    private string gameSceneName = "Main";
    private string menuSceneName = "MainMenu";
    private string cutsceneSceneName = "IntroCutscene";
    private bool generateCutsceneScene = true;
    private Color primaryColor = new Color(0.545f, 0f, 0f); // #8B0000 Dark Red
    private Color accentColor = new Color(1f, 0.27f, 0.27f); // #FF4444 Bright Red
    private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f); // #1A1A1A Near Black
    private Font customFont;

    [MenuItem("Tools/Silid 12:01/Generate Main Menu")]
    public static void ShowWindow()
    {
        GetWindow<MainMenuGenerator>("Main Menu Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Silid 12:01 - Main Menu Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("This will automatically generate a complete main menu system with:\n" +
            "• Main Menu Scene\n" +
            "• Intro Cutscene Scene (optional)\n" +
            "• Canvas with proper scaling\n" +
            "• Main Menu Panel (Title + Buttons)\n" +
            "• Settings Panel (Audio, Graphics, Gameplay)\n" +
            "• All scripts attached and configured\n" +
            "• Professional styling", MessageType.Info);

        GUILayout.Space(10);

        GUILayout.Label("Configuration:", EditorStyles.boldLabel);
        gameSceneName = EditorGUILayout.TextField("Game Scene Name:", gameSceneName);
        menuSceneName = EditorGUILayout.TextField("Menu Scene Name:", menuSceneName);
        cutsceneSceneName = EditorGUILayout.TextField("Cutscene Scene Name:", cutsceneSceneName);
        generateCutsceneScene = EditorGUILayout.Toggle("Generate Cutscene Scene:", generateCutsceneScene);
        
        GUILayout.Space(5);
        GUILayout.Label("Colors:", EditorStyles.boldLabel);
        primaryColor = EditorGUILayout.ColorField("Primary (Dark Red):", primaryColor);
        accentColor = EditorGUILayout.ColorField("Accent (Bright Red):", accentColor);
        backgroundColor = EditorGUILayout.ColorField("Background:", backgroundColor);

        GUILayout.Space(5);
        customFont = (Font)EditorGUILayout.ObjectField("Custom Font (Optional):", customFont, typeof(Font), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Main Menu", GUILayout.Height(40)))
        {
            GenerateMainMenu();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Add to Build Settings", GUILayout.Height(30)))
        {
            AddSceneToBuildSettings();
        }
    }

    private void GenerateMainMenu()
    {
        // Create scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Save scene
        string scenePath = $"Assets/Scenes/{menuSceneName}.unity";
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }
        EditorSceneManager.SaveScene(scene, scenePath);

        // Create Canvas
        GameObject canvasGO = CreateCanvas();
        
        // Create Main Menu Manager
        GameObject managerGO = CreateMainMenuManager();
        
        // Create Main Menu Panel
        GameObject mainMenuPanel = CreateMainMenuPanel(canvasGO.transform);
        
        // Create Settings Panel
        GameObject settingsPanel = CreateSettingsPanel(canvasGO.transform);
        
        // Configure Manager
        ConfigureMainMenuManager(managerGO, mainMenuPanel, settingsPanel);
        
        // Save scene
        EditorSceneManager.SaveScene(scene);
        
        // Generate cutscene scene if requested
        if (generateCutsceneScene)
        {
            GenerateCutsceneScene();
        }
        
        Debug.Log($"<color=green>✓ Main Menu generated successfully!</color>\nScene: {scenePath}");
        
        string message = $"Main Menu has been generated successfully!\n\nScene: {scenePath}\n\n";
        if (generateCutsceneScene)
        {
            message += $"Cutscene Scene: Assets/Scenes/{cutsceneSceneName}.unity\n\n";
        }
        message += "Next steps:\n1. Add scenes to Build Settings\n2. Customize fonts and images\n3. Add cutscene video/images\n4. Test in Play mode!";
        
        EditorUtility.DisplayDialog("Success!", message, "OK");
    }

    private void GenerateCutsceneScene()
    {
        // Create cutscene scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Save scene
        string scenePath = $"Assets/Scenes/{cutsceneSceneName}.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        // Create Canvas
        GameObject canvasGO = CreateCanvas();
        
        // Create Cutscene Manager
        GameObject cutsceneManagerGO = new GameObject("CutsceneManager");
        CutsceneManager cutsceneManager = cutsceneManagerGO.AddComponent<CutsceneManager>();
        cutsceneManager.nextSceneName = gameSceneName;
        cutsceneManager.allowSkip = true;
        cutsceneManager.fadeDuration = 1f;
        cutsceneManager.delayAfterCutscene = 1f;
        cutsceneManager.cutsceneType = CutsceneManager.CutsceneType.Video;

        // Create Video Display
        GameObject videoDisplayGO = new GameObject("VideoDisplay");
        videoDisplayGO.transform.SetParent(canvasGO.transform, false);
        RectTransform videoRect = videoDisplayGO.AddComponent<RectTransform>();
        videoRect.anchorMin = Vector2.zero;
        videoRect.anchorMax = Vector2.one;
        videoRect.sizeDelta = Vector2.zero;
        
        RawImage videoImage = videoDisplayGO.AddComponent<RawImage>();
        videoImage.color = Color.black;
        
        CanvasGroup videoGroup = videoDisplayGO.AddComponent<CanvasGroup>();

        // Create Image Display (for image sequence)
        GameObject imageDisplayGO = new GameObject("ImageDisplay");
        imageDisplayGO.transform.SetParent(canvasGO.transform, false);
        RectTransform imageRect = imageDisplayGO.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.sizeDelta = Vector2.zero;
        
        Image cutsceneImage = imageDisplayGO.AddComponent<Image>();
        cutsceneImage.color = Color.white;
        
        CanvasGroup imageGroup = imageDisplayGO.AddComponent<CanvasGroup>();
        imageDisplayGO.SetActive(false); // Start disabled

        // Create Skip Text
        GameObject skipTextGO = new GameObject("SkipText");
        skipTextGO.transform.SetParent(canvasGO.transform, false);
        RectTransform skipRect = skipTextGO.AddComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1, 0);
        skipRect.anchorMax = new Vector2(1, 0);
        skipRect.anchoredPosition = new Vector2(-150, 50);
        skipRect.sizeDelta = new Vector2(280, 40);
        
        Text skipText = skipTextGO.AddComponent<Text>();
        skipText.text = "Press SPACE or ESC to skip";
        skipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        skipText.fontSize = 18;
        skipText.color = new Color(1f, 1f, 1f, 0.7f);
        skipText.alignment = TextAnchor.MiddleRight;
        
        Shadow skipShadow = skipTextGO.AddComponent<Shadow>();
        skipShadow.effectColor = Color.black;
        skipShadow.effectDistance = new Vector2(2, -2);

        // Create Video Player
        GameObject videoPlayerGO = new GameObject("VideoPlayer");
        VideoPlayer videoPlayer = videoPlayerGO.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        // Create Audio Source for video
        AudioSource videoAudioSource = videoPlayerGO.AddComponent<AudioSource>();
        videoAudioSource.playOnAwake = false;

        // Link references
        cutsceneManager.videoPlayer = videoPlayer;
        cutsceneManager.videoDisplay = videoImage;
        cutsceneManager.videoAudioSource = videoAudioSource;
        cutsceneManager.cutsceneImage = cutsceneImage;
        cutsceneManager.skipText = skipText;

        // Save scene
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log($"<color=green>✓ Cutscene scene generated!</color>\nScene: {scenePath}");
    }

    private GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if not exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasGO;
    }

    private GameObject CreateMainMenuManager()
    {
        GameObject managerGO = new GameObject("MainMenuManager");
        MainMenuManager manager = managerGO.AddComponent<MainMenuManager>();
        
        // Add audio source
        AudioSource audioSource = managerGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.volume = 0.3f;
        
        return managerGO;
    }

    private GameObject CreateMainMenuPanel(Transform canvasTransform)
    {
        // Create Main Panel
        GameObject panelGO = new GameObject("MainMenuPanel");
        panelGO.transform.SetParent(canvasTransform, false);
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Add animator
        UIPanelAnimator panelAnimator = panelGO.AddComponent<UIPanelAnimator>();
        panelAnimator.animationType = UIPanelAnimator.AnimationType.FadeAndScale;
        panelAnimator.animationDuration = 0.3f;

        // Create Background Image
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(panelGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        // Create Title
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panelGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(800, 200);
        
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "SILID 12:01";
        titleText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 100;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = accentColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Add shadow
        Shadow shadow = titleGO.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(5, -5);
        
        // Add outline
        Outline outline = titleGO.AddComponent<Outline>();
        outline.effectColor = primaryColor;
        outline.effectDistance = new Vector2(3, -3);

        // Create Button Container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(panelGO.transform, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.4f);
        containerRect.anchorMax = new Vector2(0.5f, 0.4f);
        containerRect.sizeDelta = new Vector2(400, 300);

        // Create Start Button
        CreateButton(buttonContainer.transform, "StartButton", "START", new Vector2(0, 50), 
            () => FindObjectOfType<MainMenuManager>()?.OnStartButtonClicked());

        // Create Settings Button
        CreateButton(buttonContainer.transform, "SettingsButton", "SETTINGS", new Vector2(0, -50), 
            () => FindObjectOfType<MainMenuManager>()?.OnSettingsButtonClicked());

        // Optional: Create Quit Button
        CreateButton(buttonContainer.transform, "QuitButton", "QUIT", new Vector2(0, -150), 
            () => FindObjectOfType<MainMenuManager>()?.OnQuitButtonClicked());

        return panelGO;
    }

    private GameObject CreateSettingsPanel(Transform canvasTransform)
    {
        // Create Settings Panel
        GameObject panelGO = new GameObject("SettingsPanel");
        panelGO.transform.SetParent(canvasTransform, false);
        panelGO.SetActive(false); // Start disabled
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.95f);
        
        // Add animator
        UIPanelAnimator panelAnimator = panelGO.AddComponent<UIPanelAnimator>();
        panelAnimator.animationType = UIPanelAnimator.AnimationType.FadeAndScale;
        panelAnimator.animationDuration = 0.3f;
        
        // Add Settings Manager
        SettingsManager settingsManager = panelGO.AddComponent<SettingsManager>();

        // Create Content Panel
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panelGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.2f, 0.15f);
        contentRect.anchorMax = new Vector2(0.8f, 0.85f);
        contentRect.sizeDelta = Vector2.zero;

        // Create Title
        CreateLabel(contentGO.transform, "SettingsTitle", "SETTINGS", new Vector2(0, 350), 60, FontStyle.Bold);

        // Audio Settings Group
        float yPos = 250;
        CreateLabel(contentGO.transform, "AudioLabel", "AUDIO", new Vector2(0, yPos), 40, FontStyle.Bold);
        
        GameObject masterSlider = CreateSlider(contentGO.transform, "MasterVolumeSlider", "Master Volume", new Vector2(0, yPos - 60));
        GameObject musicSlider = CreateSlider(contentGO.transform, "MusicVolumeSlider", "Music Volume", new Vector2(0, yPos - 130));
        GameObject sfxSlider = CreateSlider(contentGO.transform, "SFXVolumeSlider", "SFX Volume", new Vector2(0, yPos - 200));

        // Graphics Settings Group
        yPos -= 280;
        CreateLabel(contentGO.transform, "GraphicsLabel", "GRAPHICS", new Vector2(0, yPos), 40, FontStyle.Bold);
        
        GameObject qualityDropdown = CreateDropdown(contentGO.transform, "QualityDropdown", "Quality", new Vector2(0, yPos - 60));
        GameObject resolutionDropdown = CreateDropdown(contentGO.transform, "ResolutionDropdown", "Resolution", new Vector2(0, yPos - 130));
        GameObject fullscreenToggle = CreateToggle(contentGO.transform, "FullscreenToggle", "Fullscreen", new Vector2(0, yPos - 200));

        // Gameplay Settings
        yPos -= 280;
        CreateLabel(contentGO.transform, "GameplayLabel", "GAMEPLAY", new Vector2(0, yPos), 40, FontStyle.Bold);
        GameObject mouseSensSlider = CreateSlider(contentGO.transform, "MouseSensitivitySlider", "Mouse Sensitivity", new Vector2(0, yPos - 60));

        // Configure Settings Manager references
        settingsManager.masterVolumeSlider = masterSlider.GetComponentInChildren<Slider>();
        settingsManager.musicVolumeSlider = musicSlider.GetComponentInChildren<Slider>();
        settingsManager.sfxVolumeSlider = sfxSlider.GetComponentInChildren<Slider>();
        settingsManager.qualityDropdown = qualityDropdown.GetComponentInChildren<Dropdown>();
        settingsManager.resolutionDropdown = resolutionDropdown.GetComponentInChildren<Dropdown>();
        settingsManager.fullscreenToggle = fullscreenToggle.GetComponentInChildren<Toggle>();
        settingsManager.mouseSensitivitySlider = mouseSensSlider.GetComponentInChildren<Slider>();
        
        // Get text components for value displays
        settingsManager.masterVolumeText = masterSlider.transform.Find("ValueText")?.GetComponent<Text>();
        settingsManager.musicVolumeText = musicSlider.transform.Find("ValueText")?.GetComponent<Text>();
        settingsManager.sfxVolumeText = sfxSlider.transform.Find("ValueText")?.GetComponent<Text>();
        settingsManager.mouseSensitivityValue = mouseSensSlider.transform.Find("ValueText")?.GetComponent<Text>();

        // Back Button
        CreateButton(panelGO.transform, "BackButton", "BACK", new Vector2(0, -450), 
            () => FindObjectOfType<MainMenuManager>()?.OnBackButtonClicked());

        return panelGO;
    }

    private void CreateButton(Transform parent, string name, string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = position;
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = primaryColor;
        
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Add button effects
        UIButtonEffects buttonEffects = buttonGO.AddComponent<UIButtonEffects>();
        buttonEffects.normalColor = primaryColor;
        buttonEffects.hoverColor = accentColor;
        buttonEffects.clickColor = new Color(0.6f, 0.6f, 0.6f);
        
        // Create text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 32;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
    }

    private GameObject CreateLabel(Transform parent, string name, string text, Vector2 position, int fontSize, FontStyle fontStyle)
    {
        GameObject labelGO = new GameObject(name);
        labelGO.transform.SetParent(parent, false);
        
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(600, fontSize + 20);
        labelRect.anchoredPosition = position;
        
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = text;
        labelText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = fontSize;
        labelText.fontStyle = fontStyle;
        labelText.color = accentColor;
        labelText.alignment = TextAnchor.MiddleCenter;
        
        return labelGO;
    }

    private GameObject CreateSlider(Transform parent, string name, string label, Vector2 position)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(600, 50);
        sliderRect.anchoredPosition = position;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(sliderGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.sizeDelta = new Vector2(200, 40);
        labelRect.anchoredPosition = new Vector2(100, 0);
        
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        // Slider
        GameObject sliderControlGO = new GameObject("Slider");
        sliderControlGO.transform.SetParent(sliderGO.transform, false);
        RectTransform sliderControlRect = sliderControlGO.AddComponent<RectTransform>();
        sliderControlRect.anchorMin = new Vector2(0.4f, 0.5f);
        sliderControlRect.anchorMax = new Vector2(0.4f, 0.5f);
        sliderControlRect.sizeDelta = new Vector2(250, 30);
        sliderControlRect.anchoredPosition = new Vector2(125, 0);
        
        Image sliderBg = sliderControlGO.AddComponent<Image>();
        sliderBg.color = new Color(0.2f, 0.2f, 0.2f);
        
        Slider slider = sliderControlGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        
        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderControlGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);
        
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = accentColor;
        slider.fillRect = fillRect;
        
        // Handle
        GameObject handleAreaGO = new GameObject("Handle Slide Area");
        handleAreaGO.transform.SetParent(sliderControlGO.transform, false);
        RectTransform handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        
        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        RectTransform handleRect = handleGO.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);
        
        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        
        // Value Text
        GameObject valueTextGO = new GameObject("ValueText");
        valueTextGO.transform.SetParent(sliderGO.transform, false);
        RectTransform valueTextRect = valueTextGO.AddComponent<RectTransform>();
        valueTextRect.anchorMin = new Vector2(1, 0.5f);
        valueTextRect.anchorMax = new Vector2(1, 0.5f);
        valueTextRect.sizeDelta = new Vector2(80, 40);
        valueTextRect.anchoredPosition = new Vector2(-40, 0);
        
        Text valueText = valueTextGO.AddComponent<Text>();
        valueText.text = "100%";
        valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        valueText.fontSize = 20;
        valueText.color = Color.white;
        valueText.alignment = TextAnchor.MiddleCenter;
        
        return sliderGO;
    }

    private GameObject CreateDropdown(Transform parent, string name, string label, Vector2 position)
    {
        GameObject dropdownGO = new GameObject(name);
        dropdownGO.transform.SetParent(parent, false);
        
        RectTransform dropdownRect = dropdownGO.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.5f, 0.5f);
        dropdownRect.anchorMax = new Vector2(0.5f, 0.5f);
        dropdownRect.sizeDelta = new Vector2(600, 50);
        dropdownRect.anchoredPosition = position;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.sizeDelta = new Vector2(200, 40);
        labelRect.anchoredPosition = new Vector2(100, 0);
        
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        // Dropdown Control
        GameObject dropdownControlGO = new GameObject("Dropdown");
        dropdownControlGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform dropdownControlRect = dropdownControlGO.AddComponent<RectTransform>();
        dropdownControlRect.anchorMin = new Vector2(0.4f, 0.5f);
        dropdownControlRect.anchorMax = new Vector2(0.4f, 0.5f);
        dropdownControlRect.sizeDelta = new Vector2(300, 40);
        dropdownControlRect.anchoredPosition = new Vector2(150, 0);
        
        Image dropdownBg = dropdownControlGO.AddComponent<Image>();
        dropdownBg.color = new Color(0.2f, 0.2f, 0.2f);
        
        Dropdown dropdown = dropdownControlGO.AddComponent<Dropdown>();
        
        // Dropdown parts would need more setup - simplified for now
        dropdown.options.Add(new Dropdown.OptionData("Low"));
        dropdown.options.Add(new Dropdown.OptionData("Medium"));
        dropdown.options.Add(new Dropdown.OptionData("High"));
        dropdown.options.Add(new Dropdown.OptionData("Ultra"));
        
        return dropdownGO;
    }

    private GameObject CreateToggle(Transform parent, string name, string label, Vector2 position)
    {
        GameObject toggleGO = new GameObject(name);
        toggleGO.transform.SetParent(parent, false);
        
        RectTransform toggleRect = toggleGO.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRect.sizeDelta = new Vector2(600, 50);
        toggleRect.anchoredPosition = position;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(toggleGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.sizeDelta = new Vector2(200, 40);
        labelRect.anchoredPosition = new Vector2(100, 0);
        
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        // Toggle Control
        GameObject toggleControlGO = new GameObject("Toggle");
        toggleControlGO.transform.SetParent(toggleGO.transform, false);
        RectTransform toggleControlRect = toggleControlGO.AddComponent<RectTransform>();
        toggleControlRect.anchorMin = new Vector2(0.4f, 0.5f);
        toggleControlRect.anchorMax = new Vector2(0.4f, 0.5f);
        toggleControlRect.sizeDelta = new Vector2(60, 40);
        toggleControlRect.anchoredPosition = new Vector2(30, 0);
        
        Image toggleBg = toggleControlGO.AddComponent<Image>();
        toggleBg.color = new Color(0.2f, 0.2f, 0.2f);
        
        Toggle toggle = toggleControlGO.AddComponent<Toggle>();
        toggle.isOn = true;
        
        // Checkmark
        GameObject checkmarkGO = new GameObject("Checkmark");
        checkmarkGO.transform.SetParent(toggleControlGO.transform, false);
        RectTransform checkmarkRect = checkmarkGO.AddComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = Vector2.zero;
        
        Image checkmarkImage = checkmarkGO.AddComponent<Image>();
        checkmarkImage.color = accentColor;
        toggle.graphic = checkmarkImage;
        
        return toggleGO;
    }

    private void ConfigureMainMenuManager(GameObject managerGO, GameObject mainMenuPanel, GameObject settingsPanel)
    {
        MainMenuManager manager = managerGO.GetComponent<MainMenuManager>();
        manager.mainMenuPanel = mainMenuPanel;
        manager.settingsPanel = settingsPanel;
        manager.cutsceneSceneName = cutsceneSceneName;
        manager.gameSceneName = gameSceneName;
        manager.playCutsceneOnStart = generateCutsceneScene;
        manager.fadeInDuration = 1f;
        
        EditorUtility.SetDirty(manager);
    }

    private void AddSceneToBuildSettings()
    {
        string menuPath = $"Assets/Scenes/{menuSceneName}.unity";
        string cutscenePath = $"Assets/Scenes/{cutsceneSceneName}.unity";
        
        if (!File.Exists(menuPath))
        {
            EditorUtility.DisplayDialog("Error", $"Scene not found: {menuPath}\n\nPlease generate the main menu first!", "OK");
            return;
        }

        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // Add Main Menu at index 0
        var menuScene = new EditorBuildSettingsScene(menuPath, true);
        scenes.Insert(0, menuScene);
        
        // Add Cutscene at index 1 if it exists
        if (generateCutsceneScene && File.Exists(cutscenePath))
        {
            var cutsceneScene = new EditorBuildSettingsScene(cutscenePath, true);
            scenes.Insert(1, cutsceneScene);
        }
        
        EditorBuildSettings.scenes = scenes.ToArray();
        
        string message = $"✓ MainMenu scene added to Build Settings at index 0\n";
        if (generateCutsceneScene && File.Exists(cutscenePath))
        {
            message += $"✓ Cutscene scene added to Build Settings at index 1\n";
        }
        
        Debug.Log($"<color=green>{message}</color>");
        EditorUtility.DisplayDialog("Success!", message, "OK");
    }
}
