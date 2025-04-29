using System;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;
using System.IO.Compression;
using CompressionLevel = System.IO.Compression.CompressionLevel;

[System.Serializable]
public class VoxelRange
{
    public readonly Vector3Int m_min;
    public readonly Vector3Int m_max;
    public readonly Vector3Int m_size;

    public VoxelRange(Vector3Int offset, Vector3Int size)
    {
        m_min = offset;
        m_max = m_min + size;
        m_size = size;
    }
    
    public bool IsValidRange()
    {
        return m_min.x <= m_max.x && m_min.y <= m_max.y && m_min.z <= m_max.z;
    }

    public bool Contains(VoxelRange other)
    {
        if (!IsValidRange() || !other.IsValidRange())
        {
            return false;
        }

        return m_min.x <= other.m_min.x && m_max.x >= other.m_max.x &&
            m_min.y <= other.m_min.y && m_max.y >= other.m_max.y &&
            m_min.z <= other.m_min.z && m_max.z >= other.m_max.z;
    }

    public bool Contains(ref Vector3Int voxel)
    {
        return m_min.x <= voxel.x && m_max.x >= voxel.x &&
            m_min.y <= voxel.y && m_max.y >= voxel.y &&
            m_min.z <= voxel.z && m_max.z >= voxel.z;
    }

    public bool Overlaps(VoxelRange other) // this is a contains, no? need some || ors
    {
        if (!IsValidRange() || !other.IsValidRange())
        {
            return false;
        }

        return m_min.x <= other.m_max.x && m_max.x >= other.m_min.x &&
            m_min.y <= other.m_max.y && m_max.y >= other.m_min.y &&
            m_min.z <= other.m_max.z && m_max.z >= other.m_min.z;
    }

    public VoxelRange GetClampedToRange(VoxelRange other) // I do not think this is working
    {
        Vector3Int min = Vector3Int.Max(m_min, other.m_min);
        Vector3Int max = Vector3Int.Min(m_max, other.m_max);
        return new VoxelRange(min, max - min);
    }
}
