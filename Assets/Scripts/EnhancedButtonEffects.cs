using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced button effects with beautiful hover animations, scale, glow, and particles
/// </summary>
public class EnhancedButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    public bool useScaleEffect = true;
    public float normalScale = 1f;
    public float hoverScale = 1.15f;
    public float clickScale = 0.9f;
    public float scaleSpeed = 12f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Color Animation")]
    public bool useColorEffect = true;
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    public Color hoverColor = new Color(1f, 0.95f, 0.4f, 1f); // Golden
    public Color clickColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange
    public float colorTransitionSpeed = 8f;

    [Header("Glow Effect")]
    public bool useGlowEffect = true;
    public Image glowImage; // Assign a child image for glow
    public float glowIntensityNormal = 0f;
    public float glowIntensityHover = 0.8f;
    public float glowPulseSpeed = 2f;

    [Header("Rotation Effect")]
    public bool useRotationEffect = false;
    public float hoverRotation = 5f;
    public float rotationSpeed = 10f;

    [Header("Shadow/Outline Effect")]
    public bool useShadowEffect = true;
    public Shadow shadow;
    public Vector2 shadowNormalDistance = new Vector2(2, -2);
    public Vector2 shadowHoverDistance = new Vector2(4, -4);

    [Header("Particle Effect")]
    public ParticleSystem hoverParticles;
    public ParticleSystem clickParticles;

    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Haptic Feedback")]
    public bool useHapticFeedback = true;

    // Internal variables
    private AudioSource audioSource;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Image image;
    private TextMeshProUGUI tmpText;
    private Text legacyText;
    private Color currentColor;
    private Color targetColor;
    private bool isHovering = false;
    private bool isPressed = false;
    private float glowCurrentIntensity = 0f;
    private float glowTargetIntensity = 0f;
    private Coroutine glowCoroutine;

    private void Awake()
    {
        // Get components
        image = GetComponent<Image>();
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        legacyText = GetComponentInChildren<Text>();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hoverSound != null || clickSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        // Setup shadow if not assigned
        if (useShadowEffect && shadow == null)
        {
            shadow = GetComponent<Shadow>();
            if (shadow == null && image != null)
            {
                shadow = gameObject.AddComponent<Shadow>();
                shadow.effectDistance = shadowNormalDistance;
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
            }
        }

        // Store originals
        originalScale = transform.localScale * normalScale;
        targetScale = originalScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;

        // Set initial color
        currentColor = normalColor;
        targetColor = normalColor;
        ApplyColor(currentColor);

        // Setup glow
        if (glowImage != null)
        {
            glowImage.color = new Color(hoverColor.r, hoverColor.g, hoverColor.b, 0f);
        }
    }

    private void Update()
    {
        // Smooth scale animation
        if (useScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        // Smooth rotation animation
        if (useRotationEffect)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Smooth color transition
        if (useColorEffect)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            ApplyColor(currentColor);
        }

        // Glow animation
        if (useGlowEffect && glowImage != null)
        {
            glowCurrentIntensity = Mathf.Lerp(glowCurrentIntensity, glowTargetIntensity, Time.deltaTime * scaleSpeed);
            
            float glowAlpha = glowCurrentIntensity;
            if (isHovering)
            {
                // Add pulse effect when hovering
                glowAlpha += Mathf.Sin(Time.time * glowPulseSpeed) * 0.2f;
            }
            
            glowImage.color = new Color(hoverColor.r, hoverColor.g, hoverColor.b, glowAlpha);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[EnhancedButtonEffects] Pointer ENTER detected on {gameObject.name}");
        if (!IsInteractable()) return;

        isHovering = true;

        // Scale effect
        if (useScaleEffect)
        {
            targetScale = originalScale * hoverScale;
        }

        // Rotation effect
        if (useRotationEffect)
        {
            targetRotation = originalRotation * Quaternion.Euler(0, 0, hoverRotation);
        }

        // Color effect
        if (useColorEffect)
        {
            targetColor = hoverColor;
        }

        // Glow effect
        if (useGlowEffect)
        {
            glowTargetIntensity = glowIntensityHover;
        }

        // Shadow effect
        if (useShadowEffect && shadow != null)
        {
            shadow.effectDistance = shadowHoverDistance;
        }

        // Particles
        if (hoverParticles != null)
        {
            hoverParticles.transform.position = transform.position;
            hoverParticles.Play();
        }

        // Sound
        PlaySound(hoverSound);

        // Start pulsing animation
        StartCoroutine(PulseAnimation());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Scale effect
        if (useScaleEffect)
        {
            targetScale = originalScale;
        }

        // Rotation effect
        if (useRotationEffect)
        {
            targetRotation = originalRotation;
        }

        // Color effect
        if (useColorEffect)
        {
            targetColor = normalColor;
        }

        // Glow effect
        if (useGlowEffect)
        {
            glowTargetIntensity = glowIntensityNormal;
        }

        // Shadow effect
        if (useShadowEffect && shadow != null)
        {
            shadow.effectDistance = shadowNormalDistance;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[EnhancedButtonEffects] Pointer DOWN detected on {gameObject.name}");
        if (!IsInteractable()) return;

        isPressed = true;

        // Scale effect
        if (useScaleEffect)
        {
            targetScale = originalScale * clickScale;
        }

        // Color effect
        if (useColorEffect)
        {
            targetColor = clickColor;
        }

        // Rotation effect - snap back
        if (useRotationEffect)
        {
            targetRotation = originalRotation;
        }

        // Particles
        if (clickParticles != null)
        {
            clickParticles.transform.position = transform.position;
            clickParticles.Play();
        }

        // Sound
        PlaySound(clickSound);

        // Haptic feedback
        if (useHapticFeedback)
        {
            TriggerHapticFeedback();
        }

        // Screen shake effect
        StartCoroutine(ButtonShake());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        if (isHovering)
        {
            // Return to hover state
            if (useScaleEffect)
            {
                targetScale = originalScale * hoverScale;
            }

            if (useColorEffect)
            {
                targetColor = hoverColor;
            }
        }
        else
        {
            // Return to normal state
            if (useScaleEffect)
            {
                targetScale = originalScale;
            }

            if (useColorEffect)
            {
                targetColor = normalColor;
            }
        }
    }

    /// <summary>
    /// Apply color to all visual components
    /// </summary>
    private void ApplyColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }

        if (tmpText != null)
        {
            tmpText.color = color;
        }

        if (legacyText != null)
        {
            legacyText.color = color;
        }
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// Check if button is interactable
    /// </summary>
    private bool IsInteractable()
    {
        Button button = GetComponent<Button>();
        return button == null || button.interactable;
    }

    /// <summary>
    /// Subtle pulse animation on hover
    /// </summary>
    private IEnumerator PulseAnimation()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startScale = transform.localScale;
        Vector3 pulseScale = startScale * 1.05f;

        while (elapsed < duration && isHovering)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed / duration, 1f);
            
            if (useScaleEffect)
            {
                transform.localScale = Vector3.Lerp(startScale, pulseScale, t);
            }

            yield return null;
        }
    }

    /// <summary>
    /// Button shake effect on click
    /// </summary>
    private IEnumerator ButtonShake()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.1f;
        float magnitude = 3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = originalPos + new Vector3(x, y, 0);
            magnitude *= 0.9f;

            yield return null;
        }

        transform.localPosition = originalPos;
    }

    /// <summary>
    /// Trigger haptic feedback on supported devices
    /// </summary>
    private void TriggerHapticFeedback()
    {
        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif
    }

    /// <summary>
    /// Reset button to normal state
    /// </summary>
    public void ResetButton()
    {
        isHovering = false;
        isPressed = false;
        targetScale = originalScale;
        targetRotation = originalRotation;
        targetColor = normalColor;
        glowTargetIntensity = glowIntensityNormal;
        
        if (shadow != null)
        {
            shadow.effectDistance = shadowNormalDistance;
        }
    }
}
