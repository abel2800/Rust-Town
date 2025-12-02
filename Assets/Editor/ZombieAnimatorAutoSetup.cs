#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Automatically sets up zombie with proper animations from FBX files
/// </summary>
public class ZombieAnimatorAutoSetup : EditorWindow
{
    [MenuItem("Tools/Auto Setup Zombie Animations")]
    public static void ShowWindow()
    {
        GetWindow<ZombieAnimatorAutoSetup>("Zombie Animator");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧟 Zombie Animation Auto-Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will automatically:", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Find all zombie animation FBX files", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Create an Animator Controller", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Set up Run, Attack, Death states", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Create a working Zombie Prefab", EditorStyles.wordWrappedLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("🎬 AUTO-SETUP EVERYTHING", GUILayout.Height(50)))
        {
            AutoSetupZombie();
        }
    }

    private void AutoSetupZombie()
    {
        Debug.Log("Starting zombie auto-setup...");
        
        // Step 1: Find animation clips
        Dictionary<string, AnimationClip> clips = FindAllZombieAnimations();
        
        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No zombie animations found! Make sure FBX files are imported.", "OK");
            return;
        }
        
        Debug.Log($"Found {clips.Count} animation clips");
        
        // Step 2: Create Animator Controller
        AnimatorController controller = CreateAnimatorController(clips);
        
        // Step 3: Find or create zombie model prefab
        GameObject zombiePrefab = CreateZombiePrefab(controller);
        
