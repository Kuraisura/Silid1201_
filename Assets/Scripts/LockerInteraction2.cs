using UnityEngine;

public class LockerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionDistance = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Animation")]
    public Animator lockerAnimator;
    public string openAnim = "OpenLocker";
    public string closeAnim = "CloseLocker";

    private bool isOpen = false;
    private Transform player;
    [Header("Debug")]
    public bool drawGizmo = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Add the Player tag.");
            enabled = false;
            return;
        }

        if (lockerAnimator == null)
            lockerAnimator = GetComponent<Animator>();

        // Try to initialize isOpen based on animator's current state
        if (lockerAnimator != null)
        {
            var state = lockerAnimator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(openAnim))
                isOpen = true;
            else if (state.IsName(closeAnim))
                isOpen = false;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (Input.GetKeyDown(interactKey))
            {
                ToggleLocker();
            }
        }
    }

    void ToggleLocker()
    {
        if (lockerAnimator == null)
        {
            Debug.LogWarning($"LockerInteraction: Animator missing on '{gameObject.name}'. Cannot toggle.");
            return;
        }

        Debug.Log($"LockerInteraction: Toggling locker '{gameObject.name}'. Currently open={isOpen}");

        if (isOpen)
        {
            // Try to set trigger named closeAnim first if exists
            if (lockerAnimator.HasState(0, Animator.StringToHash(closeAnim)))
                lockerAnimator.Play(closeAnim);
            else
                lockerAnimator.SetTrigger(closeAnim);
        }
        else
        {
            if (lockerAnimator.HasState(0, Animator.StringToHash(openAnim)))
                lockerAnimator.Play(openAnim);
            else
                lockerAnimator.SetTrigger(openAnim);
        }

        isOpen = !isOpen;
    }

    /// <summary>
    /// External public method so UI/interaction managers or the new Input System can call to toggle the locker.
    /// </summary>
    public void Interact()
    {
        ToggleLocker();
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
