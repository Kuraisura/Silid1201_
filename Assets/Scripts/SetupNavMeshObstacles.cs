using UnityEngine;
using UnityEngine.AI;

public class SetupNavMeshObstacles : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Add NavMesh Obstacle to objects with colliders")]
    public bool addNavMeshObstacles = true;
    
    [Tooltip("Objects to ignore (won't add obstacles to these)")]
    public string[] ignoreKeywords = new string[] 
    { 
        "player", "enemy", "ground", "floor", "ceiling", "wall_detail"
    };
    
    [Header("NavMesh Obstacle Settings")]
    public bool carveOnlyStationary = true;
    public float carveTimeToStationary = 0.5f;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    [ContextMenu("Setup NavMesh Obstacles for All Objects")]
    public void SetupObstacles()
    {
        int obstaclesAdded = 0;
        int skipped = 0;
        
        // Get all colliders in the scene
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        
        foreach (Collider col in allColliders)
        {
            // Skip if it's a trigger
            if (col.isTrigger)
            {
                if (showDebug) Debug.Log($"⊘ Skipped trigger: {col.gameObject.name}");
                skipped++;
                continue;
            }
            
            // Skip if it's a CharacterController
            if (col is CharacterController)
            {
                if (showDebug) Debug.Log($"⊘ Skipped CharacterController: {col.gameObject.name}");
                skipped++;
                continue;
            }
            
            // Skip if name contains ignore keywords
            bool shouldIgnore = false;
            string objNameLower = col.gameObject.name.ToLower();
            foreach (string keyword in ignoreKeywords)
            {
                if (objNameLower.Contains(keyword.ToLower()))
                {
                    shouldIgnore = true;
                    if (showDebug) Debug.Log($"⊘ Skipped (keyword): {col.gameObject.name}");
                    skipped++;
                    break;
                }
            }
            
            if (shouldIgnore) continue;
            
            // Check if already has NavMeshObstacle
            NavMeshObstacle obstacle = col.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                if (showDebug) Debug.Log($"• Already has obstacle: {col.gameObject.name}");
                continue;
            }
            
            // Add NavMesh Obstacle
            if (addNavMeshObstacles)
            {
                obstacle = col.gameObject.AddComponent<NavMeshObstacle>();
                obstacle.carving = true;
                obstacle.carveOnlyStationary = carveOnlyStationary;
                obstacle.carvingTimeToStationary = carveTimeToStationary;
                
                // Try to match the collider bounds
                if (col is BoxCollider)
                {
                    BoxCollider box = col as BoxCollider;
                    obstacle.center = box.center;
                    obstacle.size = box.size;
                    obstacle.shape = NavMeshObstacleShape.Box;
                }
                else if (col is CapsuleCollider)
                {
                    CapsuleCollider capsule = col as CapsuleCollider;
                    obstacle.center = capsule.center;
                    obstacle.radius = capsule.radius;
                    obstacle.height = capsule.height;
                    obstacle.shape = NavMeshObstacleShape.Capsule;
                }
                else
                {
                    // For mesh colliders and others, use bounds
                    Bounds bounds = col.bounds;
                    obstacle.center = col.transform.InverseTransformPoint(bounds.center);
                    obstacle.size = bounds.size;
                    obstacle.shape = NavMeshObstacleShape.Box;
                }
                
                obstaclesAdded++;
                if (showDebug) Debug.Log($"✓ Added NavMeshObstacle to: {col.gameObject.name}");
            }
        }
        
        Debug.Log($"<color=green>═══ NAVMESH OBSTACLES SETUP COMPLETE ═══</color>");
        Debug.Log($"<color=green>✓ Obstacles Added: {obstaclesAdded}</color>");
        Debug.Log($"<color=yellow>⊘ Objects Skipped: {skipped}</color>");
        Debug.Log($"<color=cyan>Total Colliders Processed: {allColliders.Length}</color>");
    }
    
    [ContextMenu("Remove All NavMesh Obstacles")]
    public void RemoveAllObstacles()
    {
        NavMeshObstacle[] obstacles = FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
        int removed = 0;
        
        foreach (NavMeshObstacle obstacle in obstacles)
        {
            DestroyImmediate(obstacle);
            removed++;
        }
        
        Debug.Log($"<color=yellow>Removed {removed} NavMesh Obstacles</color>");
    }
    
    [ContextMenu("Setup NavMesh Obstacles for Children Only")]
    public void SetupObstaclesForChildren()
    {
        int obstaclesAdded = 0;
        
        // Get all colliders in children
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        
        foreach (Collider col in childColliders)
        {
            // Same checks as above
            if (col.isTrigger || col is CharacterController)
                continue;
            
            // Skip if name contains ignore keywords
            bool shouldIgnore = false;
            string objNameLower = col.gameObject.name.ToLower();
            foreach (string keyword in ignoreKeywords)
            {
                if (objNameLower.Contains(keyword.ToLower()))
                {
                    shouldIgnore = true;
                    break;
                }
            }
            
            if (shouldIgnore) continue;
            
            // Check if already has NavMeshObstacle
            if (col.GetComponent<NavMeshObstacle>() != null)
                continue;
            
            // Add NavMesh Obstacle
            NavMeshObstacle obstacle = col.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.carveOnlyStationary = carveOnlyStationary;
            obstacle.carvingTimeToStationary = carveTimeToStationary;
            
            // Set size based on collider
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                obstacle.center = box.center;
                obstacle.size = box.size;
                obstacle.shape = NavMeshObstacleShape.Box;
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = col as CapsuleCollider;
                obstacle.center = capsule.center;
                obstacle.radius = capsule.radius;
                obstacle.height = capsule.height;
                obstacle.shape = NavMeshObstacleShape.Capsule;
            }
            else
            {
                Bounds bounds = col.bounds;
                obstacle.center = col.transform.InverseTransformPoint(bounds.center);
                obstacle.size = bounds.size;
                obstacle.shape = NavMeshObstacleShape.Box;
            }
            
            obstaclesAdded++;
        }
        
        Debug.Log($"<color=green>✓ Added {obstaclesAdded} NavMesh Obstacles to children</color>");
    }
}
