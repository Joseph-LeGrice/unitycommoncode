using System;
using Unity.Collections;
using UnityEngine;

public class VoxelWorldData
{
    private VoxelConstantData m_constantData;
    private CompressedVoxelData m_compressedObject;
    
    public VoxelConstantData GetConstantData()
    {
        return m_constantData;
    }

    public static Vector3Int GetCachedChunkSize(Vector3Int chunkSize)
    {
        return chunkSize + Vector3Int.one;
    }

    public VoxelWorldData(VoxelConstantData constantData)
    {
        m_constantData = constantData;
        m_compressedObject = m_constantData.GetCompressedVoxelData();
    }
    
}
