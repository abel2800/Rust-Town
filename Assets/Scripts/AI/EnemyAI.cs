using UnityEngine;

namespace NeonArena.AI
{
    /// <summary>
    /// Enemy AI with seek, chase, attack, and evasion behaviors + Animation support
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Behavior")]
        [SerializeField] private float detectionRange = 50f;
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamage = 10f;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 4f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float evadeChance = 0.3f;
        [SerializeField] private float evaseDuration = 1f;

        [Header("Stats")]
        [SerializeField] private float baseHealth = 100f;
        
        [Header("Animation")]
        [SerializeField] private float armSwingSpeed = 8f;
        [SerializeField] private float legSwingSpeed = 10f;
        [SerializeField] private float swingAmount = 30f;

        // State
        private Transform playerTransform;
        private float currentHealth;
        private float currentSpeed;
        private float nextAttackTime;
        private int waveLevel;

        // Evasion
        private bool isEvading;
        private float evadeTimer;
        private Vector3 evadeDirection;

        // Visual feedback
        private Renderer enemyRenderer;
        private Material enemyMaterial;
        private float hitFlashTimer;
        
        // Animation
        private Animator animator;
        private Transform leftArm, rightArm, leftLeg, rightLeg;
        private float animationTime;
        private bool isDying = false;
        private float deathTimer = 0f;

        private enum AIState
        {
            Seeking,
            Chasing,
            Attacking,
            Dying
        }

        private AIState currentState = AIState.Seeking;

        public void Initialize(int wave, float difficultyMultiplier)
        {
            waveLevel = wave;
            
            // Scale stats with difficulty
            float scaling = Mathf.Pow(difficultyMultiplier, wave - 1);
            currentHealth = baseHealth * scaling;
            currentSpeed = baseSpeed * (1f + (wave - 1) * 0.1f);
            attackDamage *= scaling;

            // Get references
            enemyRenderer = GetComponentInChildren<Renderer>();
            if (enemyRenderer != null)
            {
                enemyMaterial = enemyRenderer.material;
            }
            
            // Setup animator with proper animations
            SetupAnimator();
            
            // Make sure zombie starts facing forward
            transform.rotation = Quaternion.identity;

            // Find player
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
        
        private void SetupAnimator()
        {
            // Get or add Animator component
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                // Try to find SkinnedMeshRenderer and add animator to same object
                SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    animator = smr.gameObject.AddComponent<Animator>();
                }
                else
                {
                    animator = gameObject.AddComponent<Animator>();
                }
            }
            
            if (animator == null)
            {
                Debug.LogWarning("Could not create Animator!");
                return;
            }
            
            // Try to load the animator controller
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Enemies/Zombie/ZombieAnimator");
            
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.enabled = true;
                
