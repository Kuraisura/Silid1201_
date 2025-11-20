using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HideableLocker : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 2f;

    [Header("Animation")]
    public Animator lockerAnimator;
    public string openAnim = "LockerOpen";
    public string closeAnim = "LockerClose";

    [Header("Hide Settings")]
    public Transform hidePosition;         // Ideal camera position inside locker
    public Transform hideCameraLookPoint;  // Where the camera should look when hiding
    public bool autoCreatePositions = true;

    [Header("UI")]
    public GameObject interactionUI;
    public TMPro.TMP_Text interactionText;
    public string hideText = "Press E to Hide";
    public string exitText = "Press E to Exit";

    private Transform player;
    private Camera playerCam;
    private CharacterController controller;
    private BetterPlayerMovement movement;

    private bool isHiding = false;
    private bool playerInRange = false;

    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Add Player tag.");
            enabled = false;
            return;
        }

        // Find locker animator if not assigned
        if (lockerAnimator == null)
            lockerAnimator = GetComponent<Animator>();

        movement = player.GetComponent<BetterPlayerMovement>();
        controller = player.GetComponent<CharacterController>();
        playerCam = Camera.main;

        // Auto-generate hide positions
        if (autoCreatePositions)
        {
            // Create hide position if empty
            if (hidePosition == null)
            {
                GameObject hidePosObject = new GameObject("HidePosition");
                hidePosObject.transform.SetParent(transform);
                hidePosObject.transform.localPosition = new Vector3(0, 1.1f, 0.25f);
                hidePosObject.transform.localRotation = Quaternion.identity;
                hidePosition = hidePosObject.transform;
            }

            // Create look point if empty
            if (hideCameraLookPoint == null)
            {
                GameObject lookPoint = new GameObject("HideLookPoint");
                lookPoint.transform.SetParent(transform);
                lookPoint.transform.localPosition = new Vector3(0, 1.3f, -1f);
                hideCameraLookPoint = lookPoint.transform;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (hidePosition != null)
            Gizmos.DrawSphere(hidePosition.position, 0.15f);

        Gizmos.color = Color.blue;
        if (hideCameraLookPoint != null)
            Gizmos.DrawSphere(hideCameraLookPoint.position, 0.15f);
    }
}
