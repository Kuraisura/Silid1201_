using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple loading screen for scene transitions
/// Attach this to a Canvas in a "LoadingScreen" scene or prefab
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Text loadingText;
    public Slider progressBar;
    public Image backgroundImage;
    public Text tipText;

    [Header("Loading Tips")]
    public string[] horrorTips = new string[]
    {
        "Stay quiet... they can hear you.",
        "Don't look back.",
        "The lights won't stay on forever.",
        "Trust your instincts.",
        "Some doors are better left closed.",
        "12:01 AM... the witching hour begins.",
        "Your flashlight battery won't last forever.",
        "Listen carefully... footsteps echo in the halls.",
        "The school holds dark secrets.",
        "Not all shadows are empty."
    };

    [Header("Animation")]
    public float dotAnimationSpeed = 0.5f;
    public Color backgroundColor = Color.black;

    private string baseLoadingText = "Loading";
    private int dotCount = 0;
    private float dotTimer = 0f;

    private void Start()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        if (tipText != null && horrorTips.Length > 0)
        {
            tipText.text = horrorTips[Random.Range(0, horrorTips.Length)];
        }

        if (progressBar != null)
        {
            progressBar.value = 0f;
        }
    }

    private void Update()
    {
        // Animate loading text with dots
        if (loadingText != null)
        {
            dotTimer += Time.deltaTime;
            if (dotTimer >= dotAnimationSpeed)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;
                loadingText.text = baseLoadingText + new string('.', dotCount);
            }
        }
    }

    /// <summary>
    /// Update progress bar (call from AsyncOperation)
    /// </summary>
    public void SetProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }

    /// <summary>
    /// Change loading text
    /// </summary>
    public void SetLoadingText(string text)
    {
        baseLoadingText = text;
    }

    /// <summary>
    /// Show a random tip
    /// </summary>
    public void ShowRandomTip()
    {
        if (tipText != null && horrorTips.Length > 0)
        {
            tipText.text = horrorTips[Random.Range(0, horrorTips.Length)];
        }
    }
}
