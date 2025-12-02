#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Editor tool to automatically set up zombie animator with all animations
/// </summary>
public class ZombieAnimatorSetup : EditorWindow
{
    [MenuItem("Tools/Setup Zombie Animations")]
    public static void ShowWindow()
    {
        GetWindow<ZombieAnimatorSetup>("Zombie Animator Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧟 Zombie Animation Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will create an Animator Controller with:", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Running animation", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Attack animation", EditorStyles.wordWrappedLabel);
        GUILayout.Label("• Death animation", EditorStyles.wordWrappedLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("🎬 Create Zombie Animator", GUILayout.Height(40)))
        {
            CreateZombieAnimator();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🔧 Setup Complete Zombie Prefab", GUILayout.Height(40)))
        {
            SetupCompletePrefab();
        }
    }

    private void CreateZombieAnimator()
    {
        // Create Animator Controller
        string path = "Assets/Resources/Enemies/Zombie/ZombieAnimator.controller";
        
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        
        // Add parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        
        // Get the root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Find animation clips
        AnimationClip runClip = FindAnimationClip("Zombie Running");
        AnimationClip attackClip = FindAnimationClip("Zombie Attack");
        AnimationClip deathClip = FindAnimationClip("Zombie Death");
        AnimationClip walkClip = FindAnimationClip("Zombie Walk");
        
        // Use walk as run if no run clip found
        if (runClip == null && walkClip != null)
            runClip = walkClip;
        
        // Create states
        AnimatorState idleState = rootStateMachine.AddState("Idle", new Vector3(0, 0, 0));
        AnimatorState runState = rootStateMachine.AddState("Run", new Vector3(300, 0, 0));
        AnimatorState attackState = rootStateMachine.AddState("Attack", new Vector3(300, 100, 0));
        AnimatorState deathState = rootStateMachine.AddState("Death", new Vector3(150, 200, 0));
        
        // Assign clips
        if (runClip != null)
        {
            runState.motion = runClip;
            idleState.motion = runClip; // Use run for idle too
            Debug.Log($"Assigned run animation: {runClip.name}");
        }
        
        if (attackClip != null)
        {
            attackState.motion = attackClip;
            Debug.Log($"Assigned attack animation: {attackClip.name}");
        }
        
        if (deathClip != null)
        {
            deathState.motion = deathClip;
            Debug.Log($"Assigned death animation: {deathClip.name}");
        }
        
        // Set default state
        rootStateMachine.defaultState = runState;
        
        // Create transitions
        // Idle -> Run (when Speed > 0.1)
        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;
        
        // Run -> Idle (when Speed < 0.1)
        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;
        
        // Any -> Attack (on Attack trigger)
        AnimatorStateTransition toAttack = rootStateMachine.AddAnyStateTransition(attackState);
        toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        toAttack.hasExitTime = false;
        toAttack.duration = 0.1f;
        
        // Attack -> Run (after attack finishes)
        AnimatorStateTransition attackToRun = attackState.AddTransition(runState);
        attackToRun.hasExitTime = true;
        attackToRun.exitTime = 0.9f;
        attackToRun.duration = 0.1f;
        
        // Any -> Death (on Die trigger)
        AnimatorStateTransition toDeath = rootStateMachine.AddAnyStateTransition(deathState);
        toDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");
        toDeath.hasExitTime = false;
        toDeath.duration = 0.1f;
        
        // Save
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success! 🎬", 
            $"Zombie Animator created at:\n{path}\n\n" +
            "Animations found:\n" +
            $"• Run: {(runClip != null ? "✅" : "❌")}\n" +
            $"• Attack: {(attackClip != null ? "✅" : "❌")}\n" +
            $"• Death: {(deathClip != null ? "✅" : "❌")}", 
            "OK");
        
        // Select the controller
        Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
    }
    
    private AnimationClip FindAnimationClip(string name)
    {
        // Search in multiple locations
        string[] searchPaths = new string[]
        {
            "Assets/Models/Enemies/Zombie/Animations",
            "Assets/Resources/Enemies/Zombie/Animations",
            "Assets/Models/Enemies/Zombie/source",
            "Assets/Resources/Enemies/Zombie/source",
            "Assets/Models/Enemies/Zombie",
            "Assets/Resources/Enemies/Zombie"
        };
        
        foreach (string searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath)) continue;
            
            string[] files = Directory.GetFiles(searchPath, "*.fbx", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (file.ToLower().Contains(name.ToLower().Replace(" ", "")))
                {
                    // Load all assets from the FBX
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(file.Replace("\\", "/"));
                    foreach (Object asset in assets)
                    {
                        if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                        {
                            return clip;
                        }
                    }
                }
            }
        }
        
        // Also try finding by partial name
        string[] guids = AssetDatabase.FindAssets($"t:AnimationClip {name}");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip != null && !clip.name.StartsWith("__preview__"))
            {
                return clip;
            }
        }
        
        return null;
    }
    
    private void SetupCompletePrefab()
    {
        // First create the animator
        CreateZombieAnimator();
        
        // Then run the zombie setup
        ZombieSetupTool tool = GetWindow<ZombieSetupTool>();
        // The tool window will open, user needs to click the button
        
        EditorUtility.DisplayDialog("Next Step", 
            "Now click 'Auto-Setup Zombie Prefab' in the Zombie Setup window that opened.", 
            "OK");
    }
}
#endif

