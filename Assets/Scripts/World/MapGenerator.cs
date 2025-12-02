using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Procedural arena generator with randomized layouts and obstacles
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("Arena Settings")]
        [SerializeField] private int arenaSize = 50;
        [SerializeField] private float tileSize = 5f;
        [SerializeField] private int minObstacles = 15;
        [SerializeField] private int maxObstacles = 30;

        [Header("Platform Settings")]
        [SerializeField] private int platformCount = 8;
        [SerializeField] private float platformHeight = 3f;

        [Header("Generation")]
        [SerializeField] private int seed = -1;

        private List<GameObject> generatedObjects = new List<GameObject>();
        private Rendering.NeonMaterialFactory materialFactory;

        public void GenerateArena()
        {
            // Clear previous arena
            ClearArena();

            // Initialize random seed
            if (seed == -1)
            {
                seed = Random.Range(0, 1000000);
            }
            Random.InitState(seed);

            Debug.Log($"Generating arena with seed: {seed}");

            // Initialize materials
            materialFactory = gameObject.AddComponent<Rendering.NeonMaterialFactory>();
            materialFactory.SelectRandomTheme();

            // Generate arena components
            GenerateFloor();
            GenerateWalls();
            GenerateObstacles();
            GeneratePlatforms();
            GenerateUrbanDetails(); // NEW: Detailed environment
            GenerateLighting();

            Debug.Log($"Arena generation complete! Objects created: {generatedObjects.Count}");
        }

        private void ClearArena()
        {
            foreach (GameObject obj in generatedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            generatedObjects.Clear();
        }

        private void GenerateFloor()
        {
            // Create main floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Arena_Floor";
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(arenaSize, 1f, arenaSize);
            
            // Apply material
            Rendering.NeonMaterialFactory.ApplyFloorMaterial(floor.GetComponent<Renderer>());
            
            generatedObjects.Add(floor);

            // Create grid pattern with smaller tiles
            int gridCount = Mathf.RoundToInt(arenaSize / tileSize);
            for (int x = 0; x < gridCount; x++)
            {
                for (int z = 0; z < gridCount; z++)
                {
                    // Randomly skip some tiles for visual variety
                    if (Random.value > 0.15f) continue;

                    float posX = (x - gridCount / 2f) * tileSize;
                    float posZ = (z - gridCount / 2f) * tileSize;

                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"FloorTile_{x}_{z}";
                    tile.transform.position = new Vector3(posX, 0.02f, posZ);
                    tile.transform.localScale = new Vector3(tileSize * 0.9f, 0.05f, tileSize * 0.9f);

                    Rendering.NeonMaterialFactory.ApplyAccentMaterial(tile.GetComponent<Renderer>(), Random.value * 0.3f);
                    generatedObjects.Add(tile);
                }
            }
        }

        private void GenerateWalls()
        {
            float wallHeight = 8f;
            float wallThickness = 1f;
            float halfSize = arenaSize / 2f;

            // Create four walls
            CreateWall("Wall_North", new Vector3(0, wallHeight / 2f, halfSize), new Vector3(arenaSize, wallHeight, wallThickness));
            CreateWall("Wall_South", new Vector3(0, wallHeight / 2f, -halfSize), new Vector3(arenaSize, wallHeight, wallThickness));
            CreateWall("Wall_East", new Vector3(halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, arenaSize));
            CreateWall("Wall_West", new Vector3(-halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, arenaSize));
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;

            Rendering.NeonMaterialFactory.ApplyWallMaterial(wall.GetComponent<Renderer>());
            generatedObjects.Add(wall);
        }

        private void GenerateObstacles()
        {
            int obstacleCount = Random.Range(minObstacles, maxObstacles);
            float spawnRadius = arenaSize / 2f - 5f;

            for (int i = 0; i < obstacleCount; i++)
            {
                // Random position
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 position = new Vector3(randomCircle.x, 0, randomCircle.y);

                // Avoid spawning near center (player spawn)
                if (position.magnitude < 5f) continue;

                // Random obstacle type
                float roll = Random.value;
                if (roll < 0.4f)
                {
                    CreateCoverBlock(position, i);
                }
                else if (roll < 0.7f)
                {
                    CreatePillar(position, i);
                }
                else
                {
                    CreateObstacleCluster(position, i);
                }
            }
        }

        private void CreateCoverBlock(Vector3 position, int index)
        {
            GameObject coverParent = new GameObject($"Cover_{index}");
            coverParent.transform.position = position;
            coverParent.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float height = Random.Range(1.5f, 3f);
            float width = Random.Range(2f, 4f);
            
            // Main block
            GameObject mainBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBlock.name = "MainBlock";
            mainBlock.transform.SetParent(coverParent.transform);
            mainBlock.transform.localPosition = Vector3.up * (height / 2f);
            mainBlock.transform.localScale = new Vector3(width, height, width * 0.6f);
            Rendering.NeonMaterialFactory.ApplyObstacleMaterial(mainBlock.GetComponent<Renderer>());
            
            // Glowing edge accents
            for (int i = 0; i < 4; i++)
            {
                GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edge.name = $"Edge_{i}";
                edge.transform.SetParent(coverParent.transform);
                
                float angle = i * 90f;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * (width / 2f);
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * (width * 0.3f);
                
                edge.transform.localPosition = new Vector3(x, height / 2f, z);
                edge.transform.localScale = new Vector3(0.1f, height, 0.1f);
                Rendering.NeonMaterialFactory.ApplyAccentMaterial(edge.GetComponent<Renderer>(), 2f);
            }

            generatedObjects.Add(coverParent);
        }

        private void CreatePillar(Vector3 position, int index)
        {
            GameObject pillarParent = new GameObject($"Pillar_{index}");
            pillarParent.transform.position = position;
            
            float height = Random.Range(3f, 6f);
            float radius = Random.Range(0.5f, 1.2f);
            
            // Main pillar
            GameObject mainPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainPillar.name = "MainPillar";
            mainPillar.transform.SetParent(pillarParent.transform);
            mainPillar.transform.localPosition = Vector3.up * (height / 2f);
            mainPillar.transform.localScale = new Vector3(radius, height / 2f, radius);
            Rendering.NeonMaterialFactory.ApplyObstacleMaterial(mainPillar.GetComponent<Renderer>());
            
            // Glowing rings at intervals
            int ringCount = Mathf.RoundToInt(height / 1.5f);
            for (int i = 0; i < ringCount; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"Ring_{i}";
                ring.transform.SetParent(pillarParent.transform);
                float ringHeight = (height / ringCount) * i + 0.5f;
                ring.transform.localPosition = Vector3.up * ringHeight;
                ring.transform.localScale = new Vector3(radius * 1.1f, 0.05f, radius * 1.1f);
                Rendering.NeonMaterialFactory.ApplyAccentMaterial(ring.GetComponent<Renderer>(), 3f);
            }
            
            // Top cap
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(pillarParent.transform);
            cap.transform.localPosition = Vector3.up * height;
            cap.transform.localScale = Vector3.one * radius * 1.2f;
            Rendering.NeonMaterialFactory.ApplyAccentMaterial(cap.GetComponent<Renderer>(), 4f);

            generatedObjects.Add(pillarParent);
        }

        private void CreateObstacleCluster(Vector3 center, int index)
        {
            int clusterSize = Random.Range(3, 6);
            
            for (int i = 0; i < clusterSize; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-2f, 2f),
                    0,
                    Random.Range(-2f, 2f)
                );

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Cluster_{index}_{i}";
                
                float size = Random.Range(0.5f, 1.5f);
                cube.transform.position = center + offset + Vector3.up * (size / 2f);
                cube.transform.localScale = Vector3.one * size;
                cube.transform.rotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    Random.Range(0f, 360f),
                    Random.Range(-15f, 15f)
                );

                Rendering.NeonMaterialFactory.ApplyObstacleMaterial(cube.GetComponent<Renderer>());
                generatedObjects.Add(cube);
            }
        }

        private void GeneratePlatforms()
        {
            float spawnRadius = arenaSize / 2f - 8f;

            for (int i = 0; i < platformCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 position = new Vector3(randomCircle.x, platformHeight, randomCircle.y);

                // Avoid center
                if (position.magnitude < 8f) continue;

                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = $"Platform_{i}";
                
                float size = Random.Range(3f, 6f);
                platform.transform.position = position;
                platform.transform.localScale = new Vector3(size, 0.3f, size);
                platform.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Rendering.NeonMaterialFactory.ApplyPlatformMaterial(platform.GetComponent<Renderer>());
                generatedObjects.Add(platform);

                // Add support pillar
                GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                support.name = $"PlatformSupport_{i}";
                support.transform.position = new Vector3(position.x, platformHeight / 2f, position.z);
                support.transform.localScale = new Vector3(0.3f, platformHeight / 2f, 0.3f);

                Rendering.NeonMaterialFactory.ApplyAccentMaterial(support.GetComponent<Renderer>(), 0.5f);
                generatedObjects.Add(support);
            }
        }

        private void GenerateLighting()
        {
            // Main directional light
            GameObject mainLight = new GameObject("MainLight");
            Light dirLight = mainLight.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.8f, 0.9f, 1f);
            dirLight.intensity = 0.8f; // Increased for better visibility
            dirLight.shadows = LightShadows.Soft;
            mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            generatedObjects.Add(mainLight);

            // Ambient point lights for atmosphere
            int lightCount = Random.Range(12, 20); // More lights
            float lightRadius = arenaSize / 2f - 3f;

            for (int i = 0; i < lightCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * lightRadius;
                Vector3 position = new Vector3(randomCircle.x, Random.Range(3f, 8f), randomCircle.y);

                GameObject lightObj = new GameObject($"AmbientLight_{i}");
                Light pointLight = lightObj.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.color = Rendering.NeonMaterialFactory.GetCurrentThemeColor();
                pointLight.intensity = Random.Range(5f, 10f); // Brighter lights
                pointLight.range = Random.Range(15f, 25f); // Wider range
                pointLight.shadows = LightShadows.None;
                
                lightObj.transform.position = position;
                generatedObjects.Add(lightObj);
            }

            // Configure ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.25f); // Brighter ambient
            RenderSettings.ambientIntensity = 1.5f;
            
            // Fog for atmosphere
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.15f);
            RenderSettings.fogDensity = 0.008f; // Less dense fog
        }

        private void GenerateUrbanDetails()
        {
            // Add realistic urban environment details
            GenerateSidewalks();
            GenerateAsphaltDetails();
            GenerateGarbageCans();
            GenerateConcreteCracks();
            GenerateBrokenGlass();
            GenerateConcreteDebris();
            GenerateCableTowers();
            GenerateStreetLamps();
            GenerateMetalPipes();
            GenerateConcreteBarriers();
            GenerateDirtPatches();
            GenerateGraffitiMarks();
        }

        private void GenerateSidewalks()
        {
            float halfSize = arenaSize / 2f;
            float sidewalkWidth = 2f;
            float sidewalkHeight = 0.15f;

            // North sidewalk
            CreateSidewalk(new Vector3(0, sidewalkHeight/2f, halfSize - 1f), 
                          new Vector3(arenaSize, sidewalkHeight, sidewalkWidth));
            
            // South sidewalk
            CreateSidewalk(new Vector3(0, sidewalkHeight/2f, -halfSize + 1f), 
                          new Vector3(arenaSize, sidewalkHeight, sidewalkWidth));
            
            // East sidewalk
            CreateSidewalk(new Vector3(halfSize - 1f, sidewalkHeight/2f, 0), 
                          new Vector3(sidewalkWidth, sidewalkHeight, arenaSize));
            
            // West sidewalk
            CreateSidewalk(new Vector3(-halfSize + 1f, sidewalkHeight/2f, 0), 
                          new Vector3(sidewalkWidth, sidewalkHeight, arenaSize));
        }

        private void CreateSidewalk(Vector3 position, Vector3 scale)
        {
            GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidewalk.name = "Sidewalk";
            sidewalk.transform.position = position;
            sidewalk.transform.localScale = scale;

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.55f)); // Light concrete color
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Glossiness", 0.3f);
            sidewalk.GetComponent<Renderer>().material = mat;
            
            generatedObjects.Add(sidewalk);

            // Add cracks/lines on sidewalk
            for (int i = 0; i < 5; i++)
            {
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crack.name = "SidewalkCrack";
                crack.transform.SetParent(sidewalk.transform);
                
                float randomOffset = Random.Range(-scale.x/3f, scale.x/3f);
                crack.transform.localPosition = new Vector3(randomOffset, 0.1f, Random.Range(-scale.z/3f, scale.z/3f));
                crack.transform.localScale = new Vector3(0.05f, 0.02f, Random.Range(0.3f, 1f));
                crack.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 180f), 0);

                Material crackMat = new Material(Shader.Find("Standard"));
                crackMat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
                crack.GetComponent<Renderer>().material = crackMat;
            }
        }

        private void GenerateAsphaltDetails()
        {
            // Add dark patches to simulate worn asphalt
            int patchCount = Random.Range(15, 25);
            
            for (int i = 0; i < patchCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 5f);
                Vector3 position = new Vector3(randomPos.x, 0.02f, randomPos.y);

                GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patch.name = "AsphaltPatch";
                patch.transform.position = position;
                patch.transform.localScale = new Vector3(
                    Random.Range(1f, 3f), 
                    0.01f, 
                    Random.Range(1f, 3f)
                );
                patch.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f)); // Dark asphalt
                mat.SetFloat("_Metallic", 0.0f);
                mat.SetFloat("_Glossiness", 0.2f);
                patch.GetComponent<Renderer>().material = mat;
                
                generatedObjects.Add(patch);
            }
        }

        private void GenerateGarbageCans()
        {
            int canCount = Random.Range(8, 15);
            
            for (int i = 0; i < canCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * (arenaSize / 2f - 8f);
                Vector3 position = new Vector3(randomCircle.x, 0, randomCircle.y);

                // Main can body
                GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                can.name = $"GarbageCan_{i}";
                can.transform.position = position + Vector3.up * 0.75f;
                can.transform.localScale = new Vector3(0.5f, 0.75f, 0.5f);

                Material canMat = new Material(Shader.Find("Standard"));
                Color canColor = Random.value > 0.5f 
                    ? new Color(0.3f, 0.3f, 0.35f) // Grey
                    : new Color(0.2f, 0.4f, 0.25f); // Green
                canMat.SetColor("_Color", canColor);
                canMat.SetFloat("_Metallic", 0.7f);
                canMat.SetFloat("_Glossiness", 0.5f);
                can.GetComponent<Renderer>().material = canMat;
                
                generatedObjects.Add(can);

                // Lid
                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lid.name = "Lid";
                lid.transform.SetParent(can.transform);
                lid.transform.localPosition = new Vector3(0, 1.1f, 0);
                lid.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
                lid.GetComponent<Renderer>().material = canMat;

                // Rust/dirt stripes
                for (int j = 0; j < 3; j++)
                {
                    GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stripe.name = "Stripe";
                    stripe.transform.SetParent(can.transform);
                    stripe.transform.localPosition = new Vector3(0, -0.3f + j * 0.3f, 0.51f);
                    stripe.transform.localScale = new Vector3(0.8f, 0.05f, 0.02f);

                    Material stripeMat = new Material(Shader.Find("Standard"));
                    stripeMat.SetColor("_Color", new Color(0.4f, 0.25f, 0.15f)); // Rust color
                    stripe.GetComponent<Renderer>().material = stripeMat;
                }
            }
        }

        private void GenerateConcreteCracks()
        {
            int crackCount = Random.Range(20, 35);
            
            for (int i = 0; i < crackCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 3f);
                Vector3 position = new Vector3(randomPos.x, 0.01f, randomPos.y);

                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crack.name = "GroundCrack";
                crack.transform.position = position;
                
                float length = Random.Range(0.5f, 2.5f);
                crack.transform.localScale = new Vector3(Random.Range(0.03f, 0.08f), 0.005f, length);
                crack.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Rendering.NeonMaterialFactory.GetCurrentThemeColor() * 0.3f);
                crack.GetComponent<Renderer>().material = mat;
                
                generatedObjects.Add(crack);
            }
        }

        private void GenerateBrokenGlass()
        {
            int glassCount = Random.Range(15, 25);
            
            for (int i = 0; i < glassCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 5f);
                Vector3 position = new Vector3(randomPos.x, 0.02f, randomPos.y);

                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glass.name = "BrokenGlass";
                glass.transform.position = position;
                glass.transform.localScale = new Vector3(
                    Random.Range(0.1f, 0.3f), 
                    0.01f, 
                    Random.Range(0.1f, 0.3f)
                );
                glass.transform.rotation = Quaternion.Euler(
                    Random.Range(-5f, 5f), 
                    Random.Range(0f, 360f), 
                    Random.Range(-5f, 5f)
                );

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.9f, 0.9f, 0.95f, 0.3f));
                mat.SetFloat("_Metallic", 0.2f);
                mat.SetFloat("_Glossiness", 0.95f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.6f) * 0.5f);
                glass.GetComponent<Renderer>().material = mat;
                
                generatedObjects.Add(glass);
            }
        }

        private void GenerateConcreteDebris()
        {
            int debrisCount = Random.Range(25, 40);
            
            for (int i = 0; i < debrisCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 5f);
                Vector3 position = new Vector3(randomPos.x, Random.Range(0f, 0.2f), randomPos.y);

                GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debris.name = "ConcreteDebris";
                debris.transform.position = position;
                
                float size = Random.Range(0.15f, 0.5f);
                debris.transform.localScale = new Vector3(size, size * 0.5f, size * Random.Range(0.7f, 1.3f));
                debris.transform.rotation = Quaternion.Euler(
                    Random.Range(-20f, 20f), 
                    Random.Range(0f, 360f), 
                    Random.Range(-20f, 20f)
                );

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.4f, 0.4f, 0.42f));
                mat.SetFloat("_Metallic", 0.0f);
                mat.SetFloat("_Glossiness", 0.1f);
                debris.GetComponent<Renderer>().material = mat;
                
                // Disable collider so it doesn't block movement
                Destroy(debris.GetComponent<Collider>());
                
                generatedObjects.Add(debris);
            }
        }

        private void GenerateCableTowers()
        {
            int towerCount = Random.Range(4, 8);
            
            for (int i = 0; i < towerCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 8f);
                Vector3 position = new Vector3(randomPos.x, 0, randomPos.y);

                // Tower pole
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = $"CableTower_{i}";
                float height = Random.Range(4f, 7f);
                pole.transform.position = position + Vector3.up * (height / 2f);
                pole.transform.localScale = new Vector3(0.15f, height / 2f, 0.15f);

                Material poleMat = new Material(Shader.Find("Standard"));
                poleMat.SetColor("_Color", new Color(0.3f, 0.3f, 0.3f));
                poleMat.SetFloat("_Metallic", 0.8f);
                poleMat.SetFloat("_Glossiness", 0.4f);
                pole.GetComponent<Renderer>().material = poleMat;
                
                generatedObjects.Add(pole);

                // Cables from top
                int cableCount = Random.Range(2, 4);
                for (int c = 0; c < cableCount; c++)
                {
                    GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cable.name = $"Cable_{c}";
                    cable.transform.SetParent(pole.transform);
                    
                    float angle = (360f / cableCount) * c;
                    float cableLength = Random.Range(3f, 6f);
                    
                    cable.transform.localPosition = new Vector3(0, 0.9f, 0);
                    cable.transform.localScale = new Vector3(0.02f, cableLength / 2f, 0.02f);
                    cable.transform.localRotation = Quaternion.Euler(60, angle, 0);

                    Material cableMat = new Material(Shader.Find("Standard"));
                    cableMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
                    cableMat.SetFloat("_Metallic", 0.5f);
                    cableMat.SetFloat("_Glossiness", 0.3f);
                    cable.GetComponent<Renderer>().material = cableMat;
                }

                // Light on top
                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                light.name = "TowerLight";
                light.transform.SetParent(pole.transform);
                light.transform.localPosition = new Vector3(0, 1.1f, 0);
                light.transform.localScale = Vector3.one * 0.3f;

                Rendering.NeonMaterialFactory.ApplyAccentMaterial(light.GetComponent<Renderer>(), 3f);
            }
        }

        private void GenerateStreetLamps()
        {
            int lampCount = Random.Range(6, 12);
            
            for (int i = 0; i < lampCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 6f);
                Vector3 position = new Vector3(randomPos.x, 0, randomPos.y);

                // Lamp post
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"StreetLamp_{i}";
                float height = Random.Range(3f, 5f);
                post.transform.position = position + Vector3.up * (height / 2f);
                post.transform.localScale = new Vector3(0.12f, height / 2f, 0.12f);

                Material postMat = new Material(Shader.Find("Standard"));
                postMat.SetColor("_Color", new Color(0.25f, 0.25f, 0.25f));
                postMat.SetFloat("_Metallic", 0.7f);
                postMat.SetFloat("_Glossiness", 0.5f);
                post.GetComponent<Renderer>().material = postMat;
                
                generatedObjects.Add(post);

                // Lamp head
                GameObject lampHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lampHead.name = "LampHead";
                lampHead.transform.SetParent(post.transform);
                lampHead.transform.localPosition = new Vector3(0, 1.2f, 0);
                lampHead.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);

                Rendering.NeonMaterialFactory.ApplyAccentMaterial(lampHead.GetComponent<Renderer>(), 4f);
            }
        }

        private void GenerateMetalPipes()
        {
            int pipeCount = Random.Range(10, 18);
            
            for (int i = 0; i < pipeCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 5f);
                Vector3 position = new Vector3(randomPos.x, Random.Range(0.1f, 0.3f), randomPos.y);

                GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipe.name = "MetalPipe";
                
                float length = Random.Range(1f, 3f);
                pipe.transform.position = position;
                pipe.transform.localScale = new Vector3(0.08f, length / 2f, 0.08f);
                pipe.transform.rotation = Quaternion.Euler(
                    Random.Range(-10f, 10f), 
                    Random.Range(0f, 360f), 
                    Random.Range(80f, 100f) // Mostly horizontal
                );

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
                mat.SetFloat("_Metallic", 0.9f);
                mat.SetFloat("_Glossiness", 0.6f);
                pipe.GetComponent<Renderer>().material = mat;
                
                // Disable collider
                Destroy(pipe.GetComponent<Collider>());
                
                generatedObjects.Add(pipe);
            }
        }

        private void GenerateConcreteBarriers()
        {
            int barrierCount = Random.Range(5, 10);
            
            for (int i = 0; i < barrierCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 8f);
                Vector3 position = new Vector3(randomPos.x, 0, randomPos.y);

                GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                barrier.name = "ConcreteBarrier";
                barrier.transform.position = position + Vector3.up * 0.4f;
                barrier.transform.localScale = new Vector3(2f, 0.8f, 0.5f);
                barrier.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.52f));
                mat.SetFloat("_Metallic", 0.0f);
                mat.SetFloat("_Glossiness", 0.2f);
                barrier.GetComponent<Renderer>().material = mat;
                
                generatedObjects.Add(barrier);

                // Warning stripes
                for (int s = 0; s < 3; s++)
                {
                    GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stripe.name = "WarningStripe";
                    stripe.transform.SetParent(barrier.transform);
                    stripe.transform.localPosition = new Vector3(-0.6f + s * 0.6f, 0.45f, 0);
                    stripe.transform.localScale = new Vector3(0.25f, 0.05f, 1.1f);

                    Material stripeMat = new Material(Shader.Find("Standard"));
                    stripeMat.SetColor("_Color", Color.yellow);
                    stripeMat.EnableKeyword("_EMISSION");
                    stripeMat.SetColor("_EmissionColor", Color.yellow * 0.5f);
                    stripe.GetComponent<Renderer>().material = stripeMat;
                }
            }
        }

        private void GenerateDirtPatches()
        {
            int patchCount = Random.Range(12, 20);
            
            for (int i = 0; i < patchCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 4f);
                Vector3 position = new Vector3(randomPos.x, 0.01f, randomPos.y);

                GameObject dirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dirt.name = "DirtPatch";
                dirt.transform.position = position;
                dirt.transform.localScale = new Vector3(
                    Random.Range(0.8f, 2f), 
                    0.005f, 
                    Random.Range(0.8f, 2f)
                );
                dirt.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", new Color(0.3f, 0.25f, 0.2f)); // Brown dirt
                mat.SetFloat("_Metallic", 0.0f);
                mat.SetFloat("_Glossiness", 0.1f);
                dirt.GetComponent<Renderer>().material = mat;
                
                // Disable collider
                Destroy(dirt.GetComponent<Collider>());
                
                generatedObjects.Add(dirt);
            }
        }

        private void GenerateGraffitiMarks()
        {
            int graffitiCount = Random.Range(8, 15);
            
            for (int i = 0; i < graffitiCount; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * (arenaSize / 2f - 5f);
                Vector3 position = new Vector3(randomPos.x, 0.015f, randomPos.y);

                GameObject graffiti = GameObject.CreatePrimitive(PrimitiveType.Cube);
                graffiti.name = "GraffitiMark";
                graffiti.transform.position = position;
                graffiti.transform.localScale = new Vector3(
                    Random.Range(0.5f, 1.5f), 
                    0.01f, 
                    Random.Range(0.3f, 0.8f)
                );
                graffiti.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Material mat = new Material(Shader.Find("Standard"));
                Color graffitiColor = Rendering.NeonMaterialFactory.GetCurrentThemeColor() * Random.Range(0.5f, 1.5f);
                mat.SetColor("_Color", graffitiColor);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", graffitiColor * 2f);
                graffiti.GetComponent<Renderer>().material = mat;
                
                // Disable collider
                Destroy(graffiti.GetComponent<Collider>());
                
                generatedObjects.Add(graffiti);
            }
        }

        private void OnDestroy()
        {
            ClearArena();
        }
    }
}