        if (zombiePrefab != null)
        {
            Selection.activeObject = zombiePrefab;
            EditorGUIUtility.PingObject(zombiePrefab);
            
            string found = "";
            foreach (var kvp in clips)
            {
                found += $"\n• {kvp.Key}: ✅";
            }
            
            EditorUtility.DisplayDialog("Success! 🧟", 
                $"Zombie setup complete!\n\nAnimations found:{found}\n\nPrefab created at:\nAssets/Resources/Enemies/Zombie/ZombiePrefab.prefab", 
                "OK");
        }
    }
    
    private Dictionary<string, AnimationClip> FindAllZombieAnimations()
    {
        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
        
        // Search paths
        string[] searchFolders = new string[]
        {
            "Assets/Models/Enemies/Zombie",
            "Assets/Resources/Enemies/Zombie"
        };
        
        foreach (string folder in searchFolders)
        {
            if (!Directory.Exists(folder)) continue;
            
            string[] fbxFiles = Directory.GetFiles(folder, "*.fbx", SearchOption.AllDirectories);
            
            foreach (string fbxPath in fbxFiles)
            {
                string assetPath = fbxPath.Replace("\\", "/");
                
                // Load all objects from FBX
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                
                foreach (Object asset in allAssets)
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        string clipName = clip.name.ToLower();
                        string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
                        
                        // Categorize animations
                        if (clipName.Contains("run") || fileName.Contains("running") || fileName.Contains("walk"))
                        {
                            if (!clips.ContainsKey("Run"))
                            {
                                clips["Run"] = clip;
                                Debug.Log($"Found Run animation: {clip.name} from {assetPath}");
                            }
                        }
                        else if (clipName.Contains("attack") || fileName.Contains("attack"))
                        {
                            if (!clips.ContainsKey("Attack"))
                            {
                                clips["Attack"] = clip;
                                Debug.Log($"Found Attack animation: {clip.name} from {assetPath}");
                            }
                        }
                        else if (clipName.Contains("death") || clipName.Contains("die") || fileName.Contains("death"))
                        {
                            if (!clips.ContainsKey("Death"))
                            {
                                clips["Death"] = clip;
                                Debug.Log($"Found Death animation: {clip.name} from {assetPath}");
                            }
                        }
                        else if (clipName.Contains("idle"))
                        {
                            if (!clips.ContainsKey("Idle"))
                            {
                                clips["Idle"] = clip;
                                Debug.Log($"Found Idle animation: {clip.name} from {assetPath}");
                            }
                        }
                        else
                        {
                            // Default - use as run if no run found
                            if (!clips.ContainsKey("Run"))
                            {
                                clips["Run"] = clip;
                                Debug.Log($"Using {clip.name} as default Run animation from {assetPath}");
                            }
                        }
                    }
                }
            }
        }
        
        return clips;
    }
    
    private AnimatorController CreateAnimatorController(Dictionary<string, AnimationClip> clips)
    {
        string controllerPath = "Assets/Resources/Enemies/Zombie/ZombieAnimator.controller";
        
        // Create directory if needed
        string dir = Path.GetDirectoryName(controllerPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        // Delete existing
        if (File.Exists(controllerPath))
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }
        
        // Create new controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // Add parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        
        // Get root state machine
        AnimatorStateMachine rootSM = controller.layers[0].stateMachine;
        
        // Create states
        AnimatorState runState = rootSM.AddState("Run", new Vector3(300, 0, 0));
        AnimatorState attackState = rootSM.AddState("Attack", new Vector3(300, 100, 0));
        AnimatorState deathState = rootSM.AddState("Death", new Vector3(300, 200, 0));
        
        // Assign clips
        if (clips.ContainsKey("Run"))
        {
            runState.motion = clips["Run"];
        }
        if (clips.ContainsKey("Attack"))
        {
            attackState.motion = clips["Attack"];
        }
        if (clips.ContainsKey("Death"))
        {
            deathState.motion = clips["Death"];
        }
        
        // Set default state
        rootSM.defaultState = runState;
        
        // Create transitions
        // Any -> Attack
        AnimatorStateTransition toAttack = rootSM.AddAnyStateTransition(attackState);
        toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        toAttack.duration = 0.1f;
        toAttack.hasExitTime = false;
        
        // Attack -> Run
        AnimatorStateTransition attackToRun = attackState.AddTransition(runState);
        attackToRun.hasExitTime = true;
        attackToRun.exitTime = 0.9f;
        attackToRun.duration = 0.1f;
        
        // Any -> Death
        AnimatorStateTransition toDeath = rootSM.AddAnyStateTransition(deathState);
        toDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");
        toDeath.duration = 0.1f;
        toDeath.hasExitTime = false;
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Animator Controller created!");
        return controller;
    }
    
    private GameObject CreateZombiePrefab(AnimatorController controller)
    {
        // Find the zombie model
        string[] modelPaths = new string[]
        {
            "Assets/Models/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Resources/Enemies/Zombie/source/Zombie Walk.fbx",
            "Assets/Models/Enemies/Zombie/Animations/Zombie Running.fbx"
        };
        
        GameObject modelAsset = null;
        string foundPath = null;
        
        foreach (string path in modelPaths)
        {
            if (File.Exists(path))
            {
                modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (modelAsset != null)
                {
                    foundPath = path;
                    break;
                }
            }
        }
        
        // Also try finding any zombie model
        if (modelAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("Zombie t:Model");
            if (guids.Length > 0)
            {
                foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(foundPath);
            }
        }
        
        if (modelAsset == null)
        {
            Debug.LogError("Could not find zombie model!");
            return null;
        }
        
        Debug.Log($"Using zombie model from: {foundPath}");
        
        // Fix import settings
        ModelImporter importer = AssetImporter.GetAtPath(foundPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            importer.SaveAndReimport();
        }
        
        // Create prefab directory
        string prefabDir = "Assets/Resources/Enemies/Zombie";
        if (!Directory.Exists(prefabDir))
        {
            Directory.CreateDirectory(prefabDir);
        }
        
        // Delete old prefab
        string prefabPath = prefabDir + "/ZombiePrefab.prefab";
        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        // Create instance
        GameObject instance = Instantiate(modelAsset);
        instance.name = "ZombiePrefab";
        
        // Add/setup Animator
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        
        // Add Collider
        if (instance.GetComponent<Collider>() == null)
        {
            CapsuleCollider col = instance.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 1f, 0);
            col.height = 2f;
            col.radius = 0.4f;
        }
        
        // Find and apply textures
        ApplyTexturesToZombie(instance);
        
        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Prefab saved to: {prefabPath}");
        return prefab;
    }
    
    private void ApplyTexturesToZombie(GameObject zombie)
    {
        string[] textureFolders = new string[]
        {
            "Assets/Models/Enemies/Zombie/textures",
            "Assets/Resources/Enemies/Zombie/textures"
        };
        
        Texture2D diffuse = null;
        Texture2D normal = null;
        
        foreach (string folder in textureFolders)
        {
            if (!Directory.Exists(folder)) continue;
            
            string[] files = Directory.GetFiles(folder, "*.png");
            foreach (string file in files)
            {
                string path = file.Replace("\\", "/");
                if (path.ToLower().Contains("diffuse") || path.ToLower().Contains("albedo"))
                {
                    diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                else if (path.ToLower().Contains("normal"))
                {
                    normal = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
        }
        
        // Apply to all renderers
        Renderer[] renderers = zombie.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    // Create new material instance
                    Material newMat = new Material(Shader.Find("Standard"));
                    
                    if (diffuse != null)
                    {
                        newMat.mainTexture = diffuse;
                    }
                    if (normal != null)
                    {
                        newMat.SetTexture("_BumpMap", normal);
                        newMat.EnableKeyword("_NORMALMAP");
                    }
                    
                    mats[i] = newMat;
                }
            }
            r.sharedMaterials = mats;
        }
        
        Debug.Log($"Applied textures - Diffuse: {diffuse != null}, Normal: {normal != null}");
    }
}
#endif

