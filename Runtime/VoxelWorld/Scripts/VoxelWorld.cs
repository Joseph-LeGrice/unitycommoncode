using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

public class VoxelWorld : MonoBehaviour
{
    [SerializeField]
    private VoxelConstantData m_constantData;
    [SerializeField]
    private GameObject m_voxelChunkPrefab;
    
    [Space]
    [SerializeField]
    private Vector3Int m_minChunkCached;
    [SerializeField]
    private Vector3Int m_maxChunkCached;

    [Space]
    [SerializeField]
    private VoxelWorldChunk[] m_cachedChunks;
    [SerializeField]
    private Vector3Int[] m_chunksLoaded;
    [SerializeField]
    private bool m_worldInitialised;

    public VoxelConstantData GetWorldData()
    {
        return m_constantData;
    }

    public Vector3Int WorldPositionToVoxel(Vector3 worldPosition)
    {
        if (!m_worldInitialised)
        {
            throw new ArgumentException("VoxelWorld is not initialised");
        }

        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(worldPosition); 
        
        float voxelScaleFactor = m_constantData.GetVoxelScaleFactor();

        Vector3Int minVoxel = m_constantData.GetRootVoxel(m_minChunkCached);
        Vector3Int voxelPosition = new Vector3Int();
        voxelPosition.x = Mathf.Clamp(Mathf.RoundToInt(localPosition.x / voxelScaleFactor) - minVoxel.x, 0, m_constantData.GetChunkSize().x * m_constantData.GetWorldDimensions().x);
        voxelPosition.y = Mathf.Clamp(Mathf.RoundToInt(localPosition.y / voxelScaleFactor) - minVoxel.y, 0, m_constantData.GetChunkSize().y * m_constantData.GetWorldDimensions().y);
        voxelPosition.z = Mathf.Clamp(Mathf.RoundToInt(localPosition.z / voxelScaleFactor) - minVoxel.z, 0, m_constantData.GetChunkSize().z * m_constantData.GetWorldDimensions().z);

        return voxelPosition;
    }

    public Vector3 VoxelToWorldPosition(Vector3Int voxel)
    {
        Vector3Int minVoxel = m_constantData.GetRootVoxel(m_minChunkCached);
        float voxelScaleFactor = m_constantData.GetVoxelScaleFactor();
        return transform.localToWorldMatrix.MultiplyPoint(voxelScaleFactor * (Vector3)(voxel - minVoxel));
    }

    public VoxelWorldChunk GetChunk(Vector3Int chunkCoordinate)
    {
        if (!m_worldInitialised)
        {
            throw new ArgumentException("VoxelWorld is not initialised");
        }
        int i = Array.IndexOf(m_chunksLoaded, chunkCoordinate);
        if (i == -1)
        {
            return null;
        }
        return m_cachedChunks[i];
    }
    
    public bool GetIsChunkLoaded(Vector3Int chunkCoordinate)
    {
        if (!m_worldInitialised)
        {
            throw new ArgumentException("VoxelWorld is not initialised");
        }
        return Array.IndexOf(m_chunksLoaded, chunkCoordinate) != -1;
    }

    public void RegenerateWorld()
    {
        m_constantData.GenerateWorld();
        LoadChunks(m_minChunkCached, m_maxChunkCached);
    }

    public void RegenerateWorld(Vector3Int minChunk, Vector3Int maxChunk)
    {
        m_constantData.GenerateWorld();
        LoadChunks(minChunk, maxChunk);
    }

