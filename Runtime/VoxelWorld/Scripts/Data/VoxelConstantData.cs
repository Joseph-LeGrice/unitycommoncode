using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class VoxelConstantData : MonoBehaviour
{
    [SerializeField]
    private Vector3Int m_worldChunks;
    [SerializeField]
    private float m_voxelScaleFactor = 1.0f;
    [SerializeField]
    private CompressedVoxelData m_compressedObject;
    [SerializeField]
    private VoxelWorldGenerator m_worldGenerator = new VoxelWorldGenerator();
    
    private static readonly Vector3Int s_chunkSize = new Vector3Int(16, 16, 16);
    
    public VoxelWorldGenerator GetWorldGenerator()
    {
        return m_worldGenerator;
    }
    
    public Vector3Int GetWorldDimensions()
    {
        return m_worldChunks;
    }
    
    public void SetWorldDimensions(Vector3Int dimensions)
    {
        m_worldChunks = dimensions;
    }
    
    public Vector3Int GetChunkSize()
    {
        return s_chunkSize;
    }

    public float GetVoxelScaleFactor()
    {
        return m_voxelScaleFactor;
    }

    public void GenerateWorld()
    {
        m_compressedObject = CompressedVoxelData.CreateNew(this);
    }
    
    public CompressedVoxelData GetCompressedVoxelData()
    {
        return m_compressedObject;
    }

    public bool IsValidForLoad(Vector3Int chunkCoordinate)
    {
        return chunkCoordinate.x >= 0 && chunkCoordinate.x < m_worldChunks.x - 1 &&
            chunkCoordinate.y >= 0 && chunkCoordinate.y < m_worldChunks.y - 1 &&
            chunkCoordinate.z >= 0 && chunkCoordinate.z < m_worldChunks.z - 1;
    }

    public Vector3Int GetRootVoxel(Vector3Int chunkCoordinate)
    {
        return new Vector3Int(
            chunkCoordinate.x * s_chunkSize.x,
            chunkCoordinate.y * s_chunkSize.y,
            chunkCoordinate.z * s_chunkSize.z
        );
    }
    
    public Vector3Int GetChunkCoordinate(Vector3Int voxel)
    {
        return new Vector3Int(
            voxel.x / s_chunkSize.x,
            voxel.y / s_chunkSize.y,
            voxel.z / s_chunkSize.z
        );
    }

    public Vector3Int WorldToLocalVoxel(Vector3Int voxel)
    {
        return new Vector3Int(
            voxel.x % s_chunkSize.x,
            voxel.y % s_chunkSize.y,
            voxel.z % s_chunkSize.z
        );
    }
    
    public bool IsValidVoxel(Vector3Int voxel)
    {
        return voxel.x >= 0 && voxel.x <= m_worldChunks.x * s_chunkSize.x &&
            voxel.y >= 0 && voxel.y <= m_worldChunks.y * s_chunkSize.y &&
            voxel.z >= 0 && voxel.z <= m_worldChunks.z * s_chunkSize.z;
    }
    
    public Vector3Int ClampToValid(Vector3Int voxel)
    {
        return new Vector3Int(
            Mathf.Clamp(voxel.x, 0, m_worldChunks.x * s_chunkSize.x),
            Mathf.Clamp(voxel.y, 0, m_worldChunks.y * s_chunkSize.y),
            Mathf.Clamp(voxel.z, 0, m_worldChunks.z * s_chunkSize.z)
        );
    }

    public int GetIndex(Vector3Int localCoordinate)
    {
        Vector3Int cachedChunkSize = s_chunkSize + Vector3Int.one;
        return localCoordinate.y * cachedChunkSize.z * cachedChunkSize.x +
            localCoordinate.z * cachedChunkSize.x +
            localCoordinate.x;
    }

    public int GetVoxelValue(sbyte[] data, Vector3Int localCoordinate)
    {
        int i = GetIndex(localCoordinate);
        return data[i];
    }

    public float GetVoxelValueNormalised(sbyte[] data, Vector3Int localCoordinate)
    {
        int i = GetIndex(localCoordinate);
        return Mathf.InverseLerp(-127, 127, data[i]);
    }
    
    public sbyte[] GetChunkData(Vector3Int chunkCoordinate)
    {
        Vector3Int cachedChunkSize = s_chunkSize + Vector3Int.one;
        
        Vector3Int chunkDimensions = s_chunkSize;
        sbyte[] cachedChunkData = new sbyte[cachedChunkSize.y * cachedChunkSize.z * cachedChunkSize.x];
        
        for (int yChunkOffset = 0; yChunkOffset <= 1; yChunkOffset++)
        {
            for (int zChunkOffset = 0; zChunkOffset <= 1; zChunkOffset++)
            {                    
                for (int xChunkOffset = 0; xChunkOffset <= 1; xChunkOffset++)
                {
                    Vector3Int chunkOffset = new Vector3Int(xChunkOffset, yChunkOffset, zChunkOffset);
                    Vector3Int thisChunkCoords = chunkCoordinate + chunkOffset;
                    sbyte[] thisChunkData = m_compressedObject.GetCompressedChunkData(thisChunkCoords);

                    Vector3Int copySize = new Vector3Int();
                    copySize.y = yChunkOffset == 0 ? chunkDimensions.y : 1;
                    copySize.z = zChunkOffset == 0 ? chunkDimensions.z : 1;
                    copySize.x = xChunkOffset == 0 ? chunkDimensions.x : 1;
                    
                    for (int copyFromY = 0; copyFromY < copySize.y; copyFromY++)
                    {
                        for (int copyFromZ = 0; copyFromZ < copySize.z; copyFromZ++)
                        {
                            int startIndex = copyFromY * chunkDimensions.x * chunkDimensions.z +
                                copyFromZ * chunkDimensions.x;
                            
                            int destinationIndex = copyFromY * cachedChunkSize.z * cachedChunkSize.x + yChunkOffset * chunkDimensions.y * cachedChunkSize.z * cachedChunkSize.x +
                                 copyFromZ * cachedChunkSize.x + zChunkOffset * chunkDimensions.x * cachedChunkSize.z +
                                 xChunkOffset * chunkDimensions.x;
                            
                            int length = copySize.x;
                            
                            Array.Copy(thisChunkData, startIndex, cachedChunkData, destinationIndex, length);
                        }
                    }
                }
            }
        }

        return cachedChunkData;
    }
}
