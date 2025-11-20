using UnityEngine;

public class CollisionSetupHelper : MonoBehaviour
{
    [Header("Collision Setup Tool")]
    [Tooltip("Use the right-click menu options to setup collisions")]
    public bool setupInstructions = true;
    
    [Header("Filter Settings")]
    [Tooltip("Minimum size to add collider (objects smaller than this are ignored)")]
    public float minimumSize = 0.3f;
    
    [Tooltip("Use strict filtering (only add to specific objects). Uncheck to add to all objects")]
    public bool useStrictFiltering = false;
    
    [Tooltip("Object names containing these words will be ignored (case insensitive)")]
    public string[] ignoreKeywords = new string[] 
    { 
        "paper", "light", "lamp", "bulb", "cable", "wire", 
        "particle", "effect", "decal", "poster", "sign",
        "trash", "litter", "leaf", "debris", "ceiling", "roof",
        "window_glass", "glass", "transparent", "floor_detail",
        "carpet", "tile_detail", "wall_detail", "trim", "molding",
        "vent", "grate", "small", "tiny"
    };
    
    [Tooltip("Only add colliders to objects with these keywords (only used if Use Strict Filtering is checked)")]
    public string[] onlyIncludeKeywords = new string[]
    {
        "wall", "door", "locker", "fence", "pillar", "desk",
        "table", "chair", "building", "stairs", "railing"
    };
    
    [ContextMenu("Setup All Collisions (Recommended)")]
    public void SetupAllCollisions()
    {
        Debug.Log("<color=yellow>Starting collision setup...</color>");
        
        int meshCollidersAdded = 0;
        int boxCollidersAdded = 0;
        int convexFixed = 0;
        int triggersFixed = 0;
        
        // Handle all SkinnedMeshRenderers (doors, animated objects)
        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in skinnedMeshes)
        {
            // Skip if it's the player
            if (smr.GetComponent<CharacterController>() != null) continue;
            
            // Skip if object should be ignored
            if (ShouldIgnoreObject(smr.gameObject)) continue;
            
            MeshCollider mc = smr.GetComponent<MeshCollider>();
            
            if (mc == null)
            {
                // Add new mesh collider
                mc = smr.gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
                mc.isTrigger = false;
                meshCollidersAdded++;
                Debug.Log($"✓ Added MeshCollider to: {smr.gameObject.name}");
            }
            else
            {
                // Fix existing mesh collider
                if (!mc.convex)
                {
                    mc.convex = true;
                    convexFixed++;
                    Debug.Log($"✓ Fixed convex on: {smr.gameObject.name}");
                }
                if (mc.isTrigger)
                {
                    mc.isTrigger = false;
                    triggersFixed++;
                    Debug.Log($"✓ Fixed trigger on: {smr.gameObject.name}");
                }
            }
        }
        
