using UnityEngine;

namespace NeonArena.Rendering
{
    /// <summary>
    /// Creates professional, elegant neon materials with delicate color palettes
    /// Focuses on sophisticated aesthetics rather than overly bright colors
    /// </summary>
    public class NeonMaterialFactory : MonoBehaviour
    {
        // Professional color themes - delicate and elegant
        private static readonly ColorTheme[] themes = new ColorTheme[]
        {
            // Midnight Blue - Deep, professional blue with subtle cyan accents
            new ColorTheme(
                "Midnight Blue",
                new Color(0.15f, 0.25f, 0.45f),  // Base: Deep navy
                new Color(0.3f, 0.6f, 0.85f),    // Accent: Soft cyan
                new Color(0.2f, 0.35f, 0.6f)     // Secondary: Medium blue
            ),

            // Violet Dreams - Elegant purple with soft magenta
            new ColorTheme(
                "Violet Dreams",
                new Color(0.25f, 0.15f, 0.4f),   // Base: Deep purple
                new Color(0.6f, 0.3f, 0.75f),    // Accent: Soft magenta
                new Color(0.4f, 0.25f, 0.55f)    // Secondary: Medium violet
            ),

            // Emerald Mist - Sophisticated teal with mint accents
            new ColorTheme(
                "Emerald Mist",
                new Color(0.15f, 0.35f, 0.35f),  // Base: Deep teal
                new Color(0.3f, 0.7f, 0.6f),     // Accent: Mint green
                new Color(0.2f, 0.5f, 0.5f)      // Secondary: Medium teal
            ),

            // Rose Gold - Warm, elegant pink with copper tones
            new ColorTheme(
                "Rose Gold",
                new Color(0.35f, 0.2f, 0.3f),    // Base: Deep rose
                new Color(0.85f, 0.5f, 0.6f),    // Accent: Soft pink
                new Color(0.6f, 0.35f, 0.45f)    // Secondary: Medium rose
            ),

            // Amber Twilight - Refined gold with warm orange
            new ColorTheme(
                "Amber Twilight",
                new Color(0.4f, 0.3f, 0.15f),    // Base: Deep amber
                new Color(0.9f, 0.7f, 0.3f),     // Accent: Soft gold
                new Color(0.65f, 0.5f, 0.25f)    // Secondary: Medium amber
            ),

            // Arctic Steel - Cool, professional cyan-grey
            new ColorTheme(
                "Arctic Steel",
                new Color(0.2f, 0.3f, 0.35f),    // Base: Steel grey
                new Color(0.5f, 0.75f, 0.85f),   // Accent: Ice blue
                new Color(0.35f, 0.5f, 0.6f)     // Secondary: Medium steel
            )
        };

        private static int currentThemeIndex = 0;
        private static ColorTheme currentTheme;

        public void SelectRandomTheme()
        {
            currentThemeIndex = Random.Range(0, themes.Length);
            currentTheme = themes[currentThemeIndex];
            Debug.Log($"Selected theme: {currentTheme.Name}");
        }

        public static Color GetCurrentThemeColor()
        {
            if (currentTheme.Name == null)
            {
                currentTheme = themes[0];
            }
            return currentTheme.AccentColor;
        }

        // Create material with elegant emission
        private static Material CreateBaseMaterial(Color baseColor, Color emissionColor, float emissionIntensity)
        {
            Material mat = new Material(Shader.Find("Standard"));
            
            // Base properties
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Glossiness", 0.6f);
            
            // Elegant emission - enhanced for better visibility
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            
            return mat;
        }

        // Floor material - subtle with slight glow
        public static void ApplyFloorMaterial(Renderer renderer)
        {
            Material mat = CreateBaseMaterial(
                currentTheme.BaseColor * 1.2f,
                currentTheme.AccentColor,
                0.8f  // Increased emission for better visibility
            );
            mat.SetFloat("_Glossiness", 0.9f); // Very reflective
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
        }

        // Wall material - professional glass-like appearance
        public static void ApplyWallMaterial(Renderer renderer)
        {
            Material mat = CreateBaseMaterial(
                new Color(0.2f, 0.2f, 0.3f),
                currentTheme.AccentColor,
                1.2f  // Brighter glow for definition
            );
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.95f);
            renderer.material = mat;
        }

        // Obstacle material - defined but not overwhelming
        public static void ApplyObstacleMaterial(Renderer renderer)
        {
            Material mat = CreateBaseMaterial(
                currentTheme.SecondaryColor * 1.3f,
                currentTheme.AccentColor,
                1.5f  // Enhanced emission for better visibility
            );
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Glossiness", 0.7f);
            renderer.material = mat;
        }

