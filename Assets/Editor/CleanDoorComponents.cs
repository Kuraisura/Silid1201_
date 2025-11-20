using UnityEngine;
using UnityEditor;

public static class CleanDoorComponents
{
    [MenuItem("Tools/Door/Clean Selected Door Components")]
    public static void CleanSelected()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            Debug.LogWarning("No GameObject selected. Select one or more Door GameObjects and run this command.");
            return;
        }

        int totalRemoved = 0;
        foreach (var go in objs)
        {
            totalRemoved += CleanGameObjectRecursively(go);
        }

    Debug.Log($"CleanDoorComponents: cleaned selected objects, removed {totalRemoved} components (missing or named legacy door components).");
    }

    [MenuItem("Tools/Door/Clean All Doors In Scene")]
    public static void CleanAllDoorsInScene()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int totalRemoved = 0;
        foreach (var root in roots)
        {
            var gos = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in gos)
            {
                var go = t.gameObject;
                if (go.name.ToLowerInvariant().Contains("door"))
                {
                    totalRemoved += CleanGameObjectRecursively(go);
                }
            }
        }

    Debug.Log($"CleanDoorComponents: cleaned all doors in scene, removed {totalRemoved} components (missing or named legacy door components).");
    }

    private static int CleanGameObjectRecursively(GameObject go)
    {
        int removed = 0;

        // Remove missing script components (editor API helper)
        try
        {
            removed += UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        catch { }

        // DoorRotator logic removed. No longer checking for DoorRotator.

        // Recurse children
        foreach (Transform child in go.transform)
        {
            removed += CleanGameObjectRecursively(child.gameObject);
        }

        return removed;
    }
}
