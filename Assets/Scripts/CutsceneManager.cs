using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Cutscene Manager for Silid 12:01
/// Plays intro cutscene before transitioning to main game
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [Header("Video Cutscene")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public AudioSource videoAudioSource;

    [Header("Image Sequence Cutscene")]
    public Image cutsceneImage;
    public Sprite[] cutsceneSprites;
    public float imageDisplayDuration = 3f;
    public AudioClip[] narrativeAudioClips;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    [Header("Skip Settings")]
    public bool allowSkip = true;
    public Text skipText;
    public KeyCode skipKey = KeyCode.Space;
    public KeyCode skipKeyAlt = KeyCode.Escape;

    [Header("Scene Transition")]
    public string nextSceneName = "Main";
    public float delayAfterCutscene = 1f;

    [Header("Cutscene Type")]
    public CutsceneType cutsceneType = CutsceneType.Video;

    public enum CutsceneType
    {
        Video,
        ImageSequence,
        Both
    }

    private bool cutsceneFinished = false;
    private bool isSkipping = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup video player if using video cutscene
        if (videoPlayer != null && cutsceneType != CutsceneType.ImageSequence)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // Setup fade canvas group
        if (fadeCanvasGroup == null)
        {
            GameObject fadeGO = new GameObject("FadeCanvas");
            fadeGO.transform.SetParent(transform, false);
            Canvas canvas = fadeGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            fadeCanvasGroup = fadeGO.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = false;
            
            GameObject fadeImageGO = new GameObject("FadeImage");
            fadeImageGO.transform.SetParent(fadeGO.transform, false);
            RectTransform fadeRect = fadeImageGO.AddComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.sizeDelta = Vector2.zero;
            
            Image fadeImage = fadeImageGO.AddComponent<Image>();
            fadeImage.color = fadeColor;
        }

        // Setup skip text
        if (skipText != null)
        {
            skipText.text = $"Press {skipKey} or {skipKeyAlt} to skip";
        }
    }

    private void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    private void Update()
    {
        // Check for skip input
        if (allowSkip && !cutsceneFinished && !isSkipping)
        {
            if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(skipKeyAlt))
            {
                SkipCutscene();
            }
        }
    }

    /// <summary>
    /// Main cutscene playback coroutine
    /// </summary>
    private IEnumerator PlayCutscene()
    {
        // Fade in from black
        yield return StartCoroutine(FadeIn());

        // Play cutscene based on type
        switch (cutsceneType)
        {
            case CutsceneType.Video:
                yield return StartCoroutine(PlayVideoCutscene());
                break;

            case CutsceneType.ImageSequence:
                yield return StartCoroutine(PlayImageSequence());
                break;

            case CutsceneType.Both:
                yield return StartCoroutine(PlayImageSequence());
                yield return StartCoroutine(PlayVideoCutscene());
                break;
        }

        if (!isSkipping)
        {
            cutsceneFinished = true;
            yield return new WaitForSeconds(delayAfterCutscene);
            TransitionToMainScene();
        }
    }

    /// <summary>
    /// Play video cutscene
    /// </summary>
    private IEnumerator PlayVideoCutscene()
    {
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            Debug.LogWarning("No video clip assigned to VideoPlayer!");
            yield break;
        }

        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
        }

        // Setup video player
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        if (videoPlayer.targetTexture == null)
        {
            videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        }

        if (videoDisplay != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
        }

        // Setup audio
        if (videoAudioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        }

        // Play video
        videoPlayer.Play();

        // Wait for video to finish or skip
        while (videoPlayer.isPlaying && !isSkipping)
        {
            yield return null;
        }

        videoPlayer.Stop();
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Play image sequence cutscene
    /// </summary>
    private IEnumerator PlayImageSequence()
    {
        if (cutsceneSprites == null || cutsceneSprites.Length == 0)
        {
            Debug.LogWarning("No cutscene sprites assigned!");
            yield break;
        }

        if (cutsceneImage != null)
        {
            cutsceneImage.gameObject.SetActive(true);
        }

        for (int i = 0; i < cutsceneSprites.Length; i++)
        {
            if (isSkipping) break;

            // Display image
            if (cutsceneImage != null && cutsceneSprites[i] != null)
            {
                cutsceneImage.sprite = cutsceneSprites[i];
                
                // Fade in image
                CanvasGroup imageGroup = cutsceneImage.GetComponent<CanvasGroup>();
                if (imageGroup != null)
                {
                    imageGroup.alpha = 0f;
                    float elapsed = 0f;
                    while (elapsed < fadeDuration && !isSkipping)
                    {
                        elapsed += Time.deltaTime;
                        imageGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                        yield return null;
                    }
                    imageGroup.alpha = 1f;
                }
            }

            // Play narrative audio if available
            if (narrativeAudioClips != null && i < narrativeAudioClips.Length && narrativeAudioClips[i] != null)
            {
                audioSource.PlayOneShot(narrativeAudioClips[i]);
                yield return new WaitForSeconds(narrativeAudioClips[i].length);
            }
            else
            {
                // Wait for display duration
                float timer = 0f;
                while (timer < imageDisplayDuration && !isSkipping)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            // Fade out image
            if (cutsceneImage != null)
            {
                CanvasGroup imageGroup = cutsceneImage.GetComponent<CanvasGroup>();
                if (imageGroup != null)
                {
                    float elapsed = 0f;
                    while (elapsed < fadeDuration && !isSkipping)
                    {
                        elapsed += Time.deltaTime;
                        imageGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                        yield return null;
                    }
                    imageGroup.alpha = 0f;
                }
            }
        }

        if (cutsceneImage != null)
        {
            cutsceneImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Fade in from black
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.alpha = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fade out to black
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Skip the cutscene
    /// </summary>
    private void SkipCutscene()
    {
        if (isSkipping || cutsceneFinished) return;

        isSkipping = true;
        StopAllCoroutines();

        // Stop video if playing
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("Cutscene skipped!");
        StartCoroutine(SkipTransition());
    }

    /// <summary>
    /// Transition after skip
    /// </summary>
    private IEnumerator SkipTransition()
    {
        yield return StartCoroutine(FadeOut());
        TransitionToMainScene();
    }

    /// <summary>
    /// Called when video finishes
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!isSkipping && !cutsceneFinished)
        {
            cutsceneFinished = true;
            StartCoroutine(FinishCutscene());
        }
    }

    /// <summary>
    /// Finish cutscene and transition
    /// </summary>
    private IEnumerator FinishCutscene()
    {
        yield return new WaitForSeconds(delayAfterCutscene);
        yield return StartCoroutine(FadeOut());
        TransitionToMainScene();
    }

    /// <summary>
    /// Load main game scene
    /// </summary>
    private void TransitionToMainScene()
    {
        Debug.Log($"Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
