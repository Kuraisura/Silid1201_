using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages sequential playback of multiple Timeline cutscenes
/// Plays Timeline -> Timeline2 -> Loads Main Scene
/// </summary>
public class SequentialCutsceneManager : MonoBehaviour
{
    [Header("Timeline References")]
    [Tooltip("First timeline to play (Timeline)")]
    public PlayableDirector timeline1;
    
    [Tooltip("Second timeline to play (Timeline2)")]
    public PlayableDirector timeline2;

    [Header("Scene Settings")]
    [Tooltip("Scene to load after cutscenes finish")]
    public string mainSceneName = "Main";
    
    [Tooltip("Allow skipping cutscenes with ESC or Space")]
    public bool allowSkip = true;

    [Header("Transition Settings")]
    [Tooltip("Fade duration between timelines")]
    public float transitionDuration = 1f;
    
    [Tooltip("Show loading screen before main scene")]
    public bool showLoadingScreen = false;

    [Header("UI References (Optional)")]
    [Tooltip("UI Canvas to show skip hint")]
    public Canvas skipHintCanvas;
    
    [Tooltip("Fade panel for transitions")]
    public CanvasGroup fadePanel;

    private bool cutsceneComplete = false;
    private bool isSkipping = false;
    private int currentTimelineIndex = 0;

    private void Start()
    {
        // Validate references
        if (timeline1 == null || timeline2 == null)
        {
            Debug.LogError("SequentialCutsceneManager: Timeline references not set!");
            return;
        }

        // Setup
        StartCoroutine(PlayCutsceneSequence());
    }

    private void Update()
    {
        // Skip functionality
        if (allowSkip && !isSkipping && !cutsceneComplete)
        {
            if (CutsceneInput.GetSkipInput())
            {
                SkipCutscenes();
            }
        }
    }

    /// <summary>
    /// Main cutscene sequence coroutine
    /// </summary>
    private IEnumerator PlayCutsceneSequence()
    {
        Debug.Log("Starting cutscene sequence...");

        // Fade in from black
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // Show skip hint
        if (skipHintCanvas != null)
        {
            skipHintCanvas.gameObject.SetActive(true);
        }

        // Play Timeline 1
        Debug.Log("Playing Timeline 1...");
        currentTimelineIndex = 1;
        yield return StartCoroutine(PlayTimeline(timeline1));

        // Check if skipped
        if (isSkipping)
        {
            yield return StartCoroutine(TransitionToMainScene());
            yield break;
        }

        // Transition between timelines
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(FadeIn());
        }
        else
        {
            yield return new WaitForSeconds(transitionDuration);
        }

        // Play Timeline 2
        Debug.Log("Playing Timeline 2...");
        currentTimelineIndex = 2;
        yield return StartCoroutine(PlayTimeline(timeline2));

        // Check if skipped
        if (isSkipping)
        {
            yield return StartCoroutine(TransitionToMainScene());
            yield break;
        }

        // Hide skip hint
        if (skipHintCanvas != null)
        {
            skipHintCanvas.gameObject.SetActive(false);
        }

        // Transition to main scene
        yield return StartCoroutine(TransitionToMainScene());
    }

    /// <summary>
    /// Play a single timeline and wait for completion
    /// </summary>
    private IEnumerator PlayTimeline(PlayableDirector director)
    {
        if (director == null)
        {
            Debug.LogWarning("Timeline is null, skipping...");
            yield break;
        }

        // Stop if already playing
        if (director.state == PlayState.Playing)
        {
            director.Stop();
        }

        // Reset and play
        director.time = 0;
        director.Play();

        // Wait for timeline to finish or skip
        while (director.state == PlayState.Playing && !isSkipping)
        {
            yield return null;
        }

        // Stop timeline if skipping
        if (isSkipping)
        {
            director.Stop();
        }

        Debug.Log($"Timeline finished: {director.name}");
    }

    /// <summary>
    /// Skip all cutscenes and go to main scene
    /// </summary>
    private void SkipCutscenes()
    {
        if (isSkipping) return;

        Debug.Log("Skipping cutscenes...");
        isSkipping = true;

        // Stop all timelines
        if (timeline1 != null && timeline1.state == PlayState.Playing)
        {
            timeline1.Stop();
        }

        if (timeline2 != null && timeline2.state == PlayState.Playing)
        {
            timeline2.Stop();
        }
    }

    /// <summary>
    /// Transition to the main gameplay scene
    /// </summary>
    private IEnumerator TransitionToMainScene()
    {
        cutsceneComplete = true;

        Debug.Log($"Transitioning to Main Scene: {mainSceneName}");

        // Fade out
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Load main scene
        if (showLoadingScreen)
        {
            // Use async loading with loading screen
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainSceneName);
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
            // Direct scene load
            SceneManager.LoadScene(mainSceneName);
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

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = 1f - (elapsed / transitionDuration);
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

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = elapsed / transitionDuration;
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    /// <summary>
    /// Public method to manually skip cutscenes
    /// Can be called from UI button
    /// </summary>
    public void OnSkipButtonClicked()
    {
        SkipCutscenes();
    }

    /// <summary>
    /// Get current cutscene progress
    /// </summary>
    public string GetCurrentStatus()
    {
        if (cutsceneComplete)
            return "Complete";
        else if (isSkipping)
            return "Skipping...";
        else
            return $"Playing Timeline {currentTimelineIndex}";
    }

#if UNITY_EDITOR
    [ContextMenu("Test Play Timeline 1")]
    private void TestTimeline1()
    {
        if (timeline1 != null)
        {
            timeline1.time = 0;
            timeline1.Play();
        }
    }

    [ContextMenu("Test Play Timeline 2")]
    private void TestTimeline2()
    {
        if (timeline2 != null)
        {
            timeline2.time = 0;
            timeline2.Play();
        }
    }

    [ContextMenu("Test Skip to Main Scene")]
    private void TestSkip()
    {
        if (Application.isPlaying)
        {
            SkipCutscenes();
        }
    }
#endif
}
