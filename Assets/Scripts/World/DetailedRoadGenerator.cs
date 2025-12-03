using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.World
{
    /// <summary>
    /// Creates detailed road surfaces with cracks, potholes, debris, lane markings
    /// </summary>
    public class DetailedRoadGenerator : MonoBehaviour
    {
        public static void CreateDetailedRoad(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            // Multi-layer asphalt for realistic depth
            CreateAsphaltLayers(center, width, length, objectList);
            
            // Center line (faded yellow)
            CreateCenterLine(center, length, objectList);
            
            // Edge lines (faded white)
            CreateEdgeLines(center, width, length, objectList);
            
            // Potholes with depth
            CreatePotholes(center, width, length, 18, objectList);
            
            // Realistic crack network
            CreateCracks(center, width, length, 30, objectList);
            
            // Oil stains with rainbow sheen
            CreateOilStains(center, width, length, 12, objectList);
            
            // Tire marks (skid marks)
            CreateTireMarks(center, width, length, 6, objectList);
            
            // Rain puddles (wet spots)
            CreateRainPuddles(center, width, length, objectList);
            
            // Debris on road
            CreateRoadDebris(center, width, length, objectList);
            
            // Realistic curbs with gutters
            CreateCurbs(center, width, length, objectList);
            
            // Manhole covers
            CreateManholes(center, width, length, objectList);
            
            // Faded patches where asphalt was repaired
            CreateRepairPatches(center, width, length, objectList);
        }
        
        private static void CreateAsphaltLayers(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            // Base layer - dark asphalt
            GameObject baseLayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseLayer.name = "RoadBase";
            baseLayer.transform.position = center + Vector3.up * 0.01f;
            baseLayer.transform.localScale = new Vector3(width, 0.03f, length);
            
            Material baseMat = URPMaterialHelper.CreateMaterial(new Color(0.12f, 0.12f, 0.13f), 0.15f, 0f);
            baseLayer.GetComponent<Renderer>().material = baseMat;
            objectList.Add(baseLayer);
            
            // Main surface with texture variation
            int tileCount = 6;
            float tileWidth = width / tileCount;
            
            for (int i = 0; i < tileCount; i++)
            {
                float xPos = -width/2 + tileWidth/2 + i * tileWidth;
                
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "RoadSurface";
                tile.transform.position = center + new Vector3(xPos, 0.025f, 0);
                tile.transform.localScale = new Vector3(tileWidth + 0.05f, 0.02f, length);
                
                Material tileMat = new Material(URPMaterialHelper.GetLitShader());
                // Subtle color variation for realism
                float variation = Random.Range(-0.02f, 0.02f);
                tileMat.SetColor("_Color", new Color(
                    0.12f + variation,
                    0.12f + variation,
                    0.13f + variation
                ));
                tileMat.SetFloat("_Metallic", 0f);
                tileMat.SetFloat("_Glossiness", Random.Range(0.1f, 0.18f));
                tile.GetComponent<Renderer>().material = tileMat;
                objectList.Add(tile);
            }
            
            // Worn wheel tracks (slightly shinier from tire polish)
            for (int side = -1; side <= 1; side += 2)
            {
                float trackX = side * 1.5f;  // Typical wheel spacing
                
                GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
                track.name = "WheelTrack";
                track.transform.position = center + new Vector3(trackX, 0.027f, 0);
                track.transform.localScale = new Vector3(0.4f, 0.005f, length);
                
                Material trackMat = new Material(URPMaterialHelper.GetLitShader());
                trackMat.SetColor("_Color", new Color(0.1f, 0.1f, 0.11f));
                trackMat.SetFloat("_Glossiness", 0.25f);  // Slightly shinier
                track.GetComponent<Renderer>().material = trackMat;
                Destroy(track.GetComponent<Collider>());
                objectList.Add(track);
            }
        }
        
        private static void CreateRainPuddles(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            int puddleCount = Random.Range(5, 10);
            
            for (int i = 0; i < puddleCount; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2.5f, width/2.5f),
                    0.028f,
                    Random.Range(-length/2.5f, length/2.5f)
                );
                
                GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puddle.name = "RainPuddle";
                float size = Random.Range(0.4f, 1.5f);
                puddle.transform.position = pos;
                puddle.transform.localScale = new Vector3(
                    size * Random.Range(0.8f, 1.2f),
                    0.002f,
                    size * Random.Range(0.8f, 1.2f)
                );
                
                Material puddleMat = new Material(URPMaterialHelper.GetLitShader());
                puddleMat.SetColor("_Color", new Color(0.08f, 0.09f, 0.12f, 0.7f));
                puddleMat.SetFloat("_Metallic", 0.2f);
                puddleMat.SetFloat("_Glossiness", 0.92f);  // Very reflective water
                puddle.GetComponent<Renderer>().material = puddleMat;
                Destroy(puddle.GetComponent<Collider>());
                objectList.Add(puddle);
            }
        }
        
        private static void CreateManholes(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            int manholeCount = Random.Range(2, 4);
            
            for (int i = 0; i < manholeCount; i++)
            {
                float zPos = -length/2 + (i + 1) * (length / (manholeCount + 1));
                Vector3 pos = center + new Vector3(Random.Range(-2f, 2f), 0.026f, zPos);
                
                // Manhole cover
                GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cover.name = "ManholeCover";
                cover.transform.position = pos;
                cover.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
                
                Material coverMat = new Material(URPMaterialHelper.GetLitShader());
                coverMat.SetColor("_Color", new Color(0.25f, 0.25f, 0.27f));
                coverMat.SetFloat("_Metallic", 0.8f);
                coverMat.SetFloat("_Glossiness", 0.4f);
                cover.GetComponent<Renderer>().material = coverMat;
                Destroy(cover.GetComponent<Collider>());
                objectList.Add(cover);
                
                // Manhole rim
                GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rim.name = "ManholeRim";
                rim.transform.position = pos + Vector3.up * 0.005f;
                rim.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
                
                Material rimMat = new Material(URPMaterialHelper.GetLitShader());
                rimMat.SetColor("_Color", new Color(0.15f, 0.15f, 0.16f));
                rim.GetComponent<Renderer>().material = rimMat;
                Destroy(rim.GetComponent<Collider>());
                objectList.Add(rim);
                
                // Pattern on cover (cross hatching)
                for (int j = 0; j < 4; j++)
                {
                    GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    line.name = "CoverPattern";
                    line.transform.position = pos + new Vector3(0, 0.022f, -0.2f + j * 0.12f);
                    line.transform.localScale = new Vector3(0.6f, 0.005f, 0.03f);
                    
                    Material lineMat = new Material(URPMaterialHelper.GetLitShader());
                    lineMat.SetColor("_Color", new Color(0.2f, 0.2f, 0.22f));
                    lineMat.SetFloat("_Metallic", 0.7f);
                    line.GetComponent<Renderer>().material = lineMat;
                    Destroy(line.GetComponent<Collider>());
                    objectList.Add(line);
                }
            }
        }
        
        private static void CreateRepairPatches(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            int patchCount = Random.Range(3, 7);
            
            for (int i = 0; i < patchCount; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2.5f, width/2.5f),
                    0.026f,
                    Random.Range(-length/2.5f, length/2.5f)
                );
                
                GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                patch.name = "RepairPatch";
                patch.transform.position = pos;
                patch.transform.localScale = new Vector3(
                    Random.Range(0.8f, 2.5f),
                    0.01f,
                    Random.Range(0.8f, 2.5f)
                );
                patch.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                Material patchMat = new Material(URPMaterialHelper.GetLitShader());
                // Newer asphalt is darker
                float age = Random.value;
                patchMat.SetColor("_Color", new Color(
                    0.06f + age * 0.06f,
                    0.06f + age * 0.06f,
                    0.07f + age * 0.06f
                ));
                patchMat.SetFloat("_Glossiness", 0.12f + age * 0.1f);
                patch.GetComponent<Renderer>().material = patchMat;
                Destroy(patch.GetComponent<Collider>());
                objectList.Add(patch);
            }
        }
        
        private static void CreateCenterLine(Vector3 center, float length, List<GameObject> objectList)
        {
            int segmentCount = Mathf.FloorToInt(length / 4f);
            
            for (int i = -segmentCount/2; i <= segmentCount/2; i++)
            {
                // Some segments missing (faded away)
                if (Random.value > 0.2f)
                {
                    GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    line.name = "CenterLine";
                    line.transform.position = center + new Vector3(0, 0.025f, i * 4f);
                    line.transform.localScale = new Vector3(0.12f, 0.01f, Random.Range(1.5f, 2.5f));
                    
                    Material lineMat = new Material(URPMaterialHelper.GetLitShader());
                    float fade = Random.Range(0.4f, 0.7f);
                    lineMat.SetColor("_Color", new Color(0.7f * fade, 0.6f * fade, 0.2f * fade));
                    lineMat.SetFloat("_Glossiness", 0.1f);
                    line.GetComponent<Renderer>().material = lineMat;
                    Destroy(line.GetComponent<Collider>());
                    
                    objectList.Add(line);
                }
            }
        }
        
        private static void CreateEdgeLines(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float xPos = side * (width / 2 - 0.5f);
                
                // Continuous but faded line
                for (int i = 0; i < 5; i++)
                {
                    float zPos = -length/2 + Random.Range(0f, length);
                    float segLength = Random.Range(3f, 10f);
                    
                    GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    line.name = "EdgeLine";
                    line.transform.position = center + new Vector3(xPos, 0.025f, zPos);
                    line.transform.localScale = new Vector3(0.1f, 0.01f, segLength);
                    
                    Material lineMat = new Material(URPMaterialHelper.GetLitShader());
                    float fade = Random.Range(0.3f, 0.6f);
                    lineMat.SetColor("_Color", new Color(0.8f * fade, 0.8f * fade, 0.8f * fade));
                    line.GetComponent<Renderer>().material = lineMat;
                    Destroy(line.GetComponent<Collider>());
                    
                    objectList.Add(line);
                }
            }
        }
        
        private static void CreatePotholes(Vector3 center, float width, float length, int count, List<GameObject> objectList)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2.5f, width/2.5f),
                    0.015f,
                    Random.Range(-length/2.2f, length/2.2f)
                );
                
                // Pothole hole (dark)
                GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hole.name = "Pothole";
                hole.transform.position = pos;
                hole.transform.localScale = new Vector3(
                    Random.Range(0.3f, 1.2f),
                    0.02f,
                    Random.Range(0.3f, 1.2f)
                );
                
                Material holeMat = new Material(URPMaterialHelper.GetLitShader());
                holeMat.SetColor("_Color", new Color(0.03f, 0.03f, 0.04f));
                hole.GetComponent<Renderer>().material = holeMat;
                Destroy(hole.GetComponent<Collider>());
                objectList.Add(hole);
                
                // Cracked edges around pothole
                int edgeCount = Random.Range(3, 6);
                for (int j = 0; j < edgeCount; j++)
                {
                    float angle = (j / (float)edgeCount) * 360f * Mathf.Deg2Rad;
                    float dist = hole.transform.localScale.x * 0.6f;
                    
                    GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    edge.name = "PotholeEdge";
                    edge.transform.position = pos + new Vector3(Mathf.Cos(angle) * dist, 0.02f, Mathf.Sin(angle) * dist);
                    edge.transform.localScale = new Vector3(Random.Range(0.1f, 0.25f), 0.015f, Random.Range(0.05f, 0.15f));
                    edge.transform.rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg + Random.Range(-20f, 20f), 0);
                    
                    Material edgeMat = new Material(URPMaterialHelper.GetLitShader());
                    edgeMat.SetColor("_Color", new Color(0.08f, 0.08f, 0.09f));
                    edge.GetComponent<Renderer>().material = edgeMat;
                    Destroy(edge.GetComponent<Collider>());
                    objectList.Add(edge);
                }
            }
        }
        
        private static void CreateCracks(Vector3 center, float width, float length, int count, List<GameObject> objectList)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 startPos = center + new Vector3(
                    Random.Range(-width/2.2f, width/2.2f),
                    0.022f,
                    Random.Range(-length/2.2f, length/2.2f)
                );
                
                // Main crack line
                int segmentCount = Random.Range(2, 5);
                Vector3 currentPos = startPos;
                float currentAngle = Random.Range(0f, 360f);
                
                for (int j = 0; j < segmentCount; j++)
                {
                    GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crack.name = "Crack";
                    crack.transform.position = currentPos;
                    crack.transform.localScale = new Vector3(Random.Range(0.02f, 0.06f), 0.008f, Random.Range(0.3f, 1f));
                    crack.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
                    
                    Material crackMat = new Material(URPMaterialHelper.GetLitShader());
                    crackMat.SetColor("_Color", new Color(0.02f, 0.02f, 0.03f));
                    crack.GetComponent<Renderer>().material = crackMat;
                    Destroy(crack.GetComponent<Collider>());
                    objectList.Add(crack);
                    
                    // Move to next position
                    float crackLength = crack.transform.localScale.z;
                    currentPos += Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * crackLength;
                    currentAngle += Random.Range(-45f, 45f);
                }
            }
        }
        
        private static void CreateOilStains(Vector3 center, float width, float length, int count, List<GameObject> objectList)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2.5f, width/2.5f),
                    0.023f,
                    Random.Range(-length/2.5f, length/2.5f)
                );
                
                GameObject stain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stain.name = "OilStain";
                stain.transform.position = pos;
                stain.transform.localScale = new Vector3(
                    Random.Range(0.5f, 2f),
                    0.005f,
                    Random.Range(0.5f, 2f)
                );
                stain.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                Material stainMat = new Material(URPMaterialHelper.GetLitShader());
                stainMat.SetColor("_Color", new Color(0.05f, 0.04f, 0.06f));
                stainMat.SetFloat("_Metallic", 0.4f);
                stainMat.SetFloat("_Glossiness", 0.6f);
                stain.GetComponent<Renderer>().material = stainMat;
                Destroy(stain.GetComponent<Collider>());
                objectList.Add(stain);
            }
        }
        
        private static void CreateTireMarks(Vector3 center, float width, float length, int count, List<GameObject> objectList)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 startPos = center + new Vector3(
                    Random.Range(-width/3, width/3),
                    0.022f,
                    Random.Range(-length/2, length/2)
                );
                
                float markLength = Random.Range(3f, 8f);
                float curve = Random.Range(-30f, 30f);
                
                // Tire mark (curved slightly)
                int segments = 5;
                for (int j = 0; j < segments; j++)
                {
                    GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    mark.name = "TireMark";
                    
                    float t = j / (float)segments;
                    float xOffset = Mathf.Sin(t * Mathf.PI) * curve * 0.1f;
                    
                    mark.transform.position = startPos + new Vector3(xOffset, 0, t * markLength);
                    mark.transform.localScale = new Vector3(0.2f, 0.006f, markLength / segments + 0.1f);
                    mark.transform.rotation = Quaternion.Euler(0, curve * t, 0);
                    
                    Material markMat = new Material(URPMaterialHelper.GetLitShader());
                    markMat.SetColor("_Color", new Color(0.06f, 0.06f, 0.07f));
                    mark.GetComponent<Renderer>().material = markMat;
                    Destroy(mark.GetComponent<Collider>());
                    objectList.Add(mark);
                }
            }
        }
        
        private static void CreateRoadDebris(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            // Random small debris
            int debrisCount = Random.Range(20, 40);
            
            for (int i = 0; i < debrisCount; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2, width/2),
                    Random.Range(0.02f, 0.1f),
                    Random.Range(-length/2, length/2)
                );
                
                PrimitiveType type = Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Cylinder;
                GameObject debris = GameObject.CreatePrimitive(type);
                debris.name = "RoadDebris";
                debris.transform.position = pos;
                debris.transform.localScale = new Vector3(
                    Random.Range(0.05f, 0.2f),
                    Random.Range(0.02f, 0.1f),
                    Random.Range(0.05f, 0.2f)
                );
                debris.transform.rotation = Random.rotation;
                
                Material debrisMat = new Material(URPMaterialHelper.GetLitShader());
                debrisMat.SetColor("_Color", new Color(
                    Random.Range(0.15f, 0.35f),
                    Random.Range(0.13f, 0.3f),
                    Random.Range(0.1f, 0.25f)
                ));
                debris.GetComponent<Renderer>().material = debrisMat;
                Destroy(debris.GetComponent<Collider>());
                objectList.Add(debris);
            }
            
            // Larger debris pieces
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = center + new Vector3(
                    Random.Range(-width/2.5f, width/2.5f),
                    0.1f,
                    Random.Range(-length/2.5f, length/2.5f)
                );
                
                GameObject largeDebris = GameObject.CreatePrimitive(PrimitiveType.Cube);
                largeDebris.name = "LargeDebris";
                largeDebris.transform.position = pos;
                largeDebris.transform.localScale = new Vector3(
                    Random.Range(0.3f, 0.8f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(0.3f, 0.8f)
                );
                largeDebris.transform.rotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );
                
                Material debrisMat = new Material(URPMaterialHelper.GetLitShader());
                debrisMat.SetColor("_Color", new Color(0.35f, 0.32f, 0.28f));
                largeDebris.GetComponent<Renderer>().material = debrisMat;
                objectList.Add(largeDebris);
            }
        }
        
        private static void CreateCurbs(Vector3 center, float width, float length, List<GameObject> objectList)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float xPos = side * (width / 2 + 0.15f);
                
                GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curb.name = "Curb";
                curb.transform.position = center + new Vector3(xPos, 0.08f, 0);
                curb.transform.localScale = new Vector3(0.3f, 0.16f, length);
                
                Material curbMat = new Material(URPMaterialHelper.GetLitShader());
                curbMat.SetColor("_Color", new Color(0.45f, 0.43f, 0.4f));
                curbMat.SetFloat("_Glossiness", 0.15f);
                curb.GetComponent<Renderer>().material = curbMat;
                objectList.Add(curb);
                
                // Damage to curb
                int damageCount = Random.Range(3, 8);
                for (int i = 0; i < damageCount; i++)
                {
                    GameObject damage = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    damage.name = "CurbDamage";
                    damage.transform.position = center + new Vector3(
                        xPos + side * 0.05f,
                        0.12f,
                        Random.Range(-length/2.2f, length/2.2f)
                    );
                    damage.transform.localScale = new Vector3(0.15f, 0.1f, Random.Range(0.2f, 0.6f));
                    damage.transform.rotation = Quaternion.Euler(
                        Random.Range(-5f, 5f),
                        Random.Range(-10f, 10f),
                        Random.Range(-5f, 5f)
                    );
                    
                    Material damageMat = new Material(URPMaterialHelper.GetLitShader());
                    damageMat.SetColor("_Color", new Color(0.35f, 0.33f, 0.3f));
                    damage.GetComponent<Renderer>().material = damageMat;
                    Destroy(damage.GetComponent<Collider>());
                    objectList.Add(damage);
                }
            }
        }
    }
}