        // Handle all MeshRenderers (walls, lockers, static objects)
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in meshRenderers)
        {
            // Skip if it's the player
            if (mr.GetComponent<CharacterController>() != null) continue;
            
            // Skip if already has a collider
            if (mr.GetComponent<Collider>() != null) continue;
            
            // Skip if object should be ignored
            if (ShouldIgnoreObject(mr.gameObject)) continue;
            
            // Decide between Box and Mesh collider based on complexity
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                int vertexCount = mf.sharedMesh.vertexCount;
                
                // Use Box Collider for simple objects, Mesh for complex
                if (vertexCount < 100)
                {
                    BoxCollider bc = mr.gameObject.AddComponent<BoxCollider>();
                    bc.isTrigger = false;
                    boxCollidersAdded++;
                    Debug.Log($"✓ Added BoxCollider to: {mr.gameObject.name}");
                }
                else
                {
                    MeshCollider mc = mr.gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                    mc.isTrigger = false;
                    meshCollidersAdded++;
                    Debug.Log($"✓ Added MeshCollider to: {mr.gameObject.name}");
                }
            }
        }
        
        Debug.Log($"<color=green><b>═══ COLLISION SETUP COMPLETE ═══</b></color>");
        Debug.Log($"<color=green>✓ Mesh Colliders Added: {meshCollidersAdded}</color>");
        Debug.Log($"<color=green>✓ Box Colliders Added: {boxCollidersAdded}</color>");
        Debug.Log($"<color=green>✓ Convex Fixed: {convexFixed}</color>");
        Debug.Log($"<color=green>✓ Triggers Fixed: {triggersFixed}</color>");
        Debug.Log($"<color=cyan>Total objects processed: {meshCollidersAdded + boxCollidersAdded + convexFixed + triggersFixed}</color>");
    }
    
    [ContextMenu("Fix Only Existing Colliders")]
    public void FixExistingColliders()
    {
        int fixedCount = 0;
        
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>(true);
        foreach (MeshCollider mc in meshColliders)
        {
            bool changed = false;
            
            if (!mc.convex)
            {
                mc.convex = true;
                changed = true;
            }
            
            if (mc.isTrigger)
            {
                mc.isTrigger = false;
                changed = true;
            }
            
            if (changed)
            {
                fixedCount++;
                Debug.Log($"✓ Fixed: {mc.gameObject.name}");
            }
        }
        
        BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider bc in boxColliders)
        {
            if (bc.isTrigger)
            {
                bc.isTrigger = false;
                fixedCount++;
                Debug.Log($"✓ Fixed trigger on: {bc.gameObject.name}");
            }
        }
        
        Debug.Log($"<color=green>✓ Fixed {fixedCount} colliders</color>");
    }
    
    // Helper method to check if object should be ignored
    private bool ShouldIgnoreObject(GameObject obj)
    {
        string objNameLower = obj.name.ToLower();
        
        // If strict filtering is enabled, check if object matches include keywords
        if (useStrictFiltering && onlyIncludeKeywords.Length > 0)
        {
            bool matchesInclude = false;
            foreach (string keyword in onlyIncludeKeywords)
            {
                if (objNameLower.Contains(keyword.ToLower()))
                {
                    matchesInclude = true;
                    break;
                }
            }
            
            // If it doesn't match any include keywords, skip it
            if (!matchesInclude)
            {
                Debug.Log($"⊘ Skipped (not in include list): {obj.name}");
                return true;
            }
        }
        
        // Check if object name contains any ignore keywords
        foreach (string keyword in ignoreKeywords)
        {
            if (objNameLower.Contains(keyword.ToLower()))
            {
                Debug.Log($"⊘ Skipped (keyword): {obj.name}");
                return true;
            }
        }
        
        // Check object size (bounds)
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            
            if (maxSize < minimumSize)
            {
                Debug.Log($"⊘ Skipped (too small): {obj.name} (size: {maxSize:F2})");
                return true;
            }
        }
        
        return false;
    }
    
    [ContextMenu("Remove All Colliders (Clean Slate)")]
    public void RemoveAllColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        int removed = 0;
        
        foreach (Collider col in colliders)
        {
            if (!(col is CharacterController))
            {
                DestroyImmediate(col);
                removed++;
            }
        }
        
        Debug.Log($"<color=yellow>Removed {removed} colliders</color>");
    }
    
    [ContextMenu("Verify Collision Setup")]
    public void VerifySetup()
    {
        Debug.Log("<color=cyan>═══ VERIFICATION REPORT ═══</color>");
        
        int objectsWithoutColliders = 0;
        int nonConvexMeshColliders = 0;
        int triggersEnabled = 0;
        int goodColliders = 0;
        
        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in skinnedMeshes)
        {
            if (smr.GetComponent<CharacterController>() != null) continue;
            
            Collider col = smr.GetComponent<Collider>();
            if (col == null)
            {
                objectsWithoutColliders++;
                Debug.LogWarning($"⚠ No collider: {smr.gameObject.name}");
            }
            else if (col is MeshCollider mc)
            {
                if (!mc.convex)
                {
                    nonConvexMeshColliders++;
                    Debug.LogWarning($"⚠ Not convex: {smr.gameObject.name}");
                }
                if (mc.isTrigger)
                {
                    triggersEnabled++;
                    Debug.LogWarning($"⚠ Is trigger: {smr.gameObject.name}");
                }
                if (mc.convex && !mc.isTrigger) goodColliders++;
            }
        }
        
        Debug.Log($"<color=yellow>Objects without colliders: {objectsWithoutColliders}</color>");
        Debug.Log($"<color=yellow>Non-convex mesh colliders: {nonConvexMeshColliders}</color>");
        Debug.Log($"<color=yellow>Triggers enabled: {triggersEnabled}</color>");
        Debug.Log($"<color=green>Good colliders: {goodColliders}</color>");
        
        if (objectsWithoutColliders == 0 && nonConvexMeshColliders == 0 && triggersEnabled == 0)
        {
            Debug.Log($"<color=green><b>✓ ALL COLLISIONS ARE PROPERLY CONFIGURED!</b></color>");
        }
        else
        {
            Debug.Log($"<color=red>⚠ Issues found. Run 'Setup All Collisions' to fix.</color>");
        }
    }
}
