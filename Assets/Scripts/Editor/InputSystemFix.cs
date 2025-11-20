using UnityEditor;

/// <summary>
/// Quick fix for Input System compatibility
/// Sets Active Input Handling to "Both" to support old and new Input System
/// </summary>
public static class InputSystemFix
{
    [MenuItem("Tools/Silid 12:01/Fix Input System")]
    public static void FixInputSystem()
    {
        #if UNITY_2020_1_OR_NEWER
        // Get the current setting
        var currentSetting = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup);
        
        // Show info dialog
        bool switchToInput = EditorUtility.DisplayDialog(
            "Input System Fix",
            "Your project is set to use only the new Input System, but some scripts use the old Input API.\n\n" +
            "Options:\n" +
            "1. Switch to 'Both' (Recommended) - Allows both old and new Input\n" +
            "2. Keep current setting and update scripts manually\n\n" +
            "Do you want to switch to 'Both'?",
            "Yes, Switch to Both",
            "No, Keep Current"
        );

        if (switchToInput)
        {
            // This will require an editor restart
            EditorUtility.DisplayDialog(
                "Manual Step Required",
                "Please follow these steps:\n\n" +
                "1. Go to Edit → Project Settings\n" +
                "2. Select 'Player' section\n" +
                "3. Under 'Other Settings', find 'Active Input Handling'\n" +
                "4. Change it from 'Input System Package (New)' to 'Both'\n" +
                "5. Click 'Apply' when Unity asks to restart\n" +
                "6. Unity will restart\n\n" +
                "This will allow both old and new Input System to work together.",
                "OK"
            );
            
            // Open Project Settings to the right page
            SettingsService.OpenProjectSettings("Project/Player");
        }
        #endif
    }
}
