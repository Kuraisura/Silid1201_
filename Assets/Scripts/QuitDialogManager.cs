using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple Quit Dialog Manager
/// Shows a confirmation dialog when user tries to quit
/// Works with existing UIMenuManager
/// </summary>
public class QuitDialogManager : MonoBehaviour
{
    [Header("Dialog Panel")]
    public GameObject dialogPanel;
    public CanvasGroup dialogCanvasGroup;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float scaleInDuration = 0.3f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;

    [Header("Audio")]
    public AudioClip dialogOpenSound;
    public AudioClip dialogCloseSound;
    private AudioSource audioSource;

    private void Awake()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (dialogOpenSound != null || dialogCloseSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Get canvas group if not assigned
        if (dialogCanvasGroup == null && dialogPanel != null)
        {
            dialogCanvasGroup = dialogPanel.GetComponent<CanvasGroup>();
            if (dialogCanvasGroup == null)
            {
                dialogCanvasGroup = dialogPanel.AddComponent<CanvasGroup>();
            }
        }

        // Setup button listeners
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }

        // Hide dialog initially
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show the quit confirmation dialog
    /// </summary>
    public void ShowDialog()
    {
        if (dialogPanel == null) return;

        StopAllCoroutines();
        StartCoroutine(ShowDialogAnimation());
    }

    /// <summary>
    /// Hide the quit confirmation dialog
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel == null) return;

        StopAllCoroutines();
        StartCoroutine(HideDialogAnimation());
    }

    /// <summary>
    /// Yes button clicked - quit the game
    /// </summary>
    private void OnYesClicked()
    {
        Debug.Log("Quit confirmed!");
        StartCoroutine(QuitGameWithAnimation());
    }

    /// <summary>
    /// No button clicked - close dialog
    /// </summary>
    private void OnNoClicked()
    {
        Debug.Log("Quit cancelled");
        HideDialog();
    }

    /// <summary>
    /// Animate dialog appearing
    /// </summary>
    private IEnumerator ShowDialogAnimation()
    {
        dialogPanel.SetActive(true);

        // Play sound
        if (audioSource != null && dialogOpenSound != null)
        {
            audioSource.PlayOneShot(dialogOpenSound);
        }

        // Start from zero
        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.alpha = 0f;
        }
        dialogPanel.transform.localScale = Vector3.zero;

        float elapsed = 0f;
        float duration = Mathf.Max(fadeInDuration, scaleInDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Fade in
            if (dialogCanvasGroup != null)
            {
                float fadeProgress = Mathf.Clamp01(elapsed / fadeInDuration);
                dialogCanvasGroup.alpha = fadeProgress;
            }

            // Scale in with easing
            float scaleProgress = Mathf.Clamp01(elapsed / scaleInDuration);
            float easedProgress = scaleCurve.Evaluate(scaleProgress);
            dialogPanel.transform.localScale = Vector3.one * EaseOutBack(easedProgress);

            yield return null;
        }

        // Ensure final state
        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.alpha = 1f;
        }
        dialogPanel.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Animate dialog disappearing
    /// </summary>
    private IEnumerator HideDialogAnimation()
    {
        // Play sound
        if (audioSource != null && dialogCloseSound != null)
        {
            audioSource.PlayOneShot(dialogCloseSound);
        }

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - (elapsed / duration);

            // Fade out
            if (dialogCanvasGroup != null)
            {
                dialogCanvasGroup.alpha = t;
            }

            // Scale out
            dialogPanel.transform.localScale = Vector3.one * t;

            yield return null;
        }

        dialogPanel.SetActive(false);
    }

    /// <summary>
    /// Quit game with fade animation
    /// </summary>
    private IEnumerator QuitGameWithAnimation()
    {
        // Disable buttons
        if (yesButton != null) yesButton.interactable = false;
        if (noButton != null) noButton.interactable = false;

        // Fade out
        if (dialogCanvasGroup != null)
        {
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                dialogCanvasGroup.alpha = 1f - (elapsed / duration);
                yield return null;
            }
        }

        // Quit
        QuitGame();
    }

    /// <summary>
    /// Actually quit the application
    /// </summary>
    private void QuitGame()
    {
        Debug.Log("Quitting application...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Ease out back animation curve
    /// Creates a slight overshoot effect
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// Public method to be called from UIMenuManager
    /// </summary>
    public void OnQuitRequested()
    {
        ShowDialog();
    }
}
