using UnityEngine;
using TMPro;

public class EscapeDoorInteraction : MonoBehaviour
{
    public PageCollection pageCollection;
    public TMP_Text escapedText;
    public int requiredPages = 8;
    public float interactDistance = 2f;
    public Transform player;
    public string escapeDoorTag = "EscapeDoor";
    private bool canEscape = false;
    private HorrorGameUIManager uiManager;

    private void Start()
    {
    uiManager = Object.FindFirstObjectByType<HorrorGameUIManager>();
        if (escapedText != null)
            escapedText.enabled = false;
        if (uiManager != null)
            uiManager.HideInteractionPrompt();
    }

    private void Update()
    {
        if (pageCollection == null || player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool hasAllPages = false;
        // Use the actual collected page count from PageCollection
        var pageCountField = pageCollection.GetType().GetField("pagesCollected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pageCountField != null)
        {
            int collected = (int)pageCountField.GetValue(pageCollection);
            hasAllPages = (collected >= requiredPages);
        }
        canEscape = hasAllPages && (distance <= interactDistance);

        if (distance <= interactDistance)
        {
            if (uiManager != null)
                uiManager.ShowInteractionPrompt("[E] Interact");
        }
        else
        {
            if (uiManager != null)
                uiManager.HideInteractionPrompt();
        }

        if (canEscape && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"EscapeDoorInteraction: Escaped triggered at {transform.name}.");
            if (escapedText != null)
            {
                escapedText.text = "Escaped!";
                escapedText.enabled = true;
            }
            if (uiManager != null)
                uiManager.HideInteractionPrompt();
            enabled = false;
        }
        else if (distance <= interactDistance && !hasAllPages && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"EscapeDoorInteraction: Tried to escape at {transform.name} but not all pages collected.");
        }
    }
}