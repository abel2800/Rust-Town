using UnityEngine;

namespace NeonArena.Player
{
    /// <summary>
    /// Raycast-based weapon system with visual beam effects and 3D gun model
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapon Stats - AK-47")]
        [SerializeField] private float damage = 35f;
        [SerializeField] private float fireRate = 0.1f;  // Full auto
        [SerializeField] private float range = 150f;
        [SerializeField] private float accuracy = 0.95f;
        [SerializeField] private float adsAccuracy = 0.99f; // Better accuracy when aiming

        [Header("Visual Effects")]
        [SerializeField] private float beamDuration = 0.03f;
        
        [Header("Gun Model - AK-47")]
        [SerializeField] private Vector3 hipFirePosition = new Vector3(0.35f, -0.3f, 0.5f);
        [SerializeField] private Vector3 adsPosition = new Vector3(0f, -0.15f, 0.3f); // Center for ADS
        [SerializeField] private Vector3 gunRotation = new Vector3(0f, 180f, 0f);
        [SerializeField] private Vector3 gunScale = new Vector3(0.4f, 0.4f, 0.4f);
        [SerializeField] private float recoilAmount = 0.05f;
        [SerializeField] private float recoilRecovery = 12f;
        
        [Header("Aim Down Sights")]
        [SerializeField] private float adsSpeed = 8f;
        [SerializeField] private float normalFOV = 80f;
        [SerializeField] private float adsFOV = 45f;
        private bool isAiming = false;
        private Vector3 currentGunPosition;
        
        [Header("Crosshair")]
        private GameObject crosshair;
        private GameObject adsCrosshair;
        
        [Header("Muzzle Flash")]
        [SerializeField] private float muzzleFlashDuration = 0.05f;
        [SerializeField] private float muzzleFlashIntensity = 10f;

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
            currentGunPosition = hipFirePosition;

