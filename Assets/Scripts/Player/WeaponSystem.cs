using UnityEngine;

namespace NeonArena.Player
{
    /// <summary>
    /// Raycast-based weapon system with visual beam effects and 3D gun model
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Stats")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireRate = 0.1f;  // Time between shots
        [SerializeField] private float range = 100f;
        [SerializeField] private float accuracy = 0.98f; // Higher = more accurate

        [Header("Visual Effects")]
        [SerializeField] private float beamDuration = 0.03f; // Very quick - realistic
        
        [Header("Gun Model")]
        [SerializeField] private Vector3 gunPosition = new Vector3(0.35f, -0.3f, 0.6f);
        [SerializeField] private Vector3 gunRotation = new Vector3(0f, 180f, 0f);
        [SerializeField] private Vector3 gunScale = new Vector3(0.25f, 0.25f, 0.25f); // Much bigger!
        [SerializeField] private float recoilAmount = 0.08f;
        [SerializeField] private float recoilRecovery = 12f;
        
        [Header("Muzzle Flash")]
        [SerializeField] private float muzzleFlashDuration = 0.05f;
        [SerializeField] private float muzzleFlashIntensity = 8f;

        private Camera playerCamera;
        private float nextFireTime;
        private int shotsFired;
        private int shotsHit;

        // Beam effect
        private LineRenderer beamRenderer;
        private float beamTimer;
        
        // Gun model
        private GameObject gunModel;
        private Vector3 originalGunPosition;
        private float currentRecoil;
        
        // Muzzle flash
        private GameObject muzzleFlash;
        private Light muzzleLight;
        private float muzzleFlashTimer;

        public void Initialize(Camera cam)
        {
            playerCamera = cam;
            nextFireTime = 0f;
            shotsFired = 0;
            shotsHit = 0;

            CreateBeamRenderer();
            CreateGunModel();
        }

