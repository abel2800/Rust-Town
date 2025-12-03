using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Creates a realistic dense forest border around the playable area
    /// Features: Varied trees, underbrush, fallen logs, rocks, natural lighting
    /// </summary>
    public class ForestBorderGenerator : MonoBehaviour
    {
        // Tree materials
        private static Material barkMaterial;
        private static Material leafMaterial;
        private static Material deadLeafMaterial;
        private static Material pineMaterial;
        private static Material bushMaterial;
        private static Material fernMaterial;
        private static Material logMaterial;
        
        public static void InitializeMaterials()
        {
            // Realistic bark - dark brown
            barkMaterial = URPMaterialHelper.CreateMaterial(new Color(0.22f, 0.16f, 0.1f), 0.08f, 0f);
            
            // Muted green leaves - more realistic, less saturated
            leafMaterial = URPMaterialHelper.CreateMaterial(new Color(0.25f, 0.3f, 0.18f), 0.12f, 0f);
            
            // Dead/autumn leaves - muted orange-brown
            deadLeafMaterial = URPMaterialHelper.CreateMaterial(new Color(0.4f, 0.32f, 0.2f), 0.1f, 0f);
            
            // Pine needles - dark muted green
            pineMaterial = URPMaterialHelper.CreateMaterial(new Color(0.18f, 0.24f, 0.15f), 0.1f, 0f);
            
            // Bush foliage - muted green
            bushMaterial = URPMaterialHelper.CreateMaterial(new Color(0.22f, 0.28f, 0.16f), 0.1f, 0f);
            
            // Ferns - slightly brighter muted green
            fernMaterial = URPMaterialHelper.CreateMaterial(new Color(0.25f, 0.32f, 0.18f), 0.1f, 0f);
            
            // Fallen log / dead wood - gray-brown
            logMaterial = URPMaterialHelper.CreateMaterial(new Color(0.3f, 0.25f, 0.2f), 0.05f, 0f);
        }
        
        /// <summary>
        /// Creates a dense forest border around the map
        /// </summary>
        public static void CreateForestBorder(float mapSize, float borderWidth, List<GameObject> objectList, bool isPostApocalyptic = true)
        {
            if (barkMaterial == null) InitializeMaterials();
            
            GameObject forestRoot = new GameObject("ForestBorder");
            objectList.Add(forestRoot);
            
            // Create forest floor (different from main terrain)
            CreateForestFloor(forestRoot.transform, mapSize, borderWidth, objectList);
            
            // Generate trees in border area
            CreateTreeBorder(forestRoot.transform, mapSize, borderWidth, objectList, isPostApocalyptic);
            
            // Add underbrush and bushes
            CreateUnderbrush(forestRoot.transform, mapSize, borderWidth, objectList, isPostApocalyptic);
            
            // Fallen trees and logs
            CreateFallenTrees(forestRoot.transform, mapSize, borderWidth, objectList);
            
            // Forest rocks and boulders
            CreateForestRocks(forestRoot.transform, mapSize, borderWidth, objectList);
            
            // Forest edge grass transition
            CreateForestEdge(forestRoot.transform, mapSize, objectList, isPostApocalyptic);
            
            // Atmospheric elements (fog patches in forest)
            CreateForestAtmosphere(forestRoot.transform, mapSize, borderWidth, objectList);
        }
        
        private static void CreateForestFloor(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList)
        {
            // Create darker, organic forest floor outside the main area
            float outerSize = mapSize + borderWidth;
            
            // Forest floor tiles around the border
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
                float nextAngle = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                
                // Create floor segments
                for (int layer = 0; layer < 3; layer++)
                {
                    float innerRadius = mapSize + layer * (borderWidth / 3f);
                    float outerRadius = mapSize + (layer + 1) * (borderWidth / 3f);
                    float midRadius = (innerRadius + outerRadius) / 2f;
                    float midAngle = (angle + nextAngle) / 2f;
                    
                    Vector3 pos = new Vector3(
                        Mathf.Cos(midAngle) * midRadius,
                        -0.1f,
                        Mathf.Sin(midAngle) * midRadius
                    );
                    
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floor.name = "ForestFloor";
                    floor.transform.SetParent(parent);
                    floor.transform.position = pos;
                    floor.transform.localScale = new Vector3(
                        borderWidth / 2f,
                        0.3f,
                        outerSize / segments + 2f
                    );
                    floor.transform.rotation = Quaternion.Euler(0, midAngle * Mathf.Rad2Deg + 90f, 0);
                    
                    // Darker, more organic forest floor color
                    Material floorMat = new Material(URPMaterialHelper.GetLitShader());
                    float darkness = 0.15f + layer * 0.03f;
                    floorMat.SetColor("_Color", new Color(
                        darkness + Random.Range(-0.02f, 0.02f),
                        darkness * 0.9f + Random.Range(-0.02f, 0.02f),
                        darkness * 0.7f + Random.Range(-0.02f, 0.02f)
                    ));
                    floorMat.SetFloat("_Glossiness", 0.05f);
                    floor.GetComponent<Renderer>().material = floorMat;
                }
            }
            
            // Leaf litter layer
            int litterCount = 200;
            for (int i = 0; i < litterCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = mapSize + Random.Range(2f, borderWidth);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0.01f,
                    Mathf.Sin(angle) * distance
                );
                
                CreateLeafLitter(parent, pos);
            }
        }
        
        private static void CreateLeafLitter(Transform parent, Vector3 position)
        {
            int leafCount = Random.Range(3, 8);
            
            for (int i = 0; i < leafCount; i++)
            {
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leaf.name = "Leaf";
                leaf.transform.SetParent(parent);
                leaf.transform.position = position + new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(0f, 0.02f),
                    Random.Range(-0.3f, 0.3f)
                );
                leaf.transform.localScale = new Vector3(
                    Random.Range(0.06f, 0.15f),
                    0.003f,
                    Random.Range(0.04f, 0.1f)
                );
                leaf.transform.rotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );
                
                Material leafMat = new Material(URPMaterialHelper.GetLitShader());
                float type = Random.value;
                if (type < 0.4f)
                {
                    // Brown dead leaf
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.35f, 0.5f),
                        Random.Range(0.25f, 0.35f),
                        Random.Range(0.1f, 0.2f)
                    ));
                }
                else if (type < 0.7f)
                {
                    // Orange/red leaf
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.5f, 0.7f),
                        Random.Range(0.2f, 0.4f),
                        Random.Range(0.1f, 0.2f)
                    ));
                }
                else
                {
                    // Dark decomposing leaf
                    leafMat.SetColor("_Color", new Color(
                        Random.Range(0.15f, 0.25f),
                        Random.Range(0.12f, 0.2f),
                        Random.Range(0.08f, 0.15f)
                    ));
                }
                
                leaf.GetComponent<Renderer>().material = leafMat;
                Destroy(leaf.GetComponent<Collider>());
            }
        }
        
        private static void CreateTreeBorder(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList, bool isPostApocalyptic)
        {
            // Dense tree placement around the border
            int treeCount = Mathf.RoundToInt((mapSize + borderWidth) * 2f);  // Proportional to size
            
            for (int i = 0; i < treeCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = mapSize + Random.Range(3f, borderWidth - 2f);
                
                // Add some noise to distance for natural look
                distance += Mathf.PerlinNoise(angle * 3f, 0) * 5f - 2.5f;
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                // Choose tree type
                float treeType = Random.value;
                
                if (treeType < 0.4f)
                {
                    CreateDeciduousTree(parent, pos, isPostApocalyptic);
                }
                else if (treeType < 0.7f)
                {
                    CreatePineTree(parent, pos, isPostApocalyptic);
                }
                else if (treeType < 0.85f)
                {
                    CreateBirchTree(parent, pos, isPostApocalyptic);
                }
                else
                {
                    CreateDeadTree(parent, pos);
                }
            }
        }
        
        private static void CreateDeciduousTree(Transform parent, Vector3 position, bool isDead)
        {
            GameObject tree = new GameObject("DeciduousTree");
            tree.transform.SetParent(parent);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float treeHeight = Random.Range(6f, 12f);
            float trunkRadius = treeHeight * 0.03f + Random.Range(0.1f, 0.2f);
            
            // Trunk with slight taper
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = Vector3.up * (treeHeight * 0.35f);
            trunk.transform.localScale = new Vector3(trunkRadius, treeHeight * 0.35f, trunkRadius);
            trunk.transform.localRotation = Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            
            Material trunkMat = CreateVariedBarkMaterial();
            trunk.GetComponent<Renderer>().material = trunkMat;
            
            // Root flare at base
            CreateRootFlare(tree.transform, trunkRadius);
            
            // Major branches
            int branchCount = Random.Range(3, 6);
            for (int i = 0; i < branchCount; i++)
            {
                float branchHeight = treeHeight * (0.4f + i * 0.1f);
                float branchAngle = (360f / branchCount) * i + Random.Range(-20f, 20f);
                CreateBranch(tree.transform, branchHeight, branchAngle, trunkRadius, trunkMat);
            }
            
            // Foliage canopy
            if (!isDead || Random.value > 0.3f)
            {
                CreateFoliageCanopy(tree.transform, treeHeight, isDead);
            }
        }
        
        private static void CreatePineTree(Transform parent, Vector3 position, bool isDead)
        {
            GameObject tree = new GameObject("PineTree");
            tree.transform.SetParent(parent);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float treeHeight = Random.Range(8f, 15f);
            float trunkRadius = treeHeight * 0.025f + 0.1f;
            
            // Tall straight trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = Vector3.up * (treeHeight * 0.4f);
            trunk.transform.localScale = new Vector3(trunkRadius, treeHeight * 0.4f, trunkRadius);
            
            Material trunkMat = CreateVariedBarkMaterial();
            trunkMat.SetColor("_Color", new Color(0.22f, 0.15f, 0.1f));
            trunk.GetComponent<Renderer>().material = trunkMat;
            
            // Pine foliage layers (conical)
            if (!isDead || Random.value > 0.4f)
            {
                int layerCount = Random.Range(4, 7);
                for (int i = 0; i < layerCount; i++)
                {
                    float layerHeight = treeHeight * (0.35f + i * 0.1f);
                    float layerSize = (layerCount - i) * 0.8f + Random.Range(-0.2f, 0.2f);
                    
                    GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    layer.name = $"PineLayer_{i}";
                    layer.transform.SetParent(tree.transform);
                    layer.transform.localPosition = Vector3.up * layerHeight;
                    layer.transform.localScale = new Vector3(layerSize, layerSize * 0.4f, layerSize);
                    
                    Material pineMat = new Material(pineMaterial);
                    pineMat.SetColor("_Color", new Color(
                        0.1f + Random.Range(-0.02f, 0.02f),
                        0.2f + Random.Range(-0.03f, 0.03f) + (isDead ? -0.08f : 0f),
                        0.08f + Random.Range(-0.02f, 0.02f)
                    ));
                    layer.GetComponent<Renderer>().material = pineMat;
                    Destroy(layer.GetComponent<Collider>());
                }
            }
        }
        
        private static void CreateBirchTree(Transform parent, Vector3 position, bool isDead)
        {
            GameObject tree = new GameObject("BirchTree");
            tree.transform.SetParent(parent);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float treeHeight = Random.Range(7f, 11f);
            float trunkRadius = 0.12f + Random.Range(0f, 0.08f);
            
            // White/pale trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = Vector3.up * (treeHeight * 0.4f);
            trunk.transform.localScale = new Vector3(trunkRadius, treeHeight * 0.4f, trunkRadius);
            trunk.transform.localRotation = Quaternion.Euler(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            
            Material birchMat = new Material(URPMaterialHelper.GetLitShader());
            birchMat.SetColor("_Color", new Color(0.85f, 0.82f, 0.75f));
            birchMat.SetFloat("_Glossiness", 0.1f);
            trunk.GetComponent<Renderer>().material = birchMat;
            
            // Dark bark marks
            int markCount = Random.Range(4, 8);
            for (int i = 0; i < markCount; i++)
            {
                GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = "BarkMark";
                mark.transform.SetParent(trunk.transform);
                mark.transform.localPosition = new Vector3(
                    0.52f,
                    Random.Range(-0.8f, 0.8f),
                    0
                );
                mark.transform.localScale = new Vector3(0.02f, Random.Range(0.05f, 0.15f), Random.Range(0.3f, 0.6f));
                mark.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                Material markMat = new Material(URPMaterialHelper.GetLitShader());
                markMat.SetColor("_Color", new Color(0.15f, 0.12f, 0.1f));
                mark.GetComponent<Renderer>().material = markMat;
                Destroy(mark.GetComponent<Collider>());
            }
            
            // Delicate foliage
            if (!isDead || Random.value > 0.2f)
            {
                CreateBirchFoliage(tree.transform, treeHeight, isDead);
            }
        }
        
        private static void CreateDeadTree(Transform parent, Vector3 position)
        {
            GameObject tree = new GameObject("DeadTree");
            tree.transform.SetParent(parent);
            tree.transform.position = position;
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), Random.Range(-5f, 5f));
            
            float treeHeight = Random.Range(4f, 9f);
            float trunkRadius = Random.Range(0.15f, 0.3f);
            
            // Weathered gray trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "DeadTrunk";
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = Vector3.up * (treeHeight * 0.4f);
            trunk.transform.localScale = new Vector3(trunkRadius, treeHeight * 0.4f, trunkRadius);
            trunk.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
            
            Material deadMat = new Material(URPMaterialHelper.GetLitShader());
            float grayVal = Random.Range(0.35f, 0.5f);
            deadMat.SetColor("_Color", new Color(grayVal, grayVal * 0.95f, grayVal * 0.9f));
            deadMat.SetFloat("_Glossiness", 0.05f);
            trunk.GetComponent<Renderer>().material = deadMat;
            
            // Broken branches
            int branchCount = Random.Range(2, 5);
            for (int i = 0; i < branchCount; i++)
            {
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.name = "DeadBranch";
                branch.transform.SetParent(tree.transform);
                
                float branchHeight = treeHeight * Random.Range(0.3f, 0.8f);
                branch.transform.localPosition = Vector3.up * branchHeight;
                
                float branchLength = Random.Range(0.5f, 2f);
                branch.transform.localScale = new Vector3(0.05f, branchLength / 2f, 0.05f);
                branch.transform.localRotation = Quaternion.Euler(
                    Random.Range(50f, 80f),
                    Random.Range(0f, 360f),
                    0
                );
                
                Material branchMat = new Material(deadMat);
                branchMat.SetColor("_Color", new Color(grayVal * 0.9f, grayVal * 0.85f, grayVal * 0.8f));
                branch.GetComponent<Renderer>().material = branchMat;
                Destroy(branch.GetComponent<Collider>());
            }
        }
        
        private static void CreateRootFlare(Transform tree, float trunkRadius)
        {
            int rootCount = Random.Range(3, 6);
            
            for (int i = 0; i < rootCount; i++)
            {
                GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
                root.name = "Root";
                root.transform.SetParent(tree);
                
                float angle = (360f / rootCount) * i + Random.Range(-15f, 15f);
                root.transform.localPosition = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * trunkRadius * 0.8f,
                    0.1f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * trunkRadius * 0.8f
                );
                root.transform.localScale = new Vector3(
                    trunkRadius * 0.8f,
                    0.15f,
                    Random.Range(0.3f, 0.8f)
                );
                root.transform.localRotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    angle,
                    Random.Range(-5f, 5f)
                );
                
                root.GetComponent<Renderer>().material = barkMaterial;
                Destroy(root.GetComponent<Collider>());
            }
        }
        
        private static void CreateBranch(Transform tree, float height, float angle, float trunkRadius, Material trunkMat)
        {
            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = "Branch";
            branch.transform.SetParent(tree);
            
            float branchLength = Random.Range(1f, 3f);
            branch.transform.localPosition = Vector3.up * height;
            branch.transform.localScale = new Vector3(trunkRadius * 0.4f, branchLength / 2f, trunkRadius * 0.4f);
            branch.transform.localRotation = Quaternion.Euler(
                Random.Range(40f, 70f),
                angle,
                0
            );
            
            branch.GetComponent<Renderer>().material = trunkMat;
            Destroy(branch.GetComponent<Collider>());
        }
        
        private static void CreateFoliageCanopy(Transform tree, float treeHeight, bool isDead)
        {
            int clusterCount = Random.Range(4, 8);
            
            for (int i = 0; i < clusterCount; i++)
            {
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.name = "Foliage";
                foliage.transform.SetParent(tree);
                
                float clusterSize = Random.Range(1.5f, 3.5f);
                foliage.transform.localPosition = new Vector3(
                    Random.Range(-2f, 2f),
                    treeHeight * Random.Range(0.6f, 0.95f),
                    Random.Range(-2f, 2f)
                );
                foliage.transform.localScale = new Vector3(
                    clusterSize * Random.Range(0.8f, 1.2f),
                    clusterSize * Random.Range(0.6f, 1f),
                    clusterSize * Random.Range(0.8f, 1.2f)
                );
                
                Material foliageMat;
                if (isDead)
                {
                    foliageMat = new Material(deadLeafMaterial);
                    foliageMat.SetColor("_Color", new Color(
                        Random.Range(0.35f, 0.5f),
                        Random.Range(0.25f, 0.35f),
                        Random.Range(0.1f, 0.2f)
                    ));
                }
                else
                {
                    foliageMat = new Material(leafMaterial);
                    foliageMat.SetColor("_Color", new Color(
                        0.15f + Random.Range(-0.03f, 0.03f),
                        0.3f + Random.Range(-0.05f, 0.05f),
                        0.1f + Random.Range(-0.02f, 0.02f)
                    ));
                }
                
                foliage.GetComponent<Renderer>().material = foliageMat;
                Destroy(foliage.GetComponent<Collider>());
            }
        }
        
        private static void CreateBirchFoliage(Transform tree, float treeHeight, bool isDead)
        {
            int clusterCount = Random.Range(5, 10);
            
            for (int i = 0; i < clusterCount; i++)
            {
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.name = "BirchFoliage";
                foliage.transform.SetParent(tree);
                
                float clusterSize = Random.Range(0.8f, 1.8f);
                foliage.transform.localPosition = new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    treeHeight * Random.Range(0.55f, 0.95f),
                    Random.Range(-1.5f, 1.5f)
                );
                foliage.transform.localScale = Vector3.one * clusterSize;
                
                Material foliageMat = new Material(leafMaterial);
                if (isDead)
                {
                    foliageMat.SetColor("_Color", new Color(0.6f, 0.5f, 0.2f));
                }
                else
                {
                    foliageMat.SetColor("_Color", new Color(
                        0.2f + Random.Range(-0.03f, 0.03f),
                        0.4f + Random.Range(-0.05f, 0.05f),
                        0.15f + Random.Range(-0.02f, 0.02f)
                    ));
                }
                
                foliage.GetComponent<Renderer>().material = foliageMat;
                Destroy(foliage.GetComponent<Collider>());
            }
        }
        
        private static Material CreateVariedBarkMaterial()
        {
            Material mat = new Material(barkMaterial);
            Color baseColor = mat.GetColor("_Color");
            mat.SetColor("_Color", new Color(
                baseColor.r + Random.Range(-0.05f, 0.05f),
                baseColor.g + Random.Range(-0.04f, 0.04f),
                baseColor.b + Random.Range(-0.03f, 0.03f)
            ));
            return mat;
        }
        
        private static void CreateUnderbrush(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList, bool isPostApocalyptic)
        {
            int bushCount = Mathf.RoundToInt((mapSize + borderWidth) * 1.5f);
            
            for (int i = 0; i < bushCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = mapSize + Random.Range(1f, borderWidth);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                float type = Random.value;
                if (type < 0.5f)
                {
                    CreateBush(parent, pos, isPostApocalyptic);
                }
                else if (type < 0.75f)
                {
                    CreateFern(parent, pos, isPostApocalyptic);
                }
                else
                {
                    CreateTallGrass(parent, pos, isPostApocalyptic);
                }
            }
        }
        
        private static void CreateBush(Transform parent, Vector3 position, bool isDead)
        {
            GameObject bush = new GameObject("Bush");
            bush.transform.SetParent(parent);
            bush.transform.position = position;
            bush.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            int clusterCount = Random.Range(3, 6);
            float bushSize = Random.Range(0.5f, 1.2f);
            
            for (int i = 0; i < clusterCount; i++)
            {
                GameObject cluster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cluster.name = "BushCluster";
                cluster.transform.SetParent(bush.transform);
                cluster.transform.localPosition = new Vector3(
                    Random.Range(-0.4f, 0.4f),
                    bushSize * 0.5f + Random.Range(-0.2f, 0.3f),
                    Random.Range(-0.4f, 0.4f)
                );
                
                float clusterSize = bushSize * Random.Range(0.5f, 1f);
                cluster.transform.localScale = new Vector3(
                    clusterSize * Random.Range(0.8f, 1.2f),
                    clusterSize * Random.Range(0.6f, 1f),
                    clusterSize * Random.Range(0.8f, 1.2f)
                );
                
                Material bushMat = new Material(bushMaterial);
                if (isDead)
                {
                    bushMat.SetColor("_Color", new Color(
                        Random.Range(0.28f, 0.4f),
                        Random.Range(0.25f, 0.35f),
                        Random.Range(0.15f, 0.22f)
                    ));
                }
                else
                {
                    bushMat.SetColor("_Color", new Color(
                        0.12f + Random.Range(-0.02f, 0.02f),
                        0.25f + Random.Range(-0.04f, 0.04f),
                        0.08f + Random.Range(-0.02f, 0.02f)
                    ));
                }
                
                cluster.GetComponent<Renderer>().material = bushMat;
                Destroy(cluster.GetComponent<Collider>());
            }
        }
        
        private static void CreateFern(Transform parent, Vector3 position, bool isDead)
        {
            GameObject fern = new GameObject("Fern");
            fern.transform.SetParent(parent);
            fern.transform.position = position;
            fern.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            int frondCount = Random.Range(5, 9);
            
            for (int i = 0; i < frondCount; i++)
            {
                GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frond.name = "Frond";
                frond.transform.SetParent(fern.transform);
                
                float angle = (360f / frondCount) * i + Random.Range(-10f, 10f);
                float length = Random.Range(0.4f, 0.8f);
                
                frond.transform.localPosition = new Vector3(0, 0.15f, 0);
                frond.transform.localScale = new Vector3(0.15f, 0.02f, length);
                frond.transform.localRotation = Quaternion.Euler(
                    Random.Range(30f, 60f),
                    angle,
                    0
                );
                
                Material fernMat = new Material(fernMaterial);
                if (isDead)
                {
                    fernMat.SetColor("_Color", new Color(0.35f, 0.3f, 0.18f));
                }
                
                frond.GetComponent<Renderer>().material = fernMat;
                Destroy(frond.GetComponent<Collider>());
            }
        }
        
        private static void CreateTallGrass(Transform parent, Vector3 position, bool isDead)
        {
            int bladeCount = Random.Range(8, 15);
            
            for (int i = 0; i < bladeCount; i++)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "TallGrass";
                blade.transform.SetParent(parent);
                
                float height = Random.Range(0.3f, 0.7f);
                blade.transform.position = position + new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    height / 2f,
                    Random.Range(-0.2f, 0.2f)
                );
                blade.transform.localScale = new Vector3(0.02f, height, 0.01f);
                blade.transform.rotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );
                
                Material grassMat = new Material(URPMaterialHelper.GetLitShader());
                if (isDead)
                {
                    grassMat.SetColor("_Color", new Color(
                        Random.Range(0.4f, 0.55f),
                        Random.Range(0.35f, 0.45f),
                        Random.Range(0.2f, 0.3f)
                    ));
                }
                else
                {
                    grassMat.SetColor("_Color", new Color(
                        Random.Range(0.2f, 0.3f),
                        Random.Range(0.4f, 0.55f),
                        Random.Range(0.15f, 0.25f)
                    ));
                }
                grassMat.SetFloat("_Glossiness", 0.1f);
                blade.GetComponent<Renderer>().material = grassMat;
                Destroy(blade.GetComponent<Collider>());
            }
        }
        
        private static void CreateFallenTrees(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList)
        {
            int logCount = Random.Range(5, 12);
            
            for (int i = 0; i < logCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = mapSize + Random.Range(5f, borderWidth - 3f);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                CreateFallenLog(parent, pos);
            }
        }
        
        private static void CreateFallenLog(Transform parent, Vector3 position)
        {
            GameObject log = new GameObject("FallenLog");
            log.transform.SetParent(parent);
            log.transform.position = position;
            log.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            float logLength = Random.Range(3f, 8f);
            float logRadius = Random.Range(0.2f, 0.5f);
            
            // Main log
            GameObject mainLog = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainLog.name = "Log";
            mainLog.transform.SetParent(log.transform);
            mainLog.transform.localPosition = Vector3.up * logRadius;
            mainLog.transform.localScale = new Vector3(logRadius * 2f, logLength / 2f, logRadius * 2f);
            mainLog.transform.localRotation = Quaternion.Euler(0, 0, 90);
            
            Material logMat = new Material(logMaterial);
            logMat.SetColor("_Color", new Color(
                0.25f + Random.Range(-0.05f, 0.05f),
                0.18f + Random.Range(-0.03f, 0.03f),
                0.12f + Random.Range(-0.02f, 0.02f)
            ));
            mainLog.GetComponent<Renderer>().material = logMat;
            
            // Broken end
            GameObject brokenEnd = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brokenEnd.name = "BrokenEnd";
            brokenEnd.transform.SetParent(log.transform);
            brokenEnd.transform.localPosition = new Vector3(logLength / 2f, logRadius, 0);
            brokenEnd.transform.localScale = new Vector3(0.1f, logRadius * 2f, logRadius * 2f);
            brokenEnd.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
            
            Material endMat = new Material(URPMaterialHelper.GetLitShader());
            endMat.SetColor("_Color", new Color(0.5f, 0.4f, 0.3f));
            brokenEnd.GetComponent<Renderer>().material = endMat;
            Destroy(brokenEnd.GetComponent<Collider>());
            
            // Moss on log
            if (Random.value > 0.4f)
            {
                int mossCount = Random.Range(2, 5);
                for (int i = 0; i < mossCount; i++)
                {
                    GameObject moss = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    moss.name = "LogMoss";
                    moss.transform.SetParent(log.transform);
                    moss.transform.localPosition = new Vector3(
                        Random.Range(-logLength / 3f, logLength / 3f),
                        logRadius * 2f,
                        Random.Range(-logRadius * 0.5f, logRadius * 0.5f)
                    );
                    moss.transform.localScale = new Vector3(
                        Random.Range(0.3f, 0.8f),
                        0.02f,
                        Random.Range(0.2f, 0.5f)
                    );
                    
                    Material mossMat = new Material(URPMaterialHelper.GetLitShader());
                    mossMat.SetColor("_Color", new Color(0.12f, 0.22f, 0.08f));
                    moss.GetComponent<Renderer>().material = mossMat;
                    Destroy(moss.GetComponent<Collider>());
                }
            }
            
            // Mushrooms on log
            if (Random.value > 0.5f)
            {
                CreateMushroomCluster(log.transform, new Vector3(
                    Random.Range(-logLength / 4f, logLength / 4f),
                    logRadius,
                    logRadius
                ));
            }
        }
        
        private static void CreateMushroomCluster(Transform parent, Vector3 position)
        {
            int shroomCount = Random.Range(3, 8);
            
            for (int i = 0; i < shroomCount; i++)
            {
                GameObject shroom = new GameObject("Mushroom");
                shroom.transform.SetParent(parent);
                shroom.transform.localPosition = position + new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    0,
                    Random.Range(-0.1f, 0.1f)
                );
                
                // Stem
                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "Stem";
                stem.transform.SetParent(shroom.transform);
                float stemHeight = Random.Range(0.04f, 0.1f);
                stem.transform.localPosition = Vector3.up * (stemHeight / 2f);
                stem.transform.localScale = new Vector3(0.02f, stemHeight / 2f, 0.02f);
                
                Material stemMat = new Material(URPMaterialHelper.GetLitShader());
                stemMat.SetColor("_Color", new Color(0.9f, 0.88f, 0.8f));
                stem.GetComponent<Renderer>().material = stemMat;
                Destroy(stem.GetComponent<Collider>());
                
                // Cap
                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cap.name = "Cap";
                cap.transform.SetParent(shroom.transform);
                cap.transform.localPosition = Vector3.up * stemHeight;
                float capSize = Random.Range(0.03f, 0.06f);
                cap.transform.localScale = new Vector3(capSize, capSize * 0.5f, capSize);
                
                Material capMat = new Material(URPMaterialHelper.GetLitShader());
                float type = Random.value;
                if (type < 0.5f)
                {
                    capMat.SetColor("_Color", new Color(0.6f, 0.4f, 0.25f)); // Brown
                }
                else
                {
                    capMat.SetColor("_Color", new Color(0.7f, 0.15f, 0.1f)); // Red
                }
                cap.GetComponent<Renderer>().material = capMat;
                Destroy(cap.GetComponent<Collider>());
            }
        }
        
        private static void CreateForestRocks(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList)
        {
            int rockCount = Random.Range(20, 40);
            
            for (int i = 0; i < rockCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = mapSize + Random.Range(2f, borderWidth);
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                float rockSize = Random.Range(0.2f, 1f);
                
                GameObject rock = GameObject.CreatePrimitive(Random.value > 0.4f ? PrimitiveType.Cube : PrimitiveType.Sphere);
                rock.name = "ForestRock";
                rock.transform.SetParent(parent);
                rock.transform.position = pos + Vector3.up * (rockSize * 0.3f);
                rock.transform.localScale = new Vector3(
                    rockSize * Random.Range(0.7f, 1.3f),
                    rockSize * Random.Range(0.4f, 0.9f),
                    rockSize * Random.Range(0.7f, 1.3f)
                );
                rock.transform.rotation = Quaternion.Euler(
                    Random.Range(-15f, 15f),
                    Random.Range(0f, 360f),
                    Random.Range(-15f, 15f)
                );
                
                Material rockMat = new Material(URPMaterialHelper.GetLitShader());
                float grayVal = Random.Range(0.35f, 0.55f);
                rockMat.SetColor("_Color", new Color(
                    grayVal + Random.Range(-0.05f, 0.05f),
                    grayVal * 0.95f + Random.Range(-0.05f, 0.05f),
                    grayVal * 0.9f + Random.Range(-0.05f, 0.05f)
                ));
                rockMat.SetFloat("_Glossiness", Random.Range(0.1f, 0.3f));
                rock.GetComponent<Renderer>().material = rockMat;
            }
        }
        
        private static void CreateForestEdge(Transform parent, float mapSize, List<GameObject> objectList, bool isPostApocalyptic)
        {
            // Create transition zone at forest edge
            int edgeDetailCount = Mathf.RoundToInt(mapSize * 3f);
            
            for (int i = 0; i < edgeDetailCount; i++)
            {
                float angle = (i / (float)edgeDetailCount) * 360f * Mathf.Deg2Rad;
                float noise = Mathf.PerlinNoise(angle * 5f, 0) * 3f;
                float distance = mapSize + noise;
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                // Grass tufts at edge
                CreateTallGrass(parent, pos, isPostApocalyptic);
                
                // Occasional small bush
                if (Random.value > 0.85f)
                {
                    CreateBush(parent, pos + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), isPostApocalyptic);
                }
            }
        }
        
        private static void CreateForestAtmosphere(Transform parent, float mapSize, float borderWidth, List<GameObject> objectList)
        {
            // Skip transparent effects - URP handles fog differently
            // The Unity fog settings provide atmosphere instead
        }
    }
}