                // Force play Run animation
                animator.Play("Run", 0, 0f);
                Debug.Log("✅ Animator Controller loaded and Run animation started!");
            }
            else
            {
                Debug.LogWarning("⚠️ ZombieAnimator.controller not found - trying legacy animation");
                
                // Try legacy Animation component approach
                SetupLegacyAnimation();
            }
        }
        
        private void SetupLegacyAnimation()
        {
            // Load animation clips from Scary Zombie Pack (lowercase names)
            AnimationClip runClip = LoadAnimationClip("Enemies/Zombie/zombie run");
            AnimationClip attackClip = LoadAnimationClip("Enemies/Zombie/zombie attack");
            AnimationClip deathClip = LoadAnimationClip("Enemies/Zombie/zombie death");
            
            if (runClip != null)
            {
                // Add legacy Animation component
                Animation anim = GetComponentInChildren<Animation>();
                if (anim == null)
                {
                    SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        anim = smr.gameObject.AddComponent<Animation>();
                    }
                }
                
                if (anim != null)
                {
                    // Make clips legacy
                    runClip.legacy = true;
                    if (attackClip != null) attackClip.legacy = true;
                    if (deathClip != null) deathClip.legacy = true;
                    
                    anim.AddClip(runClip, "Run");
                    if (attackClip != null) anim.AddClip(attackClip, "Attack");
                    if (deathClip != null) anim.AddClip(deathClip, "Death");
                    
                    anim.wrapMode = WrapMode.Loop;
                    anim.Play("Run");
                    
                    Debug.Log("✅ Legacy animations loaded!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Could not load animation clips!");
            }
        }
        
        private AnimationClip LoadAnimationClip(string path)
        {
            // Try to load animation clip
            AnimationClip clip = Resources.Load<AnimationClip>(path);
            if (clip != null) return clip;
            
            // Try loading from FBX (the clip is embedded)
            GameObject fbx = Resources.Load<GameObject>(path);
            if (fbx != null)
            {
                // Get all animation clips from the FBX
                Animation anim = fbx.GetComponent<Animation>();
                if (anim != null && anim.clip != null)
                {
                    return anim.clip;
                }
            }
            
            return null;
        }

        private void Update()
        {
            // Handle death state
            if (isDying)
            {
                UpdateDeathAnimation();
                return;
            }
            
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            UpdateAI();
            UpdateAnimation();
            UpdateVisuals();
        }
        
        private void UpdateAnimation()
        {
            // Animate walking/running (procedural for placeholder enemies)
            if (currentState == AIState.Chasing || currentState == AIState.Seeking)
            {
                animationTime += Time.deltaTime * legSwingSpeed;
                
                // Procedural animation for limbs
                if (leftArm != null && rightArm != null)
                {
                    float armAngle = Mathf.Sin(animationTime) * swingAmount;
                    leftArm.localRotation = Quaternion.Euler(45 + armAngle, 0, 30);
                    rightArm.localRotation = Quaternion.Euler(45 - armAngle, 0, -30);
                }
                
                if (leftLeg != null && rightLeg != null)
                {
                    float legAngle = Mathf.Sin(animationTime) * swingAmount * 0.5f;
                    leftLeg.localRotation = Quaternion.Euler(legAngle, 0, 0);
                    rightLeg.localRotation = Quaternion.Euler(-legAngle, 0, 0);
                }
                
                // Body bob
                float bobAmount = Mathf.Abs(Mathf.Sin(animationTime * 2f)) * 0.05f;
                Vector3 pos = transform.position;
                pos.y = Mathf.Max(0.01f, pos.y); // Keep above ground
            }
            
            // Attacking animation - lunge forward
            if (currentState == AIState.Attacking)
            {
                animationTime += Time.deltaTime * armSwingSpeed * 2f;
                
                if (leftArm != null && rightArm != null)
                {
                    float attackAngle = Mathf.Sin(animationTime * 3f) * 45f;
                    leftArm.localRotation = Quaternion.Euler(90 + attackAngle, 0, 20);
                    rightArm.localRotation = Quaternion.Euler(90 + attackAngle, 0, -20);
                }
            }
        }
        
        private void UpdateDeathAnimation()
        {
            deathTimer += Time.deltaTime;
            
            // Try to play death animation from Animator
            if (animator != null && deathTimer < 0.1f)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);
            }
            
            // Fall backwards dramatically
            float fallDuration = 0.8f;
            float fallProgress = Mathf.Clamp01(deathTimer / fallDuration);
            
            // Smooth fall with slight bounce
            float fallAngle = Mathf.Lerp(0, -90f, Mathf.SmoothStep(0, 1, fallProgress));
            transform.rotation = Quaternion.Euler(fallAngle, transform.eulerAngles.y, 0);
            
            // Add some ragdoll-like movement
            if (deathTimer < fallDuration)
            {
                // Slight random movement while falling
                transform.position += new Vector3(
                    Random.Range(-0.02f, 0.02f),
                    0,
                    Random.Range(-0.02f, 0.02f)
                );
            }
            
            // Sink into ground after falling
            if (deathTimer > fallDuration + 0.5f)
            {
                transform.position += Vector3.down * Time.deltaTime * 0.3f;
            }
            
            // Change material to show death (turn grey/dark)
            if (deathTimer < 0.5f)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        c = Color.Lerp(c, new Color(0.3f, 0.3f, 0.3f), Time.deltaTime * 3f);
                        r.material.color = c;
                    }
                }
            }
            
            // Fade out after being on ground
            if (deathTimer > 2f)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        c.a = Mathf.Lerp(1f, 0f, (deathTimer - 2f) / 1f);
                        r.material.color = c;
                    }
                }
            }
            
            // Destroy after animation complete
            if (deathTimer >= 3f)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateAI()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Update state based on distance
            if (distanceToPlayer <= attackRange)
            {
                currentState = AIState.Attacking;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chasing;
            }
            else
            {
                currentState = AIState.Seeking;
            }

            // Execute behavior
            switch (currentState)
            {
                case AIState.Seeking:
                case AIState.Chasing:
                    ChasePlayer(distanceToPlayer);
                    break;

                case AIState.Attacking:
                    AttackPlayer();
                    break;
            }
        }

        private void ChasePlayer(float distance)
        {
            // Handle evasion behavior
            if (isEvading)
            {
                evadeTimer -= Time.deltaTime;
                if (evadeTimer <= 0f)
                {
                    isEvading = false;
                }
                else
                {
                    // Move in evade direction while still facing player
                    transform.position += evadeDirection * currentSpeed * 0.7f * Time.deltaTime;
                    
                    // Still face player while evading
                    Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                    lookDir.y = 0; // Keep upright
                    if (lookDir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(lookDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                    }
                    return;
                }
            }

            // Random chance to evade
            if (Random.value < evadeChance * Time.deltaTime && distance < 10f)
            {
                StartEvasion();
                return;
            }

            // Calculate direction to player
            Vector3 direction = (playerTransform.position - transform.position);
            direction.y = 0; // Keep on ground plane
            direction = direction.normalized;

            // FIRST rotate to face player (smooth rotation)
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 2f);
            }

            // THEN move forward in the direction we're facing
            Vector3 moveDirection = transform.forward;
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            
            // Keep zombie on ground
            Vector3 pos = transform.position;
            pos.y = 0;
            transform.position = pos;
        }

        private void StartEvasion()
        {
            isEvading = true;
            evadeTimer = evaseDuration;
            
            // Evade perpendicular to player direction
            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            evadeDirection = new Vector3(-toPlayer.z, 0, toPlayer.x); // Perpendicular
            
            // Randomize direction
            if (Random.value > 0.5f)
            {
                evadeDirection = -evadeDirection;
            }
        }

        private void AttackPlayer()
        {
            // Look at player
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Attack if cooldown ready
            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        private void PerformAttack()
        {
            if (playerTransform == null) return;

            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Deal damage to player
            Player.PlayerController player = playerTransform.GetComponent<Player.PlayerController>();
            if (player != null && player.IsAlive)
            {
                player.TakeDamage(attackDamage);
                Debug.Log($"🧟 Enemy dealt {attackDamage} damage to player");
            }
        }

        public void TakeDamage(float damage, bool isHeadshot = false)
        {
            currentHealth -= damage;
            hitFlashTimer = 0.1f;
            
            Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{baseHealth}");

            if (currentHealth <= 0f)
            {
                Die(isHeadshot);
            }
        }

        private void Die(bool isHeadshot)
        {
            if (isDying) return; // Already dying
            
            isDying = true;
            currentState = AIState.Dying;
            deathTimer = 0f;
            
            // Disable collider so player can walk through
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            // Disable child colliders too
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            
            // Play death animation if Animator exists
            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.Play("Death", 0, 0f);
            }
            
            // Add score immediately
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnEnemyKilled(isHeadshot);
            }
            
            Debug.Log("Enemy killed! Death animation playing...");
        }

        private void UpdateVisuals()
        {
            // Don't modify zombie materials - keep original colors/textures
            // Only handle hit flash if explicitly enabled
            
            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
                // Visual feedback handled by animator or UI instead
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize ranges in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}

