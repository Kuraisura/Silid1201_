using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper to setup Timeline animations on characters
/// Attach this to a character to auto-configure Timeline
/// </summary>
public class TimelineCharacterSetup : MonoBehaviour
{
    [Header("Animation Clips")]
    [Tooltip("Animation clips to add to timeline")]
    public AnimationClip[] animationClips;
    
    [Header("Timeline Settings")]
    [Tooltip("Auto-create timeline on start")]
    public bool autoCreateTimeline = false;
    
    [Tooltip("Play timeline on start")]
    public bool playOnStart = true;
    
    [Tooltip("Loop the timeline")]
    public bool loopTimeline = true;

    private PlayableDirector director;
    private Animator animator;

    private void Start()
    {
        if (autoCreateTimeline)
        {
            SetupTimeline();
        }
    }

    /// <summary>
    /// Setup timeline and animator for this character
    /// </summary>
    public void SetupTimeline()
    {
        // Get or add Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name}: No Animator component found!");
            return;
        }

        // Check avatar is humanoid
        if (animator.avatar == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Avatar assigned to Animator!");
        }
        else if (!animator.avatar.isHuman)
        {
            Debug.LogWarning($"{gameObject.name}: Avatar is not Humanoid! Timeline works best with Humanoid avatars.");
        }

        // Configure animator for Timeline
        animator.applyRootMotion = true;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Get or add PlayableDirector
        director = GetComponent<PlayableDirector>();
        if (director == null)
        {
            director = gameObject.AddComponent<PlayableDirector>();
            Debug.Log($"Added PlayableDirector to {gameObject.name}");
        }

        // Configure director
        director.playOnAwake = playOnStart;
        director.extrapolationMode = loopTimeline ? 
            DirectorWrapMode.Loop : DirectorWrapMode.Hold;

        Debug.Log($"Timeline setup completed for {gameObject.name}!");
        Debug.Log($"- Animator: {(animator != null ? "✓" : "✗")}");
        Debug.Log($"- Avatar: {(animator.avatar != null ? "✓" : "✗")}");
        Debug.Log($"- Humanoid: {(animator.avatar != null && animator.avatar.isHuman ? "✓" : "✗")}");
        Debug.Log($"- PlayableDirector: {(director != null ? "✓" : "✗")}");
    }

    /// <summary>
    /// Play the timeline
    /// </summary>
    public void PlayTimeline()
    {
        if (director != null && director.playableAsset != null)
        {
            director.Play();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No timeline asset assigned!");
        }
    }

    /// <summary>
    /// Stop the timeline
    /// </summary>
    public void StopTimeline()
    {
        if (director != null)
        {
            director.Stop();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Setup Timeline")]
    private void SetupTimelineMenu()
    {
        SetupTimeline();
    }

    [ContextMenu("Verify Timeline Setup")]
    private void VerifySetup()
    {
        animator = GetComponent<Animator>();
        director = GetComponent<PlayableDirector>();

        Debug.Log("=== Timeline Setup Verification ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Animator: {(animator != null ? "✓ Found" : "✗ Missing")}");
        
        if (animator != null)
        {
            Debug.Log($"  - Avatar: {(animator.avatar != null ? "✓ Assigned" : "✗ Missing")}");
            if (animator.avatar != null)
            {
                Debug.Log($"  - Is Humanoid: {(animator.avatar.isHuman ? "✓ Yes" : "✗ No (Generic)")}");
                Debug.Log($"  - Is Valid: {(animator.avatar.isValid ? "✓ Yes" : "✗ No")}");
            }
            Debug.Log($"  - Apply Root Motion: {(animator.applyRootMotion ? "✓ Enabled" : "✗ Disabled")}");
            Debug.Log($"  - Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}");
        }

        Debug.Log($"PlayableDirector: {(director != null ? "✓ Found" : "✗ Missing")}");
        
        if (director != null)
        {
            Debug.Log($"  - Timeline Asset: {(director.playableAsset != null ? "✓ Assigned" : "✗ Missing")}");
            Debug.Log($"  - Play On Awake: {(director.playOnAwake ? "✓ Yes" : "✗ No")}");
            Debug.Log($"  - State: {director.state}");
        }

        Debug.Log("================================");
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Editor window to setup Timeline on selected characters
/// </summary>
public class TimelineSetupWindow : EditorWindow
{
    private bool applyToChildren = false;
    private bool enableRootMotion = true;
    private bool playOnAwake = true;

    [MenuItem("Tools/Silid 12:01/Setup Timeline Characters")]
    public static void ShowWindow()
    {
        GetWindow<TimelineSetupWindow>("Timeline Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Timeline Character Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will setup Timeline on selected characters:\n" +
            "✓ Configure Animator for Timeline\n" +
            "✓ Add PlayableDirector component\n" +
            "✓ Set proper settings for animation playback\n" +
            "✓ Verify Avatar is Humanoid", 
            MessageType.Info);

        GUILayout.Space(10);

        enableRootMotion = EditorGUILayout.Toggle("Enable Root Motion", enableRootMotion);
        playOnAwake = EditorGUILayout.Toggle("Play On Awake", playOnAwake);
        applyToChildren = EditorGUILayout.Toggle("Apply to Children", applyToChildren);

        GUILayout.Space(10);

        GUI.enabled = Selection.gameObjects.Length > 0;

        if (GUILayout.Button("Setup Timeline", GUILayout.Height(40)))
        {
            SetupSelectedCharacters();
        }

        GUI.enabled = true;

        GUILayout.Space(10);

        if (Selection.gameObjects.Length == 0)
        {
            EditorGUILayout.HelpBox("Select one or more characters in Hierarchy", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Ready to setup {Selection.gameObjects.Length} character(s)", MessageType.Info);
        }
    }

    private void SetupSelectedCharacters()
    {
        int successCount = 0;
        int errorCount = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            if (SetupCharacter(go))
                successCount++;
            else
                errorCount++;

            if (applyToChildren)
            {
                foreach (Animator animator in go.GetComponentsInChildren<Animator>(true))
                {
                    if (animator.gameObject != go)
                    {
                        if (SetupCharacter(animator.gameObject))
                            successCount++;
                        else
                            errorCount++;
                    }
                }
            }
        }

        string message = $"Timeline setup completed!\n\nSuccess: {successCount}\nErrors: {errorCount}";
        EditorUtility.DisplayDialog("Setup Complete", message, "OK");
    }

    private bool SetupCharacter(GameObject go)
    {
        // Get or verify Animator
        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{go.name}: No Animator component found. Skipping.");
            return false;
        }

        // Check Avatar
        if (animator.avatar == null)
        {
            Debug.LogWarning($"{go.name}: No Avatar assigned!");
            return false;
        }

        if (!animator.avatar.isHuman)
        {
            Debug.LogWarning($"{go.name}: Avatar is not Humanoid! Consider changing to Humanoid for best Timeline support.");
        }

        // Configure Animator
        animator.applyRootMotion = enableRootMotion;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Add or get PlayableDirector
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        if (director == null)
        {
            director = go.AddComponent<PlayableDirector>();
        }

        director.playOnAwake = playOnAwake;
        director.extrapolationMode = DirectorWrapMode.Loop;

        EditorUtility.SetDirty(go);
        Debug.Log($"✓ Timeline setup completed for {go.name}");

        return true;
    }
}
#endif
