using UnityEngine;
using TMPro;

public class InteractionPromptManager : MonoBehaviour
{
    public TMP_Text interactionPrompt;
    public Transform player;
    public float interactDistance = 3f;

    private DoorInteraction[] doors;
    private PageInteraction[] pages;

    private void Start()
    {
    doors = Object.FindObjectsByType<DoorInteraction>(FindObjectsSortMode.None);
    pages = Object.FindObjectsByType<PageInteraction>(FindObjectsSortMode.None);
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    private void Update()
    {
        bool nearInteractable = false;
        string promptText = "[E] Interact";

        // Check doors
        foreach (var door in doors)
        {
            if (door != null && door.player != null)
            {
                float distance = Vector3.Distance(player.position, door.transform.position);
                if (distance <= interactDistance)
                {
                    nearInteractable = true;
                    break;
                }
            }
        }

        // Dynamically find all active pages in the scene
        if (!nearInteractable)
        {
            var activePages = Object.FindObjectsByType<PageInteraction>(FindObjectsSortMode.None);
            foreach (var page in activePages)
            {
                if (page != null && page.pageCollection != null && page.pageCollection.player != null)
                {
                    float distance = Vector3.Distance(player.position, page.transform.position);
                    if (distance <= page.pageCollection.interactDistance)
                    {
                        nearInteractable = true;
                        break;
                    }
                }
            }
        }

        // Show or hide prompt
        if (interactionPrompt != null)
        {
            if (nearInteractable)
            {
                interactionPrompt.text = promptText;
                interactionPrompt.gameObject.SetActive(true);
            }
            else
            {
                interactionPrompt.gameObject.SetActive(false);
            }
        }
    }
}
