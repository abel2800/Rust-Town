using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Handles dynamic atmosphere effects: flickering lights, dust particles, embers, wind
    /// </summary>
    public class AtmosphereEffects : MonoBehaviour
    {
        [Header("Flickering Lights")]
        private List<Light> flickerLights = new List<Light>();
        private float[] originalIntensities;
        
        [Header("Dust Particles")]
        [SerializeField] private int dustParticleCount = 100;
        private GameObject[] dustParticles;
        
        [Header("Ember Particles")]
        [SerializeField] private int emberCount = 30;
        private GameObject[] embers;
        
        [Header("Wind Settings")]
        [SerializeField] private float windSpeed = 2f;
        [SerializeField] private Vector3 windDirection = new Vector3(1f, 0.2f, 0.5f);
        
        private Material dustMaterial;
        private Material emberMaterial;
        
        private void Start()
        {
            CreateMaterials();
            FindFlickerLights();
            CreateDustParticles();
            CreateEmbers();
        }
        
        private void CreateMaterials()
        {
            // Dust material
            dustMaterial = new Material(Shader.Find("Standard"));
            dustMaterial.SetColor("_Color", new Color(0.8f, 0.75f, 0.65f, 0.4f));
            dustMaterial.EnableKeyword("_EMISSION");
            dustMaterial.SetColor("_EmissionColor", new Color(0.6f, 0.5f, 0.4f) * 0.2f);
            
            // Ember material (glowing orange)
            emberMaterial = new Material(Shader.Find("Standard"));
            emberMaterial.SetColor("_Color", new Color(1f, 0.5f, 0.1f));
            emberMaterial.EnableKeyword("_EMISSION");
            emberMaterial.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 2f);
        }
        
        private void FindFlickerLights()
        {
            // Find all fire lights and set them up for flickering
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in allLights)
            {
                if (light.gameObject.name.Contains("Fire") || (light.color.r > 0.8f && light.color.g < 0.7f))
                {
                    flickerLights.Add(light);
                }
            }
            
            // Store original intensities
            originalIntensities = new float[flickerLights.Count];
            for (int i = 0; i < flickerLights.Count; i++)
            {
                originalIntensities[i] = flickerLights[i].intensity;
            }
        }
        
        private void CreateDustParticles()
        {
            dustParticles = new GameObject[dustParticleCount];
            
            for (int i = 0; i < dustParticleCount; i++)
            {
                GameObject dust = GameObject.CreatePrimitive(PrimitiveType.Quad);
                dust.name = "DustParticle";
                dust.transform.SetParent(transform);
                dust.transform.position = GetRandomDustPosition();
                dust.transform.localScale = Vector3.one * Random.Range(0.02f, 0.08f);
                dust.transform.rotation = Random.rotation;
                
                dust.GetComponent<Renderer>().material = dustMaterial;
                Destroy(dust.GetComponent<Collider>());
                
                dustParticles[i] = dust;
            }
        }
        
        private void CreateEmbers()
        {
            embers = new GameObject[emberCount];
            
            // Find fire positions
            Light[] fireLights = flickerLights.ToArray();
            
            for (int i = 0; i < emberCount; i++)
            {
                GameObject ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ember.name = "Ember";
                ember.transform.SetParent(transform);
                
                // Start near a fire if available
                if (fireLights.Length > 0)
                {
                    Light randomFire = fireLights[Random.Range(0, fireLights.Length)];
                    ember.transform.position = randomFire.transform.position + 
                        new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 2f), Random.Range(-1f, 1f));
                }
                else
                {
                    ember.transform.position = new Vector3(
                        Random.Range(-30f, 30f),
                        Random.Range(1f, 5f),
                        Random.Range(-30f, 30f)
                    );
                }
                
                ember.transform.localScale = Vector3.one * Random.Range(0.02f, 0.05f);
                ember.GetComponent<Renderer>().material = emberMaterial;
                Destroy(ember.GetComponent<Collider>());
                
                embers[i] = ember;
            }
        }
        
        private Vector3 GetRandomDustPosition()
        {
            // Random position in the play area, slightly biased toward visible areas
            return new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(0.5f, 8f),
                Random.Range(-50f, 50f)
            );
        }
        
        private void Update()
        {
            UpdateFlickeringLights();
            UpdateDustParticles();
            UpdateEmbers();
        }
        
        private void UpdateFlickeringLights()
        {
            for (int i = 0; i < flickerLights.Count; i++)
            {
                if (flickerLights[i] != null)
                {
                    // Perlin noise based flickering for natural fire effect
                    float noise = Mathf.PerlinNoise(Time.time * 10f + i * 100f, i * 50f);
                    float flicker = originalIntensities[i] * (0.7f + noise * 0.6f);
                    flickerLights[i].intensity = flicker;
                    
                    // Slight color variation
                    float colorNoise = Mathf.PerlinNoise(Time.time * 5f + i * 200f, 0);
                    flickerLights[i].color = new Color(
                        1f,
                        0.5f + colorNoise * 0.2f,
                        0.1f + colorNoise * 0.1f
                    );
                }
            }
        }
        
        private void UpdateDustParticles()
        {
            Vector3 wind = windDirection.normalized * windSpeed * Time.deltaTime;
            
            for (int i = 0; i < dustParticles.Length; i++)
            {
                if (dustParticles[i] == null) continue;
                
                Transform t = dustParticles[i].transform;
                
                // Move with wind and slight random drift
                Vector3 drift = new Vector3(
                    Mathf.Sin(Time.time + i) * 0.1f,
                    Mathf.Sin(Time.time * 0.5f + i * 0.3f) * 0.05f,
                    Mathf.Cos(Time.time + i) * 0.1f
                );
                
                t.position += (wind + drift) * Time.deltaTime;
                
                // Slowly rotate
                t.Rotate(Vector3.up * 10f * Time.deltaTime);
                
                // Reset if too far or too low/high
                if (t.position.magnitude > 60f || t.position.y < 0 || t.position.y > 10f)
                {
                    t.position = GetRandomDustPosition();
                }
                
                // Face camera for billboarding effect
                if (Camera.main != null)
                {
                    t.LookAt(Camera.main.transform);
                }
            }
        }
        
        private void UpdateEmbers()
        {
            for (int i = 0; i < embers.Length; i++)
            {
                if (embers[i] == null) continue;
                
                Transform t = embers[i].transform;
                
                // Embers rise and drift
                float riseSpeed = 0.5f + Mathf.Sin(Time.time + i) * 0.3f;
                Vector3 movement = new Vector3(
                    Mathf.Sin(Time.time * 2f + i) * 0.5f,
                    riseSpeed,
                    Mathf.Cos(Time.time * 2f + i) * 0.5f
                ) * Time.deltaTime;
                
                t.position += movement + windDirection.normalized * windSpeed * 0.5f * Time.deltaTime;
                
                // Pulse glow
                Renderer r = embers[i].GetComponent<Renderer>();
                if (r != null)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 5f + i) * 0.3f;
                    r.material.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * pulse * 2f);
                }
                
                // Reset when too high or far
                if (t.position.y > 15f || t.position.magnitude > 70f)
                {
                    // Respawn near a random fire or burning barrel
                    Light[] fires = flickerLights.ToArray();
                    if (fires.Length > 0)
                    {
                        Light randomFire = fires[Random.Range(0, fires.Length)];
                        t.position = randomFire.transform.position + 
                            new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f), Random.Range(-0.5f, 0.5f));
                    }
                    else
                    {
                        t.position = new Vector3(
                            Random.Range(-20f, 20f),
                            Random.Range(0.5f, 2f),
                            Random.Range(-20f, 20f)
                        );
                    }
                }
            }
        }
    }
}