    public void LoadChunks(Vector3Int minChunk, Vector3Int maxChunk)
    {
        // Adjust cached chunk object arrays
        Vector3Int areaToLoad = maxChunk - minChunk;
        int targetChunksToLoad = areaToLoad.x * areaToLoad.y * areaToLoad.z;
        
        if (m_cachedChunks != null)
        {
            targetChunksToLoad = Mathf.Max(targetChunksToLoad, m_cachedChunks.Length);
        }

        if (m_chunksLoaded != null)
        {
            int newElements = targetChunksToLoad - m_chunksLoaded.Length;
            Array.Resize(ref m_chunksLoaded, targetChunksToLoad);
            Array.Fill(m_chunksLoaded, -Vector3Int.one, m_chunksLoaded.Length - newElements, newElements);
        }
        else
        {
            m_chunksLoaded = new Vector3Int[targetChunksToLoad];
            Array.Fill(m_chunksLoaded, -Vector3Int.one);
        }

        if (m_cachedChunks != null)
        {
            int newElements = targetChunksToLoad - m_cachedChunks.Length;
            Array.Resize(ref m_cachedChunks, targetChunksToLoad);
            for (int i = m_cachedChunks.Length - newElements; i < m_cachedChunks.Length; i++)
            {
                GameObject chunkInstance = Instantiate(m_voxelChunkPrefab, Vector3.zero, Quaternion.identity, transform);
                m_cachedChunks[i] = chunkInstance.GetComponent<VoxelWorldChunk>();
            }
        }
        else
        {
            m_cachedChunks = new VoxelWorldChunk[targetChunksToLoad];
            for (int i = 0; i < targetChunksToLoad; i++)
            {
                GameObject chunkInstance = Instantiate(m_voxelChunkPrefab, Vector3.zero, Quaternion.identity, transform);
                m_cachedChunks[i] = chunkInstance.GetComponent<VoxelWorldChunk>();
            }
        }

        // Build chunks
        VoxelWorldChunkBuilder[] chunkBuilders = new VoxelWorldChunkBuilder[m_chunksLoaded.Length];
        
        for (int x = minChunk.x; x < maxChunk.x; x++)
        {
            for (int y = minChunk.y; y < maxChunk.y; y++)
            {
                for (int z = minChunk.z; z < maxChunk.z; z++)
                {
                    Vector3Int chunkCoordinate = new Vector3Int(x, y, z);

                    int loadedIndex = Array.IndexOf(m_chunksLoaded, chunkCoordinate);
                    if (m_constantData.IsValidForLoad(chunkCoordinate) && loadedIndex == -1)
                    {
                        for (int i=0; i<m_cachedChunks.Length; i++)
                        {
                            VoxelWorldChunk chunk = m_cachedChunks[i];
                            if (chunkBuilders[i] == null && IsEligibleForLoad(chunk, minChunk, maxChunk))
                            {
                                chunkBuilders[i] = new VoxelWorldChunkBuilder(m_constantData, chunkCoordinate);
                                break;
                            }
                        }
                    }
                }
            }
        }

        NativeList<JobHandle> handles = new NativeList<JobHandle>(Allocator.Persistent);
        for (int i = 0; i < chunkBuilders.Length; i++)
        {
            if (chunkBuilders[i] != null)
            {
                JobHandle jh = chunkBuilders[i].BeginChunkBuild();
                handles.Add(jh);
            }
        }
        
        JobHandle.CombineDependencies(handles.AsArray()).Complete();
        handles.Dispose();

        for (int i = 0; i < chunkBuilders.Length; i++)
        {
            if (chunkBuilders[i] != null)
            {
                m_cachedChunks[i].FinaliseChunkLoad(chunkBuilders[i]);
            }
        }
        
        foreach (VoxelWorldChunk chunk in m_cachedChunks)
        {
            if (chunk.GetState() == VoxelWorldChunk.State.Loaded)
            {
                Vector3 offset = chunk.GetLoadedChunkCoordinate() - minChunk;
                
                float voxelScaleFactor = m_constantData.GetVoxelScaleFactor();
                Vector3 position = new Vector3();
                position.x = voxelScaleFactor * m_constantData.GetChunkSize().x * offset.x;
                position.y = voxelScaleFactor * m_constantData.GetChunkSize().y * offset.y;
                position.z = voxelScaleFactor * m_constantData.GetChunkSize().z * offset.z;
                chunk.transform.localPosition = position;
            }
        }

        m_minChunkCached = minChunk;
        m_maxChunkCached = maxChunk;
        
        m_worldInitialised = true;
    }

    private bool IsEligibleForLoad(VoxelWorldChunk chunk, Vector3Int minChunk, Vector3Int maxChunk)
    {
        if (chunk.GetState() == VoxelWorldChunk.State.Unloaded)
        {
            return true;
        }
        
        if (chunk.GetState() == VoxelWorldChunk.State.Loaded)
        {
            Vector3Int loadedChunkCoordinate = chunk.GetLoadedChunkCoordinate();
            return loadedChunkCoordinate.x < minChunk.x && loadedChunkCoordinate.x >= maxChunk.x &&
                loadedChunkCoordinate.y < minChunk.y && loadedChunkCoordinate.y >= maxChunk.y &&
                loadedChunkCoordinate.z < minChunk.z && loadedChunkCoordinate.z >= maxChunk.z;
        }

        return false;
    }
}
