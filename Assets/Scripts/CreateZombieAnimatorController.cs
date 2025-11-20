using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

public class CreateZombieAnimatorController : MonoBehaviour
{
    [Header("Find Animations Automatically")]
    [Tooltip("Leave empty to auto-find, or drag the zombie FBX model here")]
    public GameObject zombieModel;
    
    [Header("Manual Animation Assignment (Optional)")]
    public AnimationClip zombieIdle;
    public AnimationClip zombieWalk;
    public AnimationClip zombieRun;
    public AnimationClip zombieAttack;
    public AnimationClip zombieScream;
    
    [Header("Controller Settings")]
    public string controllerName = "ZombieAnimatorController";
    public string savePath = "Assets/Materials/";
    
    [ContextMenu("Create Zombie Animator Controller")]
    public void CreateAnimatorController()
    {
#if UNITY_EDITOR
        Debug.Log("═══ CREATING ZOMBIE ANIMATOR CONTROLLER ═══");
        
        // Try to find animations if not assigned
        if (zombieIdle == null || zombieWalk == null || zombieRun == null)
        {
            FindAnimationsAutomatically();
        }
        
        // Validate we have the required animations
        if (zombieIdle == null || zombieWalk == null || zombieRun == null)
        {
            Debug.LogError("❌ Missing required animations! Please assign Idle, Walk, and Run animations.");
            EditorUtility.DisplayDialog("Error", "Cannot create controller without Idle, Walk, and Run animations!", "OK");
            return;
        }
        
        // Create the controller
        string fullPath = savePath + controllerName + ".controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(fullPath);
        
        if (controller == null)
        {
            Debug.LogError("❌ Failed to create Animator Controller!");
            return;
        }
        
        // Clear default layer
        controller.RemoveLayer(0);
        
        // Add parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("AttackTrigger", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("ScreamTrigger", AnimatorControllerParameterType.Trigger);
        
        Debug.Log("✓ Added parameters: Speed, AttackTrigger, ScreamTrigger");
        
        // Create Base Layer
        AnimatorControllerLayer baseLayer = new AnimatorControllerLayer
        {
            name = "Base Layer",
            defaultWeight = 1f,
            stateMachine = new AnimatorStateMachine()
        };
        baseLayer.stateMachine.name = "Base Layer";
        baseLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
        
        AssetDatabase.AddObjectToAsset(baseLayer.stateMachine, controller);
        controller.AddLayer(baseLayer);
        
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Create States
        AnimatorState idleState = CreateState(controller, rootStateMachine, "Zombie Idle", zombieIdle, new Vector3(300, 50, 0));
        AnimatorState walkState = CreateState(controller, rootStateMachine, "Zombie Walk", zombieWalk, new Vector3(300, 150, 0));
        AnimatorState runState = CreateState(controller, rootStateMachine, "Zombie Run", zombieRun, new Vector3(300, 250, 0));
        
        // Set default state
        rootStateMachine.defaultState = idleState;
        
        Debug.Log("✓ Created states: Idle, Walk, Run");
        
        // Create Transitions for movement
        // Idle <-> Walk
        CreateTransition(idleState, walkState, new AnimatorCondition[]
        {
            new AnimatorCondition { mode = AnimatorConditionMode.Greater, parameter = "Speed", threshold = 0.1f }
        }, 0.2f, false);
        
        CreateTransition(walkState, idleState, new AnimatorCondition[]
        {
            new AnimatorCondition { mode = AnimatorConditionMode.Less, parameter = "Speed", threshold = 0.1f }
        }, 0.2f, false);
        
        // Walk <-> Run
        CreateTransition(walkState, runState, new AnimatorCondition[]
        {
            new AnimatorCondition { mode = AnimatorConditionMode.Greater, parameter = "Speed", threshold = 1.5f }
        }, 0.2f, false);
        
        CreateTransition(runState, walkState, new AnimatorCondition[]
        {
            new AnimatorCondition { mode = AnimatorConditionMode.Less, parameter = "Speed", threshold = 1.5f }
        }, 0.2f, false);
        
        // Run -> Idle (direct transition)
        CreateTransition(runState, idleState, new AnimatorCondition[]
        {
            new AnimatorCondition { mode = AnimatorConditionMode.Less, parameter = "Speed", threshold = 0.1f }
        }, 0.2f, false);
        
        Debug.Log("✓ Created movement transitions");
        
        // Add Attack and Scream states if animations exist
        if (zombieAttack != null)
        {
            AnimatorState attackState = CreateState(controller, rootStateMachine, "Zombie Attack", zombieAttack, new Vector3(550, 100, 0));
            
            // AnyState -> Attack
            AnimatorStateTransition attackTransition = rootStateMachine.AddAnyStateTransition(attackState);
            attackTransition.AddCondition(AnimatorConditionMode.If, 0, "AttackTrigger");
            attackTransition.duration = 0.1f;
            attackTransition.hasExitTime = false;
            
            // Attack -> Idle (with exit time)
            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.9f;
            attackToIdle.duration = 0.1f;
            
            Debug.Log("✓ Created Attack state and transitions");
        }
        
        if (zombieScream != null)
        {
            AnimatorState screamState = CreateState(controller, rootStateMachine, "Zombie Scream", zombieScream, new Vector3(550, 200, 0));
            
            // AnyState -> Scream
            AnimatorStateTransition screamTransition = rootStateMachine.AddAnyStateTransition(screamState);
            screamTransition.AddCondition(AnimatorConditionMode.If, 0, "ScreamTrigger");
            screamTransition.duration = 0.1f;
            screamTransition.hasExitTime = false;
            
            // Scream -> Idle (with exit time)
            AnimatorStateTransition screamToIdle = screamState.AddTransition(idleState);
            screamToIdle.hasExitTime = true;
            screamToIdle.exitTime = 0.9f;
            screamToIdle.duration = 0.1f;
            
            Debug.Log("✓ Created Scream state and transitions");
        }
        
        // Save everything
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>═══ ANIMATOR CONTROLLER CREATED ═══</color>");
        Debug.Log($"<color=green>✓ Saved to: {fullPath}</color>");
        Debug.Log($"<color=cyan>Now assign this controller to your zombie's Animator component!</color>");
        
        // Select the created controller
        Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
        
        EditorUtility.DisplayDialog("Success!", 
            $"Animator Controller created successfully!\n\nLocation: {fullPath}\n\nNow assign it to your zombie's Animator component.", 
            "OK");
#else
        Debug.LogError("This only works in the Unity Editor!");
#endif
    }
    
#if UNITY_EDITOR
    void FindAnimationsAutomatically()
    {
        Debug.Log("Searching for zombie animations...");
        
        // Search in Enemy folder
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Enemy" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip != null)
            {
                string clipName = clip.name.ToLower();
                
                if (clipName.Contains("idle") && zombieIdle == null)
                {
                    zombieIdle = clip;
                    Debug.Log($"✓ Found Idle: {clip.name}");
                }
                else if (clipName.Contains("walk") && zombieWalk == null)
                {
                    zombieWalk = clip;
                    Debug.Log($"✓ Found Walk: {clip.name}");
                }
                else if (clipName.Contains("run") && zombieRun == null)
                {
                    zombieRun = clip;
                    Debug.Log($"✓ Found Run: {clip.name}");
                }
                else if (clipName.Contains("attack") && zombieAttack == null)
                {
                    zombieAttack = clip;
                    Debug.Log($"✓ Found Attack: {clip.name}");
                }
                else if (clipName.Contains("scream") && zombieScream == null)
                {
                    zombieScream = clip;
                    Debug.Log($"✓ Found Scream: {clip.name}");
                }
            }
        }
    }
    
    AnimatorState CreateState(AnimatorController controller, AnimatorStateMachine stateMachine, string name, AnimationClip clip, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(name, position);
        state.motion = clip;
        state.writeDefaultValues = true;
        
        return state;
    }
    
    void CreateTransition(AnimatorState from, AnimatorState to, AnimatorCondition[] conditions, float duration, bool hasExitTime)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        
        foreach (AnimatorCondition condition in conditions)
        {
            transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
        }
        
        transition.duration = duration;
        transition.hasExitTime = hasExitTime;
        transition.exitTime = 0;
        transition.offset = 0;
    }
#endif
}
