using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Tool to fix animation clip types to match avatar type
/// Fixes the "generic clip animate transforms" warning
/// </summary>
public class AnimationClipTypeFixer : EditorWindow
{
    private List<AnimationClip> clipsToFix = new List<AnimationClip>();
    private ModelImporterAnimationType targetType = ModelImporterAnimationType.Human;
    private Vector2 scrollPos;

    [MenuItem("Tools/Silid 12:01/Fix Animation Clip Types")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipTypeFixer>("Animation Clip Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Clip Type Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool fixes the warning:\n" +
            "'Binding warning: Some generic clip(s) animate transforms...'\n\n" +
            "It converts animation clips to match your avatar type.",
            MessageType.Info);

        GUILayout.Space(10);

        // Target type selection
        EditorGUILayout.LabelField("Convert clips to:", EditorStyles.boldLabel);
        targetType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("Animation Type", targetType);

        GUILayout.Space(10);

        // Auto-detect button
        if (GUILayout.Button("Find All Animation Clips in Project", GUILayout.Height(30)))
        {
            FindAllAnimationClips();
        }

        GUILayout.Space(5);

        // Manual selection
        if (GUILayout.Button("Add Selected Clips", GUILayout.Height(25)))
        {
            AddSelectedClips();
        }

        GUILayout.Space(10);

        // List of clips to fix
        EditorGUILayout.LabelField($"Clips to Fix: {clipsToFix.Count}", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < clipsToFix.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(clipsToFix[i], typeof(AnimationClip), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                clipsToFix.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("Clear List", GUILayout.Height(25)))
        {
            clipsToFix.Clear();
        }

        GUILayout.Space(10);

        // Fix button
        GUI.enabled = clipsToFix.Count > 0;
        if (GUILayout.Button("Fix Animation Types", GUILayout.Height(40)))
        {
            FixAnimationClips();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // Quick fix for selected character
        EditorGUILayout.HelpBox(
            "Quick Fix: Select a character in Hierarchy and click below to automatically fix all its animations.",
            MessageType.None);

        if (GUILayout.Button("Fix Selected Character's Animations", GUILayout.Height(35)))
        {
            FixSelectedCharacterAnimations();
        }
    }

    private void FindAllAnimationClips()
    {
        clipsToFix.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip != null && !clip.name.Contains("__preview__"))
            {
                clipsToFix.Add(clip);
            }
        }

        Debug.Log($"Found {clipsToFix.Count} animation clips in project.");
    }

    private void AddSelectedClips()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip clip && !clipsToFix.Contains(clip))
            {
                clipsToFix.Add(clip);
            }
        }
    }

    private void FixAnimationClips()
    {
        int fixedCount = 0;
        int errors = 0;

        foreach (AnimationClip clip in clipsToFix)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null)
            {
                importer.animationType = targetType;
                
                if (targetType == ModelImporterAnimationType.Human)
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                fixedCount++;
                Debug.Log($"✓ Fixed: {clip.name}");
            }
            else
            {
                Debug.LogWarning($"✗ Could not fix: {clip.name} (not a model import)");
                errors++;
            }
        }

        string message = $"Animation Fix Complete!\n\nFixed: {fixedCount}\nErrors: {errors}";
        EditorUtility.DisplayDialog("Fix Complete", message, "OK");
        
        clipsToFix.Clear();
    }

    private void FixSelectedCharacterAnimations()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a character in the Hierarchy.", "OK");
            return;
        }

        GameObject character = Selection.activeGameObject;
        Animator animator = character.GetComponent<Animator>();

        if (animator == null)
        {
            EditorUtility.DisplayDialog("No Animator", "Selected object has no Animator component.", "OK");
            return;
        }

        // Determine target type from avatar
        ModelImporterAnimationType type = ModelImporterAnimationType.Human;
        
        if (animator.avatar != null)
        {
            type = animator.avatar.isHuman ? 
                ModelImporterAnimationType.Human : 
                ModelImporterAnimationType.Generic;
        }

        Debug.Log($"Detected avatar type: {type}");

        // Find all animation clips used by this character
        List<AnimationClip> clips = new List<AnimationClip>();

        // From Animator Controller
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null && !clips.Contains(clip))
                {
                    clips.Add(clip);
                }
            }
        }

        // From Timeline
        UnityEngine.Playables.PlayableDirector director = character.GetComponent<UnityEngine.Playables.PlayableDirector>();
        if (director != null && director.playableAsset != null)
        {
            var timeline = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
            if (timeline != null)
            {
                foreach (var track in timeline.GetOutputTracks())
                {
                    var animTrack = track as UnityEngine.Timeline.AnimationTrack;
                    if (animTrack != null)
                    {
                        foreach (var clip in animTrack.GetClips())
                        {
                            var animAsset = clip.asset as UnityEngine.Timeline.AnimationPlayableAsset;
                            if (animAsset != null && animAsset.clip != null)
                            {
                                if (!clips.Contains(animAsset.clip))
                                {
                                    clips.Add(animAsset.clip);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog("No Clips Found", "No animation clips found for this character.", "OK");
            return;
        }

        // Fix the clips
        int fixedCount = 0;
        foreach (AnimationClip clip in clips)
        {
            string path = AssetDatabase.GetAssetPath(clip);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null)
            {
                importer.animationType = type;
                
                if (type == ModelImporterAnimationType.Human)
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                }

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                fixedCount++;
                Debug.Log($"✓ Fixed: {clip.name}");
            }
        }

        EditorUtility.DisplayDialog(
            "Fix Complete", 
            $"Fixed {fixedCount} animation clips for {character.name}\n\nAvatar Type: {type}", 
            "OK");
    }
}
