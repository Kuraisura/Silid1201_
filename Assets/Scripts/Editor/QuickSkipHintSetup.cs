using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quick setup tool for skip hint UI
/// </summary>
public class QuickSkipHintSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Silid 12:01/Setup Complete Cutscene UI")]
    public static void SetupCompleteCutsceneUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CutsceneCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("✓ Created Canvas");
        }

        // Create Fade Panel
        GameObject fadePanel = CreateFadePanel(canvas);
        
        // Create Skip Hint
        GameObject skipHint = CreateSkipHint(canvas);

        // Link to Timeline objects
        LinkToTimelines(skipHint, fadePanel);

        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "Cutscene UI setup successfully!\n\n" +
            "✓ Fade Panel created\n" +
            "✓ Skip Hint created\n" +
            "✓ Linked to Timeline and Timeline2\n\n" +
            "Settings Applied:\n" +
            "• Timeline: Play On Awake = ON\n" +
            "• Timeline2: Play On Awake = OFF\n" +
            "• Both: Wrap Mode = Hold\n\n" +
            "Press Play to test!",
            "OK");
    }

    private static GameObject CreateFadePanel(Canvas canvas)
    {
        // Check if already exists
        Transform existing = canvas.transform.Find("FadePanel");
        if (existing != null)
        {
            Debug.Log("FadePanel already exists");
            return existing.gameObject;
        }

        GameObject fadeObj = new GameObject("FadePanel");
        fadeObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = fadeObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image image = fadeObj.AddComponent<Image>();
        image.color = Color.black;

        CanvasGroup group = fadeObj.AddComponent<CanvasGroup>();
        group.alpha = 1f; // Start black
        group.blocksRaycasts = false;
        group.interactable = false;

        Debug.Log("✓ Created FadePanel");
        return fadeObj;
    }

    private static GameObject CreateSkipHint(Canvas canvas)
    {
        // Check if already exists
        Transform existing = canvas.transform.Find("SkipHint");
        if (existing != null)
        {
            Debug.Log("SkipHint already exists");
            return existing.gameObject;
        }

        GameObject skipHint = new GameObject("SkipHint");
        skipHint.transform.SetParent(canvas.transform, false);

        RectTransform rect = skipHint.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 30);
        rect.sizeDelta = new Vector2(500, 60);

        CanvasGroup group = skipHint.AddComponent<CanvasGroup>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(skipHint.transform, false);
        
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(skipHint.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -10);

        Text text = textObj.AddComponent<Text>();
        text.text = "Press ESC or SPACE to skip";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        // Add pulse effect
        skipHint.AddComponent<SimpleSkipHint>();

        skipHint.SetActive(false);

        Debug.Log("✓ Created SkipHint");
        return skipHint;
    }

    private static void LinkToTimelines(GameObject skipHint, GameObject fadePanel)
    {
        var canvas = skipHint.transform.parent.GetComponent<Canvas>();
        var fadePanelGroup = fadePanel.GetComponent<CanvasGroup>();

        // Find Timeline objects
        var timeline1 = GameObject.Find("Timeline");
        var timeline2 = GameObject.Find("Timeline2");

        if (timeline1 != null)
        {
            var script = timeline1.GetComponent<AutoPlayNextTimeline>();
            if (script == null)
            {
                script = timeline1.AddComponent<AutoPlayNextTimeline>();
            }

            script.skipHintUI = skipHint;
            script.fadePanel = fadePanelGroup;
            script.allowSkip = true;
            script.fadeDuration = 0.5f;

            // Set playable director settings
            var director = timeline1.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (director != null)
            {
                director.playOnAwake = true;
                director.extrapolationMode = UnityEngine.Playables.DirectorWrapMode.Hold;
                EditorUtility.SetDirty(director);
            }

            if (timeline2 != null)
            {
                var director2 = timeline2.GetComponent<UnityEngine.Playables.PlayableDirector>();
                script.nextTimeline = director2;
                script.sceneToLoadAfter = "";
            }

            EditorUtility.SetDirty(script);
            Debug.Log("✓ Timeline configured");
        }

        if (timeline2 != null)
        {
            var script = timeline2.GetComponent<AutoPlayNextTimeline>();
            if (script == null)
            {
                script = timeline2.AddComponent<AutoPlayNextTimeline>();
            }

            script.skipHintUI = skipHint;
            script.fadePanel = fadePanelGroup;
            script.allowSkip = true;
            script.fadeDuration = 0.5f;
            script.nextTimeline = null;
            script.sceneToLoadAfter = "Main";

            // Set playable director settings
            var director = timeline2.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (director != null)
            {
                director.playOnAwake = false; // Important!
                director.extrapolationMode = UnityEngine.Playables.DirectorWrapMode.Hold;
                EditorUtility.SetDirty(director);
            }

            EditorUtility.SetDirty(script);
            Debug.Log("✓ Timeline2 configured");
        }

        if (timeline1 == null && timeline2 == null)
        {
            Debug.LogWarning("Timeline and Timeline2 not found in scene!");
        }
    }

    [MenuItem("Tools/Silid 12:01/Create Skip Hint UI")]
    public static void CreateSkipHintUI()
    {
        SetupCompleteCutsceneUI();
    }
#endif
}
