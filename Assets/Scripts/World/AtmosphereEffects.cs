using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Handles dynamic atmosphere effects: flickering lights, dust particles, embers, wind,
    /// falling leaves, insects, ambient sounds simulation, and realistic environmental motion
    /// </summary>
    public class AtmosphereEffects : MonoBehaviour
    {
        [Header("Flickering Lights")]
        private List<Light> flickerLights = new List<Light>();
        private float[] originalIntensities;
        
        [Header("Dust Particles")]
        [SerializeField] private int dustParticleCount = 150;
        private GameObject[] dustParticles;
        
        [Header("Ember Particles")]
        [SerializeField] private int emberCount = 40;
        private GameObject[] embers;
        
        [Header("Falling Leaves")]
        [SerializeField] private int fallingLeafCount = 60;
        private GameObject[] fallingLeaves;
        
        [Header("Flying Insects/Birds")]
        [SerializeField] private int insectCount = 15;
        private GameObject[] insects;
        private Vector3[] insectTargets;
        
        [Header("Floating Pollen/Spores")]
        [SerializeField] private int pollenCount = 80;
        private GameObject[] pollen;
        
        [Header("Wind Settings")]
        [SerializeField] private float windSpeed = 2.5f;
        [SerializeField] private Vector3 windDirection = new Vector3(1f, 0.15f, 0.5f);
        [SerializeField] private float windGustStrength = 1.5f;
        private float windGustTimer = 0f;
        private float currentGustStrength = 0f;
        
        [Header("Dynamic Fog")]
        [SerializeField] private int movingFogCount = 8;
        private GameObject[] movingFogPatches;
        
        private Material dustMaterial;
        private Material emberMaterial;
        private Material leafMaterial;
        private Material pollenMaterial;
        private Material insectMaterial;
        
        private void Start()
        {
            CreateMaterials();
            FindFlickerLights();
            CreateDustParticles();
            CreateEmbers();
            CreateFallingLeaves();
            CreateInsects();
            CreatePollen();
            CreateMovingFog();
        }
        
        private void CreateMaterials()
        {
            // Dust material - realistic dust motes
            dustMaterial = new Material(URPMaterialHelper.GetLitShader());
            dustMaterial.SetColor("_Color", new Color(0.85f, 0.78f, 0.65f, 0.35f));
            dustMaterial.EnableKeyword("_EMISSION");
            dustMaterial.SetColor("_EmissionColor", new Color(0.7f, 0.6f, 0.5f) * 0.15f);
            
            // Ember material (glowing orange)
            emberMaterial = new Material(URPMaterialHelper.GetLitShader());
            emberMaterial.SetColor("_Color", new Color(1f, 0.5f, 0.1f));
            emberMaterial.EnableKeyword("_EMISSION");
            emberMaterial.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 2.5f);
            
            // Falling leaf material
            leafMaterial = new Material(URPMaterialHelper.GetLitShader());
            leafMaterial.SetColor("_Color", new Color(0.55f, 0.4f, 0.2f));
            leafMaterial.SetFloat("_Glossiness", 0.1f);
            
            // Pollen/spore material - tiny golden specks
            pollenMaterial = new Material(URPMaterialHelper.GetLitShader());
            pollenMaterial.SetColor("_Color", new Color(1f, 0.95f, 0.7f, 0.5f));
            pollenMaterial.EnableKeyword("_EMISSION");
            pollenMaterial.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.6f) * 0.3f);
            
            // Insect/bird material - dark silhouette
            insectMaterial = new Material(URPMaterialHelper.GetLitShader());
            insectMaterial.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
            insectMaterial.SetFloat("_Glossiness", 0.2f);
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
            UpdateWindGusts();
            UpdateFlickeringLights();
            UpdateDustParticles();
            UpdateEmbers();
            UpdateFallingLeaves();
            UpdateInsects();
            UpdatePollen();
            UpdateMovingFog();
        }
        
        private void UpdateWindGusts()
        {
            // Simulate wind gusts
            windGustTimer += Time.deltaTime;
            
            // Create occasional gusts
            if (windGustTimer > Random.Range(3f, 8f))
            {
                windGustTimer = 0f;
                currentGustStrength = windGustStrength * Random.Range(0.5f, 1.5f);
            }
            
            // Decay gust strength
            currentGustStrength = Mathf.Lerp(currentGustStrength, 0f, Time.deltaTime * 0.5f);
        }
        
        private Vector3 GetCurrentWind()
        {
            float gustEffect = 1f + currentGustStrength;
            float turbulence = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.3f;
            
            return windDirection.normalized * windSpeed * gustEffect * (1f + turbulence);
        }
        
        private void UpdateFlickeringLights()
        {
            // Clean up destroyed lights first
            for (int i = flickerLights.Count - 1; i >= 0; i--)
            {
                if (flickerLights[i] == null)
                {
                    flickerLights.RemoveAt(i);
                    // Resize intensities array if needed
                    if (i < originalIntensities.Length)
                    {
                        float[] newIntensities = new float[flickerLights.Count];
                        for (int j = 0; j < newIntensities.Length; j++)
                        {
                            newIntensities[j] = j < originalIntensities.Length ? originalIntensities[j] : 1f;
                        }
                        originalIntensities = newIntensities;
                    }
                }
            }
            
            for (int i = 0; i < flickerLights.Count; i++)
            {
                if (flickerLights[i] != null && i < originalIntensities.Length)
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
            Vector3 wind = GetCurrentWind() * Time.deltaTime;
            
            for (int i = 0; i < dustParticles.Length; i++)
            {
                if (dustParticles[i] == null) continue;
                
                Transform t = dustParticles[i].transform;
                
                // Move with wind and slight random drift
                Vector3 drift = new Vector3(
                    Mathf.Sin(Time.time + i) * 0.15f,
                    Mathf.Sin(Time.time * 0.5f + i * 0.3f) * 0.08f,
                    Mathf.Cos(Time.time + i) * 0.15f
                );
                
                t.position += (wind + drift) * Time.deltaTime;
                
                // Slowly rotate with varied speeds
                t.Rotate(Vector3.up * (8f + i * 0.2f) * Time.deltaTime);
                
                // Catch light sparkle effect
                Renderer r = dustParticles[i].GetComponent<Renderer>();
                if (r != null)
                {
                    float sparkle = Mathf.PerlinNoise(Time.time * 2f + i * 0.5f, i * 0.3f);
                    r.material.SetColor("_EmissionColor", new Color(0.7f, 0.6f, 0.5f) * sparkle * 0.3f);
                }
                
                // Reset if too far or too low/high
                if (t.position.magnitude > 70f || t.position.y < 0 || t.position.y > 12f)
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
            Vector3 wind = GetCurrentWind();
            
            for (int i = 0; i < embers.Length; i++)
            {
                if (embers[i] == null) continue;
                
                Transform t = embers[i].transform;
                
                // Embers rise and drift with wind
                float riseSpeed = 0.5f + Mathf.Sin(Time.time + i) * 0.3f;
                Vector3 movement = new Vector3(
                    Mathf.Sin(Time.time * 2f + i) * 0.5f,
                    riseSpeed,
                    Mathf.Cos(Time.time * 2f + i) * 0.5f
                ) * Time.deltaTime;
                
                t.position += movement + wind * 0.5f * Time.deltaTime;
                
                // Pulse glow
                Renderer r = embers[i].GetComponent<Renderer>();
                if (r != null)
                {
                    float pulse = 1f + Mathf.Sin(Time.time * 5f + i) * 0.3f;
                    r.material.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * pulse * 2.5f);
                }
                
                // Reset when too high or far
                if (t.position.y > 15f || t.position.magnitude > 70f)
                {
                    // Find valid fires (remove destroyed ones)
                    flickerLights.RemoveAll(light => light == null);
                    
                    if (flickerLights.Count > 0)
                    {
                        Light randomFire = flickerLights[Random.Range(0, flickerLights.Count)];
                        if (randomFire != null)
                        {
                            t.position = randomFire.transform.position + 
                                new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f), Random.Range(-0.5f, 0.5f));
                        }
                        else
                        {
                            t.position = GetRandomEmberPosition();
                        }
                    }
                    else
                    {
                        t.position = GetRandomEmberPosition();
                    }
                }
            }
        }
        
        private Vector3 GetRandomEmberPosition()
        {
            return new Vector3(
                Random.Range(-20f, 20f),
                Random.Range(0.5f, 2f),
                Random.Range(-20f, 20f)
            );
        }
        
        // ========== FALLING LEAVES ==========
        
        private void CreateFallingLeaves()
        {
            fallingLeaves = new GameObject[fallingLeafCount];
            
            for (int i = 0; i < fallingLeafCount; i++)
            {
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
                leaf.name = "FallingLeaf";
                leaf.transform.SetParent(transform);
                leaf.transform.position = GetRandomLeafPosition();
                leaf.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);
                leaf.transform.rotation = Random.rotation;
                
                // Vary leaf color
                Material leafMat = new Material(leafMaterial);
                float colorType = Random.value;
                if (colorType < 0.4f)
                {
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.5f, 0.7f),
                        Random.Range(0.35f, 0.5f),
                        Random.Range(0.15f, 0.25f)
                    ));
                }
                else if (colorType < 0.7f)
                {
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.6f, 0.8f),
                        Random.Range(0.25f, 0.4f),
                        Random.Range(0.1f, 0.2f)
                    ));
                }
                else
                {
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.3f, 0.45f),
                        Random.Range(0.28f, 0.4f),
                        Random.Range(0.15f, 0.22f)
                    ));
                }
                
                leaf.GetComponent<Renderer>().material = leafMat;
                Destroy(leaf.GetComponent<Collider>());
                
                fallingLeaves[i] = leaf;
            }
        }
        
        private Vector3 GetRandomLeafPosition()
        {
            return new Vector3(
                Random.Range(-90f, 90f),
                Random.Range(5f, 20f),
                Random.Range(-90f, 90f)
            );
        }
        
        private void UpdateFallingLeaves()
        {
            Vector3 wind = GetCurrentWind();
            
            for (int i = 0; i < fallingLeaves.Length; i++)
            {
                if (fallingLeaves[i] == null) continue;
                
                Transform t = fallingLeaves[i].transform;
                
                // Realistic falling motion with tumbling
                float fallSpeed = 0.8f + Mathf.Sin(Time.time * 2f + i * 0.5f) * 0.4f;
                float swayX = Mathf.Sin(Time.time * 1.5f + i) * 1.5f;
                float swayZ = Mathf.Cos(Time.time * 1.2f + i * 0.7f) * 1.2f;
                
                Vector3 movement = new Vector3(
                    swayX + wind.x,
                    -fallSpeed,
                    swayZ + wind.z
                ) * Time.deltaTime;
                
                t.position += movement;
                
                // Tumbling rotation
                t.Rotate(
                    Mathf.Sin(Time.time + i) * 60f * Time.deltaTime,
                    Mathf.Cos(Time.time + i * 0.5f) * 90f * Time.deltaTime,
                    Mathf.Sin(Time.time * 0.7f + i) * 45f * Time.deltaTime
                );
                
                // Reset when too low
                if (t.position.y < 0.1f)
                {
                    t.position = GetRandomLeafPosition();
                }
                
                // Billboard to camera
                if (Camera.main != null)
                {
                    t.LookAt(Camera.main.transform);
                }
            }
        }
        
        // ========== INSECTS/BIRDS ==========
        
        private void CreateInsects()
        {
            insects = new GameObject[insectCount];
            insectTargets = new Vector3[insectCount];
            
            for (int i = 0; i < insectCount; i++)
            {
                GameObject insect = new GameObject("Insect");
                insect.transform.SetParent(transform);
                insect.transform.position = GetRandomInsectPosition();
                insectTargets[i] = GetRandomInsectPosition();
                
                // Body
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                body.name = "Body";
                body.transform.SetParent(insect.transform);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(0.03f, 0.015f, 0.04f);
                body.GetComponent<Renderer>().material = insectMaterial;
                Destroy(body.GetComponent<Collider>());
                
                // Wings (if it's a larger flying creature)
                if (Random.value > 0.5f)
                {
                    for (int w = -1; w <= 1; w += 2)
                    {
                        GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        wing.name = "Wing";
                        wing.transform.SetParent(insect.transform);
                        wing.transform.localPosition = new Vector3(w * 0.02f, 0.005f, 0);
                        wing.transform.localScale = new Vector3(0.04f, 0.02f, 1f);
                        wing.transform.localRotation = Quaternion.Euler(0, 0, w * 15f);
                        
                        Material wingMat = new Material(URPMaterialHelper.GetLitShader());
                        wingMat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 0.6f));
                        wing.GetComponent<Renderer>().material = wingMat;
                        Destroy(wing.GetComponent<Collider>());
                    }
                }
                
                insects[i] = insect;
            }
        }
        
        private Vector3 GetRandomInsectPosition()
        {
            return new Vector3(
                Random.Range(-80f, 80f),
                Random.Range(2f, 12f),
                Random.Range(-80f, 80f)
            );
        }
        
        private void UpdateInsects()
        {
            for (int i = 0; i < insects.Length; i++)
            {
                if (insects[i] == null) continue;
                
                Transform t = insects[i].transform;
                
                // Move toward target with erratic motion
                Vector3 toTarget = insectTargets[i] - t.position;
                float distToTarget = toTarget.magnitude;
                
                if (distToTarget < 2f)
                {
                    // Pick new target
                    insectTargets[i] = GetRandomInsectPosition();
                }
                
                // Erratic flying motion
                Vector3 erratic = new Vector3(
                    Mathf.Sin(Time.time * 10f + i * 3f) * 0.5f,
                    Mathf.Sin(Time.time * 8f + i * 2f) * 0.3f,
                    Mathf.Cos(Time.time * 9f + i * 4f) * 0.5f
                );
                
                float speed = Random.Range(3f, 6f);
                t.position += (toTarget.normalized * speed + erratic) * Time.deltaTime;
                
                // Face direction of movement
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * 5f);
                }
            }
        }
        
        // ========== POLLEN/SPORES ==========
        
        private void CreatePollen()
        {
            pollen = new GameObject[pollenCount];
            
            for (int i = 0; i < pollenCount; i++)
            {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "Pollen";
                p.transform.SetParent(transform);
                p.transform.position = GetRandomPollenPosition();
                p.transform.localScale = Vector3.one * Random.Range(0.01f, 0.025f);
                p.GetComponent<Renderer>().material = pollenMaterial;
                Destroy(p.GetComponent<Collider>());
                
                pollen[i] = p;
            }
        }
        
        private Vector3 GetRandomPollenPosition()
        {
            return new Vector3(
                Random.Range(-70f, 70f),
                Random.Range(0.5f, 6f),
                Random.Range(-70f, 70f)
            );
        }
        
        private void UpdatePollen()
        {
            Vector3 wind = GetCurrentWind();
            
            for (int i = 0; i < pollen.Length; i++)
            {
                if (pollen[i] == null) continue;
                
                Transform t = pollen[i].transform;
                
                // Very gentle floating motion
                float floatX = Mathf.Sin(Time.time * 0.5f + i * 0.3f) * 0.2f;
                float floatY = Mathf.Sin(Time.time * 0.3f + i * 0.5f) * 0.1f;
                float floatZ = Mathf.Cos(Time.time * 0.4f + i * 0.4f) * 0.2f;
                
                Vector3 movement = new Vector3(floatX, floatY, floatZ) + wind * 0.3f;
                t.position += movement * Time.deltaTime;
                
                // Catch light effect
                Renderer r = pollen[i].GetComponent<Renderer>();
                if (r != null && Camera.main != null)
                {
                    Vector3 toCamera = (Camera.main.transform.position - t.position).normalized;
                    Vector3 sunDir = new Vector3(-0.5f, 0.3f, -0.5f).normalized;
                    float sparkle = Mathf.Max(0, Vector3.Dot(toCamera, sunDir));
                    r.material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.6f) * sparkle * 0.5f);
                }
                
                // Reset if out of bounds
                if (t.position.magnitude > 90f || t.position.y < 0.3f || t.position.y > 10f)
                {
                    t.position = GetRandomPollenPosition();
                }
            }
        }
        
        // ========== MOVING FOG ==========
        
        private void CreateMovingFog()
        {
            // Disabled - URP transparent materials don't work the same way
            // Unity's built-in fog handles this instead
            movingFogPatches = new GameObject[0];
        }
        
        private void UpdateMovingFog()
        {
            // Disabled
        }
    }
}

