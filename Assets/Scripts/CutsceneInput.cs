using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Input helper for cutscene skip functionality
/// Works with both Input Systems
/// </summary>
public class CutsceneInput : MonoBehaviour
{
    public static bool GetSkipInput()
    {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System only
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            return UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame || 
                   UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        return false;
        #else
        // Legacy Input or Both
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space);
        #endif
    }
}
