using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple skip hint UI for cutscenes
/// Shows "Press ESC or SPACE to skip" message
/// </summary>
public class CutsceneSkipHint : MonoBehaviour
{
    [Header("UI References")]
    public Text skipText;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public bool pulseAnimation = true;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1f;

    [Header("Text Settings")]
    public string skipMessage = "Press ESC or SPACE to skip";

    private void Start()
    {
        if (skipText != null)
        {
            skipText.text = skipMessage;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        if (pulseAnimation && canvasGroup != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, 
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            canvasGroup.alpha = alpha;
        }
    }

    /// <summary>
    /// Show the skip hint
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the skip hint
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
