using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Finds and disables/deletes Global Volume causing color issues
/// </summary>
public class RemoveGlobalVolume : MonoBehaviour
{
    [ContextMenu("Disable Global Volume")]
    public void DisableGlobalVolume()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        
        foreach (Volume volume in volumes)
        {
            if (volume.isGlobal)
            {
                volume.enabled = false;
                Debug.Log($"<color=green>✅ Disabled Global Volume: {volume.gameObject.name}</color>");
            }
        }
        
        if (volumes.Length == 0)
        {
            Debug.LogWarning("No Global Volume found in scene!");
        }
        else
        {
            Debug.Log("<color=cyan>Scene should now show normal colors!</color>");
        }
    }
    
    [ContextMenu("Delete Global Volume")]
    public void DeleteGlobalVolume()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        
        foreach (Volume volume in volumes)
        {
            if (volume.isGlobal)
            {
                Debug.Log($"<color=red>🗑️ Deleting Global Volume: {volume.gameObject.name}</color>");
                DestroyImmediate(volume.gameObject);
            }
        }
        
        if (volumes.Length == 0)
        {
            Debug.LogWarning("No Global Volume found in scene!");
        }
        else
        {
            Debug.Log("<color=green>✅ Global Volume deleted permanently!</color>");
        }
    }
    
    [ContextMenu("List All Volumes")]
    public void ListAllVolumes()
    {
        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        
        Debug.Log($"<color=yellow>Found {volumes.Length} volume(s) in scene:</color>");
        
        foreach (Volume volume in volumes)
        {
            string status = volume.enabled ? "ENABLED" : "DISABLED";
            string global = volume.isGlobal ? "GLOBAL" : "LOCAL";
            Debug.Log($"  - {volume.gameObject.name} [{status}] [{global}] Profile: {volume.sharedProfile?.name}");
        }
    }
}
