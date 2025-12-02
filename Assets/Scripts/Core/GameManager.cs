using UnityEngine;
using System.Collections;

namespace NeonArena.Core
{
    /// <summary>
    /// Central game manager controlling game state, waves, scoring, and system coordination
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState CurrentState { get; private set; }
        public int CurrentWave { get; private set; }
        public int CurrentScore { get; private set; }
        public float GameTime { get; private set; }

        [Header("Wave Configuration")]
        [SerializeField] private int baseEnemyCount = 5;
        [SerializeField] private float enemiesPerWave = 2f;
        [SerializeField] private float waveCooldown = 5f;
        [SerializeField] private float difficultyScaling = 1.15f;

        [Header("References")]
        private Player.PlayerController playerController;
        private World.MapGenerator mapGenerator;
        private World.PostApocalypticMapGenerator postApocMap;
        private UISystem uiSystem;
        private System.ObjectPool enemyPool;
        
        [Header("Map Style")]
        [SerializeField] private bool usePostApocalypticMap = true;

        private int enemiesRemaining;
        private float waveTimer;
        private bool isPaused;

        public enum GameState
        {
            Initializing,
            Playing,
            Paused,
            GameOver
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            GameTime += Time.deltaTime;

            // Check for pause input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            // Wave management
            if (enemiesRemaining <= 0)
            {
                waveTimer += Time.deltaTime;
                if (waveTimer >= waveCooldown)
                {
                    StartNextWave();
                }
            }
        }

        private void InitializeGame()
        {
            CurrentState = GameState.Initializing;
            CurrentWave = 0;
            CurrentScore = 0;
            GameTime = 0f;

            // Initialize systems
            InitializeSystems();

            // Generate the arena/town
            if (usePostApocalypticMap)
            {
                // Use new post-apocalyptic "Sundown Desolation" map
                if (postApocMap == null)
                {
                    postApocMap = gameObject.AddComponent<World.PostApocalypticMapGenerator>();
                }
                postApocMap.GenerateMap();
                Debug.Log("🌅 Sundown Desolation map generated!");
            }
            else
            {
                // Use original neon arena
                if (mapGenerator == null)
                {
                    mapGenerator = gameObject.AddComponent<World.MapGenerator>();
                }
                mapGenerator.GenerateArena();
            }

            // Spawn player
            SpawnPlayer();

            // Initialize UI
            if (uiSystem == null)
            {
                uiSystem = gameObject.AddComponent<UISystem>();
            }
            uiSystem.Initialize();

            // Start first wave
            CurrentState = GameState.Playing;
            StartNextWave();
        }

        private void InitializeSystems()
        {
            // Initialize object pool
            if (enemyPool == null)
            {
                GameObject poolObj = new GameObject("EnemyPool");
                poolObj.transform.SetParent(transform);
                enemyPool = poolObj.AddComponent<System.ObjectPool>();
                enemyPool.Initialize("Enemy", 50);
            }
        }

