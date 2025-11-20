using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationType animationType = AnimationType.Fade;
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Animation")]
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    public enum AnimationType
    {
        Fade,
        Scale,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        FadeAndScale
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Show panel with animation
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateShow());
    }

    /// <summary>
    /// Hide panel with animation
    /// </summary>
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateHide());
    }

    private IEnumerator AnimateShow()
    {
        float elapsed = 0f;

        switch (animationType)
        {
            case AnimationType.Fade:
                canvasGroup.alpha = 0f;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = animationCurve.Evaluate(elapsed / animationDuration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
                break;

            case AnimationType.Scale:
                transform.localScale = startScale;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = animationCurve.Evaluate(elapsed / animationDuration);
                    transform.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }
                transform.localScale = endScale;
                break;

            case AnimationType.FadeAndScale:
                canvasGroup.alpha = 0f;
                transform.localScale = startScale;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = animationCurve.Evaluate(elapsed / animationDuration);
                    canvasGroup.alpha = t;
                    transform.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
                transform.localScale = endScale;
                break;

            case AnimationType.SlideFromLeft:
                yield return StartCoroutine(SlideIn(new Vector2(-Screen.width, 0)));
                break;

            case AnimationType.SlideFromRight:
                yield return StartCoroutine(SlideIn(new Vector2(Screen.width, 0)));
                break;

            case AnimationType.SlideFromTop:
                yield return StartCoroutine(SlideIn(new Vector2(0, Screen.height)));
                break;

            case AnimationType.SlideFromBottom:
                yield return StartCoroutine(SlideIn(new Vector2(0, -Screen.height)));
                break;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator AnimateHide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;

        switch (animationType)
        {
            case AnimationType.Fade:
                canvasGroup.alpha = 1f;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = 1f - animationCurve.Evaluate(elapsed / animationDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                break;

            case AnimationType.Scale:
                transform.localScale = endScale;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = animationCurve.Evaluate(elapsed / animationDuration);
                    transform.localScale = Vector3.Lerp(endScale, startScale, t);
                    yield return null;
                }
                transform.localScale = startScale;
                break;

            case AnimationType.FadeAndScale:
                canvasGroup.alpha = 1f;
                transform.localScale = endScale;
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = animationCurve.Evaluate(elapsed / animationDuration);
                    canvasGroup.alpha = 1f - t;
                    transform.localScale = Vector3.Lerp(endScale, startScale, t);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                transform.localScale = startScale;
                break;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator SlideIn(Vector2 startOffset)
    {
        Vector2 originalPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = originalPosition + startOffset;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(originalPosition + startOffset, originalPosition, t);
            yield return null;
        }
        rectTransform.anchoredPosition = originalPosition;
    }
}
