using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple always-visible skip hint that pulses
/// Drop this on a Canvas with Text child
/// </summary>
public class SimpleSkipHint : MonoBehaviour
{
    private Text skipText;
    private CanvasGroup canvasGroup;
    
    [Header("Settings")]
    public string message = "Press ESC or SPACE to skip";
    public float pulseSpeed = 2f;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    private void Awake()
    {
        // Get or create components
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find text in children
        skipText = GetComponentInChildren<Text>();
        if (skipText != null)
        {
            skipText.text = message;
        }
    }

    private void Update()
    {
        // Pulse effect
        if (canvasGroup != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, 
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            canvasGroup.alpha = alpha;
        }
    }
}
