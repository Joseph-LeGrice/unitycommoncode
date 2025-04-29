using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct MeshBuildJob : IJob
{
    public float m_scaleFactor;
    public Vector3Int m_chunkDimensions;
    
    [ReadOnly]
    public NativeList<int> m_indices;
    [ReadOnly]
    public NativeArray<int> m_indexCountPerCell;
    [ReadOnly]
    public NativeList<VoxelCellVertex> m_vertices;
    [ReadOnly]
    public NativeArray<int> m_vertexCountPerCell;

    public NativeList<Vector3> m_realVertices;
    [WriteOnly]
    public NativeList<int> m_realIndices;

    public void Execute()
    {
        int numberOfCells = m_chunkDimensions.x * m_chunkDimensions.y * m_chunkDimensions.z;
        
        NativeList<int> realVertexLookup = new NativeList<int>(numberOfCells, Allocator.Temp);
        NativeArray<int> vertexCountAccumulated = new NativeArray<int>(numberOfCells, Allocator.Temp);
        NativeArray<int> indexCountAccumulated = new NativeArray<int>(numberOfCells, Allocator.Temp);

        for (int i = 1; i < numberOfCells; i++)
        {
            vertexCountAccumulated[i] = vertexCountAccumulated[i - 1] + m_vertexCountPerCell[i - 1];
            indexCountAccumulated[i] = indexCountAccumulated[i - 1] + m_indexCountPerCell[i - 1];
        }
        
        for (int i = 0; i < m_vertices.Length; i++)
        {
            VoxelCellVertex v = m_vertices[i];
            if (!v.m_isReference)
            {
                Vector3 normalisedVert = v.m_value;
                normalisedVert.x /= 256.0f * m_chunkDimensions.x;
                normalisedVert.y /= 256.0f * m_chunkDimensions.y;
                normalisedVert.z /= 256.0f * m_chunkDimensions.z;
                normalisedVert.x *= m_scaleFactor * m_chunkDimensions.x;
                normalisedVert.y *= m_scaleFactor * m_chunkDimensions.y;
                normalisedVert.z *= m_scaleFactor * m_chunkDimensions.z;
                m_realVertices.Add(normalisedVert);

                realVertexLookup.Add(m_realVertices.Length - 1);
            }
            else
            {
                int realVertexCandidateIndex = i;
                VoxelCellVertex realVertexCandidate = v;
                while (realVertexCandidate.m_isReference)
                {
                    int cellIndex = realVertexCandidate.m_referenceCellCoordinate.y * m_chunkDimensions.z * m_chunkDimensions.x + realVertexCandidate.m_referenceCellCoordinate.z * m_chunkDimensions.x + realVertexCandidate.m_referenceCellCoordinate.x;
                
                    if (realVertexCandidate.m_referenceUseEndpoint) // Should this even be a thing?
                    {
                        bool found = false;
                        for (int localVertexI=0; localVertexI<m_vertexCountPerCell[cellIndex]; localVertexI++)
                        {
                            int checkIndex = vertexCountAccumulated[cellIndex] + localVertexI;
                            VoxelCellVertex vv = m_vertices[checkIndex];
                            if (((vv.m_vertexCode >> 7) & 0x01) == 1)
                            {
                                realVertexCandidate = vv;
                                realVertexCandidateIndex = checkIndex;
                                found = true;
                                break;
                            }
                        }
                
                        if (!found)
                        {
                            // throw new ArgumentException("Could not find referenced Vertex");
                            break;
                        }
                    }
                    else
                    {
                        realVertexCandidateIndex = vertexCountAccumulated[cellIndex] + realVertexCandidate.m_referenceLocalVertexIndex;
                        realVertexCandidate = m_vertices[realVertexCandidateIndex];
                    }
                }

                if (realVertexCandidate.m_isReference)
                {
                    realVertexCandidateIndex = 0;
                }
                realVertexLookup.Add(realVertexLookup[realVertexCandidateIndex]);
            }
        }

        int currentCellIndex = 0;
        for (int i = 0; i < m_indices.Length; i++)
        {
            while (currentCellIndex < numberOfCells - 1 && i >= indexCountAccumulated[currentCellIndex + 1])
            {
                currentCellIndex++;
            }

            int localVertexIndex = m_indices[i];
            int cellVertexCountOffset = vertexCountAccumulated[currentCellIndex];
            m_realIndices.Add(realVertexLookup[cellVertexCountOffset + localVertexIndex]);
        }
    }
}