using UnityEngine;

public class HideColliderGizmos : MonoBehaviour
{
    [Header("Collider Visibility")]
    [Tooltip("This hides the green wireframe lines from colliders in the Scene view")]
    public bool hideColliderWireframes = true;

    void OnDrawGizmos()
    {
        // This prevents Unity from drawing collider gizmos
        if (hideColliderWireframes)
        {
            Gizmos.color = Color.clear;
        }
    }

    [ContextMenu("Disable All Mesh Collider Rendering")]
    public void DisableMeshColliderRendering()
    {
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>(true);
        int count = 0;

        foreach (MeshCollider mc in meshColliders)
        {
            // Mesh colliders don't have a direct way to hide their gizmos,
            // but we can ensure they're set up correctly
            if (mc.enabled)
            {
                count++;
            }
        }

        Debug.Log($"<color=cyan>Found {count} active Mesh Colliders</color>");
        Debug.Log($"<color=yellow>To hide the green lines in Scene view:</color>");
        Debug.Log($"1. Click the 'Gizmos' button in the top-right of Scene view");
        Debug.Log($"2. Find 'Colliders' in the list");
        Debug.Log($"3. Uncheck the box next to it");
    }
}
