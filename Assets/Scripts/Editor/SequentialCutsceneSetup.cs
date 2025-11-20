using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Automated setup tool for sequential cutscene scene
/// Sets up Timeline -> Timeline2 -> Main Scene flow
/// </summary>
public class SequentialCutsceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Silid 12:01/Setup Sequential Cutscene Scene")]
    public static void SetupCutsceneScene()
    {
        // Check if we're in a valid scene
        if (!EditorSceneManager.GetActiveScene().IsValid())
        {
            EditorUtility.DisplayDialog("Error", "No valid scene is open.", "OK");
            return;
        }

        // Check if we have Timeline and Timeline2 in scene
        PlayableDirector[] directors = FindObjectsByType<PlayableDirector>(FindObjectsSortMode.None);
        
        PlayableDirector timeline1 = null;
        PlayableDirector timeline2 = null;

        foreach (var director in directors)
        {
            if (director.name == "Timeline")
                timeline1 = director;
            else if (director.name == "Timeline2")
                timeline2 = director;
        }

        if (timeline1 == null || timeline2 == null)
        {
            if (EditorUtility.DisplayDialog(
                "Timeline Not Found",
                "Could not find 'Timeline' and/or 'Timeline2' in the scene.\n\n" +
                "Do you want to create them?",
                "Yes, Create Them",
                "Cancel"))
            {
                timeline1 = CreateTimelineObject("Timeline");
                timeline2 = CreateTimelineObject("Timeline2");
            }
            else
            {
                return;
            }
        }

        // Create or get CutsceneManager
        SequentialCutsceneManager manager = FindFirstObjectByType<SequentialCutsceneManager>();
        
        if (manager == null)
        {
            GameObject managerObj = new GameObject("CutsceneManager");
            manager = managerObj.AddComponent<SequentialCutsceneManager>();
            Debug.Log("✓ Created CutsceneManager");
        }

        // Assign timelines
        manager.timeline1 = timeline1;
        manager.timeline2 = timeline2;
        manager.mainSceneName = "Main";
        manager.allowSkip = true;
        manager.transitionDuration = 1f;

        // Create UI Canvas if not exists
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateCutsceneUI();
            Debug.Log("✓ Created Cutscene UI Canvas");
        }

        // Find or create fade panel
        CanvasGroup fadePanel = FindFadePanel(canvas);
        if (fadePanel == null)
        {
            fadePanel = CreateFadePanel(canvas);
            Debug.Log("✓ Created Fade Panel");
        }

        // Find or create skip hint
        Canvas skipCanvas = FindSkipHintCanvas();
        if (skipCanvas == null)
        {
            skipCanvas = CreateSkipHintUI(canvas);
            Debug.Log("✓ Created Skip Hint UI");
        }

        // Link to manager
        manager.fadePanel = fadePanel;
        manager.skipHintCanvas = skipCanvas;

        // Configure timeline settings
        ConfigureTimeline(timeline1, false);
        ConfigureTimeline(timeline2, false);

        // Mark scene as dirty
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        string message = "Sequential Cutscene Setup Complete!\n\n" +
                        "✓ Timeline 1 configured\n" +
                        "✓ Timeline 2 configured\n" +
                        "✓ CutsceneManager created\n" +
                        "✓ UI Canvas with fade and skip hint\n" +
                        "✓ Scene flow: Timeline → Timeline2 → Main\n\n" +
                        "Next Steps:\n" +
                        "1. Assign Timeline Assets to Timeline and Timeline2\n" +
                        "2. Press Play to test the cutscene sequence\n" +
                        "3. Press ESC or SPACE to skip cutscenes";

        EditorUtility.DisplayDialog("Setup Complete", message, "OK");
        
        Selection.activeGameObject = manager.gameObject;
    }

    private static PlayableDirector CreateTimelineObject(string name)
    {
        GameObject timelineObj = new GameObject(name);
        PlayableDirector director = timelineObj.AddComponent<PlayableDirector>();
        director.playOnAwake = false;
        
        Debug.Log($"✓ Created {name}");
        return director;
    }

    private static void ConfigureTimeline(PlayableDirector director, bool playOnAwake)
    {
        if (director == null) return;

        director.playOnAwake = playOnAwake;
        director.timeUpdateMode = DirectorUpdateMode.GameTime;
        director.extrapolationMode = DirectorWrapMode.Hold;

        EditorUtility.SetDirty(director);
    }

    private static Canvas CreateCutsceneUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("CutsceneUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if doesn't exist
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvas;
    }

    private static CanvasGroup CreateFadePanel(Canvas parentCanvas)
    {
        GameObject fadeObj = new GameObject("FadePanel");
        fadeObj.transform.SetParent(parentCanvas.transform, false);

        RectTransform rect = fadeObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image image = fadeObj.AddComponent<Image>();
        image.color = Color.black;

        CanvasGroup group = fadeObj.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        fadeObj.SetActive(false);

        return group;
    }

    private static Canvas CreateSkipHintUI(Canvas parentCanvas)
    {
        GameObject skipObj = new GameObject("SkipHint");
        skipObj.transform.SetParent(parentCanvas.transform, false);

        Canvas skipCanvas = skipObj.AddComponent<Canvas>();
        skipCanvas.overrideSorting = true;
        skipCanvas.sortingOrder = 101;

        CanvasGroup group = skipObj.AddComponent<CanvasGroup>();

        RectTransform rect = skipObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(400, 60);
        rect.anchoredPosition = new Vector2(0, 50);

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(skipObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(skipObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -10);
        textRect.anchoredPosition = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = "Press ESC or SPACE to skip";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        // Add skip hint component
        CutsceneSkipHint skipHint = skipObj.AddComponent<CutsceneSkipHint>();
        skipHint.skipText = text;
        skipHint.canvasGroup = group;

        skipObj.SetActive(false);

        return skipCanvas;
    }

    private static CanvasGroup FindFadePanel(Canvas canvas)
    {
        if (canvas == null) return null;

        foreach (Transform child in canvas.transform)
        {
            if (child.name == "FadePanel")
            {
                return child.GetComponent<CanvasGroup>();
            }
        }

        return null;
    }

    private static Canvas FindSkipHintCanvas()
    {
        Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas c in allCanvas)
        {
            if (c.name == "SkipHint")
                return c;
        }

        return null;
    }
#endif
}
