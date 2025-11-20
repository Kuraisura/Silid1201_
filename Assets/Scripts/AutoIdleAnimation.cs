using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to setup idle animations on character models
/// Attach to any GameObject with a SkinnedMeshRenderer to auto-setup
/// </summary>
public class AutoIdleAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The idle animation clip to play")]
    public AnimationClip idleAnimation;
    
    [Tooltip("Name of the animator controller to create/use")]
    public string animatorControllerName = "IdleController";
    
    [Header("Auto Setup")]
    [Tooltip("Automatically setup on start")]
    public bool autoSetup = true;

    private Animator animator;

    private void Start()
    {
        if (autoSetup)
        {
            SetupIdleAnimation();
        }
    }

    /// <summary>
    /// Setup idle animation on this character
    /// </summary>
    public void SetupIdleAnimation()
    {
        // Get or add Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
            Debug.Log($"Added Animator to {gameObject.name}");
        }

        // Check if controller is assigned
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Animator Controller assigned. Please assign one in the Inspector.");
            
#if UNITY_EDITOR
            // Try to find or create controller
            CreateAnimatorControllerInEditor();
#endif
        }

        // If idle animation is assigned, try to play it
        if (idleAnimation != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("Idle", 0, 0f);
            Debug.Log($"Playing idle animation on {gameObject.name}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Create animator controller in editor
    /// </summary>
    private void CreateAnimatorControllerInEditor()
    {
        // This will be called in editor mode
        Debug.Log($"Please create an Animator Controller for {gameObject.name} or use the editor menu.");
    }

    [ContextMenu("Setup Idle Animation")]
    private void SetupIdleAnimationMenu()
    {
        SetupIdleAnimation();
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Editor tool to setup idle animations on selected GameObjects
/// </summary>
public class IdleAnimationSetup : EditorWindow
{
    private AnimationClip idleClip;
    private bool applyToChildren = true;

    [MenuItem("Tools/Silid 12:01/Setup Idle Animations")]
    public static void ShowWindow()
    {
        GetWindow<IdleAnimationSetup>("Idle Animation Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Setup Idle Animations", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Add Animator components to selected GameObjects\n" +
            "2. Create a simple Animator Controller\n" +
            "3. Add the idle animation to the controller\n" +
            "4. Setup everything to play automatically", 
            MessageType.Info);

        GUILayout.Space(10);

        idleClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Idle Animation Clip:", 
            idleClip, 
            typeof(AnimationClip), 
            false);

        applyToChildren = EditorGUILayout.Toggle("Apply to Children", applyToChildren);

        GUILayout.Space(10);

        GUI.enabled = idleClip != null && Selection.gameObjects.Length > 0;

        if (GUILayout.Button("Setup Idle Animation", GUILayout.Height(40)))
        {
            SetupIdleAnimations();
        }

        GUI.enabled = true;

        GUILayout.Space(10);

        if (Selection.gameObjects.Length == 0)
        {
            EditorGUILayout.HelpBox("Please select one or more GameObjects in the Hierarchy", MessageType.Warning);
        }
        else if (idleClip == null)
        {
            EditorGUILayout.HelpBox("Please assign an Idle Animation Clip", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Ready to setup {Selection.gameObjects.Length} GameObject(s)", MessageType.Info);
        }
    }

    private void SetupIdleAnimations()
    {
        int count = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            SetupGameObject(go);
            count++;

            if (applyToChildren)
            {
                foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                {
                    if (child.gameObject != go)
                    {
                        SetupGameObject(child.gameObject);
                        count++;
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("Success!", 
            $"Idle animation setup completed on {count} GameObject(s)!", 
            "OK");
    }

    private void SetupGameObject(GameObject go)
    {
        // Add or get Animator
        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.AddComponent<Animator>();
            Debug.Log($"Added Animator to {go.name}");
        }

        // Create controller if needed
        if (animator.runtimeAnimatorController == null)
        {
            string controllerPath = $"Assets/Animations/Controllers/{go.name}_IdleController.asset";
            
            // Create directory if it doesn't exist
            if (!System.IO.Directory.Exists("Assets/Animations"))
            {
                System.IO.Directory.CreateDirectory("Assets/Animations");
            }
            if (!System.IO.Directory.Exists("Assets/Animations/Controllers"))
            {
                System.IO.Directory.CreateDirectory("Assets/Animations/Controllers");
            }

            // Create controller
            UnityEditor.Animations.AnimatorController controller = 
                UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Add idle state
            var stateMachine = controller.layers[0].stateMachine;
            var idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            stateMachine.defaultState = idleState;

            // Assign controller
            animator.runtimeAnimatorController = controller;

            Debug.Log($"Created Animator Controller for {go.name} at {controllerPath}");
        }

        EditorUtility.SetDirty(go);
    }
}
#endif
