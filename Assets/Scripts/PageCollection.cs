using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PageCollection : MonoBehaviour
{
    [Tooltip("List of images for the pages.")]
    public List<Sprite> pageImages;

    [Tooltip("UI Text element to display pages collected count.")]
    public TMP_Text pagesCollectedText;

    [Tooltip("Number of pages to collect.")]
    public int totalPages = 8;

    [Tooltip("Player transform for interaction checks.")]
    public Transform player;

    [Tooltip("Distance required to interact with a page.")]
    public float interactDistance = 2f;

    [Tooltip("UI TextMeshPro element for interaction prompt.")]
    public TMP_Text interactionPrompt;

    private int pagesCollected = 0;

    // Placeholders named Page1, Page2, ... PageN in the scene
    private List<GameObject> pagePlaceholders;

    private HorrorGameUIManager uiManager;
    [Header("Escape Video")]
    [Tooltip("GameObject that contains the video UI (TaposNaSir). Should be a child of the Horror Game Canvas and cover the canvas).")]
    public GameObject taposNaSir; // Assign in Inspector
    [Tooltip("VideoPlayer component on the TaposNaSir GameObject (optional). If null, the GameObject is simply enabled.")]
    public UnityEngine.Video.VideoPlayer taposNaSirVideoPlayer; // Assign in Inspector

    private void Start()
    {
        uiManager = Object.FindFirstObjectByType<HorrorGameUIManager>();

        // Ensure player transform assigned
        if (player == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        // Find placeholders named Page1..PageN in the scene and collect them in order
        pagePlaceholders = new List<GameObject>();
        for (int i = 1; i <= totalPages; i++)
        {
            var go = GameObject.Find("Page" + i);
            if (go != null) pagePlaceholders.Add(go);
            else
            {
                Debug.LogError($"PageCollection: Missing placeholder GameObject named 'Page{ i }' in the scene.");
            }
        }

        if (pagePlaceholders.Count < totalPages)
        {
            Debug.LogError("Not all page placeholders (Page1..PageN) were found in the scene. Aborting page placement.");
            return;
        }

        // Place pages onto the placeholders (in order)
        PlacePagesOnPlaceholders();

        // Update UI
        UpdatePagesCollectedText();

        // Ensure the TaposNaSir video UI starts hidden
        if (taposNaSir != null)
        {
            taposNaSir.SetActive(false);
        }
    }

    // Place pages on the scene placeholders Page1..PageN (not randomized)
    private void PlacePagesOnPlaceholders()
    {
        for (int i = 0; i < totalPages; i++)
        {
            GameObject placeholder = pagePlaceholders[i];

            // If the placeholder already has a SpriteRenderer, just set the sprite
            var sr = placeholder.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (i < pageImages.Count && pageImages[i] != null)
                {
                    sr.sprite = pageImages[i];
                    Debug.Log($"Page {i + 1}: Assigned image '{pageImages[i].name}' to placeholder '{placeholder.name}'.");
                }
                else
                {
                    Debug.LogWarning($"Page {i + 1}: No image available in pageImages list.");
                }

                // Add interaction script to the placeholder itself if not present
                var existing = placeholder.GetComponent<PageInteraction>();
                if (existing == null)
                {
                    var pi = placeholder.AddComponent<PageInteraction>();
                    pi.pageCollection = this;
                }

                continue;
            }

            // Otherwise create a quad under the placeholder and set texture as before
            GameObject page = GameObject.CreatePrimitive(PrimitiveType.Quad);
            page.name = "PageQuad";
            page.transform.SetParent(placeholder.transform, false);
            page.transform.localPosition = Vector3.zero + Vector3.forward * 0.01f;
            page.transform.localRotation = Quaternion.identity;

            var mr = page.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Unlit/Texture"));
                if (i < pageImages.Count && pageImages[i] != null)
                {
                    mat.mainTexture = pageImages[i].texture;
                    Debug.Log($"Page {i + 1}: Assigned image '{pageImages[i].name}' to placeholder '{placeholder.name}'.");

                    float aspect = (float)pageImages[i].texture.width / Mathf.Max(1f, pageImages[i].texture.height);
                    page.transform.localScale = new Vector3(aspect, 1f, 1f);
                }
                else
                {
                    Debug.LogWarning($"Page {i + 1}: No image available in pageImages list.");
                }
                mr.material = mat;
            }

            var mc = page.GetComponent<MeshCollider>();
            if (mc != null) Destroy(mc);
            var box = page.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1f, 1f, 0.1f);

            PageInteraction interaction = page.AddComponent<PageInteraction>();
            interaction.pageCollection = this;
        }
    }

    public void CollectPage()
    {
        pagesCollected++;
        UpdatePagesCollectedText();

        if (pagesCollected >= totalPages)
        {
            OnPlayerEscaped();
        }
    }

    private void OnPlayerEscaped()
    {
        // When the player has collected all pages, show the TaposNaSir UI and play its video (if assigned)
        if (taposNaSir != null)
        {
            taposNaSir.SetActive(true);
            if (taposNaSirVideoPlayer != null)
            {
                try
                {
                    taposNaSirVideoPlayer.Play();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to Play video on taposNaSirVideoPlayer: {ex.Message}");
                }
            }
        }
        else
        {
            Debug.Log("Player escaped but 'taposNaSir' GameObject is not assigned in PageCollection.");
        }
    }

    private void UpdatePagesCollectedText()
    {
        if (pagesCollectedText != null)
        {
            pagesCollectedText.text = $"{pagesCollected}/{totalPages}";
        }
    }
}

public class PageInteraction : MonoBehaviour
{
    public PageCollection pageCollection;
    private bool canInteract = false;
    private TMP_Text interactionPrompt;
    private bool collected = false;

    private void Start()
    {
        interactionPrompt = pageCollection.interactionPrompt;
    }

    private void Update()
    {
        if (pageCollection == null || pageCollection.player == null)
            return;

        float distance = Vector3.Distance(pageCollection.player.position, transform.position);
        canInteract = distance <= pageCollection.interactDistance;

        // Remove all prompt activation/deactivation from this script
        if (canInteract && !collected && Input.GetKeyDown(KeyCode.E))
        {
            collected = true;
            pageCollection.CollectPage();

            // If this object has a SpriteRenderer (placeholder case), hide the sprite instead of destroying the GameObject
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;

                // Disable any collider on the placeholder so it won't be interactable again
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // Disable this component to stop further processing
                this.enabled = false;
            }
            else
            {
                // For dynamically created quads we can destroy the object as before
                Destroy(gameObject);
            }
        }
    }
}