            CreateBeamRenderer();
            CreateGunModel();
            CreateCrosshair();
            CreateADSCrosshair();
        }
        
        private void CreateCrosshair()
        {
            // Create hip-fire crosshair UI (AK-47 style - military reticle)
            crosshair = new GameObject("Crosshair");
            
            Canvas canvas = crosshair.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            UnityEngine.UI.CanvasScaler scaler = crosshair.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            Color mainColor = new Color(1f, 1f, 1f, 0.9f); // White
            Color shadowColor = new Color(0f, 0f, 0f, 0.5f); // Shadow
            
            // Center container
            GameObject center = new GameObject("Center");
            center.transform.SetParent(crosshair.transform);
            RectTransform centerRect = center.AddComponent<RectTransform>();
            centerRect.anchoredPosition = Vector2.zero;
            
            // Create AK-47 style crosshair - chevron/arrow style
            float gap = 6f;
            float lineLength = 12f;
            float thickness = 2f;
            
            // Top line
            CreateCrosshairElement(center.transform, "Top", new Vector2(0, gap + lineLength/2), new Vector2(thickness, lineLength), mainColor);
            // Bottom line  
            CreateCrosshairElement(center.transform, "Bottom", new Vector2(0, -gap - lineLength/2), new Vector2(thickness, lineLength), mainColor);
            // Left line
            CreateCrosshairElement(center.transform, "Left", new Vector2(-gap - lineLength/2, 0), new Vector2(lineLength, thickness), mainColor);
            // Right line
            CreateCrosshairElement(center.transform, "Right", new Vector2(gap + lineLength/2, 0), new Vector2(lineLength, thickness), mainColor);
            
            // Center dot
            CreateCrosshairElement(center.transform, "CenterDot", Vector2.zero, new Vector2(3, 3), mainColor);
            
            Debug.Log("✅ AK-47 crosshair created!");
        }
        
        private void CreateADSCrosshair()
        {
            // Create ADS crosshair (red dot sight style)
            adsCrosshair = new GameObject("ADSCrosshair");
            
            Canvas canvas = adsCrosshair.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 101;
            
            UnityEngine.UI.CanvasScaler scaler = adsCrosshair.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Red dot sight style
            Color redDot = new Color(1f, 0.1f, 0.1f, 1f); // Bright red
            
            GameObject center = new GameObject("RedDotCenter");
            center.transform.SetParent(adsCrosshair.transform);
            RectTransform centerRect = center.AddComponent<RectTransform>();
            centerRect.anchoredPosition = Vector2.zero;
            
            // Outer glow
            CreateCrosshairElement(center.transform, "Glow", Vector2.zero, new Vector2(8, 8), new Color(1f, 0f, 0f, 0.3f));
            
            // Red dot
            CreateCrosshairElement(center.transform, "RedDot", Vector2.zero, new Vector2(4, 4), redDot);
            
            // Start hidden
            adsCrosshair.SetActive(false);
            
            Debug.Log("✅ ADS red dot sight created!");
        }
        
        private void CreateCrosshairElement(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject element = new GameObject(name);
            element.transform.SetParent(parent);
            UnityEngine.UI.Image img = element.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            RectTransform rect = element.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
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
            // Try to load the AK-47 model
            GameObject loadedGun = Resources.Load<GameObject>("Weapons/AK47/AK47");
            
            if (loadedGun != null)
            {
                gunModel = Instantiate(loadedGun, playerCamera.transform);
                gunModel.name = "AK47";
                Debug.Log("✅ AK-47 model loaded!");
                
                // Position for FBX model
                gunModel.transform.localPosition = hipFirePosition;
                gunModel.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                
                // Auto-scale based on bounds
                Renderer[] renderers = gunModel.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = new Bounds(gunModel.transform.position, Vector3.zero);
                    foreach (Renderer r in renderers)
                        bounds.Encapsulate(r.bounds);
                    
                    float size = bounds.size.magnitude;
                    float targetSize = 0.5f; // AK-47 should be bigger
                    float scaleFactor = targetSize / Mathf.Max(size, 0.001f);
                    gunModel.transform.localScale = Vector3.one * scaleFactor;
                    Debug.Log($"AK-47 scaled by {scaleFactor}");
                }
            }
            else
            {
                // Create AK-47 style placeholder
                Debug.Log("⚠️ Creating placeholder AK-47");
                gunModel = CreatePlaceholderAK47();
                
                gunModel.transform.localPosition = hipFirePosition;
                gunModel.transform.localRotation = Quaternion.Euler(gunRotation);
                gunModel.transform.localScale = gunScale;
            }
            
            gunModel.transform.SetParent(playerCamera.transform);
            originalGunPosition = gunModel.transform.localPosition;
            currentGunPosition = hipFirePosition;
            
            // Remove colliders
            foreach (Collider col in gunModel.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }
            
            CreateMuzzleFlash();
            Debug.Log($"✅ AK-47 ready at {gunModel.transform.localPosition}");
        }
        
        private GameObject CreatePlaceholderAK47()
        {
            // Create AK-47 style placeholder gun
            GameObject gun = new GameObject("PlaceholderAK47");
            
            // Colors
            Color woodColor = new Color(0.4f, 0.25f, 0.1f); // Wood stock/grip
            Color metalColor = new Color(0.2f, 0.2f, 0.22f); // Dark metal
            Color blackMetal = new Color(0.1f, 0.1f, 0.1f);
            
            // Main receiver body
            GameObject receiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            receiver.name = "Receiver";
            receiver.transform.SetParent(gun.transform);
            receiver.transform.localPosition = Vector3.zero;
            receiver.transform.localScale = new Vector3(0.08f, 0.12f, 0.5f);
            ApplyGunMaterial(receiver, metalColor, false, 0.7f);
            Destroy(receiver.GetComponent<Collider>());
            
            // Barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gun.transform);
            barrel.transform.localPosition = new Vector3(0, 0.02f, 0.45f);
            barrel.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ApplyGunMaterial(barrel, blackMetal, false, 0.8f);
            Destroy(barrel.GetComponent<Collider>());
            
            // Gas tube (above barrel)
            GameObject gasTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gasTube.name = "GasTube";
            gasTube.transform.SetParent(gun.transform);
            gasTube.transform.localPosition = new Vector3(0, 0.08f, 0.25f);
            gasTube.transform.localScale = new Vector3(0.025f, 0.15f, 0.025f);
            gasTube.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ApplyGunMaterial(gasTube, metalColor, false, 0.6f);
            Destroy(gasTube.GetComponent<Collider>());
            
            // Wooden handguard
            GameObject handguard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handguard.name = "Handguard";
            handguard.transform.SetParent(gun.transform);
            handguard.transform.localPosition = new Vector3(0, -0.02f, 0.2f);
            handguard.transform.localScale = new Vector3(0.07f, 0.08f, 0.25f);
            ApplyGunMaterial(handguard, woodColor, false, 0.4f);
            Destroy(handguard.GetComponent<Collider>());
            
            // Magazine (curved AK style)
            GameObject magazine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            magazine.name = "Magazine";
            magazine.transform.SetParent(gun.transform);
            magazine.transform.localPosition = new Vector3(0, -0.18f, 0f);
            magazine.transform.localScale = new Vector3(0.06f, 0.2f, 0.12f);
            magazine.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
            ApplyGunMaterial(magazine, metalColor, false, 0.7f);
            Destroy(magazine.GetComponent<Collider>());
            
            // Pistol grip (wood)
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(gun.transform);
            grip.transform.localPosition = new Vector3(0, -0.15f, -0.12f);
            grip.transform.localScale = new Vector3(0.05f, 0.15f, 0.08f);
            grip.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
            ApplyGunMaterial(grip, woodColor, false, 0.4f);
            Destroy(grip.GetComponent<Collider>());
            
            // Stock (wood)
            GameObject stock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stock.name = "Stock";
            stock.transform.SetParent(gun.transform);
            stock.transform.localPosition = new Vector3(0, -0.02f, -0.35f);
            stock.transform.localScale = new Vector3(0.06f, 0.1f, 0.3f);
            ApplyGunMaterial(stock, woodColor, false, 0.4f);
            Destroy(stock.GetComponent<Collider>());
            
            // Stock end
            GameObject stockEnd = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stockEnd.name = "StockEnd";
            stockEnd.transform.SetParent(gun.transform);
            stockEnd.transform.localPosition = new Vector3(0, -0.04f, -0.52f);
            stockEnd.transform.localScale = new Vector3(0.07f, 0.12f, 0.08f);
            ApplyGunMaterial(stockEnd, woodColor, false, 0.4f);
            Destroy(stockEnd.GetComponent<Collider>());
            
            // Front sight
            GameObject frontSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontSight.name = "FrontSight";
            frontSight.transform.SetParent(gun.transform);
            frontSight.transform.localPosition = new Vector3(0, 0.12f, 0.4f);
            frontSight.transform.localScale = new Vector3(0.015f, 0.05f, 0.02f);
            ApplyGunMaterial(frontSight, blackMetal, false, 0.5f);
            Destroy(frontSight.GetComponent<Collider>());
            
            // Rear sight
            GameObject rearSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearSight.name = "RearSight";
            rearSight.transform.SetParent(gun.transform);
            rearSight.transform.localPosition = new Vector3(0, 0.1f, -0.1f);
            rearSight.transform.localScale = new Vector3(0.03f, 0.03f, 0.02f);
            ApplyGunMaterial(rearSight, blackMetal, false, 0.5f);
            Destroy(rearSight.GetComponent<Collider>());
            
            // Muzzle brake
            GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muzzle.name = "Muzzle";
            muzzle.transform.SetParent(gun.transform);
            muzzle.transform.localPosition = new Vector3(0, 0.02f, 0.72f);
            muzzle.transform.localScale = new Vector3(0.05f, 0.04f, 0.05f);
            muzzle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ApplyGunMaterial(muzzle, blackMetal, false, 0.6f);
            Destroy(muzzle.GetComponent<Collider>());
            
            return gun;
        }
        
        private void ApplyGoldMaterial(GameObject gun)
        {
            // Create a beautiful gold material
            Shader shader = World.URPMaterialHelper.GetLitShader();
            if (shader == null) shader = Shader.Find("Standard");
            
            Material goldMat = new Material(shader);
            
            // Rich gold color
            Color goldColor = new Color(1f, 0.84f, 0f); // Pure gold
            Color darkGold = new Color(0.85f, 0.65f, 0.13f); // Darker gold accents
            
            // Set properties based on shader type
            if (shader.name.Contains("Universal"))
            {
                // URP Lit shader properties
                goldMat.SetColor("_BaseColor", goldColor);
                goldMat.SetFloat("_Metallic", 1f);
                goldMat.SetFloat("_Smoothness", 0.85f);
            }
            else
            {
                // Standard shader properties
                goldMat.SetColor("_Color", goldColor);
                goldMat.SetFloat("_Metallic", 1f);
                goldMat.SetFloat("_Glossiness", 0.85f);
            }
            
            // Apply to all renderers
            Renderer[] renderers = gun.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                // Create material array with gold material
                Material[] mats = new Material[r.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = goldMat;
                }
                r.materials = mats;
            }
            
            Debug.Log($"✅ Applied gold material to {renderers.Length} renderer(s)");
        }
        
        private void CreateMuzzleFlash()
        {
            // Create muzzle flash container at gun barrel
            muzzleFlash = new GameObject("MuzzleFlash");
            muzzleFlash.transform.SetParent(gunModel.transform);
            // Position at barrel end
            muzzleFlash.transform.localPosition = new Vector3(0, 0.1f, 0.5f); // Forward from gun
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
            // Create a detailed GOLD placeholder gun using primitives
            GameObject gun = new GameObject("PlaceholderGoldPistol");
            
            // Gold colors
            Color brightGold = new Color(1f, 0.84f, 0f);      // Bright gold
            Color darkGold = new Color(0.85f, 0.65f, 0.13f);  // Dark gold
            Color blackAccent = new Color(0.1f, 0.1f, 0.1f);  // Black accents
            
            // Gun body/frame (main rectangle) - GOLD
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(gun.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(0.15f, 0.22f, 0.35f);
            ApplyGunMaterial(frame, brightGold, false, 0.9f);
            Destroy(frame.GetComponent<Collider>());
            
            // Barrel - extended and visible - GOLD
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gun.transform);
            barrel.transform.localPosition = new Vector3(0, 0.03f, 0.25f);
            barrel.transform.localScale = new Vector3(0.1f, 0.08f, 0.3f);
            ApplyGunMaterial(barrel, darkGold, false, 0.95f);
            Destroy(barrel.GetComponent<Collider>());
            
            // Slide (top part) - prominent - GOLD
            GameObject slide = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slide.name = "Slide";
            slide.transform.SetParent(gun.transform);
            slide.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            slide.transform.localScale = new Vector3(0.13f, 0.06f, 0.42f);
            ApplyGunMaterial(slide, brightGold, false, 0.9f);
            Destroy(slide.GetComponent<Collider>());
            
            // Slide grooves (detail lines) - dark accent
            for (int i = 0; i < 8; i++)
            {
                GameObject groove = GameObject.CreatePrimitive(PrimitiveType.Cube);
                groove.name = $"SlideGroove_{i}";
                groove.transform.SetParent(slide.transform);
                groove.transform.localPosition = new Vector3(0, 0.55f, -0.3f + i * 0.08f);
                groove.transform.localScale = new Vector3(1.05f, 0.1f, 0.03f);
                ApplyGunMaterial(groove, darkGold, false, 0.8f);
                Destroy(groove.GetComponent<Collider>());
            }
            
            // Handle/Grip - black with gold accents
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Grip";
            grip.transform.SetParent(gun.transform);
            grip.transform.localPosition = new Vector3(0, -0.15f, -0.08f);
            grip.transform.localScale = new Vector3(0.12f, 0.25f, 0.15f);
            grip.transform.localRotation = Quaternion.Euler(-18f, 0, 0);
            ApplyGunMaterial(grip, blackAccent, false, 0.5f);
            Destroy(grip.GetComponent<Collider>());
            
            // Grip texture lines - gold accents
            for (int i = 0; i < 6; i++)
            {
                GameObject gripLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gripLine.name = $"GripLine_{i}";
                gripLine.transform.SetParent(grip.transform);
                gripLine.transform.localPosition = new Vector3(0.51f, -0.3f + i * 0.12f, 0);
                gripLine.transform.localScale = new Vector3(0.02f, 0.08f, 0.9f);
                ApplyGunMaterial(gripLine, darkGold, false, 0.7f);
                Destroy(gripLine.GetComponent<Collider>());
            }
            
            // Trigger guard - gold
            GameObject triggerGuard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerGuard.name = "TriggerGuard";
            triggerGuard.transform.SetParent(gun.transform);
            triggerGuard.transform.localPosition = new Vector3(0, -0.1f, 0.03f);
            triggerGuard.transform.localScale = new Vector3(0.1f, 0.03f, 0.12f);
            ApplyGunMaterial(triggerGuard, brightGold, false, 0.9f);
            Destroy(triggerGuard.GetComponent<Collider>());
            
            // Trigger - dark gold
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = "Trigger";
            trigger.transform.SetParent(gun.transform);
            trigger.transform.localPosition = new Vector3(0, -0.06f, 0.02f);
            trigger.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);
            trigger.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
            ApplyGunMaterial(trigger, darkGold, false, 0.85f);
            Destroy(trigger.GetComponent<Collider>());
            
            // Magazine - gold
            GameObject magazine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            magazine.name = "Magazine";
            magazine.transform.SetParent(gun.transform);
            magazine.transform.localPosition = new Vector3(0, -0.28f, -0.05f);
            magazine.transform.localScale = new Vector3(0.08f, 0.12f, 0.12f);
            ApplyGunMaterial(magazine, brightGold, false, 0.9f);
            Destroy(magazine.GetComponent<Collider>());
            
            // Front sight - black
            GameObject frontSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontSight.name = "FrontSight";
            frontSight.transform.SetParent(gun.transform);
            frontSight.transform.localPosition = new Vector3(0, 0.13f, 0.2f);
            frontSight.transform.localScale = new Vector3(0.025f, 0.04f, 0.025f);
            ApplyGunMaterial(frontSight, blackAccent, false, 0.6f);
            Destroy(frontSight.GetComponent<Collider>());
            
            // Front sight dot (glowing green)
            GameObject frontDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            frontDot.name = "FrontSightDot";
            frontDot.transform.SetParent(frontSight.transform);
            frontDot.transform.localPosition = new Vector3(0, 0.6f, 0);
            frontDot.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            ApplyGunMaterial(frontDot, new Color(0f, 1f, 0f), true, 0.9f); // Green glow
            Destroy(frontDot.GetComponent<Collider>());
            
            // Rear sight - black
            GameObject rearSight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearSight.name = "RearSight";
            rearSight.transform.SetParent(gun.transform);
            rearSight.transform.localPosition = new Vector3(0, 0.13f, -0.12f);
            rearSight.transform.localScale = new Vector3(0.05f, 0.04f, 0.025f);
            ApplyGunMaterial(rearSight, blackAccent, false, 0.6f);
            Destroy(rearSight.GetComponent<Collider>());
            
            // Rear sight notch
            GameObject rearNotch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearNotch.name = "RearNotch";
            rearNotch.transform.SetParent(rearSight.transform);
            rearNotch.transform.localPosition = new Vector3(0, 0.3f, 0);
            rearNotch.transform.localScale = new Vector3(0.3f, 0.5f, 1f);
            ApplyGunMaterial(rearNotch, new Color(0.02f, 0.02f, 0.02f), false, 0.3f);
            Destroy(rearNotch.GetComponent<Collider>());
            
            // Muzzle (barrel end) - gold
            GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muzzle.name = "Muzzle";
            muzzle.transform.SetParent(gun.transform);
            muzzle.transform.localPosition = new Vector3(0, 0.03f, 0.42f);
            muzzle.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);
            muzzle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ApplyGunMaterial(muzzle, brightGold, false, 0.95f);
            Destroy(muzzle.GetComponent<Collider>());
            
            // Inner barrel (dark hole)
            GameObject innerBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerBarrel.name = "InnerBarrel";
            innerBarrel.transform.SetParent(muzzle.transform);
            innerBarrel.transform.localPosition = new Vector3(0, 0.6f, 0);
            innerBarrel.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            ApplyGunMaterial(innerBarrel, new Color(0.02f, 0.02f, 0.02f), false, 0.1f);
            Destroy(innerBarrel.GetComponent<Collider>());
            
            // Ejection port - dark
            GameObject ejectionPort = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ejectionPort.name = "EjectionPort";
            ejectionPort.transform.SetParent(gun.transform);
            ejectionPort.transform.localPosition = new Vector3(0.07f, 0.07f, 0f);
            ejectionPort.transform.localScale = new Vector3(0.02f, 0.04f, 0.08f);
            ApplyGunMaterial(ejectionPort, blackAccent, false, 0.3f);
            Destroy(ejectionPort.GetComponent<Collider>());
            
            // Rail (under barrel) - gold
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(gun.transform);
            rail.transform.localPosition = new Vector3(0, -0.02f, 0.12f);
            rail.transform.localScale = new Vector3(0.08f, 0.02f, 0.15f);
            ApplyGunMaterial(rail, darkGold, false, 0.85f);
            Destroy(rail.GetComponent<Collider>());
            
            return gun;
        }
        
        private void ApplyGunMaterial(GameObject obj, Color color, bool emissive = false, float glossiness = 0.7f)
        {
            Shader shader = World.URPMaterialHelper.GetLitShader();
            if (shader == null) shader = Shader.Find("Standard");
            
            Material mat = new Material(shader);
            
            if (shader.name.Contains("Universal"))
            {
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Metallic", 0.95f);
                mat.SetFloat("_Smoothness", glossiness);
            }
            else
            {
                mat.SetColor("_Color", color);
                mat.SetFloat("_Metallic", 0.95f);
                mat.SetFloat("_Glossiness", glossiness);
                
                if (emissive)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * 3f);
                }
            }
            
            obj.GetComponent<Renderer>().material = mat;
        }

        private void Update()
        {
            // Handle ADS (Aim Down Sights) with right mouse button
            HandleADS();
            
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
            
            // Handle gun recoil recovery and position
            if (gunModel != null)
            {
                // Smoothly move gun to target position (hip or ADS)
                Vector3 targetPos = isAiming ? adsPosition : hipFirePosition;
                currentGunPosition = Vector3.Lerp(currentGunPosition, targetPos, Time.deltaTime * adsSpeed);
                
                // Apply recoil on top of current position
                if (currentRecoil > 0f)
                {
                    currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilRecovery);
                }
                
                gunModel.transform.localPosition = currentGunPosition + new Vector3(0, currentRecoil * 0.15f, -currentRecoil * 0.3f);
                gunModel.transform.localRotation = Quaternion.Euler(gunRotation.x - currentRecoil * 60f, gunRotation.y, gunRotation.z);
            }
            
            // Smooth FOV transition
            if (playerCamera != null)
            {
                float targetFOV = isAiming ? adsFOV : normalFOV;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);
            }
        }
        
        private void HandleADS()
        {
            // Right mouse button to aim
            if (Input.GetMouseButton(1))
            {
                isAiming = true;
                // Show ADS crosshair, hide normal crosshair
                if (crosshair != null) crosshair.SetActive(false);
                if (adsCrosshair != null) adsCrosshair.SetActive(true);
            }
            else
            {
                isAiming = false;
                // Show normal crosshair, hide ADS crosshair
                if (crosshair != null) crosshair.SetActive(true);
                if (adsCrosshair != null) adsCrosshair.SetActive(false);
            }
        }
        
        public bool IsAiming()
        {
            return isAiming;
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
            // Position shell ejection at the gun's ejection port (right side)
            shell.transform.position = gunModel.transform.TransformPoint(new Vector3(0.1f, 0.05f, 0.1f));
            shell.transform.localScale = new Vector3(0.008f, 0.015f, 0.008f);
            
            // Gold brass material to match gold pistol
            Shader shader = World.URPMaterialHelper.GetLitShader();
            if (shader == null) shader = Shader.Find("Standard");
            
            Material shellMat = new Material(shader);
            Color brassColor = new Color(0.95f, 0.75f, 0.2f); // Golden brass
            
            if (shader.name.Contains("Universal"))
            {
                shellMat.SetColor("_BaseColor", brassColor);
                shellMat.SetFloat("_Metallic", 0.95f);
                shellMat.SetFloat("_Smoothness", 0.8f);
            }
            else
            {
                shellMat.SetColor("_Color", brassColor);
                shellMat.SetFloat("_Metallic", 0.95f);
                shellMat.SetFloat("_Glossiness", 0.8f);
                shellMat.EnableKeyword("_EMISSION");
                shellMat.SetColor("_EmissionColor", brassColor * 0.3f);
            }
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

            // Apply spread for accuracy - better accuracy when aiming down sights
            float currentAccuracy = isAiming ? adsAccuracy : accuracy;
            Vector3 spread = Random.insideUnitSphere * (1f - currentAccuracy);
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

