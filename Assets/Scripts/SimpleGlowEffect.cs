using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple glow effect for UI elements
/// Animates the glow intensity on hover
/// </summary>
[RequireComponent(typeof(Image))]
public class SimpleGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [Range(0f, 2f)]
    public float glowIntensity = 1f;
    
    [Range(0f, 5f)]
    public float glowSpeed = 2f;
    
    public Color glowColor = new Color(1f, 0.95f, 0.4f, 1f);
    
    [Header("Animation")]
    public bool animateGlow = true;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;

    private Image image;
    private Material glowMaterial;
    private float currentIntensity;

    private void Awake()
    {
        image = GetComponent<Image>();
        
        // Create a material instance
        if (image.material != null)
        {
            glowMaterial = new Material(image.material);
            image.material = glowMaterial;
        }
    }

    private void Update()
    {
        if (animateGlow)
        {
            // Pulse effect
            float pulse = Mathf.Sin(Time.time * glowSpeed) * 0.5f + 0.5f;
            currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
        else
        {
            currentIntensity = glowIntensity;
        }

        // Apply glow color with intensity
        if (image != null)
        {
            Color finalColor = glowColor * currentIntensity;
            finalColor.a = glowColor.a;
            image.color = finalColor;
        }
    }

    /// <summary>
    /// Set glow intensity directly
    /// </summary>
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        animateGlow = false;
    }

    /// <summary>
    /// Start pulsing animation
    /// </summary>
    public void StartPulse()
    {
        animateGlow = true;
    }

    /// <summary>
    /// Stop pulsing animation
    /// </summary>
    public void StopPulse()
    {
        animateGlow = false;
    }
}
