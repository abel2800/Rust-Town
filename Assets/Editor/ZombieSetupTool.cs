#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to automatically set up the zombie model as a prefab
/// </summary>
public class ZombieSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Zombie Model")]
    public static void ShowWindow()
    {
        GetWindow<ZombieSetupTool>("Zombie Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧟 Zombie Model Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will automatically create a prefab from your zombie model.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("🔧 Auto-Setup Zombie Prefab", GUILayout.Height(40)))
        {
            SetupZombiePrefab();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("Manual Steps (if auto doesn't work):", EditorStyles.boldLabel);
        GUILayout.Label("1. Find model.glb in Assets/Models/Enemies/Zombie/source/", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. Drag it to the Scene view", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. Drag from Hierarchy to Assets/Resources/Enemies/Zombie/", EditorStyles.wordWrappedLabel);
        GUILayout.Label("4. Rename to 'ZombiePrefab'", EditorStyles.wordWrappedLabel);
    }

    private void SetupZombiePrefab()
    {
        // Find the model file
        string[] possiblePaths = new string[]
        {
            "Assets/Models/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Resources/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Models/Enemies/Zombie/source/model.glb",
            "Assets/Models/Enemies/Zombie/source/model.fbx",
            "Assets/Resources/Enemies/Zombie/source/model.glb",
            "Assets/Resources/Enemies/Zombie/source/model"
        };

        GameObject modelPrefab = null;
        string foundPath = null;

        foreach (string path in possiblePaths)
        {
            modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (modelPrefab != null)
            {
                foundPath = path;
                Debug.Log($"Found model at: {path}");
                break;
            }
        }

        if (modelPrefab == null)
        {
            // Try to find any .glb file
            string[] glbFiles = Directory.GetFiles(Application.dataPath, "*.glb", SearchOption.AllDirectories);
            if (glbFiles.Length > 0)
            {
                string relativePath = "Assets" + glbFiles[0].Substring(Application.dataPath.Length).Replace("\\", "/");
                modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                foundPath = relativePath;
                Debug.Log($"Found GLB at: {relativePath}");
            }
        }

        if (modelPrefab == null)
        {
            EditorUtility.DisplayDialog("Model Not Found", 
                "Could not find the zombie model. Please make sure it's in:\n" +
                "Assets/Models/Enemies/Zombie/source/model.glb\n\n" +
                "You may need to manually drag it to the scene and create a prefab.", 
                "OK");
            return;
        }

        // Create Resources folder if it doesn't exist
        string resourcesPath = "Assets/Resources/Enemies/Zombie";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Enemies"))
            AssetDatabase.CreateFolder("Assets/Resources", "Enemies");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Enemies/Zombie"))
            AssetDatabase.CreateFolder("Assets/Resources/Enemies", "Zombie");

        // Instantiate the model
        GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
        if (instance == null)
        {
            instance = Instantiate(modelPrefab);
        }
        
        instance.name = "ZombiePrefab";
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        
        // Adjust scale if needed (common issue with GLB files)
        // Try to detect if scale is wrong
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            
            float height = bounds.size.y;
            Debug.Log($"Model height: {height}");
            
            // If model is tiny or huge, adjust scale
            if (height < 0.1f)
            {
                instance.transform.localScale = Vector3.one * 100f;
                Debug.Log("Model was very small, scaled up 100x");
            }
            else if (height > 10f)
            {
                instance.transform.localScale = Vector3.one * 0.01f;
                Debug.Log("Model was very large, scaled down to 0.01x");
            }
        }

        // Save as prefab
        string prefabPath = $"{resourcesPath}/ZombiePrefab.prefab";
        
        // Delete existing prefab if it exists
        if (File.Exists(Application.dataPath.Replace("Assets", "") + prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        // Create the prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        
        // Clean up the scene instance
        DestroyImmediate(instance);

        if (prefab != null)
        {
            EditorUtility.DisplayDialog("Success! 🧟", 
                $"Zombie prefab created at:\n{prefabPath}\n\n" +
                "Press Play to test - your zombie model should now appear!", 
                "Awesome!");
            
            // Ping the prefab in project window
            EditorGUIUtility.PingObject(prefab);
            Selection.activeObject = prefab;
            
            Debug.Log($"✅ Zombie prefab created successfully at {prefabPath}");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", 
                "Failed to create prefab. Please try the manual method.", 
                "OK");
        }

        AssetDatabase.Refresh();
    }
}
#endif

