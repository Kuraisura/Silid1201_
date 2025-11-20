using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to an Enemy to provide a reliable "catch the player" trigger.
/// This script will detect the Player by collision, trigger, or proximity and call
/// GameManager.PlayerCaught(). If a GameManager instance is not present it will
/// create one so the caught flow still runs.
/// </summary>
public class EnemyCatcher : MonoBehaviour
{
    [Tooltip("Distance (meters) under which the enemy will consider the player caught.")]
    public float catchDistance = 1.0f;

    [Tooltip("Optional: layer mask for the player. If left empty, detection will use the 'Player' tag.")]
    public LayerMask playerLayer = ~0;

    [Tooltip("If true, the proximity check runs every FixedUpdate. If false, only collisions/triggers invoke catch.")]
    public bool useProximityCheck = true;

    // Prevent multiple catches
    private bool hasCaught = false;

    private Transform player;

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null)
            player = go.transform;
        else
        {
            var byName = GameObject.Find("Player");
            if (byName != null)
                player = byName.transform;
        }
    }

    void FixedUpdate()
    {
        if (hasCaught) return;
        if (!useProximityCheck) return;

        if (player == null) FindPlayer();
        if (player == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d <= catchDistance)
        {
            TriggerCaught("proximity");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasCaught) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            TriggerCaught("collision");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasCaught) return;
        if (other.gameObject.CompareTag("Player"))
        {
            TriggerCaught("trigger");
        }
    }

    void TriggerCaught(string reason)
    {
        if (hasCaught) return;
        hasCaught = true;

        Debug.Log($"EnemyCatcher: Player caught by '{gameObject.name}' (reason: {reason}).");

        // Ensure GameManager exists
        if (GameManager.Instance == null)
        {
            var gmObj = GameObject.Find("GameManager");
            if (gmObj == null)
            {
                Debug.Log("EnemyCatcher: GameManager instance not found; creating one.");
                gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }
            else
            {
                // If found but not active or missing component, add component
                if (gmObj.GetComponent<GameManager>() == null)
                {
                    gmObj.AddComponent<GameManager>();
                }
            }
        }

        // Call caught on GameManager (it will guard against double-calls)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCaught();
        }
        else
        {
            Debug.LogError("EnemyCatcher: Failed to obtain GameManager instance. Cannot invoke PlayerCaught().");
        }
    }
}
