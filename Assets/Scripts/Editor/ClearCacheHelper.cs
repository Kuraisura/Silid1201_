using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Clear Unity cache and fix common serialization issues
/// </summary>
public class ClearCacheHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Silid 12:01/Clear Cache and Reimport")]
    public static void ClearCacheAndReimport()
    {
        if (EditorUtility.DisplayDialog(
            "Clear Cache",
            "This will clear Unity's cache and reimport all assets.\n\n" +
            "This fixes serialization warnings and refresh issues.\n\n" +
            "Unity will restart. Continue?",
            "Yes, Clear Cache",
            "Cancel"))
        {
            Debug.Log("Clearing cache...");
            
            // Clear console
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
            
            // Force reimport
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            Debug.Log("✓ Cache cleared! Restarting Unity is recommended.");
            
            if (EditorUtility.DisplayDialog(
                "Restart Unity",
                "Cache cleared!\n\nRestart Unity now to complete the fix?",
                "Restart",
                "Later"))
            {
                EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
            }
        }
    }

    [MenuItem("Tools/Silid 12:01/Fix Input System Setting")]
    public static void FixInputSystemSetting()
    {
        string message = "To fix Input System errors:\n\n" +
                        "1. Edit → Project Settings\n" +
                        "2. Click 'Player' in left sidebar\n" +
                        "3. Other Settings → Configuration section\n" +
                        "4. Find 'Active Input Handling'\n" +
                        "5. Change to 'Both'\n" +
                        "6. Click 'Apply'\n" +
                        "7. Restart Unity when prompted\n\n" +
                        "This allows both old Input.GetKey() and new Input System to work.";

        if (EditorUtility.DisplayDialog(
            "Fix Input System",
            message,
            "Open Project Settings",
            "Cancel"))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
    }

    [MenuItem("Tools/Silid 12:01/Clear Console")]
    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
        
        Debug.Log("Console cleared!");
    }
#endif
}
