using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform playerCamera;
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;
    private bool isFrozen = false;
    private Rigidbody rb;
    private Camera cam;
    [Header("Door Interaction")]
    public float doorInteractDistance = 1.5f;
    private HashSet<int> openedDoors = new HashSet<int>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }

        cam = playerCamera.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (playerCamera == null || cam == null) return;

        isFrozen = IsVisibleToCamera();
    }

    void FixedUpdate()
    {
        // If GameManager exists and gameplay is not active yet (start delay or caught), don't move
        if (GameManager.Instance != null && !GameManager.Instance.gameplayActive)
            return;
        // MOVEMENT LOGIC: Only move when not visible to camera
        if (!isFrozen && playerCamera != null)
        {
            Vector3 direction = (playerCamera.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, playerCamera.position);
            if (distance > stopDistance)
            {
                rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);
            }
        }

        // Attempt to interact with nearby doors (open them) so the enemy can move through
        TryOpenNearbyDoor();
    }

    private void TryOpenNearbyDoor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, doorInteractDistance);
        foreach (var col in hits)
        {
            var door = col.GetComponentInParent<DoorInteraction>();
            if (door == null) continue;

            int id = door.GetInstanceID();
            if (openedDoors.Contains(id)) continue;

            // Attempt to interact (DoorInteraction has safety checks)
            Debug.Log($"Enemy: attempting to Interact with door '{door.gameObject.name}'");
            door.Interact();
            openedDoors.Add(id);
            // Only open one door per FixedUpdate
            break;
        }
    }

    // If the enemy physically touches the player, trigger caught event
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerCaught();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerCaught();
        }
    }

    bool IsVisibleToCamera()
    {
        // 1. Check if enemy is in FRONT of the camera
        Vector3 toEnemy = transform.position - playerCamera.position;
        if (Vector3.Dot(playerCamera.forward, toEnemy.normalized) < 0f)
            return false;

        // 2. Check if enemy is within camera FOV
        float angle = Vector3.Angle(playerCamera.forward, toEnemy);
        if (angle > cam.fieldOfView * 0.6f) // *0.6 for a more natural edge
            return false;

        // 3. Check viewport boundaries
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.z > 0 &&
               viewportPos.x > 0 && viewportPos.x < 1 &&
               viewportPos.y > 0 && viewportPos.y < 1;
    }

}