        private void CreateBeamRenderer()
        {
            GameObject beamObj = new GameObject("WeaponBeam");
            beamObj.transform.SetParent(transform);

            beamRenderer = beamObj.AddComponent<LineRenderer>();
            beamRenderer.startWidth = 0.01f; // Very thin - realistic bullet tracer
            beamRenderer.endWidth = 0.005f;
            beamRenderer.positionCount = 2;
            beamRenderer.enabled = false;

            // Create subtle tracer material - faint yellow/white streak
            Material beamMat = new Material(Shader.Find("Standard"));
            beamMat.SetColor("_Color", new Color(1f, 0.95f, 0.8f, 0.5f));
            beamMat.EnableKeyword("_EMISSION");
            beamMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.7f) * 2f); // Subtle glow
            beamMat.SetFloat("_Mode", 3); // Transparent
            beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            beamRenderer.material = beamMat;
            beamRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            beamRenderer.receiveShadows = false;
        }
        
        private void CreateGunModel()
        {
            // Try to load the gun model from Resources or create placeholder
            GameObject loadedGun = Resources.Load<GameObject>("Weapons/Glock/source/Lowpoly New");
            
            if (loadedGun != null)
            {
                // Use the actual gun model
                gunModel = Instantiate(loadedGun, playerCamera.transform);
                Debug.Log("Gun model loaded successfully!");
            }
            else
            {
                // Create a detailed placeholder gun model
                Debug.Log("Creating placeholder gun (model not in Resources folder yet)");
                gunModel = CreatePlaceholderGun();
            }
            
            // Position the gun in first-person view
            gunModel.transform.SetParent(playerCamera.transform);
            gunModel.transform.localPosition = gunPosition;
            gunModel.transform.localRotation = Quaternion.Euler(gunRotation);
            gunModel.transform.localScale = gunScale;
            
            originalGunPosition = gunPosition;
            
            // Create muzzle flash system
            CreateMuzzleFlash();
        }
        
        private void CreateMuzzleFlash()
        {
            // Create muzzle flash container at gun barrel
            muzzleFlash = new GameObject("MuzzleFlash");
            muzzleFlash.transform.SetParent(gunModel.transform);
            muzzleFlash.transform.localPosition = new Vector3(0, 0.08f, 0.45f); // At barrel end
            muzzleFlash.transform.localRotation = Quaternion.identity;
            
            // REALISTIC muzzle flash - just bright light, no geometric shapes!
            
            // Main flash light - bright and quick
            muzzleLight = muzzleFlash.AddComponent<Light>();
            muzzleLight.type = LightType.Point;
            muzzleLight.color = new Color(1f, 0.85f, 0.5f); // Warm yellow-orange
            muzzleLight.intensity = 0;
            muzzleLight.range = 10f;
            muzzleLight.shadows = LightShadows.None;
            
            // Secondary spot light for directional flash
            GameObject spotLightObj = new GameObject("MuzzleSpotLight");
            spotLightObj.transform.SetParent(muzzleFlash.transform);
            spotLightObj.transform.localPosition = Vector3.zero;
            spotLightObj.transform.localRotation = Quaternion.identity;
            Light spotLight = spotLightObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(1f, 0.9f, 0.6f);
            spotLight.intensity = 0;
            spotLight.range = 5f;
            spotLight.spotAngle = 60f;
            spotLight.shadows = LightShadows.None;
            
            // Initially hide the flash
            muzzleFlash.SetActive(false);
        }
        
        private GameObject CreatePlaceholderGun()
        {
            // Create a detailed, VISIBLE placeholder gun using primitives
            GameObject gun = new GameObject("PlaceholderGun");
            
            // Gun body/frame (main rectangle) - LARGER and more visible
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(gun.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(0.15f, 0.22f, 0.35f);
            ApplyGunMaterial(frame, new Color(0.25f, 0.25f, 0.28f), false, 0.8f); // Visible grey
            Destroy(frame.GetComponent<Collider>());
            
            // Barrel - extended and visible
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gun.transform);
            barrel.transform.localPosition = new Vector3(0, 0.03f, 0.25f);
            barrel.transform.localScale = new Vector3(0.1f, 0.08f, 0.3f);
            ApplyGunMaterial(barrel, new Color(0.35f, 0.35f, 0.38f), false, 0.9f);
            Destroy(barrel.GetComponent<Collider>());
            
            // Slide (top part) - prominent
            GameObject slide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slide.name = "Slide";
            slide.transform.SetParent(gun.transform);
            slide.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            slide.transform.localScale = new Vector3(0.13f, 0.06f, 0.42f);
            ApplyGunMaterial(slide, new Color(0.2f, 0.2f, 0.22f), false, 0.85f);
            Destroy(slide.GetComponent<Collider>());
            
            // Slide grooves (detail lines)
            for (int i = 0; i < 8; i++)
            {
                GameObject groove = GameObject.CreatePrimitive(PrimitiveType.Cube);
                groove.name = $"SlideGroove_{i}";
                groove.transform.SetParent(slide.transform);
                groove.transform.localPosition = new Vector3(0, 0.55f, -0.3f + i * 0.08f);
                groove.transform.localScale = new Vector3(1.05f, 0.1f, 0.03f);
                ApplyGunMaterial(groove, new Color(0.15f, 0.15f, 0.15f), false, 0.5f);
                Destroy(groove.GetComponent<Collider>());
            }
            
            // Handle/Grip - ergonomic angle
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(gun.transform);
            grip.transform.localPosition = new Vector3(0, -0.15f, -0.08f);
            grip.transform.localScale = new Vector3(0.12f, 0.25f, 0.15f);
            grip.transform.localRotation = Quaternion.Euler(-18f, 0, 0);
            ApplyGunMaterial(grip, new Color(0.12f, 0.12f, 0.12f), false, 0.3f);
            Destroy(grip.GetComponent<Collider>());
            
            // Grip texture lines
            for (int i = 0; i < 6; i++)
            {
                GameObject gripLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gripLine.name = $"GripLine_{i}";
                gripLine.transform.SetParent(grip.transform);
                gripLine.transform.localPosition = new Vector3(0.51f, -0.3f + i * 0.12f, 0);
                gripLine.transform.localScale = new Vector3(0.02f, 0.08f, 0.9f);
                ApplyGunMaterial(gripLine, new Color(0.08f, 0.08f, 0.08f), false, 0.2f);
                Destroy(gripLine.GetComponent<Collider>());
            }
            
            // Trigger guard - curved look
            GameObject triggerGuard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerGuard.name = "TriggerGuard";
            triggerGuard.transform.SetParent(gun.transform);
            triggerGuard.transform.localPosition = new Vector3(0, -0.1f, 0.03f);
            triggerGuard.transform.localScale = new Vector3(0.1f, 0.03f, 0.12f);
            ApplyGunMaterial(triggerGuard, new Color(0.2f, 0.2f, 0.2f), false, 0.7f);
            Destroy(triggerGuard.GetComponent<Collider>());
            
            // Trigger
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = "Trigger";
            trigger.transform.SetParent(gun.transform);
            trigger.transform.localPosition = new Vector3(0, -0.06f, 0.02f);
            trigger.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);
            trigger.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
            ApplyGunMaterial(trigger, new Color(0.15f, 0.15f, 0.15f), false, 0.6f);
            Destroy(trigger.GetComponent<Collider>());
            
            // Magazine - visible base
            GameObject magazine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            magazine.name = "Magazine";
            magazine.transform.SetParent(gun.transform);
            magazine.transform.localPosition = new Vector3(0, -0.28f, -0.05f);
            magazine.transform.localScale = new Vector3(0.08f, 0.12f, 0.12f);
            ApplyGunMaterial(magazine, new Color(0.18f, 0.18f, 0.18f), false, 0.7f);
            Destroy(magazine.GetComponent<Collider>());
            
            // Front sight - with glow dot
            GameObject frontSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontSight.name = "FrontSight";
            frontSight.transform.SetParent(gun.transform);
            frontSight.transform.localPosition = new Vector3(0, 0.13f, 0.2f);
            frontSight.transform.localScale = new Vector3(0.025f, 0.04f, 0.025f);
            ApplyGunMaterial(frontSight, new Color(0.1f, 0.1f, 0.1f), false, 0.5f);
            Destroy(frontSight.GetComponent<Collider>());
            
            // Front sight dot (glowing)
            GameObject frontDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            frontDot.name = "FrontSightDot";
            frontDot.transform.SetParent(frontSight.transform);
            frontDot.transform.localPosition = new Vector3(0, 0.6f, 0);
            frontDot.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            ApplyGunMaterial(frontDot, new Color(0f, 1f, 0f), true, 0.9f); // Green glow
            Destroy(frontDot.GetComponent<Collider>());
            
            // Rear sight
            GameObject rearSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearSight.name = "RearSight";
            rearSight.transform.SetParent(gun.transform);
            rearSight.transform.localPosition = new Vector3(0, 0.13f, -0.12f);
            rearSight.transform.localScale = new Vector3(0.05f, 0.04f, 0.025f);
            ApplyGunMaterial(rearSight, new Color(0.1f, 0.1f, 0.1f), false, 0.5f);
            Destroy(rearSight.GetComponent<Collider>());
            
            // Rear sight notch
            GameObject rearNotch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearNotch.name = "RearNotch";
            rearNotch.transform.SetParent(rearSight.transform);
            rearNotch.transform.localPosition = new Vector3(0, 0.3f, 0);
            rearNotch.transform.localScale = new Vector3(0.3f, 0.5f, 1f);
            ApplyGunMaterial(rearNotch, new Color(0.05f, 0.05f, 0.05f), false, 0.3f);
            Destroy(rearNotch.GetComponent<Collider>());
            
            // Muzzle (barrel end) - larger
            GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muzzle.name = "Muzzle";
            muzzle.transform.SetParent(gun.transform);
            muzzle.transform.localPosition = new Vector3(0, 0.03f, 0.42f);
            muzzle.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);
            muzzle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ApplyGunMaterial(muzzle, new Color(0.4f, 0.4f, 0.4f), true, 0.8f);
            Destroy(muzzle.GetComponent<Collider>());
            
            // Inner barrel (dark hole)
            GameObject innerBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerBarrel.name = "InnerBarrel";
            innerBarrel.transform.SetParent(muzzle.transform);
            innerBarrel.transform.localPosition = new Vector3(0, 0.6f, 0);
            innerBarrel.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            ApplyGunMaterial(innerBarrel, new Color(0.02f, 0.02f, 0.02f), false, 0.1f);
            Destroy(innerBarrel.GetComponent<Collider>());
            
            // Ejection port
            GameObject ejectionPort = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ejectionPort.name = "EjectionPort";
            ejectionPort.transform.SetParent(gun.transform);
            ejectionPort.transform.localPosition = new Vector3(0.07f, 0.07f, 0f);
            ejectionPort.transform.localScale = new Vector3(0.02f, 0.04f, 0.08f);
            ApplyGunMaterial(ejectionPort, new Color(0.05f, 0.05f, 0.05f), false, 0.2f);
            Destroy(ejectionPort.GetComponent<Collider>());
            
            // Rail (under barrel)
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(gun.transform);
            rail.transform.localPosition = new Vector3(0, -0.02f, 0.12f);
            rail.transform.localScale = new Vector3(0.08f, 0.02f, 0.15f);
            ApplyGunMaterial(rail, new Color(0.22f, 0.22f, 0.22f), false, 0.7f);
            Destroy(rail.GetComponent<Collider>());
            
            return gun;
        }
        
        private void ApplyGunMaterial(GameObject obj, Color color, bool emissive = false, float glossiness = 0.7f)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", color);
            mat.SetFloat("_Metallic", 0.85f);
            mat.SetFloat("_Glossiness", glossiness);
            
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 3f);
            }
            
            obj.GetComponent<Renderer>().material = mat;
        }

        private void Update()
        {
            // Handle beam visual lifetime
            if (beamTimer > 0f)
            {
                beamTimer -= Time.deltaTime;
                if (beamTimer <= 0f)
                {
                    beamRenderer.enabled = false;
                }
            }
            
            // Handle muzzle flash lifetime
            if (muzzleFlashTimer > 0f)
            {
                muzzleFlashTimer -= Time.deltaTime;
                
                // Fade out the lights quickly
                float fadeRatio = muzzleFlashTimer / muzzleFlashDuration;
                
                if (muzzleLight != null)
                {
                    muzzleLight.intensity = Mathf.Lerp(0, muzzleFlashIntensity * 2f, fadeRatio);
                }
                
                // Fade spot light too
                if (muzzleFlash != null)
                {
                    Light[] lights = muzzleFlash.GetComponentsInChildren<Light>();
                    foreach (Light light in lights)
                    {
                        if (light != muzzleLight)
                        {
                            light.intensity = Mathf.Lerp(0, muzzleFlashIntensity * 3f, fadeRatio);
                        }
                    }
                }
                
                if (muzzleFlashTimer <= 0f)
                {
                    muzzleFlash.SetActive(false);
                }
            }
            
            // Handle gun recoil recovery
            if (gunModel != null && currentRecoil > 0f)
            {
                currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilRecovery);
                gunModel.transform.localPosition = originalGunPosition + new Vector3(0, currentRecoil * 0.3f, -currentRecoil);
                gunModel.transform.localRotation = Quaternion.Euler(gunRotation.x - currentRecoil * 150f, gunRotation.y, gunRotation.z);
            }
        }

        public bool TryFire()
        {
            if (Time.time < nextFireTime)
                return false;

            Fire();
            ApplyRecoil();
            nextFireTime = Time.time + fireRate;
            return true;
        }
        
        private void ApplyRecoil()
        {
            // Add gun recoil animation
            currentRecoil = recoilAmount;
            
            // Show muzzle flash
            ShowMuzzleFlash();
            
            // Eject shell casing
            EjectShellCasing();
        }
        
        private void ShowMuzzleFlash()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                muzzleFlashTimer = muzzleFlashDuration;
                
                // Set point light intensity
                if (muzzleLight != null)
                {
                    muzzleLight.intensity = muzzleFlashIntensity * 2f;
                }
                
                // Set spot light intensity
                Light spotLight = muzzleFlash.GetComponentInChildren<Light>();
                if (spotLight != null && spotLight != muzzleLight)
                {
                    spotLight.intensity = muzzleFlashIntensity * 3f;
                }
            }
        }
        
        private void EjectShellCasing()
        {
            // Create shell casing effect
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shell.name = "ShellCasing";
            shell.transform.position = gunModel.transform.TransformPoint(new Vector3(0.1f, 0.05f, 0.1f));
            shell.transform.localScale = new Vector3(0.015f, 0.025f, 0.015f);
            
            // Brass material
            Material shellMat = new Material(Shader.Find("Standard"));
            shellMat.SetColor("_Color", new Color(0.85f, 0.65f, 0.13f)); // Brass color
            shellMat.SetFloat("_Metallic", 0.95f);
            shellMat.SetFloat("_Glossiness", 0.8f);
            shellMat.EnableKeyword("_EMISSION");
            shellMat.SetColor("_EmissionColor", new Color(0.85f, 0.65f, 0.13f) * 0.3f);
            shell.GetComponent<Renderer>().material = shellMat;
            
            // Add physics
            Rigidbody rb = shell.AddComponent<Rigidbody>();
            rb.mass = 0.01f;
            rb.useGravity = true;
            
            // Eject to the right and up
            Vector3 ejectDirection = gunModel.transform.TransformDirection(new Vector3(1f, 0.8f, -0.2f));
            rb.AddForce(ejectDirection * Random.Range(2f, 4f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            
            // Destroy after 3 seconds
            Destroy(shell, 3f);
        }

        private void Fire()
        {
            shotsFired++;

            if (playerCamera == null)
            {
                Debug.LogWarning("WeaponSystem: No camera reference!");
                return;
            }

            // Get aim ray from camera center
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            // Apply spread for accuracy
            Vector3 spread = Random.insideUnitSphere * (1f - accuracy);
            spread.z = 0; // Only horizontal/vertical spread
            ray.direction = (ray.direction + spread).normalized;

            // Perform raycast
            RaycastHit hit;
            bool didHit = Physics.Raycast(ray, out hit, range);

            Vector3 endPoint;
            if (didHit)
            {
                endPoint = hit.point;
                
                // Debug: Show what we hit
                Debug.Log($"Hit: {hit.collider.gameObject.name} at {hit.point}");

                // Check if we hit an enemy
                AI.EnemyAI enemy = hit.collider.GetComponentInParent<AI.EnemyAI>();
                if (enemy != null)
                {
                    shotsHit++;
                    
                    // Check for headshot (top 30% of capsule)
                    bool isHeadshot = hit.point.y > enemy.transform.position.y + 0.7f;
                    
                    float finalDamage = damage;
                    if (isHeadshot)
                    {
                        finalDamage *= 2f;
                    }

                    Debug.Log($"Enemy hit! Damage: {finalDamage}, Headshot: {isHeadshot}");
                    enemy.TakeDamage(finalDamage, isHeadshot);
                }
                else
                {
                    Debug.Log($"Hit non-enemy: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                endPoint = ray.origin + ray.direction * range;
            }

            // Show visual beam
            ShowBeam(ray.origin, endPoint);

            // Check for accuracy bonus
            if (shotsFired > 20)
            {
                float accuracy = (float)shotsHit / shotsFired;
                if (accuracy > 0.8f && shotsFired % 50 == 0)
                {
                    Core.GameManager.Instance?.AddScore(300);
                }
            }
        }

        private void ShowBeam(Vector3 start, Vector3 end)
        {
            if (beamRenderer == null) return;

            beamRenderer.SetPosition(0, start);
            beamRenderer.SetPosition(1, end);
            beamRenderer.enabled = true;
            beamTimer = beamDuration;
        }

        public float GetAccuracy()
        {
            if (shotsFired == 0) return 1f;
            return (float)shotsHit / shotsFired;
        }
    }
}

