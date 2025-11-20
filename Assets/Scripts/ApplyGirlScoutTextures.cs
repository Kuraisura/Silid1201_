using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplyGirlScoutTextures : MonoBehaviour
{
    [Header("Texture Files - Drag from Project")]
    public Texture2D diffuseTexture;
    public Texture2D normalTexture;
    public Texture2D specularTexture;
    
    [Header("Settings")]
    public bool autoFindTextures = true;
    public string textureBaseName = "GirlScout";
    
    [ContextMenu("Apply Girl Scout Textures")]
    public void ApplyTextures()
    {
        if (autoFindTextures)
        {
            FindTexturesAutomatically();
        }
        
        if (diffuseTexture == null)
        {
            Debug.LogError("❌ No diffuse texture found! Please assign it manually.");
            return;
        }
        
        // Find and update materials
        UpdateMaterials();
        
        Debug.Log("<color=green>✓ Girl Scout textures applied!</color>");
    }
    
    void FindTexturesAutomatically()
    {
        #if UNITY_EDITOR
        // Search for textures in the project
        string[] guids = AssetDatabase.FindAssets($"{textureBaseName} t:Texture2D");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
            
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            if (fileName.Contains("diffuse") || fileName.Contains("albedo") || fileName.Contains("base"))
            {
                diffuseTexture = tex;
                Debug.Log($"✓ Found diffuse: {path}");
            }
            else if (fileName.Contains("normal"))
            {
                normalTexture = tex;
                Debug.Log($"✓ Found normal: {path}");
            }
            else if (fileName.Contains("specular") || fileName.Contains("metallic") || fileName.Contains("roughness"))
            {
                specularTexture = tex;
                Debug.Log($"✓ Found specular: {path}");
            }
        }
        #else
        Debug.LogWarning("Auto-find only works in Unity Editor");
        #endif
    }
    
    void UpdateMaterials()
    {
        // Get all renderers on this GameObject and its children
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("⚠ No renderers found on this GameObject!");
            return;
        }
        
        int materialsUpdated = 0;
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    // Create a new instance to avoid modifying the shared material
                    Material mat = new Material(materials[i]);
                    
                    // Apply diffuse texture
                    if (diffuseTexture != null)
                    {
                        mat.mainTexture = diffuseTexture;
                        mat.SetTexture("_MainTex", diffuseTexture);
                        Debug.Log($"✓ Applied diffuse to {mat.name}");
                    }
                    
                    // Apply normal map
                    if (normalTexture != null)
                    {
                        mat.SetTexture("_BumpMap", normalTexture);
                        mat.EnableKeyword("_NORMALMAP");
                        Debug.Log($"✓ Applied normal map to {mat.name}");
                    }
                    
                    // Apply specular/metallic
                    if (specularTexture != null)
                    {
                        mat.SetTexture("_MetallicGlossMap", specularTexture);
                        Debug.Log($"✓ Applied specular to {mat.name}");
                    }
                    
                    // Make sure we're using Standard shader
                    if (mat.shader.name != "Standard")
                    {
                        mat.shader = Shader.Find("Standard");
                        Debug.Log($"✓ Changed shader to Standard on {mat.name}");
                    }
                    
                    #if UNITY_EDITOR
                    // Save the material as an asset
                    string materialPath = $"Assets/Materials/{mat.name}_Updated.mat";
                    
                    // Create Materials folder if needed
                    if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Materials");
                    }
                    
                    Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (existingMat != null)
                    {
                        EditorUtility.CopySerialized(mat, existingMat);
                        AssetDatabase.SaveAssets();
                        materials[i] = existingMat;
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(mat, materialPath);
                        AssetDatabase.SaveAssets();
                        materials[i] = mat;
                    }
                    #else
                    materials[i] = mat;
                    #endif
                    
                    materialsUpdated++;
                }
            }
            
            renderer.sharedMaterials = materials;
        }
        
        Debug.Log($"<color=cyan>✓ Updated {materialsUpdated} material(s)</color>");
    }
    
    [ContextMenu("Find Textures in Project")]
    public void FindTextures()
    {
        FindTexturesAutomatically();
        
        if (diffuseTexture != null)
        {
            Debug.Log($"<color=green>✓ Diffuse texture found: {diffuseTexture.name}</color>");
        }
        else
        {
            Debug.LogWarning("⚠ No diffuse texture found!");
        }
        
        if (normalTexture != null)
        {
            Debug.Log($"<color=green>✓ Normal texture found: {normalTexture.name}</color>");
        }
        
        if (specularTexture != null)
        {
            Debug.Log($"<color=green>✓ Specular texture found: {specularTexture.name}</color>");
        }
    }
}
