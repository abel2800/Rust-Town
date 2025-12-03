using UnityEngine;

namespace NeonArena.World
{
    /// <summary>
    /// Helper class for creating materials compatible with Universal Render Pipeline (URP)
    /// </summary>
    public static class URPMaterialHelper
    {
        private static Shader _litShader;
        private static Shader _unlitShader;
        
        /// <summary>
        /// Gets the appropriate lit shader for the current render pipeline
        /// </summary>
        public static Shader GetLitShader()
        {
            if (_litShader != null) return _litShader;
            
            // Try URP shaders first
            _litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_litShader != null) return _litShader;
            
            _litShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (_litShader != null) return _litShader;
            
            // Fallback to Standard (Built-in RP)
            _litShader = Shader.Find("Standard");
            if (_litShader != null) return _litShader;
            
            // Last resort
            _litShader = Shader.Find("Diffuse");
            return _litShader;
        }
        
        /// <summary>
        /// Gets an unlit shader for the current render pipeline
        /// </summary>
        public static Shader GetUnlitShader()
        {
            if (_unlitShader != null) return _unlitShader;
            
            _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (_unlitShader != null) return _unlitShader;
            
            _unlitShader = Shader.Find("Unlit/Color");
            return _unlitShader;
        }
        
        /// <summary>
        /// Creates a new material with the correct shader for current render pipeline
        /// </summary>
        public static Material CreateMaterial()
        {
            return new Material(GetLitShader());
        }
        
        /// <summary>
        /// Creates a material with specified color
        /// </summary>
        public static Material CreateMaterial(Color color, float smoothness = 0.5f, float metallic = 0f)
        {
            Material mat = new Material(GetLitShader());
            SetMaterialProperties(mat, color, smoothness, metallic);
            return mat;
        }
        
        /// <summary>
        /// Sets material properties compatible with both URP and Built-in
        /// </summary>
        public static void SetMaterialProperties(Material mat, Color color, float smoothness = 0.5f, float metallic = 0f)
        {
            // Base color - works for both URP and Standard
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            
            // Smoothness
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", smoothness);
            
            // Metallic
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", metallic);
        }
        
        /// <summary>
        /// Sets emission on a material
        /// </summary>
        public static void SetEmission(Material mat, Color emissionColor)
        {
            // URP
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
            }
            
            // Enable emission
            if (mat.HasProperty("_EmissiveColor"))
            {
                mat.SetColor("_EmissiveColor", emissionColor);
            }
        }
        
        /// <summary>
        /// Creates a transparent material
        /// </summary>
        public static Material CreateTransparentMaterial(Color color)
        {
            Material mat = new Material(GetLitShader());
            
            // Set surface type to transparent
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            }
            
            // Set blend mode
            if (mat.HasProperty("_Blend"))
            {
                mat.SetFloat("_Blend", 0); // Alpha blend
            }
            
            // URP specific
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
            
            // Set render queue for transparency
            mat.renderQueue = 3000;
            
            // Enable alpha blending keywords
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            
            return mat;
        }
    }
}

