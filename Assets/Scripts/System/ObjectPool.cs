using UnityEngine;
using System.Collections.Generic;

namespace NeonArena.System
{
    /// <summary>
    /// Object pooling system for performance optimization
    /// Reduces garbage collection by reusing game objects
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private string pooledObjectName;
        private int poolSize;
        private Queue<GameObject> availableObjects;
        private List<GameObject> allObjects;

        public void Initialize(string objectName, int size)
        {
            pooledObjectName = objectName;
            poolSize = size;
            availableObjects = new Queue<GameObject>();
            allObjects = new List<GameObject>();

            // Pre-instantiate pool objects
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewObject();
            }
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = new GameObject($"{pooledObjectName}_Pooled");
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            
            availableObjects.Enqueue(obj);
            allObjects.Add(obj);
            
            return obj;
        }

        public GameObject GetObject()
        {
            GameObject obj;

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                // Pool exhausted, create new object
                obj = CreateNewObject();
                availableObjects.Dequeue(); // Remove it immediately since we're using it
            }

            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            if (!availableObjects.Contains(obj))
            {
                availableObjects.Enqueue(obj);
            }
        }

        public void ReturnAll()
        {
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    ReturnObject(obj);
                }
            }
        }

        public int GetActiveCount()
        {
            int count = 0;
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetAvailableCount()
        {
            return availableObjects.Count;
        }

        private void OnDestroy()
        {
            // Clean up all pooled objects
            if (allObjects != null)
            {
                foreach (GameObject obj in allObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
                allObjects.Clear();
            }
            
            if (availableObjects != null)
            {
                availableObjects.Clear();
            }
        }
    }
}

