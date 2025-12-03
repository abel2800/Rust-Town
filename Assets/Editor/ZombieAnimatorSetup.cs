using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace NeonArena.Editor
{
    /// <summary>
    /// Editor tool to set up zombie animator controller with proper animations
    /// </summary>
    public class ZombieAnimatorSetup : EditorWindow
    {
        [MenuItem("Tools/Setup Zombie Animator")]
        public static void ShowWindow()
        {
            GetWindow<ZombieAnimatorSetup>("Zombie Animator Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Zombie Animator Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This will create an Animator Controller using:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Running animation (for movement)", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Attack animation", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Death animation", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);
            
            if (GUILayout.Button("Create Zombie Animator Controller", GUILayout.Height(40)))
            {
                CreateZombieAnimator();
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Apply Animator to Zombie Prefab", GUILayout.Height(30)))
            {
                ApplyAnimatorToPrefab();
            }
        }

        private static void CreateZombieAnimator()
        {
            string animatorPath = "Assets/Resources/Enemies/Zombie/ZombieAnimator.controller";
            
            // Delete existing if present
            if (File.Exists(animatorPath))
            {
                AssetDatabase.DeleteAsset(animatorPath);
            }
            
            // Create the animator controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);
            
            // Add parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Bite", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
            
            // Get the root state machine
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            
            // Find animation clips from the Scary Zombie Pack
            string animFolder = "Assets/Resources/Enemies/Zombie";
            
            // Load animations from the new pack (lowercase names)
            AnimationClip idleClip = LoadAnimationFromFBX(animFolder + "/zombie idle.fbx");
            AnimationClip walkClip = LoadAnimationFromFBX(animFolder + "/zombie walk.fbx");
            AnimationClip runClip = LoadAnimationFromFBX(animFolder + "/zombie run.fbx");
            AnimationClip attackClip = LoadAnimationFromFBX(animFolder + "/zombie attack.fbx");
            AnimationClip biteClip = LoadAnimationFromFBX(animFolder + "/zombie biting.fbx");
            AnimationClip deathClip = LoadAnimationFromFBX(animFolder + "/zombie death.fbx");
            AnimationClip dyingClip = LoadAnimationFromFBX(animFolder + "/zombie dying.fbx");
            AnimationClip screamClip = LoadAnimationFromFBX(animFolder + "/zombie scream.fbx");
            
            // Create states
            AnimatorState idleState = rootStateMachine.AddState("Idle", new Vector3(100, 0, 0));
            AnimatorState walkState = rootStateMachine.AddState("Walk", new Vector3(250, -50, 0));
            AnimatorState runState = rootStateMachine.AddState("Run", new Vector3(250, 50, 0));
            AnimatorState attackState = rootStateMachine.AddState("Attack", new Vector3(400, 0, 0));
            AnimatorState biteState = rootStateMachine.AddState("Bite", new Vector3(400, 100, 0));
            AnimatorState deathState = rootStateMachine.AddState("Death", new Vector3(250, 200, 0));
            AnimatorState screamState = rootStateMachine.AddState("Scream", new Vector3(100, 100, 0));
            
            // Assign clips
            if (idleClip != null) { idleState.motion = idleClip; Debug.Log("✅ Idle animation assigned"); }
            if (walkClip != null) { walkState.motion = walkClip; Debug.Log("✅ Walk animation assigned"); }
            if (runClip != null) { runState.motion = runClip; Debug.Log("✅ Run animation assigned"); }
            else Debug.LogError("❌ Could not find Run animation!");
            
            if (attackClip != null) { attackState.motion = attackClip; Debug.Log("✅ Attack animation assigned"); }
            else Debug.LogError("❌ Could not find Attack animation!");
            
            if (biteClip != null) { biteState.motion = biteClip; Debug.Log("✅ Bite animation assigned"); }
            
            if (deathClip != null) { deathState.motion = deathClip; Debug.Log("✅ Death animation assigned"); }
            else if (dyingClip != null) { deathState.motion = dyingClip; Debug.Log("✅ Dying animation assigned to Death state"); }
            else Debug.LogError("❌ Could not find Death animation!");
            
            if (screamClip != null) { screamState.motion = screamClip; Debug.Log("✅ Scream animation assigned"); }
            
            // Set default state to Run (zombie should always be running toward player)
            rootStateMachine.defaultState = runState;
            
            // Transitions
            
            // Run -> Attack (on Attack trigger)
            AnimatorStateTransition runToAttack = runState.AddTransition(attackState);
            runToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            runToAttack.hasExitTime = false;
            runToAttack.duration = 0.15f;
            
            // Attack -> Run (after attack animation finishes)
            AnimatorStateTransition attackToRun = attackState.AddTransition(runState);
            attackToRun.hasExitTime = true;
            attackToRun.exitTime = 0.85f;
            attackToRun.duration = 0.15f;
            
            // Run -> Bite (alternate attack)
            AnimatorStateTransition runToBite = runState.AddTransition(biteState);
            runToBite.AddCondition(AnimatorConditionMode.If, 0, "Bite");
            runToBite.hasExitTime = false;
            runToBite.duration = 0.15f;
            
            // Bite -> Run
            AnimatorStateTransition biteToRun = biteState.AddTransition(runState);
            biteToRun.hasExitTime = true;
            biteToRun.exitTime = 0.85f;
            biteToRun.duration = 0.15f;
            
            // Any State -> Death (on Die trigger)
            AnimatorStateTransition anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.1f;
            
            // Save
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Zombie Animator Controller created at: " + animatorPath);
            EditorUtility.DisplayDialog("Success", 
                "Zombie Animator Controller created!\n\n" +
                "Now click 'Apply Animator to Zombie Prefab' to finish setup.", "OK");
        }
        
        private static AnimationClip LoadAnimationFromFBX(string fbxPath)
        {
            if (!File.Exists(fbxPath))
            {
                Debug.LogWarning("FBX not found: " + fbxPath);
                return null;
            }
            
            // Load all assets from FBX
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    Debug.Log("Found animation clip: " + clip.name + " in " + fbxPath);
                    return clip;
                }
            }
            
            Debug.LogWarning("No animation clip found in: " + fbxPath);
            return null;
        }
        
        private static void ApplyAnimatorToPrefab()
        {
            // Find the zombie prefab
            string[] prefabGuids = AssetDatabase.FindAssets("Zombie t:Prefab", new[] { "Assets/Resources/Enemies/Zombie" });
            
            GameObject zombiePrefab = null;
            
            if (prefabGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            
            // Also try loading by exact name
            if (zombiePrefab == null)
            {
                zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Enemies/Zombie/Zombie.prefab");
            }
            
            if (zombiePrefab == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find Zombie prefab!\n\n" +
                    "Make sure you have a prefab named 'Zombie' in:\n" +
                    "Assets/Resources/Enemies/Zombie/", "OK");
                return;
            }
            
            // Load the animator controller
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Resources/Enemies/Zombie/ZombieAnimator.controller");
            
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find ZombieAnimator.controller!\n\n" +
                    "Click 'Create Zombie Animator Controller' first.", "OK");
                return;
            }
            
            // Apply to prefab
            Animator animator = zombiePrefab.GetComponent<Animator>();
            if (animator == null)
            {
                animator = zombiePrefab.GetComponentInChildren<Animator>();
            }
            if (animator == null)
            {
                animator = zombiePrefab.AddComponent<Animator>();
            }
            
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            
            EditorUtility.SetDirty(zombiePrefab);
            AssetDatabase.SaveAssets();
            
            Debug.Log("✅ Animator applied to Zombie prefab!");
            EditorUtility.DisplayDialog("Success", 
                "Animator Controller applied to Zombie prefab!\n\n" +
                "Press Play to test!", "OK");
        }
    }
}
