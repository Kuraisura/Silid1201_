using UnityEngine;

/// <summary>
/// Battery pickup that restores flashlight battery
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class BatteryPickup : MonoBehaviour
{
    [Header("Battery Settings")]
    [Tooltip("Amount of battery to restore (0-100)")]
    [Range(0f, 100f)]
    public float batteryAmount = 50f;
    
    [Header("Visual")]
    public GameObject batteryModel;
    public bool rotateInPlace = true;
    public float rotationSpeed = 100f;
    public bool floatAnimation = true;
    public float floatHeight = 0.3f;
    public float floatSpeed = 3f;
    
    [Header("Effects")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    public Light pickupLight;
    public Color lightColor = Color.cyan;
    
    [Header("Interaction")]
    public float interactionRange = 2.5f;
    public bool autoPickup = true; // Auto-collect on trigger
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private bool isCollected = false;
    private Vector3 startPosition;
    private Transform playerTransform;
    private HorrorGameUI gameUI;
    private BoxCollider boxCollider;
    
    void Start()
    {
        startPosition = transform.position;
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Find UI
        gameUI = FindFirstObjectByType<HorrorGameUI>();
        
        // Setup light
        if (pickupLight == null)
        {
            GameObject lightObj = new GameObject("Pickup Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            pickupLight = lightObj.AddComponent<Light>();
            pickupLight.type = LightType.Point;
            pickupLight.range = 4f;
            pickupLight.intensity = 3f;
            pickupLight.color = lightColor;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>🔋 Battery Pickup (+{batteryAmount}%) initialized at {transform.position}</color>");
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Rotation animation
        if (rotateInPlace)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Float animation
        if (floatAnimation)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        // Pulsing light
        if (pickupLight != null)
        {
            pickupLight.intensity = 3f + Mathf.Sin(Time.time * 4f) * 0.5f;
        }
        
        // Check for player interaction (if not auto-pickup)
        if (!autoPickup && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= interactionRange)
            {
                // Show interaction prompt
                if (gameUI != null)
                {
                    gameUI.ShowInteractionPrompt($"[E] Collect Battery (+{batteryAmount}%)");
                }
                
                // Check for interaction key
                if (Input.GetKeyDown(interactKey))
                {
                    CollectBattery();
                }
            }
            else if (gameUI != null)
            {
                gameUI.HideInteractionPrompt();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        // Auto-collect when player enters trigger
        if (autoPickup && other.CompareTag("Player"))
        {
            CollectBattery();
        }
    }
    
    void CollectBattery()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Find player's battery system
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FlashlightBatterySystem batterySystem = player.GetComponent<FlashlightBatterySystem>();
            if (batterySystem != null)
            {
                batterySystem.AddBattery(batteryAmount);
            }
        }
        
        // Play effects
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Update UI
        if (gameUI != null)
        {
            gameUI.ShowWarning($"BATTERY COLLECTED +{batteryAmount}%", 2f);
            if (!autoPickup)
            {
                gameUI.HideInteractionPrompt();
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>✅ Battery Collected! +{batteryAmount}%</color>");
        }
        
        // Destroy pickup
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    /// <summary>
    /// Create a battery pickup at a specific position
    /// </summary>
    public static GameObject CreateBatteryPickup(Vector3 position, float amount = 50f)
    {
        GameObject battery = GameObject.CreatePrimitive(PrimitiveType.Cube);
        battery.name = "Battery Pickup";
        battery.transform.position = position;
        battery.transform.localScale = new Vector3(0.3f, 0.5f, 0.15f);
        
        // Make it yellow/cyan colored
        Renderer renderer = battery.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.cyan;
        }
        
        // Add battery pickup component
        BatteryPickup pickup = battery.AddComponent<BatteryPickup>();
        pickup.batteryAmount = amount;
        pickup.batteryModel = battery;
        
        return battery;
    }
}
