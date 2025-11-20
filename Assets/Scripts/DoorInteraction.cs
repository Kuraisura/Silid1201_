using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Attach to a Door GameObject that has an Animator with states named `DoorOpen` and `DoorClose`.
/// When the player presses E while within `interactDistance`, the script toggles between
/// opening and closing by playing the corresponding animator state once. While the
/// animation plays the script ignores additional input so the animation does not loop.
///
/// Usage:
/// - Add this component to the door GameObject that contains the Animator.
/// - Either assign `player` in inspector or make sure the player GameObject is tagged `Player` or named `Player`.
/// - Configure `interactDistance` to control how close the player must be.
/// - Ensure the Animator has states matching `openStateName` / `closeStateName`.
/// </summary>
[RequireComponent(typeof(Animator))]
public class DoorInteraction : MonoBehaviour
{
    [Tooltip("Reference to the player's transform. If left empty we try to find GameObject tagged 'Player' or named 'Player'.")]
    public Transform player;

    [Tooltip("Maximum distance from door to allow interaction.")]
    public float interactDistance = 2.0f;

    [Tooltip("Animator state name used for opening the door.")]
    public string openStateName = "DoorOpen";

    [Tooltip("Animator state name used for closing the door.")]
    public string closeStateName = "DoorClose";

    [Tooltip("Animator Trigger parameter name to fire when opening. If empty or missing, script will fall back to playing the state directly.")]
    public string openTriggerName = "Open";

    [Tooltip("Animator Trigger parameter name to fire when closing. If empty or missing, script will fall back to playing the state directly.")]
    public string closeTriggerName = "Close";

    [Tooltip("UI TextMeshPro element for interaction prompt.")]
    public TMP_Text interactionPrompt;

    // layer 0 by default
    private Animator animator;
    private HorrorGameUIManager uiManager;
    private bool busy = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    uiManager = Object.FindFirstObjectByType<HorrorGameUIManager>();

        if (player == null)
        {
            var byTag = GameObject.FindWithTag("Player");
            if (byTag != null)
                player = byTag.transform;
            else
            {
                var byName = GameObject.Find("Player");
                if (byName != null)
                    player = byName.transform;
            }
        }
        if (player == null)
            Debug.LogWarning($"DoorInteraction on '{gameObject.name}' has no Player assigned and automatic lookup failed. Please assign Player Transform or tag the player 'Player'.");
        else
            Debug.Log($"DoorInteraction: found player '{player.name}' for door '{gameObject.name}'");

        // If animator exists but has no controller assigned, attempt an editor-only auto-assign
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            TryAutoAssignAnimatorController();
        }

        // If we have a controller, ensure the door starts in the closed state so it doesn't "open on its own".
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                // Try to find the close state across all layers
                int closeLayer = -1;
                int layerCount = animator.layerCount;
                int closeHash = Animator.StringToHash(closeStateName);
                for (int i = 0; i < layerCount; i++)
                {
                    if (animator.HasState(i, closeHash))
                    {
                        closeLayer = i;
                        break;
                    }
                }

                if (closeLayer != -1)
                {
                    animator.Play(closeStateName, closeLayer, 0f);
                    animator.Update(0f);
                    Debug.Log($"DoorInteraction: forced initial animator state to '{closeStateName}' on '{gameObject.name}' (layer {closeLayer})");
                }
                else
                {
                    Debug.LogWarning($"DoorInteraction: could not find close state '{closeStateName}' on Animator for '{gameObject.name}'. Make sure the state name matches exactly.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"DoorInteraction: could not force initial state '{closeStateName}' on '{gameObject.name}': {ex.Message}");
            }
        }
    }

#if UNITY_EDITOR
    // Editor-only: try to find a likely AnimatorController in the project whose name contains "door"
    private void TryAutoAssignAnimatorController()
    {
        try
        {
            // Use AssetDatabase to find .controller assets
            var guids = UnityEditor.AssetDatabase.FindAssets("t:RuntimeAnimatorController");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (name.Contains("door"))
                {
                    var ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                    if (ctrl != null)
                    {
                        animator.runtimeAnimatorController = ctrl;
                        Debug.Log($"DoorInteraction: auto-assigned AnimatorController '{ctrl.name}' to '{gameObject.name}' (path: {path})");
                        return;
                    }
                }
            }

            Debug.LogWarning($"DoorInteraction: no AnimatorController with 'door' in the name was found to auto-assign for '{gameObject.name}'. Please assign one in the Inspector.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DoorInteraction: auto-assign failed: {ex}");
        }
    }
#endif

    // Ensure the script interacts with the specific door's Animator
    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        Debug.Log($"[DoorInteraction] Distance to player: {distance} for door: {gameObject.name}");
        // Remove all prompt activation/deactivation from this script
        if (distance <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!busy)
                {
                    StartCoroutine(ToggleDoorCoroutine());
                }
            }
        }
    }

    private IEnumerator ToggleDoorCoroutine()
    {
        busy = true;

        // Ensure animator has a controller
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"DoorInteraction: Animator on '{gameObject.name}' has no Controller assigned (or Animator is null). Assign a RuntimeAnimatorController in the Inspector.");
            busy = false;
            yield break;
        }

        // Read current state
        var state = animator.GetCurrentAnimatorStateInfo(0);

        // Determine target state
        string target = openStateName;
        if (state.IsName(openStateName))
        {
            target = closeStateName;
        }
        else if (state.IsName(closeStateName))
        {
            target = openStateName;
        }

        Debug.Log($"DoorInteraction: attempting to play animator state '{target}' on '{gameObject.name}' (layer 0)");

        // Search all layers for the state so the script works even if the state is on a non-default layer.
        int foundLayer = -1;
        int targetHash = Animator.StringToHash(target);
        int layerCount = animator.layerCount;
        for (int i = 0; i < layerCount; i++)
        {
            if (animator.HasState(i, targetHash))
            {
                foundLayer = i;
                break;
            }
        }

        if (foundLayer == -1)
        {
            Debug.LogError($"DoorInteraction: Animator on '{gameObject.name}' does not contain a state named '{target}' on any layer. Make sure the state name matches exactly (case-sensitive) or change Open/Close State Name on the component.");
            busy = false;
            yield break;
        }

        // Play the target state immediately at normalized time 0 on the found layer
        animator.Play(target, foundLayer, 0f);

        // Wait for animation to start and finish (with safety timeout)
        yield return StartCoroutine(WaitForAnimationToFinish(target, 5f));

        busy = false;
    }

    private IEnumerator WaitForAnimationToFinish(string stateName, float maxSeconds)
    {
        float timer = 0f;

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"DoorInteraction: WaitForAnimationToFinish called but Animator on '{gameObject.name}' has no Controller assigned.");
            yield break;
        }

        // Wait for animator to enter the named state (timeout fallback)
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && timer < maxSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (timer >= maxSeconds)
            yield break; // give up

        // Now wait until the state has played at least once (normalizedTime >= 1)
        timer = 0f;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && timer < maxSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"DoorInteraction: finished animation state '{stateName}' on '{gameObject.name}'");
        yield break;
    }

    /// <summary>
    /// Public method to allow external callers (new Input System, UI buttons) to trigger interaction.
    /// </summary>
    public void Interact()
    {
        if (player != null && !busy)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= interactDistance)
            {
                StartCoroutine(ToggleDoorCoroutine());
            }
        }
    }
}
