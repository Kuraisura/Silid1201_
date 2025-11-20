using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Caught UI")]
    [Tooltip("Parent GameObject that contains the 'You Got Caught' TextMeshPro UI. If left empty, will try to Find('Caught') on Start.")]
    public GameObject caughtPanel;
    [Tooltip("TextMeshProUGUI element that will show the caught message and countdown. If left empty, will search inside caughtPanel.")]
    public TextMeshProUGUI caughtText;

    [Header("Caught Sound")]
    [Tooltip("Optional audio clip to play when the player is caught.")]
    public AudioClip caughtSound;
    [Tooltip("Optional AudioSource to play the caught sound. If left empty, PlayClipAtPoint will be used.")]
    public AudioSource caughtAudioSource;

    [Tooltip("Seconds before the scene is reloaded when the player is caught.")]
    public int restartSeconds = 3;

    [Header("Gameplay Start Delay")]
    [Tooltip("Seconds to wait at scene start before enemies are allowed to roam. Player can still move/hide during this time.")]
    public float gameStartDelay = 10f;

    // True when enemies and normal gameplay are active
    [HideInInspector]
    public bool gameplayActive = false;

    [Tooltip("When true, the player's movement component (BetterPlayerMovement) will be disabled while the restart/countdown runs.")]
    public bool disablePlayerOnCaught = true;

    bool isRestarting = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Listen for scene loads so we can re-bind UI and re-enable player movement after a restart
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads (including reload), try to re-find UI and reset state
        caughtPanel = GameObject.Find("Caught");
        if (caughtPanel != null)
        {
            caughtText = caughtPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            caughtPanel.SetActive(false);
        }

        // Re-enable player movement if it was disabled previously (new Player instance)
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && disablePlayerOnCaught)
        {
            var move = playerObj.GetComponent<BetterPlayerMovement>();
            if (move != null) move.enabled = true;
        }

        // Restart the gameplay start delay for the new scene
        gameplayActive = false;
        isRestarting = false;
        if (gameStartDelay > 0f)
            StartCoroutine(StartGameplayAfterDelay());
        else
            gameplayActive = true;
    }

    void Start()
    {
        // Auto-find the panel if not assigned in inspector
        if (caughtPanel == null)
        {
            // Try exact name first
            caughtPanel = GameObject.Find("Caught");
            if (caughtPanel == null)
            {
                // Search all transforms (including inactive) for a name containing "caught" (case-insensitive)
                var allTransforms = FindObjectsOfType<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t.name.ToLowerInvariant().Contains("caught"))
                    {
                        caughtPanel = t.gameObject;
                        Debug.Log($"GameManager: auto-found caught panel by name '{t.name}'");
                        break;
                    }
                }
            }
        }

        if (caughtPanel != null)
        {
            if (caughtText == null)
                caughtText = caughtPanel.GetComponentInChildren<TextMeshProUGUI>(true);

            caughtPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: 'Caught' panel not assigned and not found in scene. Assign it in the inspector.");
        }

        if (caughtPanel != null)
        {
            // If the caughtText is still null, try to locate a TMP text child whose text contains 'YOU GOT CAUGHT' or name contains 'caught'
            if (caughtText == null)
            {
                var tmps = caughtPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t.name.ToLowerInvariant().Contains("caught") || (t.text != null && t.text.ToUpperInvariant().Contains("YOU GOT CAUGHT")))
                    {
                        caughtText = t;
                        Debug.Log($"GameManager: auto-assigned caughtText from child '{t.name}'");
                        break;
                    }
                }
                // fallback to first TMP child
                if (caughtText == null && tmps.Length > 0)
                {
                    caughtText = tmps[0];
                    Debug.Log($"GameManager: assigned first TMP child '{caughtText.name}' as caughtText");
                }
            }
        }

        // Begin the gameplay start delay (enemies will remain frozen until this completes)
        gameplayActive = false;
        if (gameStartDelay > 0f)
            StartCoroutine(StartGameplayAfterDelay());
        else
            gameplayActive = true;
    }

    IEnumerator StartGameplayAfterDelay()
    {
        float remaining = gameStartDelay;
        while (remaining > 0f)
        {
            // Could update a start UI here if desired in the future
            yield return new WaitForSecondsRealtime(1f);
            remaining -= 1f;
        }

        gameplayActive = true;
    }

    /// <summary>
    /// Call this when the enemy catches the player.
    /// Shows the caught UI, runs a restart countdown, and reloads the active scene.
    /// </summary>
    public void PlayerCaught()
    {
        if (isRestarting) return;
        // Freeze gameplay (stop enemies) and optionally disable player movement
        gameplayActive = false;
        Debug.Log("GameManager: PlayerCaught() called - showing caught UI and starting restart countdown.");
        if (disablePlayerOnCaught)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var move = playerObj.GetComponent<BetterPlayerMovement>();
                if (move != null) move.enabled = false;
            }
        }

        // Try to make the UI visible on top of other canvases
        if (caughtPanel != null)
        {
            // If panel contains a Canvas component, force override sorting
            var canvas = caughtPanel.GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 10000;
            }

            // If there is a CanvasGroup, ensure it's visible
            var cg = caughtPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            caughtPanel.SetActive(true);
            // Bring to front in hierarchy if possible
            caughtPanel.transform.SetAsLastSibling();
        }

        // Play caught sound if assigned
        if (caughtSound != null)
        {
            if (caughtAudioSource != null)
            {
                try
                {
                    caughtAudioSource.PlayOneShot(caughtSound);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"GameManager: failed to play caughtAudioSource.PlayOneShot: {ex.Message}");
                }
            }
            else
            {
                // Fallback: play at main camera position so it's audible to the player
                var cam = Camera.main;
                if (cam != null)
                    AudioSource.PlayClipAtPoint(caughtSound, cam.transform.position);
                else
                    AudioSource.PlayClipAtPoint(caughtSound, Vector3.zero);
            }
        }

        StartCoroutine(RestartSequence());
    }

    IEnumerator RestartSequence()
    {
        isRestarting = true;

        if (caughtPanel != null)
            caughtPanel.SetActive(true);

        if (caughtText == null && caughtPanel != null)
            caughtText = caughtPanel.GetComponentInChildren<TextMeshProUGUI>(true);

        if (caughtText != null)
        {
            caughtText.text = "YOU GOT CAUGHT!\nRestarting in " + restartSeconds + "...";
        }
        else
        {
            Debug.LogWarning("GameManager: No TextMeshProUGUI found in caughtPanel to show message/countdown.");
        }

        // Use realtime so this works even if timeScale changes elsewhere
        for (int i = restartSeconds; i > 0; i--)
        {
            if (caughtText != null)
                caughtText.text = $"YOU GOT CAUGHT!\nRestarting in {i}...";
            yield return new WaitForSecondsRealtime(1f);
        }

        // Reload the active scene
        // Before reloading, re-enable player movement on the new scene (singleton persists)
        // The Awake() logic will ensure duplicate GameManager is not created.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
