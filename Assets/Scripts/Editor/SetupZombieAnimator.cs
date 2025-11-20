using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class SetupZombieAnimator : EditorWindow
{
    public AnimatorController controller;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationClip attackClip;
    public AnimationClip screamClip;
    
    [MenuItem("Tools/Setup Zombie Animator")]
    public static void ShowWindow()
    {
        GetWindow<SetupZombieAnimator>("Zombie Animator Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Zombie Animator Controller Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        controller = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false);
        
        GUILayout.Space(10);
        GUILayout.Label("Animation Clips:", EditorStyles.boldLabel);
        
        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle Animation", idleClip, typeof(AnimationClip), false);
        walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk Animation", walkClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run Animation", runClip, typeof(AnimationClip), false);
        attackClip = (AnimationClip)EditorGUILayout.ObjectField("Attack Animation", attackClip, typeof(AnimationClip), false);
        screamClip = (AnimationClip)EditorGUILayout.ObjectField("Scream Animation", screamClip, typeof(AnimationClip), false);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Create Animator Controller", GUILayout.Height(40)))
        {
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign an Animator Controller first!", "OK");
                return;
            }
            
            SetupAnimatorController();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create New Animator Controller", GUILayout.Height(30)))
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Animator Controller",
                "ZombieAnimatorController",
                "controller",
                "Please enter a file name to save the animator controller"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                SetupAnimatorController();
            }
        }
    }
    
    void SetupAnimatorController()
    {
        if (controller == null)
        {
            Debug.LogError("No Animator Controller assigned!");
            return;
        }
        
        // Clear existing layers and parameters
        controller.layers = new AnimatorControllerLayer[0];
        controller.parameters = new AnimatorControllerParameter[0];
        
        // Add Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("AttackTrigger", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("ScreamTrigger", AnimatorControllerParameterType.Trigger);
        
        // Create Base Layer
        AnimatorControllerLayer baseLayer = new AnimatorControllerLayer
        {
            name = "Base Layer",
            defaultWeight = 1f,
            stateMachine = new AnimatorStateMachine()
        };
        baseLayer.stateMachine.name = "Base Layer";
        baseLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
        
        if (AssetDatabase.GetAssetPath(controller) != "")
        {
            AssetDatabase.AddObjectToAsset(baseLayer.stateMachine, AssetDatabase.GetAssetPath(controller));
        }
        
        controller.AddLayer(baseLayer);
        
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Create States
        AnimatorState idleState = CreateState(rootStateMachine, "Idle", idleClip, new Vector3(300, 0, 0));
        AnimatorState walkState = CreateState(rootStateMachine, "Walk", walkClip, new Vector3(300, 100, 0));
        AnimatorState runState = CreateState(rootStateMachine, "Run", runClip, new Vector3(300, 200, 0));
        AnimatorState attackState = CreateState(rootStateMachine, "Attack", attackClip, new Vector3(550, 50, 0));
        AnimatorState screamState = CreateState(rootStateMachine, "Scream", screamClip, new Vector3(550, 150, 0));
        
        // Set default state
        rootStateMachine.defaultState = idleState;
        
        // Create Transitions
        
        // Idle <-> Walk
        CreateTransition(idleState, walkState, "Speed", AnimatorConditionMode.Greater, 0.1f);
        CreateTransition(walkState, idleState, "Speed", AnimatorConditionMode.Less, 0.1f);
        
        // Walk <-> Run
        CreateTransition(walkState, runState, "Speed", AnimatorConditionMode.Greater, 1.5f);
        CreateTransition(runState, walkState, "Speed", AnimatorConditionMode.Less, 1.5f);
        
        // Run -> Idle (direct)
        CreateTransition(runState, idleState, "Speed", AnimatorConditionMode.Less, 0.1f);
        
        // AnyState -> Attack
        AnimatorStateTransition attackTransition = rootStateMachine.AddAnyStateTransition(attackState);
        attackTransition.AddCondition(AnimatorConditionMode.If, 0, "AttackTrigger");
        attackTransition.duration = 0.1f;
        attackTransition.hasExitTime = false;
        attackTransition.exitTime = 0;
        
        // AnyState -> Scream
        AnimatorStateTransition screamTransition = rootStateMachine.AddAnyStateTransition(screamState);
        screamTransition.AddCondition(AnimatorConditionMode.If, 0, "ScreamTrigger");
        screamTransition.duration = 0.1f;
        screamTransition.hasExitTime = false;
        screamTransition.exitTime = 0;
        
        // Attack -> Idle (with exit time)
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;
        attackToIdle.duration = 0.1f;
        
        // Scream -> Idle (with exit time)
        AnimatorStateTransition screamToIdle = screamState.AddTransition(idleState);
        screamToIdle.hasExitTime = true;
        screamToIdle.exitTime = 0.9f;
        screamToIdle.duration = 0.1f;
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=green>✓ Zombie Animator Controller setup complete!</color>");
        EditorUtility.DisplayDialog("Success", "Animator Controller has been set up with all states and transitions!", "OK");
    }
    
    AnimatorState CreateState(AnimatorStateMachine stateMachine, string name, AnimationClip clip, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(name, position);
        state.motion = clip;
        state.writeDefaultValues = true;
        
        return state;
    }
    
    void CreateTransition(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.AddCondition(mode, threshold, parameter);
        transition.hasExitTime = false;
        transition.exitTime = 0;
        transition.duration = 0.2f;
        transition.offset = 0;
    }
}
