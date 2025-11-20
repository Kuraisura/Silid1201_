using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class SyncDoorInteraction
{
    [MenuItem("Tools/Door/Sync DoorInteraction State Names for Selected GameObject")]
    public static void SyncSelectedDoorInteraction()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("No GameObject selected. Select the Door GameObject in the Hierarchy and run this.");
            return;
        }

        var di = go.GetComponent<DoorInteraction>();
        var animator = go.GetComponent<Animator>();
        if (di == null || animator == null)
        {
            Debug.LogWarning("Selected GameObject must have both DoorInteraction and Animator components.");
            return;
        }

        var rac = animator.runtimeAnimatorController as AnimatorController;
        if (rac == null)
        {
            Debug.LogWarning("Animator does not have an AnimatorController asset assigned (or it's not an AnimatorController).");
            return;
        }

        string foundOpen = null;
        string foundClose = null;

        foreach (var layer in rac.layers)
        {
            var sm = layer.stateMachine;
            foreach (var child in sm.states)
            {
                var state = child.state;
                if (state.motion != null)
                {
                    var motionName = state.motion.name.ToLowerInvariant();
                    if (foundOpen == null && motionName.Contains("open"))
                        foundOpen = state.name;
                    if (foundClose == null && motionName.Contains("close"))
                        foundClose = state.name;
                }
            }
        }

        if (foundOpen != null)
        {
            di.openStateName = foundOpen;
            Debug.Log($"Synced openStateName to '{foundOpen}' on DoorInteraction of '{go.name}'");
        }
        else
        {
            Debug.LogWarning("Could not find a state whose motion name contains 'open'. Consider renaming the Animator states or set the value manually.");
        }

        if (foundClose != null)
        {
            di.closeStateName = foundClose;
            Debug.Log($"Synced closeStateName to '{foundClose}' on DoorInteraction of '{go.name}'");
        }
        else
        {
            Debug.LogWarning("Could not find a state whose motion name contains 'close'. Consider renaming the Animator states or set the value manually.");
        }

        EditorUtility.SetDirty(di);
    }
}
