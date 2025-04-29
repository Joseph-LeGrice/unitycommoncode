using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class VoxelWorldChunkBuilder : IDisposable
{
    private NativeList<int> m_indices;
    private NativeArray<int> m_indexCountPerCell;
    private NativeList<VoxelCellVertex> m_vertices;
    private NativeArray<int> m_vertexCountPerCell;
    private NativeList<Vector3> m_realVertices;
    private NativeList<int> m_realIndices;
    private NativeArray<sbyte> m_chunkData;

    private VoxelConstantData m_constantData;
    private Vector3Int m_chunkCoordinate;
    
    public VoxelWorldChunkBuilder(VoxelConstantData constantData, Vector3Int chunkCoordinate)
    {
        m_constantData = constantData;
        m_chunkCoordinate = chunkCoordinate;
        
        if (m_chunkCoordinate.x >= m_constantData.GetWorldDimensions().x - 1 ||
            m_chunkCoordinate.y >= m_constantData.GetWorldDimensions().y - 1||
            m_chunkCoordinate.z >= m_constantData.GetWorldDimensions().z - 1)
        {
            throw new ArgumentException("Cannot build a chunk which lies on the world boundary.");
        }
    }
    
    public JobHandle BeginChunkBuild()
    {
        Vector3Int chunkDimensions = m_constantData.GetChunkSize();
        int numCells = chunkDimensions.x * chunkDimensions.y * chunkDimensions.z;

        m_chunkData = new NativeArray<sbyte>(m_constantData.GetChunkData(m_chunkCoordinate), Allocator.TempJob);
        m_indices = new NativeList<int>(Allocator.TempJob);
        m_indexCountPerCell = new NativeArray<int>(numCells, Allocator.TempJob);
        m_vertices = new NativeList<VoxelCellVertex>(Allocator.TempJob);
        m_vertexCountPerCell = new NativeArray<int>(numCells, Allocator.TempJob);

        ChunkBuildJob chunkBuildJobJob = new ChunkBuildJob()
        {
            m_chunkData = m_chunkData,
            m_chunkDimensions = chunkDimensions,
            m_cachedDataSize = chunkDimensions + Vector3Int.one,
            
            m_indices = m_indices,
            m_indexCountPerCell = m_indexCountPerCell,
            m_vertices = m_vertices,
            m_vertexCountPerCell = m_vertexCountPerCell,
        };

        m_realVertices = new NativeList<Vector3>(Allocator.TempJob);
        m_realIndices = new NativeList<int>(Allocator.TempJob);
        
        float scaleFactor = m_constantData.GetVoxelScaleFactor();
        
        MeshBuildJob meshBuildJobJob = new MeshBuildJob()
        {
            m_scaleFactor = scaleFactor,
            m_chunkDimensions = chunkDimensions,
            m_indices = m_indices,
            m_indexCountPerCell = m_indexCountPerCell,
            m_vertices = m_vertices,
            m_vertexCountPerCell = m_vertexCountPerCell,
            
            m_realVertices = m_realVertices,
            m_realIndices = m_realIndices,
        };

        return meshBuildJobJob.Schedule(chunkBuildJobJob.Schedule());
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = m_realVertices.AsArray().ToArray();
        mesh.triangles = m_realIndices.AsArray().ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }

    public Vector3Int GetChunkCoordinate()
    {
        return m_chunkCoordinate;
    }

    public void Dispose()
    {
        m_indices.Dispose();
        m_indexCountPerCell.Dispose();
        m_vertices.Dispose();
        m_vertexCountPerCell.Dispose();
        m_realVertices.Dispose();
        m_realIndices.Dispose();
        m_chunkData.Dispose();
    }
}
