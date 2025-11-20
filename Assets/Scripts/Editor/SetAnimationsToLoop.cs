using UnityEngine;
using UnityEditor;

public class SetAnimationsToLoop : EditorWindow
{
    [MenuItem("Tools/Set Animations to Loop")]
    public static void ShowWindow()
    {
        GetWindow<SetAnimationsToLoop>("Set Animations to Loop");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Set Animation Clips to Loop", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will set Walk and Run animations to loop.");
        GUILayout.Space(20);
        
        if (GUILayout.Button("Set All Animations to Loop", GUILayout.Height(40)))
        {
            SetAllAnimationsLoop();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Set Selected Animation to Loop", GUILayout.Height(30)))
        {
            SetSelectedAnimationLoop();
        }
    }
    
    void SetAllAnimationsLoop()
    {
        // Find all animation clips in the project
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        int loopCount = 0;
        int totalCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip != null)
            {
                totalCount++;
                string clipName = clip.name.ToLower();
                
                // Set loop for Walk, Run, Idle animations
                if (clipName.Contains("walk") || clipName.Contains("run") || 
                    clipName.Contains("idle") || clipName.Contains("roam"))
                {
                    if (SetClipToLoop(clip, path))
                    {
                        loopCount++;
                        Debug.Log($"✓ Set '{clip.name}' to loop");
                    }
                }
            }
        }
        
        Debug.Log($"<color=green>═══ ANIMATION LOOP SETUP COMPLETE ═══</color>");
        Debug.Log($"<color=green>✓ Set {loopCount} animations to loop (out of {totalCount} total)</color>");
        
        EditorUtility.DisplayDialog("Success", 
            $"Set {loopCount} animations to loop!\n\nCheck the Console for details.", "OK");
    }
    
    void SetSelectedAnimationLoop()
    {
        Object[] selectedObjects = Selection.objects;
        int count = 0;
        
        foreach (Object obj in selectedObjects)
        {
            AnimationClip clip = obj as AnimationClip;
            if (clip != null)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                if (SetClipToLoop(clip, path))
                {
                    count++;
                    Debug.Log($"✓ Set '{clip.name}' to loop");
                }
            }
        }
        
        if (count > 0)
        {
            Debug.Log($"<color=green>✓ Set {count} selected animations to loop</color>");
            EditorUtility.DisplayDialog("Success", $"Set {count} animations to loop!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "No AnimationClips selected!", "OK");
        }
    }
    
    bool SetClipToLoop(AnimationClip clip, string path)
    {
        // Get the ModelImporter for this clip
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        
        if (importer != null)
        {
            // Get existing clip animations
            ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
            
            if (clipAnimations.Length == 0)
            {
                // Use default clip animations
                clipAnimations = importer.defaultClipAnimations;
            }
            
            bool changed = false;
            
            for (int i = 0; i < clipAnimations.Length; i++)
            {
                if (clipAnimations[i].name == clip.name)
                {
                    if (!clipAnimations[i].loopTime)
                    {
                        clipAnimations[i].loopTime = true;
                        changed = true;
                    }
                }
            }
            
            if (changed)
            {
                importer.clipAnimations = clipAnimations;
                importer.SaveAndReimport();
                return true;
            }
        }
        else
        {
            // For standalone animation clips (not from FBX)
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (!settings.loopTime)
            {
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                return true;
            }
        }
        
        return false;
    }
}
