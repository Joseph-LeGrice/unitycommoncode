using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct VoxelCellVertex
{
    public int m_localIndex;
    public ushort m_vertexCode;
    
    // Absolute Vertex value
    public Vector3Int m_value;
    
    // VertexReference ivars
    public bool m_isReference;
    public bool m_referenceUseEndpoint;
    public Vector3Int  m_referenceCellCoordinate;
    public int m_referenceLocalVertexIndex;

    public VoxelCellVertex(int localIndex, ushort vertexCode, Vector3Int cellCoordinate, bool useEndpoint)
    {
        m_isReference = true;
        m_localIndex = localIndex;
        m_vertexCode = vertexCode;
        m_referenceCellCoordinate = cellCoordinate;
        m_referenceUseEndpoint = useEndpoint;
        m_referenceLocalVertexIndex = -1;

        m_value = Vector3Int.zero;
    }
    
    public VoxelCellVertex(int localIndex, ushort vertexCode, Vector3Int cellCoordinate, int localVertexIndex)
    {
        m_isReference = true;
        m_localIndex = localIndex;
        m_vertexCode = vertexCode;
        m_referenceCellCoordinate = cellCoordinate;
        m_referenceLocalVertexIndex = localVertexIndex;
        m_referenceUseEndpoint = false;

        m_value = Vector3Int.zero;
    }

    public VoxelCellVertex(int localIndex, ushort vertexCode, Vector3Int value)
    {
        m_localIndex = localIndex;
        m_vertexCode = vertexCode;
        m_value = value;
        
        m_isReference = false;
        m_referenceUseEndpoint = false;
        m_referenceCellCoordinate = Vector3Int.zero;
        m_referenceLocalVertexIndex = -1;
    }
}
