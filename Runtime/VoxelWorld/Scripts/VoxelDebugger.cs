using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class VoxelDebugger : MonoBehaviour
{
    [SerializeField]
    private VoxelWorld m_world;
    [SerializeField]
    private float m_gizmosSize;
    [SerializeField]
    private Vector3Int m_previewSize;
    
    private Dictionary<Vector3Int, sbyte[]> m_cachedData = new Dictionary<Vector3Int, sbyte[]>();

    public void SampleData()
    {
        m_cachedData.Clear();

        VoxelConstantData constantData = m_world.GetWorldData();
        
        Vector3Int minVoxel = m_world.WorldPositionToVoxel(transform.position);
        minVoxel.x -= Mathf.FloorToInt(0.5f * m_previewSize.x);
        minVoxel.y -= Mathf.FloorToInt(0.5f * m_previewSize.y);
        minVoxel.z -= Mathf.FloorToInt(0.5f * m_previewSize.z);
        minVoxel = constantData.ClampToValid(minVoxel);

        Vector3Int maxVoxel = minVoxel + m_previewSize;
        maxVoxel = constantData.ClampToValid(maxVoxel);

        Vector3Int minChunkCoordinate = constantData.GetChunkCoordinate(minVoxel);
        Vector3Int maxChunkCoordinate = constantData.GetChunkCoordinate(maxVoxel);

        for (int x = minChunkCoordinate.x; x <= maxChunkCoordinate.x; x++)
        {
            for (int y = minChunkCoordinate.y; y <= maxChunkCoordinate.y; y++)
            {
                for (int z = minChunkCoordinate.z; z <= maxChunkCoordinate.z; z++)
                {
                    Vector3Int thisChunkCoordinate = new Vector3Int(x, y, z);
                    m_cachedData[thisChunkCoordinate] = constantData.GetChunkData(thisChunkCoordinate);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3Int rootVoxel = m_world.WorldPositionToVoxel(transform.position);
        
        VoxelConstantData constantData = m_world.GetWorldData();
        
        for (int y = 0; y < m_previewSize.y; y++)
        {
            for (int x = 0; x < m_previewSize.x; x++)
            {
                for (int z = 0; z < m_previewSize.z; z++)
                {
                    Vector3Int voxel = new Vector3Int();
                    voxel.x = rootVoxel.x + x - Mathf.FloorToInt(0.5f * m_previewSize.x);
                    voxel.y = rootVoxel.y + y - Mathf.FloorToInt(0.5f * m_previewSize.y);
                    voxel.z = rootVoxel.z + z - Mathf.FloorToInt(0.5f * m_previewSize.z);

                    if (constantData.IsValidVoxel(voxel))
                    {
                        Vector3Int chunkCoordinate = constantData.GetChunkCoordinate(voxel);
                        if (m_cachedData.TryGetValue(chunkCoordinate, out sbyte[] data))
                        {
                            Vector3Int localVoxel = constantData.WorldToLocalVoxel(voxel);
                            int voxelValue = constantData.GetVoxelValue(data, localVoxel);
                            float value = Mathf.InverseLerp(ChunkBuildData.VOXEL_INSIDE_TERRAIN, ChunkBuildData.VOXEL_OUTSIDE_TERRAIN, voxelValue);
                            Color c = Color.green * value + Color.blue * (1.0f - value);

                            if (voxelValue == 0)
                            {
                                c = Color.white;
                            }
                            
                            c.a = 1.0f;
                            Gizmos.color = c;

                            Gizmos.DrawSphere(m_world.VoxelToWorldPosition(voxel), m_gizmosSize);
                        }
                    }
                }
            }
        }
    }
}
