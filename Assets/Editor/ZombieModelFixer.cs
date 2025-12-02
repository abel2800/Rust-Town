#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Fixes zombie model import settings and creates proper prefab
/// </summary>
public class ZombieModelFixer : EditorWindow
{
    [MenuItem("Tools/Fix Zombie Model")]
    public static void ShowWindow()
    {
        GetWindow<ZombieModelFixer>("Fix Zombie");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧟 Zombie Model Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will:", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Find your zombie FBX model", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Fix import settings", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Create a proper prefab", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Set correct scale and materials", EditorStyles.wordWrappedLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("🔧 FIX ZOMBIE NOW", GUILayout.Height(50)))
        {
            FixZombieModel();
        }
    }

    private void FixZombieModel()
    {
        // Find the zombie FBX
        string[] searchPaths = new string[]
        {
            "Assets/Models/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Resources/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Models/Enemies/Zombie/Animations/Zombie Running.fbx"
        };
        
        string foundPath = null;
        foreach (string path in searchPaths)
        {
            if (File.Exists(path))
            {
                foundPath = path;
                break;
            }
        }
        
        if (foundPath == null)
        {
            // Search for any zombie fbx
            string[] guids = AssetDatabase.FindAssets("Zombie t:Model");
            if (guids.Length > 0)
            {
                foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }
        }
        
        if (foundPath == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find zombie model!", "OK");
            return;
        }
        
        Debug.Log($"Found zombie model at: {foundPath}");
        
        // Fix import settings
        ModelImporter importer = AssetImporter.GetAtPath(foundPath) as ModelImporter;
        if (importer != null)
        {
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            
            // Apply changes
            importer.SaveAndReimport();
            Debug.Log("Import settings updated!");
        }
        
        // Load the model
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(foundPath);
        if (modelAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not load zombie model!", "OK");
            return;
        }
        
        // Create prefab folder if needed
        string prefabFolder = "Assets/Resources/Enemies/Zombie";
        if (!Directory.Exists(prefabFolder))
        {
            Directory.CreateDirectory(prefabFolder);
            AssetDatabase.Refresh();
        }
        
        // Create instance and set up
        GameObject instance = Instantiate(modelAsset);
        instance.name = "ZombiePrefab";
        
        // Set proper scale
        instance.transform.localScale = Vector3.one;
        
        // Add collider if missing
        if (instance.GetComponent<Collider>() == null)
        {
            CapsuleCollider col = instance.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 1f, 0);
            col.height = 2f;
            col.radius = 0.4f;
        }
        
        // Find and assign textures
        AssignTextures(instance);
        
        // Save as prefab
        string prefabPath = prefabFolder + "/ZombiePrefab.prefab";
        
        // Remove old prefab if exists
        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the new prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        
        EditorUtility.DisplayDialog("Success! 🧟", 
            $"Zombie prefab created at:\n{prefabPath}\n\n" +
            "The game will now use this model!", 
            "OK");
    }
    
    private void AssignTextures(GameObject zombie)
    {
        // Find textures
        string[] texturePaths = new string[]
        {
            "Assets/Models/Enemies/Zombie/textures",
            "Assets/Resources/Enemies/Zombie/textures"
        };
        
        Texture2D diffuse = null;
        Texture2D normal = null;
        
        foreach (string texPath in texturePaths)
        {
            if (Directory.Exists(texPath))
            {
                string[] files = Directory.GetFiles(texPath, "*.png");
                foreach (string file in files)
                {
                    string path = file.Replace("\\", "/");
                    if (path.ToLower().Contains("diffuse"))
                    {
                        diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                    else if (path.ToLower().Contains("normal"))
                    {
                        normal = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    }
                }
            }
        }
        
        // Apply textures to renderers
        Renderer[] renderers = zombie.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat != null)
                {
                    if (diffuse != null && mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", diffuse);
                    }
                    if (normal != null && mat.HasProperty("_BumpMap"))
                    {
                        mat.SetTexture("_BumpMap", normal);
                    }
                }
            }
        }
        
        Debug.Log($"Textures assigned - Diffuse: {diffuse != null}, Normal: {normal != null}");
    }
}
#endif

