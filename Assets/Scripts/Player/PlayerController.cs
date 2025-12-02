using UnityEngine;

namespace NeonArena.Player
{
    /// <summary>
    /// Handles player movement, camera control, and health management
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 20f;

        [Header("Camera")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 85f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float healthRegenDelay = 5f;
        [SerializeField] private float healthRegenRate = 10f;

        // Components
        private CharacterController controller;
        private Camera playerCamera;
        private WeaponSystem weaponSystem;

        // Movement state
        private Vector3 moveDirection = Vector3.zero;
        private float verticalVelocity = 0f;

        // Camera state
        private float cameraPitch = 0f;
        private float cameraShakeIntensity = 0f;

        // Health state
        private float currentHealth;
        private float timeSinceLastDamage;

        // Input state
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool sprintInput;
        private bool jumpInput;
        private bool fireInput;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Setup character controller (already added by RequireComponent attribute)
            controller = GetComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.zero;

            // Find camera
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("PlayerController: No camera found!");
            }

            // Add weapon system
            weaponSystem = gameObject.AddComponent<WeaponSystem>();
            weaponSystem.Initialize(playerCamera);

            // Initialize health
            currentHealth = maxHealth;
            timeSinceLastDamage = 0f;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!IsAlive || Core.GameManager.Instance.CurrentState != Core.GameManager.GameState.Playing)
                return;

            HandleInput();
            HandleMovement();
            HandleCamera();
            HandleHealth();
            HandleWeapon();
        }

        private void HandleInput()
        {
            // Movement input
            moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D
            moveInput.y = Input.GetAxisRaw("Vertical");   // W/S

            // Look input
            lookInput.x = Input.GetAxisRaw("Mouse X");
            lookInput.y = Input.GetAxisRaw("Mouse Y");

            // Action inputs
            sprintInput = Input.GetKey(KeyCode.LeftShift);
            jumpInput = Input.GetButtonDown("Jump"); // Space
            fireInput = Input.GetMouseButton(0);     // Left click
        }

        private void HandleMovement()
        {
            // Calculate move direction
            float speed = sprintInput ? sprintSpeed : walkSpeed;
            Vector3 forward = transform.forward * moveInput.y;
            Vector3 right = transform.right * moveInput.x;
            moveDirection = (forward + right).normalized * speed;

            // Handle jumping
            if (controller.isGrounded)
            {
                verticalVelocity = -2f; // Small downward force to keep grounded
                
                if (jumpInput)
                {
                    verticalVelocity = jumpForce;
                }
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            // Apply movement
            Vector3 motion = moveDirection * Time.deltaTime;
            motion.y = verticalVelocity * Time.deltaTime;
            controller.Move(motion);
        }

        private void HandleCamera()
        {
            if (playerCamera == null) return;

            // Horizontal rotation (yaw) - rotate player body
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

            // Vertical rotation (pitch) - rotate camera
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);

            // Apply camera rotation with shake
            Vector3 shakeOffset = Vector3.zero;
            if (cameraShakeIntensity > 0f)
            {
                shakeOffset = Random.insideUnitSphere * cameraShakeIntensity;
                cameraShakeIntensity = Mathf.Lerp(cameraShakeIntensity, 0f, Time.deltaTime * 10f);
            }

            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            playerCamera.transform.localPosition = new Vector3(shakeOffset.x, 0.6f + shakeOffset.y, shakeOffset.z);
        }

        private void HandleHealth()
        {
            if (currentHealth < maxHealth)
            {
                timeSinceLastDamage += Time.deltaTime;

                // Regenerate health after delay
                if (timeSinceLastDamage >= healthRegenDelay)
                {
                    currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
                    UpdateHealthUI();
                }
            }
        }

        private void HandleWeapon()
        {
            if (weaponSystem != null && fireInput)
            {
                bool fired = weaponSystem.TryFire();
                if (fired)
                {
                    ApplyCameraRecoil();
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            timeSinceLastDamage = 0f;

            UpdateHealthUI();
            ApplyCameraShake(0.2f);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player died!");
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.PlayerDied();
            }
        }

        private void UpdateHealthUI()
        {
            var uiSystem = Core.GameManager.Instance?.GetComponent<Core.UISystem>();
            if (uiSystem != null)
            {
                uiSystem.UpdateHealth(currentHealth, maxHealth);
            }
        }

        public void ApplyCameraShake(float intensity)
        {
            cameraShakeIntensity = Mathf.Max(cameraShakeIntensity, intensity);
        }

        private void ApplyCameraRecoil()
        {
            // Small upward kick
            cameraPitch -= Random.Range(0.2f, 0.5f);
            ApplyCameraShake(0.05f);
        }

        public Vector3 GetAimPoint()
        {
            if (playerCamera != null)
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                return ray.origin + ray.direction * 100f;
            }
            return transform.position + transform.forward * 100f;
        }
    }
}

