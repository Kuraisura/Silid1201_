using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Smooth sequential timeline playback with skip functionality
/// Attach to Timeline objects to chain them together
/// IMPORTANT: Only Timeline should have Play On Awake = true
/// Timeline2 should have Play On Awake = false
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class AutoPlayNextTimeline : MonoBehaviour
{
    [Header("Next Timeline")]
    [Tooltip("Timeline to play after this one finishes")]
    public PlayableDirector nextTimeline;

    [Header("Settings")]
    [Tooltip("Delay before starting next timeline (seconds)")]
    public float delayBeforeNext = 0.5f;

    [Tooltip("Scene to load after all timelines finish (leave empty to do nothing)")]
    public string sceneToLoadAfter = "Main";

    [Header("Skip UI")]
    [Tooltip("UI to show skip hint (optional)")]
    public GameObject skipHintUI;
    
    [Tooltip("Allow skipping with ESC or SPACE")]
    public bool allowSkip = true;

    [Header("Fade Settings")]
    [Tooltip("Fade panel for smooth transitions (optional)")]
    public CanvasGroup fadePanel;
    
    [Tooltip("Fade duration")]
    public float fadeDuration = 0.5f;

    private PlayableDirector myDirector;
    private bool hasStarted = false;
    private bool hasFinished = false;
    private bool isTransitioning = false;
    private static bool anyTimelinePlaying = false;

    private void Awake()
    {
        myDirector = GetComponent<PlayableDirector>();
        
        // Force wrap mode to Hold
        if (myDirector != null)
        {
            myDirector.extrapolationMode = DirectorWrapMode.Hold;
            Debug.Log($"[{gameObject.name}] Initialized. Play On Awake: {myDirector.playOnAwake}, Duration: {myDirector.duration}s");
        }
    }

    private void OnDestroy()
    {
        anyTimelinePlaying = false;
    }

    private void Start()
    {
        // Only the first timeline (with Play On Awake) will trigger this
        if (myDirector != null && myDirector.playOnAwake && !anyTimelinePlaying)
        {
            Debug.Log($"[{gameObject.name}] Auto-starting because Play On Awake is enabled");
            StartCoroutine(PlayTimelineSequence());
        }
        else
        {
            Debug.Log($"[{gameObject.name}] NOT auto-starting. Play On Awake: {myDirector.playOnAwake}, Already playing: {anyTimelinePlaying}");
        }
    }

    private IEnumerator PlayTimelineSequence()
    {
        // Prevent multiple instances
        if (hasStarted)
        {
            Debug.LogWarning($"[{gameObject.name}] Already started! Ignoring duplicate call.");
            yield break;
        }

        anyTimelinePlaying = true;
        hasStarted = true;

        Debug.Log($"▶▶▶ [{gameObject.name}] STARTING PLAYBACK ◀◀◀");

        // Fade in from black
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // Show skip hint
        if (skipHintUI != null)
        {
            skipHintUI.SetActive(true);
            Debug.Log($"[{gameObject.name}] Skip hint shown");
        }

        // Ensure timeline is at beginning
        if (myDirector != null)
        {
            myDirector.time = 0;
            myDirector.Evaluate();
            
            // Give it a frame
            yield return null;
            
            // Check if it's actually playing (Play On Awake should have started it)
            if (myDirector.state != PlayState.Playing && myDirector.playOnAwake)
            {
                Debug.LogWarning($"[{gameObject.name}] Not playing despite Play On Awake! Forcing play...");
                myDirector.Play();
                yield return null;
            }
            
            Debug.Log($"[{gameObject.name}] Timeline state: {myDirector.state}, Duration: {myDirector.duration}s");
        }

        // Wait for timeline to complete
        yield return StartCoroutine(WaitForTimelineCompletion());

        // Mark as finished
        hasFinished = true;

        Debug.Log($"■■■ [{gameObject.name}] FINISHED ■■■");

        // Hide skip hint
        if (skipHintUI != null)
        {
            skipHintUI.SetActive(false);
        }

        // Stop timeline explicitly
        if (myDirector != null && myDirector.state == PlayState.Playing)
        {
            myDirector.Stop();
        }

        // Transition to next
        yield return StartCoroutine(TransitionToNext());
    }

    private IEnumerator WaitForTimelineCompletion()
    {
        if (myDirector == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayableDirector is null!");
            yield break;
        }

        // Check if timeline has valid duration
        if (myDirector.duration <= 0 || myDirector.duration > 10000)
        {
            Debug.LogError($"[{gameObject.name}] INVALID DURATION: {myDirector.duration}s - Timeline Asset may not be assigned or is corrupt!");
            Debug.LogError($"[{gameObject.name}] Please assign a valid Timeline Asset in the Playable Director component!");
            yield return new WaitForSeconds(5f); // Wait 5 seconds so user can see the error
            yield break;
        }

        float timelineDuration = (float)myDirector.duration;
        float startRealTime = Time.realtimeSinceStartup;

        Debug.Log($"[{gameObject.name}] Waiting for timeline completion. Duration: {timelineDuration:F2}s");

        // Force play if not playing
        if (myDirector.state != PlayState.Playing)
        {
            Debug.LogWarning($"[{gameObject.name}] Timeline is NOT playing! State: {myDirector.state}. Forcing play...");
            myDirector.time = 0;
            myDirector.Play();
            yield return new WaitForSeconds(0.5f);
            
            if (myDirector.state != PlayState.Playing)
            {
                Debug.LogError($"[{gameObject.name}] FAILED TO START TIMELINE! Check Timeline Asset assignment!");
                yield break;
            }
        }

        float lastLogTime = 0f;
        
        while (Time.realtimeSinceStartup - startRealTime < timelineDuration + 2f)
        {
            // Check skip input
            if (allowSkip && CutsceneInput.GetSkipInput())
            {
                Debug.Log($"⏭⏭⏭ [{gameObject.name}] USER SKIPPED! ⏭⏭⏭");
                yield break;
            }

            // Get current state
            float currentTime = (float)myDirector.time;
            PlayState currentState = myDirector.state;

            // Log progress every 2 seconds
            if (Time.realtimeSinceStartup - lastLogTime > 2f)
            {
                Debug.Log($"[{gameObject.name}] Progress: {currentTime:F1}s / {timelineDuration:F1}s (State: {currentState})");
                lastLogTime = Time.realtimeSinceStartup;
            }

            // Check if timeline stopped playing (and we're past 1 second)
            if (currentState != PlayState.Playing && currentTime > 1f)
            {
                Debug.Log($"[{gameObject.name}] Timeline stopped at {currentTime:F2}s / {timelineDuration:F2}s");
                yield break;
            }

            // Check if we're at the end
            if (currentTime >= timelineDuration - 0.2f && currentTime > 1f)
            {
                Debug.Log($"[{gameObject.name}] Reached end: {currentTime:F2}s / {timelineDuration:F2}s");
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"[{gameObject.name}] Timeout reached after {timelineDuration}s");
    }

    private IEnumerator TransitionToNext()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Debug.Log($"[{gameObject.name}] Starting transition...");

        // Fade out
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        else
        {
            yield return new WaitForSeconds(delayBeforeNext);
        }

        // Play next timeline or load scene
        if (nextTimeline != null)
        {
            Debug.Log($"→→→ [{gameObject.name}] Triggering next timeline: {nextTimeline.name} ←←←");
            
            // Get the next timeline's script
            AutoPlayNextTimeline nextScript = nextTimeline.GetComponent<AutoPlayNextTimeline>();
            if (nextScript != null)
            {
                // Manually trigger the next timeline
                nextScript.StartCoroutine(nextScript.PlayTimelineSequence());
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Next timeline {nextTimeline.name} has no AutoPlayNextTimeline script!");
            }
        }
        else if (!string.IsNullOrEmpty(sceneToLoadAfter))
        {
            Debug.Log($"🎬🎬🎬 [{gameObject.name}] Loading scene: {sceneToLoadAfter} 🎬🎬🎬");
            yield return new WaitForSeconds(0.5f);
            anyTimelinePlaying = false;
            SceneManager.LoadScene(sceneToLoadAfter);
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No next action configured");
            anyTimelinePlaying = false;
        }

        isTransitioning = false;
    }

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

    // Public method to manually start this timeline
    public void PlayTimeline()
    {
        if (!hasStarted)
        {
            Debug.Log($"[{gameObject.name}] PlayTimeline() called manually");
            StartCoroutine(PlayTimelineSequence());
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayTimeline() called but already started!");
        }
    }

    // Force finish this timeline and move to next
    public void ForceFinish()
    {
        if (!hasFinished)
        {
            Debug.Log($"🔨 [{gameObject.name}] Force finishing");
            hasFinished = true;
        }
    }

    private void OnDisable()
    {
        // Reset static flag when scene unloads
        anyTimelinePlaying = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (myDirector == null)
            myDirector = GetComponent<PlayableDirector>();
        
        if (myDirector != null && myDirector.extrapolationMode != DirectorWrapMode.Hold)
        {
            myDirector.extrapolationMode = DirectorWrapMode.Hold;
            UnityEditor.EditorUtility.SetDirty(myDirector);
        }
    }
#endif
}
