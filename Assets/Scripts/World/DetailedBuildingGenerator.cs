using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Creates detailed post-apocalyptic buildings with proper windows, doors, damage, and grime
    /// </summary>
    public class DetailedBuildingGenerator : MonoBehaviour
    {
        private static Material brickMaterial;
        private static Material windowMaterial;
        private static Material doorMaterial;
        private static Material roofMaterial;
        private static Material woodMaterial;
        private static Material metalMaterial;
        private static Material grimeMaterial;
        
        public static void InitializeMaterials()
        {
            // Weathered brick - muted reddish-brown
            brickMaterial = URPMaterialHelper.CreateMaterial(new Color(0.42f, 0.35f, 0.3f), 0.1f, 0f);
            
            // Dark broken windows - very dark with slight blue tint
            windowMaterial = URPMaterialHelper.CreateMaterial(new Color(0.05f, 0.06f, 0.08f), 0.6f, 0.1f);
            
            // Old wooden door - weathered brown
            doorMaterial = URPMaterialHelper.CreateMaterial(new Color(0.3f, 0.24f, 0.18f), 0.12f, 0f);
            
            // Damaged roof - dark gray-brown
            roofMaterial = URPMaterialHelper.CreateMaterial(new Color(0.25f, 0.22f, 0.2f), 0.15f, 0.05f);
            
            // Old wood - gray-brown weathered
            woodMaterial = URPMaterialHelper.CreateMaterial(new Color(0.35f, 0.3f, 0.25f), 0.08f, 0f);
            
            // Rusty metal - dark rust color
            metalMaterial = URPMaterialHelper.CreateMaterial(new Color(0.35f, 0.25f, 0.18f), 0.3f, 0.5f);
            
            // Grime/dirt overlay - very dark
            grimeMaterial = URPMaterialHelper.CreateMaterial(new Color(0.15f, 0.13f, 0.11f), 0.02f, 0f);
        }
        
        public static GameObject CreateDetailedHouse(Vector3 position, List<GameObject> objectList)
        {
            if (brickMaterial == null) InitializeMaterials();
            
            GameObject house = new GameObject("DetailedHouse");
            house.transform.position = position;
            
            float width = Random.Range(7f, 10f);
            float height = Random.Range(3.5f, 4.5f);
            float depth = Random.Range(8f, 11f);
            
            // Main walls with brick texture
            CreateWallWithDetails(house.transform, Vector3.up * height/2, new Vector3(width, height, depth), objectList);
            
            // Roof with damage
            CreateDamagedRoof(house.transform, new Vector3(0, height, 0), width, depth, objectList);
            
            // Front door (possibly broken/open)
            CreateDoor(house.transform, new Vector3(0, 1f, depth/2 + 0.05f), objectList);
            
            // Windows with frames and some broken
            CreateWindowRow(house.transform, new Vector3(0, 2.2f, depth/2 + 0.05f), width, 2, objectList);
            CreateWindowRow(house.transform, new Vector3(0, 2.2f, -depth/2 - 0.05f), width, 2, objectList);
            CreateWindowRow(house.transform, new Vector3(width/2 + 0.05f, 2.2f, 0), depth, 2, objectList);
            CreateWindowRow(house.transform, new Vector3(-width/2 - 0.05f, 2.2f, 0), depth, 2, objectList);
            
            // Porch/steps
            CreatePorch(house.transform, new Vector3(0, 0, depth/2 + 1f), width, objectList);
            
            // Chimney
            if (Random.value > 0.4f)
            {
                CreateChimney(house.transform, new Vector3(width/4, height + 1f, 0), objectList);
            }
            
            // Add grime and damage details
            AddGrimePatches(house.transform, width, height, depth, objectList);
            
            // Overgrown vegetation
            AddOvergrowth(house.transform, width, depth, objectList);
            
            objectList.Add(house);
            return house;
        }
        
        private static void CreateWallWithDetails(Transform parent, Vector3 pos, Vector3 size, List<GameObject> objectList)
        {
            // Main structure
            GameObject walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls.name = "Walls";
            walls.transform.SetParent(parent);
            walls.transform.localPosition = pos;
            walls.transform.localScale = size;
            walls.GetComponent<Renderer>().material = brickMaterial;
            
            // Add trim at base
            GameObject baseTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseTrim.name = "BaseTrim";
            baseTrim.transform.SetParent(parent);
            baseTrim.transform.localPosition = new Vector3(0, 0.15f, 0);
            baseTrim.transform.localScale = new Vector3(size.x + 0.1f, 0.3f, size.z + 0.1f);
            
            Material trimMat = new Material(URPMaterialHelper.GetLitShader());
            trimMat.SetColor("_Color", new Color(0.35f, 0.32f, 0.3f));
            baseTrim.GetComponent<Renderer>().material = trimMat;
            
            // Corner details
            float cornerSize = 0.15f;
            Vector3[] corners = {
                new Vector3(size.x/2, size.y/2, size.z/2),
                new Vector3(-size.x/2, size.y/2, size.z/2),
                new Vector3(size.x/2, size.y/2, -size.z/2),
                new Vector3(-size.x/2, size.y/2, -size.z/2)
            };
            
            foreach (Vector3 corner in corners)
            {
                GameObject cornerPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cornerPiece.name = "Corner";
                cornerPiece.transform.SetParent(parent);
                cornerPiece.transform.localPosition = corner;
                cornerPiece.transform.localScale = new Vector3(cornerSize, size.y, cornerSize);
                
                Material cornerMat = new Material(URPMaterialHelper.GetLitShader());
                cornerMat.SetColor("_Color", new Color(0.38f, 0.28f, 0.24f));
                cornerPiece.GetComponent<Renderer>().material = cornerMat;
                Destroy(cornerPiece.GetComponent<Collider>());
            }
        }
        
        private static void CreateDamagedRoof(Transform parent, Vector3 pos, float width, float depth, List<GameObject> objectList)
        {
            // Main roof
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(parent);
            roof.transform.localPosition = pos + Vector3.up * 0.8f;
            roof.transform.localScale = new Vector3(width + 0.5f, 0.15f, depth + 0.5f);
            roof.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
            roof.GetComponent<Renderer>().material = roofMaterial;
            
            // Roof peak (triangular effect with cubes)
            GameObject roofPeak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofPeak.name = "RoofPeak";
            roofPeak.transform.SetParent(parent);
            roofPeak.transform.localPosition = pos + Vector3.up * 1.3f;
            roofPeak.transform.localScale = new Vector3(width * 0.7f, 0.8f, depth * 0.5f);
            roofPeak.transform.rotation = Quaternion.Euler(0, 0, 45);
            roofPeak.GetComponent<Renderer>().material = roofMaterial;
            
            // Damaged holes in roof
            if (Random.value > 0.5f)
            {
                GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hole.name = "RoofDamage";
                hole.transform.SetParent(parent);
                hole.transform.localPosition = pos + new Vector3(Random.Range(-width/4, width/4), 0.9f, Random.Range(-depth/4, depth/4));
                hole.transform.localScale = new Vector3(Random.Range(0.8f, 1.5f), 0.2f, Random.Range(0.8f, 1.5f));
                hole.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0, 360f), Random.Range(-10f, 10f));
                
                Material darkMat = new Material(URPMaterialHelper.GetLitShader());
                darkMat.SetColor("_Color", new Color(0.05f, 0.05f, 0.05f));
                hole.GetComponent<Renderer>().material = darkMat;
                Destroy(hole.GetComponent<Collider>());
            }
            
            // Gutter
            GameObject gutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gutter.name = "Gutter";
            gutter.transform.SetParent(parent);
            gutter.transform.localPosition = pos + new Vector3(0, 0.1f, depth/2 + 0.3f);
            gutter.transform.localScale = new Vector3(width + 0.4f, 0.1f, 0.15f);
            gutter.GetComponent<Renderer>().material = metalMaterial;
            Destroy(gutter.GetComponent<Collider>());
        }
        
        private static void CreateDoor(Transform parent, Vector3 pos, List<GameObject> objectList)
        {
            // Door frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "DoorFrame";
            frame.transform.SetParent(parent);
            frame.transform.localPosition = pos;
            frame.transform.localScale = new Vector3(1.2f, 2.2f, 0.15f);
            frame.GetComponent<Renderer>().material = woodMaterial;
            Destroy(frame.GetComponent<Collider>());
            
            // Door (possibly ajar)
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(parent);
            
            bool isOpen = Random.value > 0.4f;
            if (isOpen)
            {
                door.transform.localPosition = pos + new Vector3(0.5f, 0, 0.3f);
                door.transform.localRotation = Quaternion.Euler(0, Random.Range(30f, 70f), 0);
            }
            else
            {
                door.transform.localPosition = pos + new Vector3(0, 0, 0.05f);
            }
            door.transform.localScale = new Vector3(0.9f, 2f, 0.08f);
            door.GetComponent<Renderer>().material = doorMaterial;
            
            // Door handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "DoorHandle";
            handle.transform.SetParent(door.transform);
            handle.transform.localPosition = new Vector3(0.35f, 0, 0.1f);
            handle.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            handle.GetComponent<Renderer>().material = metalMaterial;
            Destroy(handle.GetComponent<Collider>());
            
            // Step
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "DoorStep";
            step.transform.SetParent(parent);
            step.transform.localPosition = pos + new Vector3(0, -1f, 0.3f);
            step.transform.localScale = new Vector3(1.4f, 0.15f, 0.5f);
            
            Material stepMat = new Material(URPMaterialHelper.GetLitShader());
            stepMat.SetColor("_Color", new Color(0.4f, 0.38f, 0.35f));
            step.GetComponent<Renderer>().material = stepMat;
        }
        
        private static void CreateWindowRow(Transform parent, Vector3 basePos, float length, int count, List<GameObject> objectList)
        {
            float spacing = length / (count + 1);
            bool isZFacing = Mathf.Abs(basePos.z) > Mathf.Abs(basePos.x);
            
            for (int i = 0; i < count; i++)
            {
                Vector3 offset;
                if (isZFacing)
                    offset = new Vector3(-length/2 + spacing * (i + 1), 0, 0);
                else
                    offset = new Vector3(0, 0, -length/2 + spacing * (i + 1));
                
                CreateDetailedWindow(parent, basePos + offset, isZFacing, objectList);
            }
        }
        
        private static void CreateDetailedWindow(Transform parent, Vector3 pos, bool zFacing, List<GameObject> objectList)
        {
            Vector3 frameScale = zFacing ? new Vector3(1.2f, 1.5f, 0.15f) : new Vector3(0.15f, 1.5f, 1.2f);
            Vector3 glassScale = zFacing ? new Vector3(1f, 1.3f, 0.05f) : new Vector3(0.05f, 1.3f, 1f);
            
            // Window frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "WindowFrame";
            frame.transform.SetParent(parent);
            frame.transform.localPosition = pos;
            frame.transform.localScale = frameScale;
            frame.GetComponent<Renderer>().material = woodMaterial;
            Destroy(frame.GetComponent<Collider>());
            
            // Window glass (dark)
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "WindowGlass";
            glass.transform.SetParent(parent);
            glass.transform.localPosition = pos;
            glass.transform.localScale = glassScale;
            glass.GetComponent<Renderer>().material = windowMaterial;
            Destroy(glass.GetComponent<Collider>());
            
            // Cross bars
            GameObject hBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBar.name = "WindowBar";
            hBar.transform.SetParent(parent);
            hBar.transform.localPosition = pos;
            hBar.transform.localScale = zFacing ? new Vector3(1f, 0.06f, 0.08f) : new Vector3(0.08f, 0.06f, 1f);
            hBar.GetComponent<Renderer>().material = woodMaterial;
            Destroy(hBar.GetComponent<Collider>());
            
            GameObject vBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vBar.name = "WindowBar";
            vBar.transform.SetParent(parent);
            vBar.transform.localPosition = pos;
            vBar.transform.localScale = zFacing ? new Vector3(0.06f, 1.3f, 0.08f) : new Vector3(0.08f, 1.3f, 0.06f);
            vBar.GetComponent<Renderer>().material = woodMaterial;
            Destroy(vBar.GetComponent<Collider>());
            
            // Boards on some windows
            if (Random.value > 0.5f)
            {
                int boardCount = Random.Range(1, 4);
                for (int i = 0; i < boardCount; i++)
                {
                    GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    board.name = "Board";
                    board.transform.SetParent(parent);
                    
                    float boardOffset = -0.5f + i * 0.5f;
                    Vector3 boardPos = pos + (zFacing ? new Vector3(0, boardOffset, 0.1f) : new Vector3(0.1f, boardOffset, 0));
                    board.transform.localPosition = boardPos;
                    board.transform.localScale = zFacing ? new Vector3(1.4f, 0.12f, 0.04f) : new Vector3(0.04f, 0.12f, 1.4f);
                    board.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
                    board.GetComponent<Renderer>().material = woodMaterial;
                    Destroy(board.GetComponent<Collider>());
                }
            }
        }
        
        private static void CreatePorch(Transform parent, Vector3 pos, float width, List<GameObject> objectList)
        {
            // Porch floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "PorchFloor";
            floor.transform.SetParent(parent);
            floor.transform.localPosition = pos + new Vector3(0, 0.1f, 0);
            floor.transform.localScale = new Vector3(width * 0.8f, 0.15f, 1.5f);
            floor.GetComponent<Renderer>().material = woodMaterial;
            
            // Porch posts
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "PorchPost";
                post.transform.SetParent(parent);
                post.transform.localPosition = pos + new Vector3(i * width * 0.35f, 1.2f, 0.6f);
                post.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
                post.GetComponent<Renderer>().material = woodMaterial;
            }
            
            // Porch roof
            GameObject porchRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            porchRoof.name = "PorchRoof";
            porchRoof.transform.SetParent(parent);
            porchRoof.transform.localPosition = pos + new Vector3(0, 2.3f, 0.3f);
            porchRoof.transform.localScale = new Vector3(width * 0.85f, 0.1f, 1.8f);
            porchRoof.transform.rotation = Quaternion.Euler(5f, 0, 0);
            porchRoof.GetComponent<Renderer>().material = roofMaterial;
        }
        
        private static void CreateChimney(Transform parent, Vector3 pos, List<GameObject> objectList)
        {
            GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chimney.name = "Chimney";
            chimney.transform.SetParent(parent);
            chimney.transform.localPosition = pos;
            chimney.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            chimney.GetComponent<Renderer>().material = brickMaterial;
            
            // Chimney cap
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "ChimneyCap";
            cap.transform.SetParent(parent);
            cap.transform.localPosition = pos + Vector3.up * 1.1f;
            cap.transform.localScale = new Vector3(0.9f, 0.15f, 0.9f);
            cap.GetComponent<Renderer>().material = metalMaterial;
            Destroy(cap.GetComponent<Collider>());
        }
        
        private static void AddGrimePatches(Transform parent, float width, float height, float depth, List<GameObject> objectList)
        {
            int patchCount = Random.Range(3, 8);
            
            for (int i = 0; i < patchCount; i++)
            {
                GameObject grime = GameObject.CreatePrimitive(PrimitiveType.Quad);
                grime.name = "Grime";
                grime.transform.SetParent(parent);
                
                // Place on random wall
                int wall = Random.Range(0, 4);
                Vector3 grimePos;
                Quaternion grimeRot;
                
                switch (wall)
                {
                    case 0: // Front
                        grimePos = new Vector3(Random.Range(-width/3, width/3), Random.Range(0.5f, height-0.5f), depth/2 + 0.02f);
                        grimeRot = Quaternion.identity;
                        break;
                    case 1: // Back
                        grimePos = new Vector3(Random.Range(-width/3, width/3), Random.Range(0.5f, height-0.5f), -depth/2 - 0.02f);
                        grimeRot = Quaternion.Euler(0, 180, 0);
                        break;
                    case 2: // Left
                        grimePos = new Vector3(-width/2 - 0.02f, Random.Range(0.5f, height-0.5f), Random.Range(-depth/3, depth/3));
                        grimeRot = Quaternion.Euler(0, -90, 0);
                        break;
                    default: // Right
                        grimePos = new Vector3(width/2 + 0.02f, Random.Range(0.5f, height-0.5f), Random.Range(-depth/3, depth/3));
                        grimeRot = Quaternion.Euler(0, 90, 0);
                        break;
                }
                
                grime.transform.localPosition = grimePos;
                grime.transform.localRotation = grimeRot;
                grime.transform.localScale = new Vector3(Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), 1f);
                grime.GetComponent<Renderer>().material = grimeMaterial;
                Destroy(grime.GetComponent<Collider>());
            }
        }
        
        private static void AddOvergrowth(Transform parent, float width, float depth, List<GameObject> objectList)
        {
            // Weeds and grass around base
            int grassCount = Random.Range(10, 20);
            
            for (int i = 0; i < grassCount; i++)
            {
                GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grass.name = "Grass";
                grass.transform.SetParent(parent);
                grass.transform.localPosition = new Vector3(
                    Random.Range(-width/2 - 1f, width/2 + 1f),
                    Random.Range(0.1f, 0.4f),
                    Random.Range(-depth/2 - 1f, depth/2 + 1f)
                );
                grass.transform.localScale = new Vector3(0.05f, Random.Range(0.2f, 0.6f), 0.05f);
                grass.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0, 360f), Random.Range(-10f, 10f));
                
                Material grassMat = new Material(URPMaterialHelper.GetLitShader());
                grassMat.SetColor("_Color", new Color(
                    Random.Range(0.2f, 0.35f),
                    Random.Range(0.3f, 0.45f),
                    Random.Range(0.15f, 0.25f)
                ));
                grass.GetComponent<Renderer>().material = grassMat;
                Destroy(grass.GetComponent<Collider>());
            }
            
            // Vines on walls (occasionally)
            if (Random.value > 0.6f)
            {
                int vineCount = Random.Range(3, 8);
                for (int i = 0; i < vineCount; i++)
                {
                    GameObject vine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    vine.name = "Vine";
                    vine.transform.SetParent(parent);
                    vine.transform.localPosition = new Vector3(
                        Random.Range(-width/3, width/3),
                        Random.Range(0.5f, 3f),
                        depth/2 + 0.03f
                    );
                    vine.transform.localScale = new Vector3(0.08f, Random.Range(0.5f, 2f), 0.02f);
                    
                    Material vineMat = new Material(URPMaterialHelper.GetLitShader());
                    vineMat.SetColor("_Color", new Color(0.15f, 0.25f, 0.1f));
                    vine.GetComponent<Renderer>().material = vineMat;
                    Destroy(vine.GetComponent<Collider>());
                }
            }
        }
    }
}

