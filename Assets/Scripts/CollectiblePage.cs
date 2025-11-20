using UnityEngine;

/// <summary>
/// Collectible page object - place around the map for players to find
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CollectiblePage : MonoBehaviour
{
    [Header("Page Settings")]
    public int pageNumber = 1;
    public string pageTitle = "Page 1";
    
    [Header("Visual")]
    public GameObject pageModel;
    public bool rotateInPlace = true;
    public float rotationSpeed = 50f;
    public bool floatAnimation = true;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    
    [Header("Effects")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    public Light pageLight;
    
    [Header("Interaction")]
    public float interactionRange = 3f;
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
        if (pageLight == null)
        {
            GameObject lightObj = new GameObject("Page Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            pageLight = lightObj.AddComponent<Light>();
            pageLight.type = LightType.Point;
            pageLight.range = 5f;
            pageLight.intensity = 2f;
            pageLight.color = Color.yellow;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>📄 Page {pageNumber} initialized at {transform.position}</color>");
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
        if (pageLight != null)
        {
            pageLight.intensity = 2f + Mathf.Sin(Time.time * 3f) * 0.5f;
        }
        
        // Check for player interaction
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= interactionRange)
            {
                // Show interaction prompt
                if (gameUI != null)
                {
                    gameUI.ShowInteractionPrompt($"[E] Collect {pageTitle}");
                }
                
                // Check for interaction key
                if (Input.GetKeyDown(interactKey))
                {
                    CollectPage();
                }
            }
            else if (gameUI != null)
            {
                gameUI.HideInteractionPrompt();
            }
        }
    }
    
    void CollectPage()
    {
        if (isCollected) return;
        
        isCollected = true;
        
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
            gameUI.CollectPage();
            gameUI.HideInteractionPrompt();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>✅ Collected {pageTitle}!</color>");
        }
        
        // Destroy page
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
