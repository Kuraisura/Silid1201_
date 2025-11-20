using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Diagnostic tool to check Timeline setup
/// Add this temporarily to see what's happening
/// </summary>
public class TimelineDiagnostic : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== TIMELINE DIAGNOSTIC ===");
        
        // Find all Timeline-related objects
        var directors = FindObjectsByType<PlayableDirector>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {directors.Length} PlayableDirector(s) in scene:");
        
        foreach (var director in directors)
        {
            Debug.Log($"\n--- {director.gameObject.name} ---");
            Debug.Log($"  Play On Awake: {director.playOnAwake}");
            Debug.Log($"  Duration: {director.duration}s");
            Debug.Log($"  Wrap Mode: {director.extrapolationMode}");
            Debug.Log($"  State: {director.state}");
            Debug.Log($"  Has Timeline Asset: {(director.playableAsset != null ? "YES" : "NO")}");
            
            var autoPlay = director.GetComponent<AutoPlayNextTimeline>();
            if (autoPlay != null)
            {
                Debug.Log($"  Has AutoPlayNextTimeline: YES");
                Debug.Log($"    Next Timeline: {(autoPlay.nextTimeline != null ? autoPlay.nextTimeline.name : "None")}");
                Debug.Log($"    Scene To Load: {(string.IsNullOrEmpty(autoPlay.sceneToLoadAfter) ? "None" : autoPlay.sceneToLoadAfter)}");
                Debug.Log($"    Allow Skip: {autoPlay.allowSkip}");
            }
            else
            {
                Debug.Log($"  Has AutoPlayNextTimeline: NO");
            }
            
            // Check for conflicting scripts
            var sequentialManager = director.GetComponent<SequentialCutsceneManager>();
            if (sequentialManager != null)
            {
                Debug.LogWarning($"  ⚠️ CONFLICT: Has SequentialCutsceneManager! Remove this script!");
            }
            
            var cutsceneManager = director.GetComponent<CutsceneManager>();
            if (cutsceneManager != null)
            {
                Debug.LogWarning($"  ⚠️ CONFLICT: Has CutsceneManager! Remove this script!");
            }
        }
        
        Debug.Log("\n=== END DIAGNOSTIC ===\n");
    }
}
