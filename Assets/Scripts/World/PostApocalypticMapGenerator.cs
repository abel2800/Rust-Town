using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Generates a post-apocalyptic small town environment - "Sundown Desolation"
    /// Features: abandoned buildings, gas station, burning barrels, fog, dust particles
    /// </summary>
    public class PostApocalypticMapGenerator : MonoBehaviour
    {
        [Header("Town Settings")]
        [SerializeField] private float townSize = 80f;
        [SerializeField] private float roadWidth = 12f;
        
        [Header("Atmosphere")]
        [SerializeField] private Color sunsetColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private Color skyColor = new Color(0.6f, 0.25f, 0.15f);
        [SerializeField] private float fogDensity = 0.02f;
        
        private List<GameObject> generatedObjects = new List<GameObject>();
        
        // Materials
        private Material asphaltMaterial;
        private Material concreteMaterial;
        private Material rustMaterial;
        private Material woodMaterial;
        private Material glassMaterial;
        private Material fireMaterial;
        
        public void GenerateMap()
        {
            ClearMap();
            CreateMaterials();
            DetailedBuildingGenerator.InitializeMaterials();
            
            // Generate environment with DETAILED components
            GenerateGround();
            
            // Use detailed road generator
            DetailedRoadGenerator.CreateDetailedRoad(Vector3.zero, roadWidth, townSize * 1.5f, generatedObjects);
            
            // Sidewalks with detail
            GenerateDetailedSidewalks();
            
            GenerateGasStation();
            GenerateAbandonedBuildings();
            
            // Use detailed house generator
            GenerateDetailedHouses();
            
            GenerateAbandonedVehicles();
            GenerateAlleyways();
            GenerateBurningBarrels();
            GenerateDebrisAndClutter();
            GenerateUtilityPoles();
            GenerateFog();
            SetupSundownLighting();
            SetupAtmosphere();
            
            // Add atmosphere effects (flickering lights, dust, embers)
            if (GetComponent<AtmosphereEffects>() == null)
            {
                gameObject.AddComponent<AtmosphereEffects>();
            }
            
            Debug.Log($"🌅 Post-Apocalyptic 'Sundown Desolation' map generated! Objects: {generatedObjects.Count}");
        }
        
        private void GenerateDetailedSidewalks()
        {
            float sidewalkWidth = 3f;
            
            for (int side = -1; side <= 1; side += 2)
            {
                float xPos = side * (roadWidth/2 + sidewalkWidth/2 + 0.3f);
                
                // Main sidewalk
                GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sidewalk.name = "Sidewalk";
                sidewalk.transform.position = new Vector3(xPos, 0.1f, 0);
                sidewalk.transform.localScale = new Vector3(sidewalkWidth, 0.2f, townSize * 1.5f);
                sidewalk.GetComponent<Renderer>().material = concreteMaterial;
                generatedObjects.Add(sidewalk);
                
                // Sidewalk cracks
                for (int i = 0; i < 20; i++)
                {
                    GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crack.name = "SidewalkCrack";
                    crack.transform.position = new Vector3(
                        xPos + Random.Range(-sidewalkWidth/3, sidewalkWidth/3),
                        0.105f,
                        Random.Range(-townSize * 0.7f, townSize * 0.7f)
                    );
                    crack.transform.localScale = new Vector3(
                        Random.Range(0.03f, 0.08f),
                        0.01f,
                        Random.Range(0.5f, 2f)
                    );
                    crack.transform.rotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);
                    
                    Material crackMat = new Material(Shader.Find("Standard"));
                    crackMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
                    crack.GetComponent<Renderer>().material = crackMat;
                    Destroy(crack.GetComponent<Collider>());
                    generatedObjects.Add(crack);
                }
                
                // Sidewalk slabs (joints)
                for (int i = -15; i <= 15; i++)
                {
                    GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    joint.name = "SlabJoint";
                    joint.transform.position = new Vector3(xPos, 0.105f, i * 4f);
                    joint.transform.localScale = new Vector3(sidewalkWidth + 0.1f, 0.008f, 0.05f);
                    
                    Material jointMat = new Material(Shader.Find("Standard"));
                    jointMat.SetColor("_Color", new Color(0.25f, 0.24f, 0.22f));
                    joint.GetComponent<Renderer>().material = jointMat;
                    Destroy(joint.GetComponent<Collider>());
                    generatedObjects.Add(joint);
                }
            }
        }
        
        private void GenerateDetailedHouses()
        {
            // Detailed houses using the new generator
            Vector3[] housePositions = new Vector3[]
            {
                new Vector3(28f, 0, 35f),
                new Vector3(38f, 0, 15f),
                new Vector3(32f, 0, -25f),
                new Vector3(-30f, 0, 32f),
                new Vector3(-35f, 0, -30f),
                new Vector3(42f, 0, -10f),
                new Vector3(-40f, 0, 5f),
            };
            
            foreach (Vector3 pos in housePositions)
            {
                DetailedBuildingGenerator.CreateDetailedHouse(pos, generatedObjects);
            }
        }
        
        private void ClearMap()
        {
            foreach (GameObject obj in generatedObjects)
            {
                if (obj != null) Destroy(obj);
            }
            generatedObjects.Clear();
        }
        
        private void CreateMaterials()
        {
            // Cracked asphalt
            asphaltMaterial = new Material(Shader.Find("Standard"));
            asphaltMaterial.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f));
            asphaltMaterial.SetFloat("_Metallic", 0f);
            asphaltMaterial.SetFloat("_Glossiness", 0.1f);
            
            // Dirty concrete
            concreteMaterial = new Material(Shader.Find("Standard"));
            concreteMaterial.SetColor("_Color", new Color(0.4f, 0.38f, 0.35f));
            concreteMaterial.SetFloat("_Metallic", 0f);
            concreteMaterial.SetFloat("_Glossiness", 0.15f);
            
            // Rusty metal
            rustMaterial = new Material(Shader.Find("Standard"));
            rustMaterial.SetColor("_Color", new Color(0.45f, 0.25f, 0.15f));
            rustMaterial.SetFloat("_Metallic", 0.6f);
            rustMaterial.SetFloat("_Glossiness", 0.3f);
            
            // Weathered wood
            woodMaterial = new Material(Shader.Find("Standard"));
            woodMaterial.SetColor("_Color", new Color(0.35f, 0.25f, 0.18f));
            woodMaterial.SetFloat("_Metallic", 0f);
            woodMaterial.SetFloat("_Glossiness", 0.1f);
            
            // Dirty glass
            glassMaterial = new Material(Shader.Find("Standard"));
            glassMaterial.SetColor("_Color", new Color(0.3f, 0.35f, 0.4f, 0.5f));
            glassMaterial.SetFloat("_Metallic", 0.1f);
            glassMaterial.SetFloat("_Glossiness", 0.8f);
            
            // Fire glow
            fireMaterial = new Material(Shader.Find("Standard"));
            fireMaterial.SetColor("_Color", new Color(1f, 0.5f, 0.1f));
            fireMaterial.EnableKeyword("_EMISSION");
            fireMaterial.SetColor("_EmissionColor", new Color(2f, 1f, 0.3f) * 3f);
        }
        
        private void GenerateGround()
        {
            // Main ground - dusty dirt
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, -0.5f, 0);
            ground.transform.localScale = new Vector3(townSize * 2, 1f, townSize * 2);
            
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.SetColor("_Color", new Color(0.35f, 0.3f, 0.25f));
            groundMat.SetFloat("_Glossiness", 0.05f);
            ground.GetComponent<Renderer>().material = groundMat;
            
            generatedObjects.Add(ground);
            
            // Add dirt patches
            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-townSize, townSize),
                    0.01f,
                    Random.Range(-townSize, townSize)
                );
                
                GameObject dirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dirt.name = "DirtPatch";
                dirt.transform.position = pos;
                dirt.transform.localScale = new Vector3(
                    Random.Range(2f, 6f),
                    0.02f,
                    Random.Range(2f, 6f)
                );
                dirt.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                Material dirtMat = new Material(Shader.Find("Standard"));
                dirtMat.SetColor("_Color", new Color(
                    0.3f + Random.Range(-0.05f, 0.05f),
                    0.25f + Random.Range(-0.05f, 0.05f),
                    0.2f + Random.Range(-0.05f, 0.05f)
                ));
                dirtMat.SetFloat("_Glossiness", 0.02f);
                dirt.GetComponent<Renderer>().material = dirtMat;
                Destroy(dirt.GetComponent<Collider>());
                
                generatedObjects.Add(dirt);
            }
        }
        
        private void GenerateMainRoad()
        {
            // Main road running through town
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "MainRoad";
            road.transform.position = new Vector3(0, 0.02f, 0);
            road.transform.localScale = new Vector3(roadWidth, 0.05f, townSize * 1.5f);
            road.GetComponent<Renderer>().material = asphaltMaterial;
            generatedObjects.Add(road);
            
            // Road markings (faded yellow lines)
            for (int i = -10; i <= 10; i++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "RoadLine";
                line.transform.position = new Vector3(0, 0.03f, i * 5f);
                line.transform.localScale = new Vector3(0.15f, 0.01f, 2f);
                
                Material lineMat = new Material(Shader.Find("Standard"));
                lineMat.SetColor("_Color", new Color(0.6f, 0.55f, 0.2f, 0.5f));
                line.GetComponent<Renderer>().material = lineMat;
                Destroy(line.GetComponent<Collider>());
                
                generatedObjects.Add(line);
            }
            
            // Potholes and cracks
            for (int i = 0; i < 20; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-roadWidth/2.5f, roadWidth/2.5f),
                    0.01f,
                    Random.Range(-townSize * 0.7f, townSize * 0.7f)
                );
                
                GameObject pothole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pothole.name = "Pothole";
                pothole.transform.position = pos;
                pothole.transform.localScale = new Vector3(
                    Random.Range(0.3f, 1f),
                    0.02f,
                    Random.Range(0.3f, 1f)
                );
                
                Material potholeMat = new Material(Shader.Find("Standard"));
                potholeMat.SetColor("_Color", new Color(0.08f, 0.08f, 0.08f));
                pothole.GetComponent<Renderer>().material = potholeMat;
                Destroy(pothole.GetComponent<Collider>());
                
                generatedObjects.Add(pothole);
            }
            
            // Sidewalks
            CreateSidewalk(new Vector3(roadWidth/2 + 1.5f, 0.1f, 0), new Vector3(3f, 0.2f, townSize * 1.5f));
            CreateSidewalk(new Vector3(-roadWidth/2 - 1.5f, 0.1f, 0), new Vector3(3f, 0.2f, townSize * 1.5f));
        }
        
        private void CreateSidewalk(Vector3 position, Vector3 scale)
        {
            GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidewalk.name = "Sidewalk";
            sidewalk.transform.position = position;
            sidewalk.transform.localScale = scale;
            sidewalk.GetComponent<Renderer>().material = concreteMaterial;
            generatedObjects.Add(sidewalk);
            
            // Cracks in sidewalk
            for (int i = 0; i < 10; i++)
            {
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crack.name = "SidewalkCrack";
                crack.transform.position = position + new Vector3(
                    Random.Range(-scale.x/3, scale.x/3),
                    0.11f,
                    Random.Range(-scale.z/2, scale.z/2)
                );
                crack.transform.localScale = new Vector3(
                    Random.Range(0.05f, 0.15f),
                    0.01f,
                    Random.Range(0.5f, 2f)
                );
                crack.transform.rotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);
                
                Material crackMat = new Material(Shader.Find("Standard"));
                crackMat.SetColor("_Color", new Color(0.15f, 0.14f, 0.13f));
                crack.GetComponent<Renderer>().material = crackMat;
                Destroy(crack.GetComponent<Collider>());
                
                generatedObjects.Add(crack);
            }
        }
        
        private void GenerateGasStation()
        {
            Vector3 gasStationPos = new Vector3(-roadWidth - 12f, 0, -15f);
            
            // Main building
            GameObject building = CreateBuilding(
                gasStationPos + new Vector3(-5f, 0, 0),
                new Vector3(10f, 4f, 8f),
                "GasStation_Building"
            );
            
            // Canopy over pumps
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "GasStation_Canopy";
            canopy.transform.position = gasStationPos + new Vector3(5f, 4f, 0);
            canopy.transform.localScale = new Vector3(8f, 0.3f, 12f);
            canopy.GetComponent<Renderer>().material = concreteMaterial;
            generatedObjects.Add(canopy);
            
            // Canopy supports
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    support.name = "Canopy_Support";
                    support.transform.position = gasStationPos + new Vector3(5f + x * 3f, 2f, z * 4f);
                    support.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                    support.GetComponent<Renderer>().material = rustMaterial;
                    generatedObjects.Add(support);
                }
            }
            
            // Gas pumps
            for (int i = 0; i < 3; i++)
            {
                Vector3 pumpPos = gasStationPos + new Vector3(5f, 0, -4f + i * 4f);
                CreateGasPump(pumpPos);
            }
            
            // Burning barrel near entrance
            CreateBurningBarrel(gasStationPos + new Vector3(-2f, 0, 5f));
            
            // Oil puddles
            for (int i = 0; i < 5; i++)
            {
                GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puddle.name = "OilPuddle";
                puddle.transform.position = gasStationPos + new Vector3(
                    5f + Random.Range(-3f, 3f),
                    0.01f,
                    Random.Range(-5f, 5f)
                );
                puddle.transform.localScale = new Vector3(
                    Random.Range(0.5f, 1.5f),
                    0.01f,
                    Random.Range(0.5f, 1.5f)
                );
                
                Material oilMat = new Material(Shader.Find("Standard"));
                oilMat.SetColor("_Color", new Color(0.05f, 0.05f, 0.08f));
                oilMat.SetFloat("_Metallic", 0.8f);
                oilMat.SetFloat("_Glossiness", 0.9f);
                puddle.GetComponent<Renderer>().material = oilMat;
                Destroy(puddle.GetComponent<Collider>());
                
                generatedObjects.Add(puddle);
            }
        }
        
        private void CreateGasPump(Vector3 position)
        {
            // Pump body
            GameObject pump = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pump.name = "GasPump";
            pump.transform.position = position + Vector3.up * 0.75f;
            pump.transform.localScale = new Vector3(0.6f, 1.5f, 0.4f);
            pump.GetComponent<Renderer>().material = rustMaterial;
            generatedObjects.Add(pump);
            
            // Pump top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "GasPump_Top";
            top.transform.position = position + Vector3.up * 1.6f;
            top.transform.localScale = new Vector3(0.65f, 0.2f, 0.45f);
            
            Material topMat = new Material(Shader.Find("Standard"));
            topMat.SetColor("_Color", new Color(0.6f, 0.1f, 0.1f));
            top.GetComponent<Renderer>().material = topMat;
            generatedObjects.Add(top);
            
            // Hose (dangling)
            GameObject hose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hose.name = "GasPump_Hose";
            hose.transform.position = position + new Vector3(0.35f, 0.5f, 0);
            hose.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            hose.transform.rotation = Quaternion.Euler(0, 0, 60);
            
            Material hoseMat = new Material(Shader.Find("Standard"));
            hoseMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
            hose.GetComponent<Renderer>().material = hoseMat;
            Destroy(hose.GetComponent<Collider>());
            
            generatedObjects.Add(hose);
        }
        
        private void GenerateAbandonedBuildings()
        {
            // Right side of road - shops
            CreateShopBuilding(new Vector3(roadWidth/2 + 8f, 0, 20f), "Rusty's Diner");
            CreateShopBuilding(new Vector3(roadWidth/2 + 8f, 0, 5f), "General Store");
            CreateShopBuilding(new Vector3(roadWidth/2 + 8f, 0, -10f), "Hardware");
            CreateShopBuilding(new Vector3(roadWidth/2 + 8f, 0, -25f), "Pharmacy");
            
            // Left side buildings
            CreateShopBuilding(new Vector3(-roadWidth/2 - 8f, 0, 25f), "Bar");
            CreateShopBuilding(new Vector3(-roadWidth/2 - 8f, 0, 10f), "Laundromat");
        }
        
        private void CreateShopBuilding(Vector3 position, string shopName)
        {
            float width = Random.Range(8f, 12f);
            float height = Random.Range(4f, 6f);
            float depth = Random.Range(10f, 14f);
            
            // Main structure
            GameObject building = CreateBuilding(position, new Vector3(width, height, depth), $"Shop_{shopName}");
            
            // Awning
            GameObject awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = $"{shopName}_Awning";
            float awningZ = position.x > 0 ? position.z - depth/2 - 0.5f : position.z + depth/2 + 0.5f;
            awning.transform.position = new Vector3(position.x, height - 0.5f, awningZ);
            awning.transform.localScale = new Vector3(width + 0.5f, 0.1f, 1.5f);
            awning.transform.rotation = Quaternion.Euler(position.x > 0 ? 10 : -10, 0, 0);
            
            Material awningMat = new Material(Shader.Find("Standard"));
            awningMat.SetColor("_Color", new Color(
                Random.Range(0.3f, 0.5f),
                Random.Range(0.2f, 0.35f),
                Random.Range(0.15f, 0.25f)
            ));
            awning.GetComponent<Renderer>().material = awningMat;
            generatedObjects.Add(awning);
            
            // Boarded windows
            int windowCount = Mathf.FloorToInt(width / 3);
            for (int i = 0; i < windowCount; i++)
            {
                float windowX = position.x - width/2 + 1.5f + i * 3f;
                float windowZ = position.x > 0 ? position.z - depth/2 - 0.01f : position.z + depth/2 + 0.01f;
                
                // Window frame
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = "Window";
                window.transform.position = new Vector3(windowX, 2f, windowZ);
                window.transform.localScale = new Vector3(1.5f, 2f, 0.1f);
                
                Material windowMat = new Material(Shader.Find("Standard"));
                windowMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.12f));
                window.GetComponent<Renderer>().material = windowMat;
                generatedObjects.Add(window);
                
                // Boards (if boarded up - 50% chance)
                if (Random.value > 0.5f)
                {
                    for (int b = 0; b < 3; b++)
                    {
                        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        board.name = "Board";
                        board.transform.position = new Vector3(
                            windowX,
                            1.5f + b * 0.7f,
                            windowZ + (position.x > 0 ? -0.1f : 0.1f)
                        );
                        board.transform.localScale = new Vector3(1.8f, 0.15f, 0.05f);
                        board.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
                        board.GetComponent<Renderer>().material = woodMaterial;
                        Destroy(board.GetComponent<Collider>());
                        generatedObjects.Add(board);
                    }
                }
            }
        }
        
        private GameObject CreateBuilding(Vector3 position, Vector3 size, string name)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = name;
            building.transform.position = position + Vector3.up * (size.y / 2);
            building.transform.localScale = size;
            
            Material buildingMat = new Material(Shader.Find("Standard"));
            buildingMat.SetColor("_Color", new Color(
                0.4f + Random.Range(-0.1f, 0.1f),
                0.38f + Random.Range(-0.1f, 0.1f),
                0.35f + Random.Range(-0.1f, 0.1f)
            ));
            buildingMat.SetFloat("_Glossiness", 0.1f);
            building.GetComponent<Renderer>().material = buildingMat;
            
            generatedObjects.Add(building);
            return building;
        }
        
        private void GenerateResidentialHouses()
        {
            // Houses on the outskirts
            Vector3[] housePositions = new Vector3[]
            {
                new Vector3(30f, 0, 35f),
                new Vector3(40f, 0, 20f),
                new Vector3(35f, 0, -30f),
                new Vector3(-35f, 0, 35f),
                new Vector3(-40f, 0, -35f),
            };
            
            foreach (Vector3 pos in housePositions)
            {
                CreateHouse(pos);
            }
        }
        
        private void CreateHouse(Vector3 position)
        {
            // Main structure
            float width = Random.Range(8f, 12f);
            float height = Random.Range(3.5f, 5f);
            float depth = Random.Range(8f, 12f);
            
            GameObject house = CreateBuilding(position, new Vector3(width, height, depth), "House");
            
            // Roof
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "House_Roof";
            roof.transform.position = position + Vector3.up * (height + 0.5f);
            roof.transform.localScale = new Vector3(width + 1f, 1f, depth + 1f);
            roof.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
            
            Material roofMat = new Material(Shader.Find("Standard"));
            roofMat.SetColor("_Color", new Color(0.25f, 0.2f, 0.18f));
            roof.GetComponent<Renderer>().material = roofMat;
            generatedObjects.Add(roof);
            
            // Broken fence
            CreateBrokenFence(position + new Vector3(0, 0, depth/2 + 2f), width + 4f);
            
            // Overgrown yard (tall grass patches)
            for (int i = 0; i < 8; i++)
            {
                GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grass.name = "TallGrass";
                grass.transform.position = position + new Vector3(
                    Random.Range(-width/2 - 2, width/2 + 2),
                    0.3f,
                    depth/2 + 1f + Random.Range(0f, 3f)
                );
                grass.transform.localScale = new Vector3(0.1f, Random.Range(0.4f, 0.8f), 0.1f);
                
                Material grassMat = new Material(Shader.Find("Standard"));
                grassMat.SetColor("_Color", new Color(0.3f, 0.35f, 0.2f));
                grass.GetComponent<Renderer>().material = grassMat;
                Destroy(grass.GetComponent<Collider>());
                
                generatedObjects.Add(grass);
            }
        }
        
        private void CreateBrokenFence(Vector3 position, float length)
        {
            int postCount = Mathf.FloorToInt(length / 2f);
            
            for (int i = 0; i < postCount; i++)
            {
                // Some posts are missing
                if (Random.value > 0.3f)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    post.name = "FencePost";
                    post.transform.position = position + new Vector3(-length/2 + i * 2f, 0.5f, 0);
                    
                    // Some posts are tilted
                    float tilt = Random.Range(-15f, 15f);
                    post.transform.rotation = Quaternion.Euler(tilt, 0, Random.Range(-5f, 5f));
                    post.transform.localScale = new Vector3(0.1f, Random.Range(0.6f, 1f), 0.1f);
                    post.GetComponent<Renderer>().material = woodMaterial;
                    
                    generatedObjects.Add(post);
                }
            }
        }
        
        private void GenerateAbandonedVehicles()
        {
            Vector3[] carPositions = new Vector3[]
            {
                new Vector3(3f, 0, 10f),
                new Vector3(-4f, 0, -20f),
                new Vector3(2f, 0, 30f),
                new Vector3(-3f, 0, -35f),
                new Vector3(15f, 0, 5f),
                new Vector3(-18f, 0, 25f),
                new Vector3(20f, 0, -15f),
            };
            
            foreach (Vector3 pos in carPositions)
            {
                CreateAbandonedCar(pos);
            }
            
            // Overturned truck as barricade
            CreateOverturnedTruck(new Vector3(0, 0, -45f));
        }
        
        private void CreateAbandonedCar(Vector3 position)
        {
            GameObject car = new GameObject("AbandonedCar");
            car.transform.position = position;
            car.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), Random.Range(-5f, 5f));
            
            // Car body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CarBody";
            body.transform.SetParent(car.transform);
            body.transform.localPosition = new Vector3(0, 0.6f, 0);
            body.transform.localScale = new Vector3(2f, 0.8f, 4f);
            body.GetComponent<Renderer>().material = rustMaterial;
            
            // Car top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "CarTop";
            top.transform.SetParent(car.transform);
            top.transform.localPosition = new Vector3(0, 1.2f, -0.3f);
            top.transform.localScale = new Vector3(1.8f, 0.6f, 2f);
            top.GetComponent<Renderer>().material = rustMaterial;
            
            // Wheels (some flat/missing)
            Vector3[] wheelPositions = new Vector3[]
            {
                new Vector3(-0.9f, 0.3f, 1.2f),
                new Vector3(0.9f, 0.3f, 1.2f),
                new Vector3(-0.9f, 0.3f, -1.2f),
                new Vector3(0.9f, 0.3f, -1.2f),
            };
            
            foreach (Vector3 wheelPos in wheelPositions)
            {
                if (Random.value > 0.2f) // 80% chance wheel exists
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Wheel";
                    wheel.transform.SetParent(car.transform);
                    wheel.transform.localPosition = wheelPos;
                    wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    wheel.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
                    
                    Material wheelMat = new Material(Shader.Find("Standard"));
                    wheelMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
                    wheel.GetComponent<Renderer>().material = wheelMat;
                }
            }
            
            // Broken windows (dark interior)
            GameObject windows = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windows.name = "Windows";
            windows.transform.SetParent(car.transform);
            windows.transform.localPosition = new Vector3(0, 1.2f, -0.3f);
            windows.transform.localScale = new Vector3(1.6f, 0.4f, 1.8f);
            windows.GetComponent<Renderer>().material = glassMaterial;
            Destroy(windows.GetComponent<Collider>());
            
            generatedObjects.Add(car);
        }
        
        private void CreateOverturnedTruck(Vector3 position)
        {
            GameObject truck = new GameObject("OverturnedTruck");
            truck.transform.position = position;
            truck.transform.rotation = Quaternion.Euler(0, 90, 85); // On its side
            
            // Truck body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "TruckBody";
            body.transform.SetParent(truck.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(2.5f, 2f, 6f);
            body.GetComponent<Renderer>().material = rustMaterial;
            
            // Cab
            GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cab.name = "TruckCab";
            cab.transform.SetParent(truck.transform);
            cab.transform.localPosition = new Vector3(0, 0.2f, 4f);
            cab.transform.localScale = new Vector3(2.5f, 2.2f, 2.5f);
            cab.GetComponent<Renderer>().material = rustMaterial;
            
            generatedObjects.Add(truck);
        }
        
        private void GenerateAlleyways()
        {
            // Dumpsters
            Vector3[] dumpsterPositions = new Vector3[]
            {
                new Vector3(roadWidth/2 + 15f, 0, 12f),
                new Vector3(roadWidth/2 + 14f, 0, -5f),
                new Vector3(-roadWidth/2 - 15f, 0, 18f),
                new Vector3(-roadWidth/2 - 13f, 0, -28f),
            };
            
            foreach (Vector3 pos in dumpsterPositions)
            {
                CreateDumpster(pos);
            }
        }
        
        private void CreateDumpster(Vector3 position)
        {
            GameObject dumpster = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dumpster.name = "Dumpster";
            dumpster.transform.position = position + Vector3.up * 0.8f;
            dumpster.transform.localScale = new Vector3(2f, 1.6f, 1.5f);
            dumpster.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            Material dumpsterMat = new Material(Shader.Find("Standard"));
            dumpsterMat.SetColor("_Color", new Color(0.2f, 0.35f, 0.2f));
            dumpsterMat.SetFloat("_Metallic", 0.7f);
            dumpster.GetComponent<Renderer>().material = dumpsterMat;
            
            generatedObjects.Add(dumpster);
            
            // Trash around dumpster
            for (int i = 0; i < 8; i++)
            {
                GameObject trash = GameObject.CreatePrimitive(Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                trash.name = "Trash";
                trash.transform.position = position + new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-2f, 2f)
                );
                trash.transform.localScale = Vector3.one * Random.Range(0.1f, 0.4f);
                trash.transform.rotation = Random.rotation;
                
                Material trashMat = new Material(Shader.Find("Standard"));
                trashMat.SetColor("_Color", new Color(
                    Random.Range(0.2f, 0.5f),
                    Random.Range(0.2f, 0.4f),
                    Random.Range(0.15f, 0.3f)
                ));
                trash.GetComponent<Renderer>().material = trashMat;
                Destroy(trash.GetComponent<Collider>());
                
                generatedObjects.Add(trash);
            }
        }
        
        private void GenerateBurningBarrels()
        {
            Vector3[] barrelPositions = new Vector3[]
            {
                new Vector3(5f, 0, 15f),
                new Vector3(-6f, 0, -10f),
                new Vector3(roadWidth/2 + 3f, 0, 0),
                new Vector3(-roadWidth/2 - 3f, 0, 20f),
                new Vector3(25f, 0, -20f),
                new Vector3(-30f, 0, 30f),
            };
            
            foreach (Vector3 pos in barrelPositions)
            {
                CreateBurningBarrel(pos);
            }
        }
        
        private void CreateBurningBarrel(Vector3 position)
        {
            // Metal barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "BurningBarrel";
            barrel.transform.position = position + Vector3.up * 0.5f;
            barrel.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            barrel.GetComponent<Renderer>().material = rustMaterial;
            generatedObjects.Add(barrel);
            
            // Fire glow inside
            GameObject fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.name = "Fire";
            fire.transform.position = position + Vector3.up * 0.9f;
            fire.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
            fire.GetComponent<Renderer>().material = fireMaterial;
            Destroy(fire.GetComponent<Collider>());
            generatedObjects.Add(fire);
            
            // Fire light
            GameObject lightObj = new GameObject("FireLight");
            lightObj.transform.position = position + Vector3.up * 1.2f;
            Light fireLight = lightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.6f, 0.2f);
            fireLight.intensity = 2f;
            fireLight.range = 8f;
            fireLight.shadows = LightShadows.Soft;
            generatedObjects.Add(lightObj);
            
            // Smoke effect (simple)
            for (int i = 0; i < 3; i++)
            {
                GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                smoke.name = "Smoke";
                smoke.transform.position = position + new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    1.2f + i * 0.5f,
                    Random.Range(-0.2f, 0.2f)
                );
                smoke.transform.localScale = Vector3.one * (0.3f + i * 0.2f);
                
                Material smokeMat = new Material(Shader.Find("Standard"));
                smokeMat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 0.3f));
                smoke.GetComponent<Renderer>().material = smokeMat;
                Destroy(smoke.GetComponent<Collider>());
                
                generatedObjects.Add(smoke);
            }
        }
        
        private void GenerateDebrisAndClutter()
        {
            // Scattered debris throughout
            for (int i = 0; i < 50; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-townSize * 0.8f, townSize * 0.8f),
                    0,
                    Random.Range(-townSize * 0.8f, townSize * 0.8f)
                );
                
                GameObject debris = GameObject.CreatePrimitive(
                    Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Cylinder
                );
                debris.name = "Debris";
                debris.transform.position = pos + Vector3.up * Random.Range(0.05f, 0.2f);
                debris.transform.localScale = new Vector3(
                    Random.Range(0.2f, 0.8f),
                    Random.Range(0.1f, 0.4f),
                    Random.Range(0.2f, 0.8f)
                );
                debris.transform.rotation = Random.rotation;
                
                Material debrisMat = new Material(Shader.Find("Standard"));
                debrisMat.SetColor("_Color", new Color(
                    Random.Range(0.25f, 0.45f),
                    Random.Range(0.22f, 0.4f),
                    Random.Range(0.2f, 0.35f)
                ));
                debrisMat.SetFloat("_Glossiness", Random.Range(0.05f, 0.3f));
                debris.GetComponent<Renderer>().material = debrisMat;
                Destroy(debris.GetComponent<Collider>());
                
                generatedObjects.Add(debris);
            }
            
            // Shopping carts
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-20f, 20f),
                    0,
                    Random.Range(-30f, 30f)
                );
                CreateShoppingCart(pos);
            }
        }
        
        private void CreateShoppingCart(Vector3 position)
        {
            GameObject cart = new GameObject("ShoppingCart");
            cart.transform.position = position;
            cart.transform.rotation = Quaternion.Euler(
                Random.Range(-20f, 20f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );
            
            // Cart basket
            GameObject basket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basket.name = "CartBasket";
            basket.transform.SetParent(cart.transform);
            basket.transform.localPosition = new Vector3(0, 0.5f, 0);
            basket.transform.localScale = new Vector3(0.6f, 0.4f, 0.8f);
            
            Material cartMat = new Material(Shader.Find("Standard"));
            cartMat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
            cartMat.SetFloat("_Metallic", 0.8f);
            basket.GetComponent<Renderer>().material = cartMat;
            
            generatedObjects.Add(cart);
        }
        
        private void GenerateUtilityPoles()
        {
            Vector3[] polePositions = new Vector3[]
            {
                new Vector3(roadWidth/2 + 2f, 0, -30f),
                new Vector3(roadWidth/2 + 2f, 0, 0),
                new Vector3(roadWidth/2 + 2f, 0, 30f),
                new Vector3(-roadWidth/2 - 2f, 0, -20f),
                new Vector3(-roadWidth/2 - 2f, 0, 15f),
            };
            
            for (int i = 0; i < polePositions.Length; i++)
            {
                CreateUtilityPole(polePositions[i], i < polePositions.Length - 1 ? polePositions[i + 1] : Vector3.zero);
            }
        }
        
        private void CreateUtilityPole(Vector3 position, Vector3 nextPole)
        {
            // Pole (slightly tilted)
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "UtilityPole";
            pole.transform.position = position + Vector3.up * 5f;
            pole.transform.localScale = new Vector3(0.2f, 5f, 0.2f);
            pole.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            pole.GetComponent<Renderer>().material = woodMaterial;
            generatedObjects.Add(pole);
            
            // Crossbar
            GameObject crossbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossbar.name = "Crossbar";
            crossbar.transform.position = position + Vector3.up * 9.5f;
            crossbar.transform.localScale = new Vector3(3f, 0.15f, 0.15f);
            crossbar.GetComponent<Renderer>().material = woodMaterial;
            generatedObjects.Add(crossbar);
            
            // Dangling wires
            if (nextPole != Vector3.zero)
            {
                for (int w = 0; w < 2; w++)
                {
                    GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wire.name = "Wire";
                    Vector3 midpoint = (position + nextPole) / 2f;
                    wire.transform.position = midpoint + Vector3.up * (8f - w * 0.5f);
                    
                    float distance = Vector3.Distance(position, nextPole);
                    wire.transform.localScale = new Vector3(0.02f, distance / 2f, 0.02f);
                    wire.transform.LookAt(nextPole + Vector3.up * 9f);
                    wire.transform.Rotate(90, 0, 0);
                    
                    Material wireMat = new Material(Shader.Find("Standard"));
                    wireMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.1f));
                    wire.GetComponent<Renderer>().material = wireMat;
                    Destroy(wire.GetComponent<Collider>());
                    
                    generatedObjects.Add(wire);
                }
            }
        }
        
        private void GenerateFog()
        {
            // Low-lying fog patches
            for (int i = 0; i < 15; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-townSize * 0.7f, townSize * 0.7f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-townSize * 0.7f, townSize * 0.7f)
                );
                
                GameObject fogPatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fogPatch.name = "FogPatch";
                fogPatch.transform.position = pos;
                fogPatch.transform.localScale = new Vector3(
                    Random.Range(5f, 15f),
                    Random.Range(1f, 3f),
                    Random.Range(5f, 15f)
                );
                
                Material fogMat = new Material(Shader.Find("Standard"));
                fogMat.SetColor("_Color", new Color(0.5f, 0.45f, 0.4f, 0.15f));
                fogMat.SetFloat("_Mode", 3); // Transparent
                fogMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                fogMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                fogPatch.GetComponent<Renderer>().material = fogMat;
                Destroy(fogPatch.GetComponent<Collider>());
                
                generatedObjects.Add(fogPatch);
            }
        }
        
        private void SetupSundownLighting()
        {
            // Main sundown directional light
            GameObject sunLight = new GameObject("SundownLight");
            Light sun = sunLight.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = sunsetColor;
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;
            sunLight.transform.rotation = Quaternion.Euler(15f, -30f, 0); // Low sun angle
            generatedObjects.Add(sunLight);
            
            // Ambient fill light (purple/blue for shadows)
            GameObject ambientLight = new GameObject("AmbientFill");
            Light ambient = ambientLight.AddComponent<Light>();
            ambient.type = LightType.Directional;
            ambient.color = new Color(0.4f, 0.3f, 0.5f);
            ambient.intensity = 0.3f;
            ambient.shadows = LightShadows.None;
            ambientLight.transform.rotation = Quaternion.Euler(45f, 150f, 0);
            generatedObjects.Add(ambientLight);
        }
        
        private void SetupAtmosphere()
        {
            // Skybox color (sunset) - Camera may not exist yet, will be set later
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = skyColor;
                cam.clearFlags = CameraClearFlags.SolidColor;
            }
            
            // Fog settings
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.5f, 0.35f, 0.25f);
            RenderSettings.fogDensity = fogDensity;
            
            // Ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.3f, 0.25f);
            RenderSettings.ambientIntensity = 0.8f;
        }
        
        // Called after camera is created to set up camera-specific settings
        private void LateUpdate()
        {
            // Apply camera settings if camera exists now
            Camera cam = Camera.main;
            if (cam != null && cam.clearFlags != CameraClearFlags.SolidColor)
            {
                cam.backgroundColor = skyColor;
                cam.clearFlags = CameraClearFlags.SolidColor;
            }
        }
        
        private void OnDestroy()
        {
            ClearMap();
        }
    }
}

