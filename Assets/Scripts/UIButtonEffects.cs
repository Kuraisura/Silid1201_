using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Effect")]
    public bool useScaleEffect = true;
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    public float animationSpeed = 10f;

    [Header("Color Effect")]
    public bool useColorEffect = true;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.9f, 0.3f); // Yellowish
    public Color clickColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image image;
    private Text text;
    private bool isHovering = false;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hoverSound != null || clickSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Set initial color
        if (useColorEffect)
        {
            if (image != null) image.color = normalColor;
            if (text != null) text.color = normalColor;
        }
    }

    private void Update()
    {
        // Smooth scale animation
        if (useScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (useScaleEffect)
        {
            targetScale = originalScale * hoverScale;
        }

        if (useColorEffect)
        {
            if (image != null) image.color = hoverColor;
            if (text != null) text.color = hoverColor;
        }

        // Play hover sound
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (useScaleEffect)
        {
            targetScale = originalScale;
        }

        if (useColorEffect)
        {
            if (image != null) image.color = normalColor;
            if (text != null) text.color = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (useScaleEffect)
        {
            targetScale = originalScale * clickScale;
        }

        if (useColorEffect)
        {
            if (image != null) image.color = clickColor;
            if (text != null) text.color = clickColor;
        }

        // Play click sound
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isHovering)
        {
            if (useScaleEffect)
            {
                targetScale = originalScale * hoverScale;
            }

            if (useColorEffect)
            {
                if (image != null) image.color = hoverColor;
                if (text != null) text.color = hoverColor;
            }
        }
        else
        {
            if (useScaleEffect)
            {
                targetScale = originalScale;
            }

            if (useColorEffect)
            {
                if (image != null) image.color = normalColor;
                if (text != null) text.color = normalColor;
            }
        }
    }
}
