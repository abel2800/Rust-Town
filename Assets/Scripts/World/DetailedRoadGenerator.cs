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
            // Main asphalt surface
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.position = center + Vector3.up * 0.02f;
            road.transform.localScale = new Vector3(width, 0.05f, length);
            
            Material asphaltMat = new Material(Shader.Find("Standard"));
            asphaltMat.SetColor("_Color", new Color(0.12f, 0.12f, 0.13f));
            asphaltMat.SetFloat("_Metallic", 0f);
            asphaltMat.SetFloat("_Glossiness", 0.15f);
            road.GetComponent<Renderer>().material = asphaltMat;
            objectList.Add(road);
            
            // Center line (faded yellow)
            CreateCenterLine(center, length, objectList);
            
            // Edge lines (faded white)
            CreateEdgeLines(center, width, length, objectList);
            
            // Potholes
            CreatePotholes(center, width, length, 15, objectList);
            
            // Cracks
            CreateCracks(center, width, length, 25, objectList);
            
            // Oil stains
            CreateOilStains(center, width, length, 8, objectList);
            
            // Tire marks
            CreateTireMarks(center, width, length, 5, objectList);
            
            // Debris on road
            CreateRoadDebris(center, width, length, objectList);
            
            // Curbs
            CreateCurbs(center, width, length, objectList);
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
                    
                    Material lineMat = new Material(Shader.Find("Standard"));
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
                    
                    Material lineMat = new Material(Shader.Find("Standard"));
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
                
                Material holeMat = new Material(Shader.Find("Standard"));
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
                    
                    Material edgeMat = new Material(Shader.Find("Standard"));
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
                    
                    Material crackMat = new Material(Shader.Find("Standard"));
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
                
                Material stainMat = new Material(Shader.Find("Standard"));
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
                    
                    Material markMat = new Material(Shader.Find("Standard"));
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
                
                Material debrisMat = new Material(Shader.Find("Standard"));
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
                
                Material debrisMat = new Material(Shader.Find("Standard"));
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
                
                Material curbMat = new Material(Shader.Find("Standard"));
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
                    
                    Material damageMat = new Material(Shader.Find("Standard"));
                    damageMat.SetColor("_Color", new Color(0.35f, 0.33f, 0.3f));
                    damage.GetComponent<Renderer>().material = damageMat;
                    Destroy(damage.GetComponent<Collider>());
                    objectList.Add(damage);
                }
            }
        }
    }
}

