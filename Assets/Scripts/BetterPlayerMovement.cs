using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class BetterPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 10f;
    
    [Header("Crouch Settings")]
    public float crouchCameraHeight = 0.5f; // How much to lower camera when crouching
    public float crouchTransitionSpeed = 10f; // How fast to crouch/stand
    
    [Header("Physics")]
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = -1; // All layers by default
    
    [Header("Collision Settings")]
    public float pushPower = 2f;
    public bool canPushRigidbodies = true;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    // Private variables
    private CharacterController characterController;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private float currentSpeed;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isCrouching;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private bool isCrouchToggled = false; // Track crouch toggle state
    private float originalCameraHeight; // Store original camera Y position
    private float targetCameraHeight; // Target camera Y for smooth transition

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found! Adding one...");
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 2f;
            characterController.center = new Vector3(0, 1, 0);
        }
        
        // Find camera if not assigned
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
            else
            {
                Debug.LogWarning("No camera found for player!");
            }
        }
        
        // Lock cursor for better gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentSpeed = walkSpeed;
        
        // Store original camera height
        if (cameraTransform != null)
        {
            originalCameraHeight = cameraTransform.localPosition.y;
            targetCameraHeight = originalCameraHeight;
        }
        
        Debug.Log($"<color=green>✓ BetterPlayerMovement initialized!</color>");
        Debug.Log($"CharacterController - Radius: {characterController.radius}, Height: {characterController.height}");
    }

    void Update()
    {
        // Ground check
        CheckGrounded();
        
        // Check sprint input directly (HOLD behavior)
        if (Keyboard.current != null)
        {
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }
        
        // Determine movement speed
        DetermineSpeed();
        
        // Handle movement
        HandleMovement();
        
        // Handle camera rotation
        HandleCameraRotation();
        
        // Handle crouch camera lowering
        HandleCrouchCamera();
        
        // Unlock cursor with Escape (using new Input System)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void CheckGrounded()
    {
        // More reliable ground check
        isGrounded = characterController.isGrounded;
        
        if (!isGrounded)
        {
            // Additional raycast check
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, groundLayer);
        }
        
        if (showDebugInfo && isGrounded)
        {
            Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.green);
        }
    }

    void DetermineSpeed()
    {
        // Sprint when holding shift (not crouching)
        if (isRunning && !isCrouchToggled)
        {
            currentSpeed = runSpeed;
        }
        else if (isCrouchToggled)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    void HandleMovement()
    {
        // Calculate move direction relative to player's forward direction
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Remove vertical component
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Calculate desired move direction
        Vector3 desiredMoveDirection = (forward * moveInput.y + right * moveInput.x);
        
        // Smooth acceleration
        moveDirection = Vector3.Lerp(moveDirection, desiredMoveDirection * currentSpeed, acceleration * Time.deltaTime);
        
        // Apply gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small force to keep grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        // Combine horizontal and vertical movement
        Vector3 finalMovement = moveDirection * Time.deltaTime;
        finalMovement.y = velocity.y * Time.deltaTime;
        
        // Apply movement
        CollisionFlags collisionFlags = characterController.Move(finalMovement);
        
        // Debug collision
        if (showDebugInfo && collisionFlags != CollisionFlags.None)
        {
            Debug.Log($"Collision detected: {collisionFlags}");
        }
    }

    void HandleCameraRotation()
    {
        if (cameraTransform == null) return;
        
        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
        
        // Vertical rotation (camera only)
        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
    
    void HandleCrouchCamera()
    {
        if (cameraTransform == null) return;
        
        // Set target camera height based on crouch state
        targetCameraHeight = isCrouchToggled ? originalCameraHeight - crouchCameraHeight : originalCameraHeight;
        
        // Smoothly move camera to target height
        Vector3 currentPos = cameraTransform.localPosition;
        currentPos.y = Mathf.Lerp(currentPos.y, targetCameraHeight, crouchTransitionSpeed * Time.deltaTime);
        cameraTransform.localPosition = currentPos;
    }

    // Handle collisions with objects
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>► Hit: {hit.gameObject.name}</color>");
            
            Collider col = hit.collider;
            string colliderType = col.GetType().Name;
            bool isTrigger = col.isTrigger;
            
            if (col is MeshCollider meshCol)
            {
                Debug.Log($"  Type: {colliderType} | Convex: {meshCol.convex} | Trigger: {isTrigger}");
            }
            else
            {
                Debug.Log($"  Type: {colliderType} | Trigger: {isTrigger}");
            }
        }
        
        // Push rigidbodies
        if (canPushRigidbodies)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            
            // Don't push kinematic rigidbodies or if no rigidbody
            if (body != null && !body.isKinematic)
            {
                Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
                body.linearVelocity = pushDir * pushPower;
            }
        }
    }

    // Input System callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        
        if (showDebugInfo && moveInput != Vector2.zero)
        {
            Debug.Log($"Move Input: {moveInput}");
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        // This callback is kept for compatibility but we check directly in Update()
        // to ensure proper HOLD behavior
        if (showDebugInfo)
        {
            Debug.Log($"Sprint Input Event: {value.isPressed}");
        }
    }

    public void OnCrouch(InputValue value)
    {
        // Crouch is now TOGGLE (press C to toggle)
        if (value.isPressed)
        {
            isCrouchToggled = !isCrouchToggled;
            if (showDebugInfo)
            {
                Debug.Log($"Crouch Toggled: {(isCrouchToggled ? "ON" : "OFF")}");
            }
        }
    }

    // Jump functionality removed - no jumping in this game

    // Draw collision capsule in editor
    void OnDrawGizmosSelected()
    {
        if (characterController == null) return;
        
        Gizmos.color = Color.green;
        Vector3 center = transform.position + characterController.center;
        float radius = characterController.radius;
        float height = characterController.height;
        
        // Draw capsule representation
        Gizmos.DrawWireSphere(center + Vector3.up * (height/2 - radius), radius);
        Gizmos.DrawWireSphere(center - Vector3.up * (height/2 - radius), radius);
    }
}