        private void SpawnPlayer()
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0, 2, 0);
            playerObj.layer = 0; // Default layer
            
            // Add camera FIRST (before PlayerController so it can find it)
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(playerObj.transform);
            cameraObj.transform.localPosition = new Vector3(0, 0.6f, 0);
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.fieldOfView = 80f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;
            cam.allowHDR = true; // Enable HDR for better emission rendering
            cam.allowMSAA = true; // Enable anti-aliasing
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f); // Dark background
            
            // Add audio listener
            cameraObj.AddComponent<AudioListener>();
            
            // Add player components AFTER camera exists
            playerController = playerObj.AddComponent<Player.PlayerController>();
        }

        private void StartNextWave()
        {
            CurrentWave++;
            waveTimer = 0f;

            int enemyCount = Mathf.RoundToInt(baseEnemyCount + (CurrentWave - 1) * enemiesPerWave);
            enemiesRemaining = enemyCount;

            // Award wave completion bonus
            if (CurrentWave > 1)
            {
                AddScore(250);
            }

            // Spawn enemies
            StartCoroutine(SpawnWaveEnemies(enemyCount));

            // Update UI
            if (uiSystem != null)
            {
                uiSystem.UpdateWaveDisplay(CurrentWave);
                uiSystem.ShowWaveNotification($"WAVE {CurrentWave}");
            }

            Debug.Log($"Starting Wave {CurrentWave} with {enemyCount} enemies");
        }

        private IEnumerator SpawnWaveEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void SpawnEnemy()
        {
            // Get random spawn position (around arena perimeter)
            Vector3 spawnPos = GetRandomSpawnPosition();

            GameObject enemyObj = new GameObject($"Enemy_{CurrentWave}_{enemiesRemaining}");
            enemyObj.transform.position = spawnPos;
            enemyObj.layer = 0; // Default layer

            // Try to load the zombie model from multiple possible locations
            GameObject zombieModel = null;
            
            // Try loading prefab first (most reliable)
            zombieModel = Resources.Load<GameObject>("Enemies/Zombie/ZombiePrefab");
            
            // Try FBX file (new zombie walk model)
            if (zombieModel == null)
                zombieModel = Resources.Load<GameObject>("Enemies/Zombie/source/Zombie Walk");
            
            // Try other possible names
            if (zombieModel == null)
                zombieModel = Resources.Load<GameObject>("Enemies/Zombie/source/model");
            if (zombieModel == null)
                zombieModel = Resources.Load<GameObject>("Enemies/Zombie/model");
            if (zombieModel == null)
                zombieModel = Resources.Load<GameObject>("Zombie");
            
            if (zombieModel != null)
            {
                // Use the actual zombie model
                GameObject model = Instantiate(zombieModel, enemyObj.transform);
                model.name = "ZombieModel";
                model.transform.localPosition = new Vector3(0, 0, 0);
                model.transform.localScale = new Vector3(1f, 1f, 1f); // Normal scale - adjust if needed!
                model.transform.localRotation = Quaternion.Euler(0, 0, 0); // Face forward
                
                // Check model bounds and adjust scale if needed
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    foreach (Renderer r in renderers)
                        bounds.Encapsulate(r.bounds);
                    
                    float height = bounds.size.y;
                    Debug.Log($"Zombie model height: {height}");
                    
                    // Auto-scale to reasonable size (target ~2 meters tall)
                    if (height > 0.01f)
                    {
                        float targetHeight = 2f;
                        float scaleFactor = targetHeight / height;
                        model.transform.localScale = Vector3.one * scaleFactor;
                        Debug.Log($"Auto-scaled zombie by {scaleFactor}");
                    }
                }
                
                // Add collider to parent
                CapsuleCollider col = enemyObj.AddComponent<CapsuleCollider>();
                col.height = 2f;
                col.radius = 0.5f;
                col.center = new Vector3(0, 1f, 0);
                
                // Setup animator if the model has one
                Animator anim = model.GetComponent<Animator>();
                if (anim == null)
                    anim = model.GetComponentInChildren<Animator>();
                
                Debug.Log("✅ Zombie model loaded successfully!");
            }
            else
            {
                // Create placeholder enemy using primitives (fallback)
                Debug.Log("⚠️ Zombie model not found - using placeholder. See HOW_TO_USE_YOUR_ZOMBIE_MODEL.md");
                CreatePlaceholderEnemy(enemyObj);
            }

            // Add AI component with animation support
            AI.EnemyAI enemyAI = enemyObj.AddComponent<AI.EnemyAI>();
            enemyAI.Initialize(CurrentWave, difficultyScaling);
        }
        
        private void CreatePlaceholderEnemy(GameObject enemyObj)
        {
            // Create enemy body (main capsule)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(enemyObj.transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            
            // Create enemy head (sphere on top)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(enemyObj.transform);
            head.transform.localPosition = new Vector3(0, 2.2f, 0);
            head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Create arms
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(enemyObj.transform);
            leftArm.transform.localPosition = new Vector3(-0.6f, 1.5f, 0.3f);
            leftArm.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            leftArm.transform.localRotation = Quaternion.Euler(45, 0, 30);
            Destroy(leftArm.GetComponent<Collider>());
            
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(enemyObj.transform);
            rightArm.transform.localPosition = new Vector3(0.6f, 1.5f, 0.3f);
            rightArm.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            rightArm.transform.localRotation = Quaternion.Euler(45, 0, -30);
            Destroy(rightArm.GetComponent<Collider>());
            
            // Create legs
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(enemyObj.transform);
            leftLeg.transform.localPosition = new Vector3(-0.25f, 0.4f, 0);
            leftLeg.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
            Destroy(leftLeg.GetComponent<Collider>());
            
            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(enemyObj.transform);
            rightLeg.transform.localPosition = new Vector3(0.25f, 0.4f, 0);
            rightLeg.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
            Destroy(rightLeg.GetComponent<Collider>());
            
            // Create glowing eyes
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.3f, 0.1f, 0.8f);
            leftEye.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
            Destroy(leftEye.GetComponent<Collider>());
            
            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.3f, 0.1f, 0.8f);
            rightEye.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
            Destroy(rightEye.GetComponent<Collider>());

            // Apply zombie materials
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(body.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(head.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(leftArm.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(rightArm.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(leftLeg.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieMaterial(rightLeg.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieEyeMaterial(leftEye.GetComponent<Renderer>());
            Rendering.NeonMaterialFactory.ApplyZombieEyeMaterial(rightEye.GetComponent<Renderer>());
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (usePostApocalypticMap)
            {
                // Spawn from various locations in the town
                Vector3[] spawnAreas = new Vector3[]
                {
                    // Behind buildings on the right
                    new Vector3(Random.Range(25f, 35f), 0, Random.Range(-40f, 40f)),
                    // Behind buildings on the left
                    new Vector3(Random.Range(-35f, -25f), 0, Random.Range(-40f, 40f)),
                    // From the gas station area
                    new Vector3(Random.Range(-25f, -15f), 0, Random.Range(-25f, -5f)),
                    // From residential area
                    new Vector3(Random.Range(30f, 45f), 0, Random.Range(20f, 40f)),
                    new Vector3(Random.Range(-40f, -30f), 0, Random.Range(25f, 40f)),
                    // From down the road
                    new Vector3(Random.Range(-5f, 5f), 0, Random.Range(50f, 60f)),
                    new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-60f, -50f)),
                    // From alleyways
                    new Vector3(Random.Range(15f, 20f), 0, Random.Range(-10f, 15f)),
                    new Vector3(Random.Range(-20f, -15f), 0, Random.Range(10f, 25f)),
                };
                
                // Pick a random spawn area
                Vector3 basePos = spawnAreas[Random.Range(0, spawnAreas.Length)];
                // Add some randomness
                basePos.x += Random.Range(-3f, 3f);
                basePos.z += Random.Range(-3f, 3f);
                basePos.y = 0.1f;
                
                return basePos;
            }
            else
            {
                // Original arena spawn
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(20f, 30f);
                return new Vector3(Mathf.Cos(angle) * distance, 1f, Mathf.Sin(angle) * distance);
            }
        }

        public void OnEnemyKilled(bool isHeadshot = false)
        {
            enemiesRemaining--;
            
            int points = 100;
            if (isHeadshot) points += 50;
            
            AddScore(points);
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            if (uiSystem != null)
            {
                uiSystem.UpdateScore(CurrentScore);
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (uiSystem != null)
            {
                uiSystem.SetPauseScreen(isPaused);
            }
        }

        public void PlayerDied()
        {
            CurrentState = GameState.GameOver;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (uiSystem != null)
            {
                uiSystem.ShowGameOver(CurrentScore, CurrentWave, GameTime);
            }

            Debug.Log($"Game Over! Score: {CurrentScore}, Wave: {CurrentWave}, Time: {GameTime:F1}s");
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        public float GetDifficultyMultiplier()
        {
            return Mathf.Pow(difficultyScaling, CurrentWave - 1);
        }
    }
}

