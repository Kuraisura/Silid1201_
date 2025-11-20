using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Resets Global Volume to remove color tints and effects
/// </summary>
public class ResetGlobalVolume : MonoBehaviour
{
    [ContextMenu("Disable All Post Processing")]
    public void DisableAllPostProcessing()
    {
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Disable all effects
            if (volume.profile.TryGet<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.active = false;
            }
            
            if (volume.profile.TryGet<ShadowsMidtonesHighlights>(out var smh))
            {
                smh.active = false;
            }
            
            if (volume.profile.TryGet<Bloom>(out var bloom))
            {
                bloom.active = false;
            }
            
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.active = false;
            }
            
            if (volume.profile.TryGet<FilmGrain>(out var grain))
            {
                grain.active = false;
            }
            
            if (volume.profile.TryGet<ChromaticAberration>(out var ca))
            {
                ca.active = false;
            }
            
            if (volume.profile.TryGet<MotionBlur>(out var mb))
            {
                mb.active = false;
            }
            
            Debug.Log("<color=green>✅ All post-processing effects disabled!</color>");
        }
        else
        {
            Debug.LogWarning("No Volume found in scene!");
        }
    }
    
    [ContextMenu("Reset to Neutral Colors")]
    public void ResetToNeutralColors()
    {
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Reset color adjustments to neutral
            if (volume.profile.TryGet<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.active = true;
                colorAdj.postExposure.value = 0f;
                colorAdj.contrast.value = 0f;
                colorAdj.colorFilter.value = Color.white;
                colorAdj.hueShift.value = 0f;
                colorAdj.saturation.value = 0f;
            }
            
            // Reset shadows/midtones/highlights
            if (volume.profile.TryGet<ShadowsMidtonesHighlights>(out var smh))
            {
                smh.active = false;
            }
            
            // Disable bloom tint
            if (volume.profile.TryGet<Bloom>(out var bloom))
            {
                bloom.tint.value = Color.white;
            }
            
            Debug.Log("<color=green>✅ Colors reset to neutral (no tint)!</color>");
        }
        else
        {
            Debug.LogWarning("No Volume found in scene!");
        }
    }
    
    [ContextMenu("Keep Only Essential Effects")]
    public void KeepOnlyEssential()
    {
        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            // Disable color grading effects
            if (volume.profile.TryGet<ColorAdjustments>(out var colorAdj))
            {
                colorAdj.active = true;
                colorAdj.postExposure.value = 0f;
                colorAdj.contrast.value = 0f;
                colorAdj.colorFilter.value = Color.white;
                colorAdj.saturation.value = 0f;
            }
            
            if (volume.profile.TryGet<ShadowsMidtonesHighlights>(out var smh))
            {
                smh.active = false;
            }
            
            // Keep subtle bloom (no tint)
            if (volume.profile.TryGet<Bloom>(out var bloom))
            {
                bloom.active = true;
                bloom.intensity.value = 0.3f;
                bloom.tint.value = Color.white;
            }
            
            // Disable other effects
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.active = false;
            }
            
            if (volume.profile.TryGet<FilmGrain>(out var grain))
            {
                grain.active = false;
            }
            
            if (volume.profile.TryGet<ChromaticAberration>(out var ca))
            {
                ca.active = false;
            }
            
            if (volume.profile.TryGet<MotionBlur>(out var mb))
            {
                mb.active = false;
            }
            
            Debug.Log("<color=cyan>✅ Kept only essential effects (subtle bloom, no color tint)!</color>");
        }
    }
}
