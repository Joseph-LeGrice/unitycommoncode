using System;
using System.Collections.Generic;
using UnityEngine;

public class PooledGameObject : MonoBehaviour
{
    private GameObjectPool m_owner;

    public void Init(GameObjectPool owner)
    {
        m_owner = owner;
    }

    void OnDisable()
    {
        m_owner.Return(this);
    }

    void OnDestroy()
    {
        m_owner.MarkDestroyed(this);
    }
}

public class GameObjectPool : IDisposable
{
    private GameObject m_prefab;
    private Transform m_parentHierarchy;
    private int m_layer;
    
    private List<PooledGameObject> m_activeObjects = new List<PooledGameObject>();
    private List<PooledGameObject> m_pooledObjects = new List<PooledGameObject>();

    public GameObjectPool(GameObject prefab, int layer, Transform parentTransform, int initialPoolCount)
    {
        m_prefab = prefab;
        m_parentHierarchy = parentTransform;
        m_layer = layer;
        PoolObjects(initialPoolCount);
    }

    public List<PooledGameObject> GetCurrentActiveObjects()
    {
        return m_activeObjects;
    }

    public PooledGameObject GetNext()
    {
        if (m_pooledObjects.Count == 0)
        {
            PoolObjects(1);
        }
        
        PooledGameObject next = m_pooledObjects[0];
        m_activeObjects.Add(next);
        m_pooledObjects.RemoveAt(0);
        return next;
    }

    public void SetActiveObjectCount(int count)
    {
        int diff = count - m_activeObjects.Count;
        
        int toAdd = Mathf.Max(diff, 0);
        if (toAdd > 0)
        {
            int toInstantiate = Mathf.Max(toAdd - m_pooledObjects.Count, 0);
            PoolObjects(toInstantiate);
            
            var toActivate = m_pooledObjects.GetRange(0, toAdd);
            m_activeObjects.AddRange(toActivate);
            m_pooledObjects.RemoveRange(0, toAdd);
            foreach (var pooledGameObject in toActivate)
            {
                pooledGameObject.gameObject.SetActive(true);
            }
        }
        
        int toRemove = Mathf.Abs(Mathf.Min(diff, 0));
        if (toRemove > 0)
        {
            var toDeactivate = m_activeObjects.GetRange(0, toRemove);
            m_pooledObjects.AddRange(toDeactivate);
            m_activeObjects.RemoveRange(0, toRemove);
            foreach (PooledGameObject pooledGameObject in toDeactivate)
            {
                pooledGameObject.gameObject.SetActive(false);
            }
        }
    }

    public void Return(PooledGameObject obj)
    {
        int activeIndex = m_activeObjects.IndexOf(obj);
        if (activeIndex > -1)
        {
            m_pooledObjects.Add(m_activeObjects[activeIndex]);
            m_activeObjects.RemoveAt(activeIndex);
        }
    }

    public void MarkDestroyed(PooledGameObject obj)
    {
        int activeIndex = m_activeObjects.IndexOf(obj);
        if (activeIndex > -1)
        {
            m_activeObjects.RemoveAt(activeIndex);
        }
        
        int pooledIndex = m_pooledObjects.IndexOf(obj);
        if (pooledIndex > -1)
        {
            m_pooledObjects.RemoveAt(activeIndex);
        }
    }

    private void PoolObjects(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newInstance = GameObject.Instantiate(m_prefab, m_parentHierarchy);
            newInstance.layer = m_layer;
            var pooledItem = newInstance.AddComponent<PooledGameObject>();
            pooledItem.Init(this);
            newInstance.SetActive(false);
            m_pooledObjects.Add(pooledItem);
        }
    }
    
    public void Dispose()
    {
        foreach (PooledGameObject activeObject in m_activeObjects)
        {
            GameObject.Destroy(activeObject.gameObject);
        }
        
        foreach (PooledGameObject pooledObject in m_pooledObjects)
        {
            GameObject.Destroy(pooledObject.gameObject);
        }
    }
}
