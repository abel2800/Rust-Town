using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Creates realistic terrain with proper height variation, texturing, and natural details
    /// Uses procedural noise for organic-looking landscapes
    /// </summary>
    public class RealisticTerrainGenerator : MonoBehaviour
    {
        // Terrain materials with realistic properties
        private static Material grassMaterial;
        private static Material dirtMaterial;
        private static Material rockMaterial;
        private static Material sandMaterial;
        private static Material mudMaterial;
        private static Material deadGrassMaterial;
        
        public static void InitializeMaterials()
        {
            // Muted grass - more realistic, less saturated
            grassMaterial = CreatePBRMaterial(
                new Color(0.28f, 0.32f, 0.22f),  // Muted olive green
                0.0f, 0.1f
            );
            
            // Natural dirt/soil - brown earth tones
            dirtMaterial = CreatePBRMaterial(
                new Color(0.35f, 0.28f, 0.22f),  // Brown dirt
                0.0f, 0.08f
            );
            
            // Rocky surface - gray with slight warmth
            rockMaterial = CreatePBRMaterial(
                new Color(0.4f, 0.38f, 0.36f),
                0.0f, 0.2f
            );
            
            // Sandy/dusty areas - muted beige
            sandMaterial = CreatePBRMaterial(
                new Color(0.45f, 0.4f, 0.32f),
                0.0f, 0.05f
            );
            
            // Wet mud - dark brown
            mudMaterial = CreatePBRMaterial(
                new Color(0.2f, 0.16f, 0.12f),
                0.0f, 0.35f
            );
            
            // Dead/dry grass - muted tan/brown
            deadGrassMaterial = CreatePBRMaterial(
                new Color(0.38f, 0.33f, 0.25f),
                0.0f, 0.08f
            );
        }
        
        private static Material CreatePBRMaterial(Color baseColor, float metallic, float smoothness)
        {
            return URPMaterialHelper.CreateMaterial(baseColor, smoothness, metallic);
        }
        
        /// <summary>
        /// Creates the main realistic terrain with height variation and natural texturing
        /// </summary>
        public static void CreateRealisticGround(float size, List<GameObject> objectList, bool isPostApocalyptic = true)
        {
            if (grassMaterial == null) InitializeMaterials();
            
            GameObject terrainRoot = new GameObject("RealisticTerrain");
            objectList.Add(terrainRoot);
            
            // Create multi-layer terrain
            CreateBaseTerrainLayer(terrainRoot.transform, size, objectList);
            CreateTerrainVariation(terrainRoot.transform, size, objectList, isPostApocalyptic);
            CreateNaturalGroundCover(terrainRoot.transform, size, objectList, isPostApocalyptic);
            CreateTerrainDetails(terrainRoot.transform, size, objectList, isPostApocalyptic);
            CreatePuddlesAndWetAreas(terrainRoot.transform, size, objectList);
            CreateRocksAndBoulders(terrainRoot.transform, size, objectList);
        }
        
        private static void CreateBaseTerrainLayer(Transform parent, float size, List<GameObject> objectList)
        {
            // Main ground - single unified layer with subtle variation
            GameObject mainGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainGround.name = "MainGround";
            mainGround.transform.SetParent(parent);
            mainGround.transform.position = new Vector3(0, -0.5f, 0);
            mainGround.transform.localScale = new Vector3(size * 2.2f, 1f, size * 2.2f);
            
            // Base earth tone - muted brown
            Material groundMat = URPMaterialHelper.CreateMaterial(new Color(0.32f, 0.28f, 0.22f), 0.1f, 0f);
            mainGround.GetComponent<Renderer>().material = groundMat;
            
            // Add subtle tile variations on top
            int resolution = 6;
            float tileSize = size * 2f / resolution;
            
            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float posX = (x - resolution / 2f + 0.5f) * tileSize;
                    float posZ = (z - resolution / 2f + 0.5f) * tileSize;
                    
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"TerrainTile_{x}_{z}";
                    tile.transform.SetParent(parent);
                    tile.transform.position = new Vector3(posX, 0.01f, posZ);
                    tile.transform.localScale = new Vector3(tileSize + 0.5f, 0.02f, tileSize + 0.5f);
                    
                    // Subtle color variation - all similar earth tones
                    float variation = Random.Range(-0.04f, 0.04f);
                    Material tileMat = URPMaterialHelper.CreateMaterial(
                        new Color(0.33f + variation, 0.28f + variation, 0.22f + variation),
                        0.08f, 0f
                    );
                    tile.GetComponent<Renderer>().material = tileMat;
                    Destroy(tile.GetComponent<Collider>());
                }
            }
        }
        
        private static Material CreateVariedMaterial(Material baseMat, float variation)
        {
            Material mat = new Material(baseMat);
            Color baseColor = mat.GetColor("_Color");
            mat.SetColor("_Color", new Color(
                baseColor.r + Random.Range(-variation, variation),
                baseColor.g + Random.Range(-variation, variation),
                baseColor.b + Random.Range(-variation, variation)
            ));
            return mat;
        }
        
        private static void CreateTerrainVariation(Transform parent, float size, List<GameObject> objectList, bool isPostApocalyptic)
        {
            int patchCount = isPostApocalyptic ? 80 : 50;
            
            for (int i = 0; i < patchCount; i++)
            {
                float noiseX = Random.Range(-size * 0.9f, size * 0.9f);
                float noiseZ = Random.Range(-size * 0.9f, size * 0.9f);
                
                // Use noise to determine patch placement
                float noise = Mathf.PerlinNoise(noiseX * 0.03f + 50f, noiseZ * 0.03f + 50f);
                
                GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patch.name = "TerrainPatch";
                patch.transform.SetParent(parent);
                patch.transform.position = new Vector3(noiseX, 0.005f + noise * 0.02f, noiseZ);
                
                float patchSize = Random.Range(2f, 7f);
                patch.transform.localScale = new Vector3(
                    patchSize * Random.Range(0.8f, 1.5f),
                    0.01f,  // Very thin
                    patchSize * Random.Range(0.8f, 1.5f)
                );
                patch.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                // More subtle, muted colors - all browns and tans
                Material patchMat = URPMaterialHelper.CreateMaterial();
                float baseValue = 0.3f + noise * 0.1f;  // Base brightness
                float variation = Random.Range(-0.03f, 0.03f);
                
                // All earth tones - browns, tans, grays
                patchMat.SetColor("_BaseColor", new Color(
                    baseValue + variation + 0.02f,
                    baseValue + variation - 0.02f,
                    baseValue + variation - 0.05f
                ));
                patchMat.SetColor("_Color", new Color(
                    baseValue + variation + 0.02f,
                    baseValue + variation - 0.02f,
                    baseValue + variation - 0.05f
                ));
                URPMaterialHelper.SetMaterialProperties(patchMat, 
                    new Color(baseValue + variation + 0.02f, baseValue + variation - 0.02f, baseValue + variation - 0.05f),
                    0.08f, 0f);
                
                patch.GetComponent<Renderer>().material = patchMat;
                Destroy(patch.GetComponent<Collider>());
            }
        }
        
        private static void CreateNaturalGroundCover(Transform parent, float size, List<GameObject> objectList, bool isPostApocalyptic)
        {
            // Grass tufts
            int grassCount = isPostApocalyptic ? 150 : 300;
            
            for (int i = 0; i < grassCount; i++)
            {
                float x = Random.Range(-size * 0.95f, size * 0.95f);
                float z = Random.Range(-size * 0.95f, size * 0.95f);
                
                // Skip areas that should be clear (roads, buildings)
                if (Mathf.Abs(x) < 8f && Mathf.Abs(z) < size * 0.8f) continue;
                
                CreateGrassTuft(parent, new Vector3(x, 0, z), isPostApocalyptic);
            }
            
            // Small debris and organic matter
            int debrisCount = isPostApocalyptic ? 80 : 30;
            for (int i = 0; i < debrisCount; i++)
            {
                float x = Random.Range(-size * 0.9f, size * 0.9f);
                float z = Random.Range(-size * 0.9f, size * 0.9f);
                
                CreateOrganicDebris(parent, new Vector3(x, 0, z), isPostApocalyptic);
            }
        }
        
        private static void CreateGrassTuft(Transform parent, Vector3 position, bool isDead)
        {
            int bladeCount = Random.Range(3, 8);
            
            for (int i = 0; i < bladeCount; i++)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "GrassBlade";
                blade.transform.SetParent(parent);
                
                float offset = Random.Range(-0.15f, 0.15f);
                float height = Random.Range(0.15f, 0.5f);
                
                blade.transform.position = position + new Vector3(offset, height / 2f, offset);
                blade.transform.localScale = new Vector3(0.03f, height, 0.015f);
                blade.transform.rotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );
                
                Material grassMat = new Material(URPMaterialHelper.GetLitShader());
                if (isDead)
                {
                    grassMat.SetColor("_Color", new Color(
                        Random.Range(0.35f, 0.5f),
                        Random.Range(0.3f, 0.4f),
                        Random.Range(0.15f, 0.25f)
                    ));
                }
                else
                {
                    grassMat.SetColor("_Color", new Color(
                        Random.Range(0.15f, 0.25f),
                        Random.Range(0.35f, 0.5f),
                        Random.Range(0.1f, 0.2f)
                    ));
                }
                grassMat.SetFloat("_Glossiness", 0.1f);
                blade.GetComponent<Renderer>().material = grassMat;
                Destroy(blade.GetComponent<Collider>());
            }
        }
        
        private static void CreateOrganicDebris(Transform parent, Vector3 position, bool isPostApocalyptic)
        {
            float type = Random.value;
            
            if (type < 0.4f)
            {
                // Twigs and sticks
                GameObject twig = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                twig.name = "Twig";
                twig.transform.SetParent(parent);
                twig.transform.position = position + Vector3.up * 0.02f;
                twig.transform.localScale = new Vector3(0.02f, Random.Range(0.15f, 0.4f), 0.02f);
                twig.transform.rotation = Quaternion.Euler(
                    Random.Range(70f, 90f),
                    Random.Range(0f, 360f),
                    0
                );
                
                Material twigMat = new Material(URPMaterialHelper.GetLitShader());
                twigMat.SetColor("_Color", new Color(0.25f, 0.18f, 0.1f));
                twig.GetComponent<Renderer>().material = twigMat;
                Destroy(twig.GetComponent<Collider>());
            }
            else if (type < 0.7f)
            {
                // Dead leaves / organic matter
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leaf.name = "DeadLeaf";
                leaf.transform.SetParent(parent);
                leaf.transform.position = position + Vector3.up * 0.01f;
                leaf.transform.localScale = new Vector3(
                    Random.Range(0.08f, 0.2f),
                    0.005f,
                    Random.Range(0.06f, 0.15f)
                );
                leaf.transform.rotation = Quaternion.Euler(
                    Random.Range(-5f, 5f),
                    Random.Range(0f, 360f),
                    Random.Range(-5f, 5f)
                );
                
                Material leafMat = new Material(URPMaterialHelper.GetLitShader());
                leafMat.SetColor("_Color", new Color(
                    Random.Range(0.3f, 0.45f),
                    Random.Range(0.2f, 0.35f),
                    Random.Range(0.1f, 0.2f)
                ));
                leaf.GetComponent<Renderer>().material = leafMat;
                Destroy(leaf.GetComponent<Collider>());
            }
            else
            {
                // Small stones
                GameObject stone = GameObject.CreatePrimitive(Random.value > 0.5f ? PrimitiveType.Sphere : PrimitiveType.Cube);
                stone.name = "SmallStone";
                stone.transform.SetParent(parent);
                stone.transform.position = position + Vector3.up * 0.03f;
                float stoneSize = Random.Range(0.05f, 0.15f);
                stone.transform.localScale = new Vector3(
                    stoneSize * Random.Range(0.8f, 1.2f),
                    stoneSize * Random.Range(0.5f, 1f),
                    stoneSize * Random.Range(0.8f, 1.2f)
                );
                stone.transform.rotation = Random.rotation;
                
                Material stoneMat = new Material(URPMaterialHelper.GetLitShader());
                float grayValue = Random.Range(0.3f, 0.5f);
                stoneMat.SetColor("_Color", new Color(grayValue, grayValue * 0.95f, grayValue * 0.9f));
                stoneMat.SetFloat("_Glossiness", 0.2f);
                stone.GetComponent<Renderer>().material = stoneMat;
                Destroy(stone.GetComponent<Collider>());
            }
        }
        
        private static void CreateTerrainDetails(Transform parent, float size, List<GameObject> objectList, bool isPostApocalyptic)
        {
            // Create dirt mounds and natural elevation changes
            int moundCount = Random.Range(8, 15);
            
            for (int i = 0; i < moundCount; i++)
            {
                float x = Random.Range(-size * 0.85f, size * 0.85f);
                float z = Random.Range(-size * 0.85f, size * 0.85f);
                
                // Skip central area
                if (Mathf.Abs(x) < 15f && Mathf.Abs(z) < 20f) continue;
                
                CreateDirtMound(parent, new Vector3(x, 0, z));
            }
            
            // Erosion channels
            int channelCount = Random.Range(3, 6);
            for (int i = 0; i < channelCount; i++)
            {
                CreateErosionChannel(parent, size);
            }
        }
        
        private static void CreateDirtMound(Transform parent, Vector3 position)
        {
            GameObject mound = new GameObject("DirtMound");
            mound.transform.SetParent(parent);
            mound.transform.position = position;
            
            // Create layered mound for natural look
            int layerCount = Random.Range(3, 6);
            float baseSize = Random.Range(1.5f, 4f);
            float maxHeight = Random.Range(0.3f, 0.8f);
            
            for (int i = 0; i < layerCount; i++)
            {
                float t = i / (float)layerCount;
                float layerSize = baseSize * (1f - t * 0.7f);
                float layerHeight = maxHeight * (1f - t);
                
                GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                layer.name = $"MoundLayer_{i}";
                layer.transform.SetParent(mound.transform);
                layer.transform.localPosition = Vector3.up * (layerHeight * 0.5f + i * 0.05f);
                layer.transform.localScale = new Vector3(layerSize, 0.1f, layerSize);
                
                Material layerMat = CreateVariedMaterial(dirtMaterial, 0.08f);
                layer.GetComponent<Renderer>().material = layerMat;
                Destroy(layer.GetComponent<Collider>());
            }
        }
        
        private static void CreateErosionChannel(Transform parent, float size)
        {
            Vector3 start = new Vector3(
                Random.Range(-size * 0.7f, size * 0.7f),
                -0.02f,
                Random.Range(-size * 0.7f, size * 0.7f)
            );
            
            Vector3 direction = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            
            float length = Random.Range(5f, 15f);
            int segmentCount = Mathf.CeilToInt(length / 2f);
            
            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 pos = start + direction * i * 2f;
                
                // Add some curve to the channel
                pos += new Vector3(
                    Mathf.Sin(i * 0.5f) * 0.5f,
                    0,
                    Mathf.Cos(i * 0.5f) * 0.5f
                );
                
                GameObject channel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                channel.name = "ErosionChannel";
                channel.transform.SetParent(parent);
                channel.transform.position = pos;
                channel.transform.localScale = new Vector3(
                    Random.Range(0.3f, 0.8f),
                    0.04f,
                    2.5f
                );
                channel.transform.LookAt(pos + direction);
                channel.transform.Rotate(0, 0, Random.Range(-5f, 5f));
                
                Material channelMat = new Material(URPMaterialHelper.GetLitShader());
                channelMat.SetColor("_Color", new Color(0.18f, 0.14f, 0.1f));
                channel.GetComponent<Renderer>().material = channelMat;
                Destroy(channel.GetComponent<Collider>());
            }
        }
        
        private static void CreatePuddlesAndWetAreas(Transform parent, float size, List<GameObject> objectList)
        {
            int puddleCount = Random.Range(5, 12);
            
            for (int i = 0; i < puddleCount; i++)
            {
                float x = Random.Range(-size * 0.8f, size * 0.8f);
                float z = Random.Range(-size * 0.8f, size * 0.8f);
                
                CreatePuddle(parent, new Vector3(x, 0.005f, z));
            }
        }
        
        private static void CreatePuddle(Transform parent, Vector3 position)
        {
            GameObject puddle = new GameObject("Puddle");
            puddle.transform.SetParent(parent);
            puddle.transform.position = position;
            
            // Wet ground ring
            GameObject wetArea = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wetArea.name = "WetGround";
            wetArea.transform.SetParent(puddle.transform);
            wetArea.transform.localPosition = Vector3.zero;
            float wetSize = Random.Range(1f, 3f);
            wetArea.transform.localScale = new Vector3(wetSize, 0.01f, wetSize);
            
            Material wetMat = new Material(URPMaterialHelper.GetLitShader());
            wetMat.SetColor("_Color", new Color(0.2f, 0.18f, 0.14f));
            wetMat.SetFloat("_Glossiness", 0.5f);
            wetArea.GetComponent<Renderer>().material = wetMat;
            Destroy(wetArea.GetComponent<Collider>());
            
            // Water surface
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "Water";
            water.transform.SetParent(puddle.transform);
            water.transform.localPosition = Vector3.up * 0.005f;
            water.transform.localScale = new Vector3(wetSize * 0.7f, 0.005f, wetSize * 0.7f);
            
            Material waterMat = new Material(URPMaterialHelper.GetLitShader());
            waterMat.SetColor("_Color", new Color(0.15f, 0.18f, 0.22f, 0.8f));
            waterMat.SetFloat("_Metallic", 0.1f);
            waterMat.SetFloat("_Glossiness", 0.9f);
            water.GetComponent<Renderer>().material = waterMat;
            Destroy(water.GetComponent<Collider>());
        }
        
        private static void CreateRocksAndBoulders(Transform parent, float size, List<GameObject> objectList)
        {
            // Medium rocks scattered around
            int rockCount = Random.Range(15, 30);
            
            for (int i = 0; i < rockCount; i++)
            {
                float x = Random.Range(-size * 0.9f, size * 0.9f);
                float z = Random.Range(-size * 0.9f, size * 0.9f);
                
                // Avoid center
                if (Mathf.Abs(x) < 10f && Mathf.Abs(z) < 15f) continue;
                
                CreateRock(parent, new Vector3(x, 0, z), Random.Range(0.2f, 0.8f));
            }
            
            // Large boulders on edges
            int boulderCount = Random.Range(4, 8);
            for (int i = 0; i < boulderCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = size * Random.Range(0.7f, 0.95f);
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                CreateBoulder(parent, pos);
            }
        }
        
        private static void CreateRock(Transform parent, Vector3 position, float scale)
        {
            GameObject rock = GameObject.CreatePrimitive(Random.value > 0.3f ? PrimitiveType.Cube : PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(parent);
            rock.transform.position = position + Vector3.up * scale * 0.3f;
            rock.transform.localScale = new Vector3(
                scale * Random.Range(0.8f, 1.3f),
                scale * Random.Range(0.5f, 1f),
                scale * Random.Range(0.8f, 1.3f)
            );
            rock.transform.rotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );
            
            Material rockMat = new Material(URPMaterialHelper.GetLitShader());
            float grayBase = Random.Range(0.3f, 0.5f);
            rockMat.SetColor("_Color", new Color(
                grayBase + Random.Range(-0.05f, 0.05f),
                grayBase * 0.95f + Random.Range(-0.05f, 0.05f),
                grayBase * 0.9f + Random.Range(-0.05f, 0.05f)
            ));
            rockMat.SetFloat("_Metallic", 0.0f);
            rockMat.SetFloat("_Glossiness", Random.Range(0.15f, 0.3f));
            rock.GetComponent<Renderer>().material = rockMat;
        }
        
        private static void CreateBoulder(Transform parent, Vector3 position)
        {
            GameObject boulder = new GameObject("Boulder");
            boulder.transform.SetParent(parent);
            boulder.transform.position = position;
            boulder.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float baseSize = Random.Range(1.5f, 3f);
            
            // Main boulder body (multiple shapes for organic look)
            int partCount = Random.Range(2, 4);
            for (int i = 0; i < partCount; i++)
            {
                GameObject part = GameObject.CreatePrimitive(Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere);
                part.name = $"BoulderPart_{i}";
                part.transform.SetParent(boulder.transform);
                
                float partSize = baseSize * Random.Range(0.5f, 1f);
                part.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    partSize * 0.3f + i * 0.1f,
                    Random.Range(-0.3f, 0.3f)
                );
                part.transform.localScale = new Vector3(
                    partSize * Random.Range(0.7f, 1.3f),
                    partSize * Random.Range(0.4f, 0.8f),
                    partSize * Random.Range(0.7f, 1.3f)
                );
                part.transform.localRotation = Quaternion.Euler(
                    Random.Range(-20f, 20f),
                    Random.Range(0f, 360f),
                    Random.Range(-20f, 20f)
                );
                
                Material boulderMat = new Material(URPMaterialHelper.GetLitShader());
                float grayBase = Random.Range(0.35f, 0.5f);
                boulderMat.SetColor("_Color", new Color(grayBase, grayBase * 0.95f, grayBase * 0.9f));
                boulderMat.SetFloat("_Glossiness", 0.2f);
                part.GetComponent<Renderer>().material = boulderMat;
            }
            
            // Moss patches on boulder
            if (Random.value > 0.4f)
            {
                int mossCount = Random.Range(1, 4);
                for (int i = 0; i < mossCount; i++)
                {
                    GameObject moss = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    moss.name = "Moss";
                    moss.transform.SetParent(boulder.transform);
                    moss.transform.localPosition = new Vector3(
                        Random.Range(-baseSize * 0.3f, baseSize * 0.3f),
                        baseSize * Random.Range(0.2f, 0.5f),
                        Random.Range(-baseSize * 0.3f, baseSize * 0.3f)
                    );
                    moss.transform.localScale = new Vector3(
                        Random.Range(0.2f, 0.6f),
                        0.02f,
                        Random.Range(0.2f, 0.6f)
                    );
                    moss.transform.localRotation = Random.rotation;
                    
                    Material mossMat = new Material(URPMaterialHelper.GetLitShader());
                    mossMat.SetColor("_Color", new Color(0.15f, 0.28f, 0.1f));
                    mossMat.SetFloat("_Glossiness", 0.1f);
                    moss.GetComponent<Renderer>().material = mossMat;
                    Destroy(moss.GetComponent<Collider>());
                }
            }
        }
    }
}

