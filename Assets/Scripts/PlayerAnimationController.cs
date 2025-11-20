using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    
    [Header("Animation Parameters")]
    private readonly string SPEED_PARAM = "Speed";
    // Commented out unused parameters to avoid warnings
    // private readonly string IS_CROUCHING_PARAM = "IsCrouching";
    // private readonly string IS_RUNNING_PARAM = "IsRunning";
    
    private BetterPlayerMovement playerMovement;
    private CharacterController characterController;
    
    void Start()
    {
        // Get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Get player movement
        playerMovement = GetComponent<BetterPlayerMovement>();
        characterController = GetComponent<CharacterController>();
        
        if (animator == null)
        {
            Debug.LogError("No Animator found on Player!");
        }
        
        Debug.Log("✓ Player Animation Controller initialized");
    }
    
    void Update()
    {
        if (animator == null) return;
        
        UpdateAnimations();
    }
    
    void UpdateAnimations()
    {
        // Calculate movement speed
        float horizontalSpeed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;
        
        // Try to set speed parameter (check if it exists first)
        try
        {
            animator.SetFloat(SPEED_PARAM, horizontalSpeed);
        }
        catch
        {
            // Parameter doesn't exist, that's okay - animations will work without it
        }
        
        // You can also use boolean parameters if your animator uses them
        // Uncomment these if you have these parameters in your Animator:
        
        // animator.SetBool(IS_RUNNING_PARAM, horizontalSpeed > 5f);
        // animator.SetBool(IS_CROUCHING_PARAM, /* check if crouching */);
        
        // Debug (optional)
        // Debug.Log($"Speed: {horizontalSpeed:F2}");
    }
}