        // Platform material - elegant and inviting
        public static void ApplyPlatformMaterial(Renderer renderer)
        {
            Material mat = CreateBaseMaterial(
                currentTheme.SecondaryColor * 1.2f,
                currentTheme.AccentColor,
                0.8f  // Noticeable but refined
            );
            mat.SetFloat("_Glossiness", 0.7f);
            renderer.material = mat;
        }

        // Accent material - for highlights and details
        public static void ApplyAccentMaterial(Renderer renderer, float glowIntensity = 1f)
        {
            Material mat = CreateBaseMaterial(
                currentTheme.AccentColor * 0.5f,
                currentTheme.AccentColor,
                glowIntensity  // Variable intensity
            );
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.8f);
            renderer.material = mat;
        }

        // Enemy material - distinctive but elegant
        public static void ApplyEnemyMaterial(Renderer renderer)
        {
            // Enemies use a contrasting warm red/orange tone for clarity
            Color enemyBase = new Color(0.8f, 0.2f, 0.2f);
            Color enemyEmission = new Color(2f, 0.5f, 0.5f);
            
            Material mat = CreateBaseMaterial(
                enemyBase,
                enemyEmission,
                3.0f  // Bright glow so they're easy to see
            );
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Glossiness", 0.8f);
            renderer.material = mat;
        }
        
        // Enemy head material - brighter for distinction
        public static void ApplyEnemyHeadMaterial(Renderer renderer)
        {
            Color headBase = new Color(1f, 0.3f, 0.3f);
            Color headEmission = new Color(3f, 0.8f, 0.8f);
            
            Material mat = CreateBaseMaterial(
                headBase,
                headEmission,
                4.0f  // Very bright for headshot targeting
            );
            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Glossiness", 0.9f);
            renderer.material = mat;
        }
        
        // Enemy core material - super bright pulsing center
        public static void ApplyEnemyCoreMaterial(Renderer renderer)
        {
            Color coreBase = Color.white;
            Color coreEmission = new Color(4f, 1f, 1f);
            
            Material mat = CreateBaseMaterial(
                coreBase,
                coreEmission,
                6.0f  // Extremely bright core
            );
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 1.0f);
            renderer.material = mat;
        }
        
        // Zombie body material - pale, sickly color
        public static void ApplyZombieMaterial(Renderer renderer)
        {
            // Pale greenish-grey zombie skin
            Color zombieBase = new Color(0.45f, 0.55f, 0.4f);
            Color zombieEmission = new Color(0.3f, 0.5f, 0.3f);
            
            Material mat = CreateBaseMaterial(
                zombieBase,
                zombieEmission,
                0.5f  // Subtle glow
            );
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Glossiness", 0.3f);
            renderer.material = mat;
        }
        
        // Zombie glowing eyes - eerie red/orange glow
        public static void ApplyZombieEyeMaterial(Renderer renderer)
        {
            Color eyeBase = new Color(1f, 0.3f, 0.1f);
            Color eyeEmission = new Color(3f, 0.5f, 0.2f);
            
            Material mat = CreateBaseMaterial(
                eyeBase,
                eyeEmission,
                5.0f  // Bright glowing eyes
            );
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.9f);
            renderer.material = mat;
        }

        // UI accent color for consistency
        public static Color GetUIAccentColor()
        {
            return currentTheme.AccentColor;
        }

        public static Color GetUISecondaryColor()
        {
            return currentTheme.SecondaryColor;
        }

        // Helper struct for color themes
        private struct ColorTheme
        {
            public string Name;
            public Color BaseColor;
            public Color AccentColor;
            public Color SecondaryColor;

            public ColorTheme(string name, Color baseColor, Color accentColor, Color secondaryColor)
            {
                Name = name;
                BaseColor = baseColor;
                AccentColor = accentColor;
                SecondaryColor = secondaryColor;
            }
        }

        // Animated pulsing material for special effects
        public static Material CreatePulseMaterial(float pulseSpeed = 2f)
        {
            Material mat = CreateBaseMaterial(
                currentTheme.BaseColor,
                currentTheme.AccentColor,
                1f
            );
            
            // Note: Pulse animation should be handled in Update() of the component using this material
            return mat;
        }

        // Get theme info for debugging
        public static string GetCurrentThemeName()
        {
            return currentTheme.Name ?? "Default";
        }
    }
}

