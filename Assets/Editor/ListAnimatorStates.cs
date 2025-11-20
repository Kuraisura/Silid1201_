using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

public static class ListAnimatorStates
{
    [MenuItem("Tools/Debug/List Door 1 Animator States")]
    public static void ListDoorStates()
    {
        // Try to find an AnimatorController asset named 'Door 1' (case-insensitive)
        var guids = AssetDatabase.FindAssets("t:AnimatorController Door 1");
        if (guids == null || guids.Length == 0)
        {
            // fallback: search all AnimatorController assets and find one with 'door' in the name
            guids = AssetDatabase.FindAssets("t:AnimatorController");
        }

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!name.ToLowerInvariant().Contains("door"))
                continue;

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) continue;

            Debug.Log($"Found AnimatorController '{controller.name}' at {path}");
            for (int layer = 0; layer < controller.layers.Length; layer++)
            {
                var l = controller.layers[layer];
                Debug.Log($" Layer {layer}: {l.name}");
                var root = l.stateMachine;
                ListStatesRecursive(root, "  ");
            }
        }
    }

    private static void ListStatesRecursive(AnimatorStateMachine sm, string indent)
    {
        foreach (var state in sm.states)
        {
            Debug.Log(indent + "State: " + state.state.name + " (motion: " + (state.state.motion != null ? state.state.motion.name : "null") + ")");
        }

        // Recurse into child state machines
        foreach (var child in sm.stateMachines)
        {
            Debug.Log(indent + "Sub-StateMachine: " + child.stateMachine.name);
            ListStatesRecursive(child.stateMachine, indent + "  ");
        }
    }
}
