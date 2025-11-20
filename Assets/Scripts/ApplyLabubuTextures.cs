using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApplyLabubuTextures : MonoBehaviour
{
    [Header("Texture Files")]
    [Tooltip("Drag textures from Assets/Enemy/Labubu/textures folder")]
    public Texture2D bodyBaseColor;
    public Texture2D bodyNormal;
    public Texture2D bodyRoughness;
    public Texture2D bodyAO;
    
    public Texture2D headBaseColor;
    public Texture2D headNormal;
    public Texture2D headRoughness;
    
    [Header("Auto Find Textures")]
    [Tooltip("Check this to automatically find textures in the textures folder")]
    public bool autoFindTextures = true;
    
    [ContextMenu("Apply Textures to Labubu")]
    public void ApplyTextures()
    {
        if (autoFindTextures)
        {
            LoadTexturesFromResources();
        }
        
        // Create materials
        Material bodyMaterial = CreateBodyMaterial();
        Material headMaterial = CreateHeadMaterial();
        
        if (bodyMaterial == null || headMaterial == null)
        {
            Debug.LogError("Failed to create materials! Make sure textures are assigned.");
            return;
        }
        
        #if UNITY_EDITOR
        // Save materials as assets
        SaveMaterialAsset(bodyMaterial, "Assets/Materials/LabubuBody.mat");
        SaveMaterialAsset(headMaterial, "Assets/Materials/LabubuHead.mat");
        #endif
        
        // Apply materials to mesh renderers
        ApplyMaterialsToModel(bodyMaterial, headMaterial);
        
        Debug.Log("<color=green>✓ Textures applied to Labubu!</color>");
    }
    
    #if UNITY_EDITOR
    void SaveMaterialAsset(Material mat, string path)
    {
        // Create Materials folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        
        // Save or update the material
        Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existingMat != null)
        {
            EditorUtility.CopySerialized(mat, existingMat);
            AssetDatabase.SaveAssets();
            Debug.Log($"✓ Updated existing material: {path}");
        }
        else
        {
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"✓ Created new material: {path}");
        }
    }
    #endif
    
    void LoadTexturesFromResources()
    {
        // Try to load textures from the textures folder
        string basePath = "Enemy/Labubu/textures/";
        
        bodyBaseColor = Resources.Load<Texture2D>(basePath + "Body_low_Base_color");
        bodyNormal = Resources.Load<Texture2D>(basePath + "Body_low_Normal_OpenGL");
        bodyRoughness = Resources.Load<Texture2D>(basePath + "Body_low_Roughness");
        bodyAO = Resources.Load<Texture2D>(basePath + "Body_low_Mixed_AO");
        
        headBaseColor = Resources.Load<Texture2D>(basePath + "Head_Low_Base_color");
        headNormal = Resources.Load<Texture2D>(basePath + "Head_Low_Normal_OpenGL");
        headRoughness = Resources.Load<Texture2D>(basePath + "Head_Low_Roughness");
        
        Debug.Log("Attempted to load textures from Resources folder");
    }
    
    Material CreateBodyMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "LabubuBody";
        
        if (bodyBaseColor != null)
        {
            mat.mainTexture = bodyBaseColor;
            Debug.Log("✓ Body base color applied");
        }
        else
        {
            Debug.LogWarning("⚠ Body base color texture not found!");
        }
        
        if (bodyNormal != null)
        {
            mat.SetTexture("_BumpMap", bodyNormal);
            mat.EnableKeyword("_NORMALMAP");
            Debug.Log("✓ Body normal map applied");
        }
        
        if (bodyRoughness != null)
        {
            mat.SetTexture("_MetallicGlossMap", bodyRoughness);
            Debug.Log("✓ Body roughness applied");
        }
        
        if (bodyAO != null)
        {
            mat.SetTexture("_OcclusionMap", bodyAO);
            mat.EnableKeyword("_OCCLUSIONMAP");
            Debug.Log("✓ Body AO applied");
        }
        
        return mat;
    }
    
    Material CreateHeadMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "LabubuHead";
        
        if (headBaseColor != null)
        {
            mat.mainTexture = headBaseColor;
            Debug.Log("✓ Head base color applied");
        }
        else
        {
            Debug.LogWarning("⚠ Head base color texture not found!");
        }
        
        if (headNormal != null)
        {
            mat.SetTexture("_BumpMap", headNormal);
            mat.EnableKeyword("_NORMALMAP");
            Debug.Log("✓ Head normal map applied");
        }
        
        if (headRoughness != null)
        {
            mat.SetTexture("_MetallicGlossMap", headRoughness);
            Debug.Log("✓ Head roughness applied");
        }
        
        return mat;
    }
    
    void ApplyMaterialsToModel(Material bodyMat, Material headMat)
    {
        // Find all SkinnedMeshRenderers and MeshRenderers in children
        SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        
        int appliedCount = 0;
        
        // Apply to SkinnedMeshRenderers
        foreach (SkinnedMeshRenderer smr in skinnedRenderers)
        {
            string objName = smr.gameObject.name.ToLower();
            
            if (objName.Contains("body"))
            {
                smr.material = bodyMat;
                appliedCount++;
                Debug.Log($"✓ Applied body material to {smr.gameObject.name}");
            }
            else if (objName.Contains("head"))
            {
                smr.material = headMat;
                appliedCount++;
                Debug.Log($"✓ Applied head material to {smr.gameObject.name}");
            }
        }
        
        // Apply to MeshRenderers
        foreach (MeshRenderer mr in meshRenderers)
        {
            string objName = mr.gameObject.name.ToLower();
            
            if (objName.Contains("body"))
            {
                mr.material = bodyMat;
                appliedCount++;
                Debug.Log($"✓ Applied body material to {mr.gameObject.name}");
            }
            else if (objName.Contains("head"))
            {
                mr.material = headMat;
                appliedCount++;
                Debug.Log($"✓ Applied head material to {mr.gameObject.name}");
            }
        }
        
        if (appliedCount == 0)
        {
            Debug.LogWarning("⚠ No meshes found to apply materials to! Make sure the Labubu model has child objects named 'Body' or 'Head'");
        }
        else
        {
            Debug.Log($"<color=green>✓ Applied materials to {appliedCount} mesh(es)</color>");
        }
    }
    
    [ContextMenu("Manual Texture Assignment")]
    public void ShowManualInstructions()
    {
        Debug.Log("═══ MANUAL TEXTURE ASSIGNMENT ═══");
        Debug.Log("1. Select the Labubu GameObject");
        Debug.Log("2. In Inspector, find the 'Apply Labubu Textures' script");
        Debug.Log("3. UNCHECK 'Auto Find Textures'");
        Debug.Log("4. Drag textures from Project window:");
        Debug.Log("   - Body Base Color: Body_low_Base_color.png");
        Debug.Log("   - Body Normal: Body_low_Normal_OpenGL.png");
        Debug.Log("   - Body Roughness: Body_low_Roughness.png");
        Debug.Log("   - Body AO: Body_low_Mixed_AO.png");
        Debug.Log("   - Head Base Color: Head_Low_Base_color.png");
        Debug.Log("   - Head Normal: Head_Low_Normal_OpenGL.png");
        Debug.Log("   - Head Roughness: Head_Low_Roughness.png");
        Debug.Log("5. Right-click script → 'Apply Textures to Labubu'");
    }
}
