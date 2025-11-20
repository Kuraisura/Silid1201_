using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// Automated setup tool for Demo1 Main Menu
/// Creates horror-themed UI with proper layout
/// </summary>
public class Demo1MenuSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Silid 12:01/Setup Demo1 Main Menu")]
    public static void SetupMainMenu()
    {
        // Check current scene
        if (SceneManager.GetActiveScene().name != "Demo1")
        {
            if (!EditorUtility.DisplayDialog(
                "Wrong Scene?",
                $"Current scene is '{SceneManager.GetActiveScene().name}', not 'Demo1'.\n\nContinue anyway?",
                "Yes, Continue",
                "Cancel"))
            {
                return;
            }
        }

        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateCanvas();
        }

        // Create Fade Panel
        CanvasGroup fadePanel = CreateFadePanel(canvas);

        // Create Main Menu
        GameObject mainMenuPanel = CreateMainMenuPanel(canvas);

        // Create Settings Panel (optional, hidden by default)
        GameObject settingsPanel = CreateSettingsPanel(canvas);
        settingsPanel.SetActive(false);

        // Create MainMenuController
        GameObject controller = new GameObject("MainMenuController");
        MainMenuController menuController = controller.AddComponent<MainMenuController>();
        
        // Link everything
        menuController.mainMenuPanel = mainMenuPanel;
        menuController.settingsPanel = settingsPanel;
        menuController.fadePanel = fadePanel;
        menuController.cutsceneSceneName = "Cutscene";
        menuController.fadeDuration = 1f;

        // Find and link buttons
        menuController.playButton = mainMenuPanel.transform.Find("ButtonContainer/PlayButton")?.GetComponent<Button>();
        menuController.settingsButton = mainMenuPanel.transform.Find("ButtonContainer/SettingsButton")?.GetComponent<Button>();
        menuController.quitButton = mainMenuPanel.transform.Find("ButtonContainer/QuitButton")?.GetComponent<Button>();
        menuController.backButton = settingsPanel.transform.Find("BackButton")?.GetComponent<Button>();
        menuController.titleText = mainMenuPanel.transform.Find("TitleText")?.GetComponent<Text>();

        // Mark dirty
        EditorUtility.SetDirty(controller);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Selection.activeGameObject = controller;

        EditorUtility.DisplayDialog(
            "Demo1 Main Menu Setup Complete!",
            "✓ Canvas created with horror theme\n" +
            "✓ Main Menu panel with Play/Settings/Quit buttons\n" +
            "✓ Settings panel\n" +
            "✓ Fade transition panel\n" +
            "✓ MainMenuController configured\n\n" +
            "Scene Flow:\n" +
            "Demo1 (Main Menu) → Cutscene → Main\n\n" +
            "Next Steps:\n" +
            "1. Customize colors/fonts in Inspector\n" +
            "2. Add background image to MainMenuPanel\n" +
            "3. Add menu music (optional)\n" +
            "4. Test by pressing Play!",
            "OK");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("✓ Canvas created");
        return canvas;
    }

    private static CanvasGroup CreateFadePanel(Canvas canvas)
    {
        GameObject fadeObj = new GameObject("FadePanel");
        fadeObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = fadeObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image image = fadeObj.AddComponent<Image>();
        image.color = Color.black;

        CanvasGroup group = fadeObj.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        fadeObj.SetActive(false);

        Debug.Log("✓ Fade Panel created");
        return group;
    }

    private static GameObject CreateMainMenuPanel(Canvas canvas)
    {
        GameObject panel = new GameObject("MainMenuPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image bgImage = panel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 1f); // Very dark gray

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.sizeDelta = new Vector2(800, 150);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "SILID 12:01";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 80;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.8f, 0f, 0f); // Dark red

        // Shadow effect
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = Color.black;
        titleShadow.effectDistance = new Vector2(5, -5);

        // Button Container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(panel.transform, false);

        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.4f);
        containerRect.anchorMax = new Vector2(0.5f, 0.4f);
        containerRect.sizeDelta = new Vector2(400, 400);

        // Create buttons
        CreateMenuButton(buttonContainer, "PlayButton", "PLAY", new Vector2(0, 100));
        CreateMenuButton(buttonContainer, "SettingsButton", "SETTINGS", new Vector2(0, 0));
        CreateMenuButton(buttonContainer, "QuitButton", "QUIT", new Vector2(0, -100));

        Debug.Log("✓ Main Menu Panel created");
        return panel;
    }

    private static GameObject CreateMenuButton(GameObject parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(350, 80);
        rect.anchoredPosition = position;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark gray

        Button button = buttonObj.AddComponent<Button>();
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f);
        colors.highlightedColor = new Color(0.5f, 0f, 0f); // Red on hover
        colors.pressedColor = new Color(0.8f, 0f, 0f); // Bright red on click
        colors.selectedColor = new Color(0.3f, 0f, 0f);
        button.colors = colors;

        // Button Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 32;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;

        return buttonObj;
    }

    private static GameObject CreateSettingsPanel(Canvas canvas)
    {
        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image bgImage = panel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

        // Settings Title
        GameObject titleObj = new GameObject("SettingsTitle");
        titleObj.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(600, 80);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "SETTINGS";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 50;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.8f, 0f, 0f);

        // Back Button
        CreateMenuButton(panel, "BackButton", "BACK", new Vector2(0, -350));

        // Add note text
        GameObject noteObj = new GameObject("ComingSoonText");
        noteObj.transform.SetParent(panel.transform, false);

        RectTransform noteRect = noteObj.AddComponent<RectTransform>();
        noteRect.anchorMin = new Vector2(0.5f, 0.5f);
        noteRect.anchorMax = new Vector2(0.5f, 0.5f);
        noteRect.sizeDelta = new Vector2(600, 100);

        Text noteText = noteObj.AddComponent<Text>();
        noteText.text = "Settings Coming Soon...";
        noteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        noteText.fontSize = 28;
        noteText.alignment = TextAnchor.MiddleCenter;
        noteText.color = new Color(0.6f, 0.6f, 0.6f);

        Debug.Log("✓ Settings Panel created");
        return panel;
    }
#endif
}
