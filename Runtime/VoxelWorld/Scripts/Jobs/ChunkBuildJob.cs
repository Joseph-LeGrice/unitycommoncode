using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

// Maybe should be this? https://docs.unity3d.com/Packages/com.unity.entities@0.17/manual/ecs_ijobentitybatch.html
// https://github.com/Zylann/godot_voxel/blob/master/meshers/transvoxel/transvoxel.cpp

[BurstCompile]
public struct ChunkBuildJob : IJob
{
    public Vector3Int m_chunkDimensions;
    public Vector3Int m_cachedDataSize;
    
    [ReadOnly]
    public NativeArray<sbyte> m_chunkData;
    
    [WriteOnly]
    public NativeList<int> m_indices;
    [WriteOnly]
    public NativeArray<int> m_indexCountPerCell;
    public NativeList<VoxelCellVertex> m_vertices;
    
    [WriteOnly]
    public NativeArray<int> m_vertexCountPerCell;
    
    public void Execute()
    {
        int numberOfCells = m_chunkDimensions.x * m_chunkDimensions.y * m_chunkDimensions.z;
        for (int cellIndex = 0; cellIndex < numberOfCells; cellIndex++)
        {
            Vector3Int cellCoordinate = new Vector3Int();
            cellCoordinate.y = cellIndex / (m_chunkDimensions.x * m_chunkDimensions.z);

            int xz = cellIndex - (cellCoordinate.y * m_chunkDimensions.x * m_chunkDimensions.z);
            cellCoordinate.x = xz % m_chunkDimensions.x;
            cellCoordinate.z = xz / m_chunkDimensions.x;

            if (HasVertices(cellCoordinate))
            {
                byte caseCode = GetCaseCode(ref cellCoordinate);

                byte equivalenceClassIndex = TransvoxelConsts.RegularCellClass[caseCode];
                RegularCellData equivalenceClass = TransvoxelConsts.RegularCellData[equivalenceClassIndex];
                
                int thisIndexCount = 3 * equivalenceClass.GetTriangleCount();
                for (int i=0; i<thisIndexCount; i++)
                {
                    m_indices.Add(equivalenceClass[i]);
                }

                int lastVerticesLength = m_vertices.Length;
                
                GenerateVertices(cellCoordinate);
                
                m_vertexCountPerCell[cellIndex] = m_vertices.Length - lastVerticesLength;
                m_indexCountPerCell[cellIndex] = thisIndexCount;
            }
        }
    }
    
     public bool IsInBounds(Vector3Int cell)
     {
         return cell.x >= 0 && cell.x < m_chunkDimensions.x
             && cell.y >= 0 && cell.y < m_chunkDimensions.y
             && cell.z >= 0 && cell.z < m_chunkDimensions.z;
     }
    
     public byte GetCaseCode(ref Vector3Int voxel)
     {
         Vector3Int corner0 = new Vector3Int(voxel.x,     voxel.y,     voxel.z    );
         Vector3Int corner1 = new Vector3Int(voxel.x + 1, voxel.y,     voxel.z    );
         Vector3Int corner2 = new Vector3Int(voxel.x,     voxel.y,     voxel.z + 1);
         Vector3Int corner3 = new Vector3Int(voxel.x + 1, voxel.y,     voxel.z + 1);
         Vector3Int corner4 = new Vector3Int(voxel.x,     voxel.y + 1, voxel.z    );
         Vector3Int corner5 = new Vector3Int(voxel.x + 1, voxel.y + 1, voxel.z    );
         Vector3Int corner6 = new Vector3Int(voxel.x,     voxel.y + 1, voxel.z + 1);
         Vector3Int corner7 = new Vector3Int(voxel.x + 1, voxel.y + 1, voxel.z + 1);

         return (byte)(((m_chunkData[GetIndex(corner0)] >> 7) & 0x01)
             | ((m_chunkData[GetIndex(corner1)] >> 6) & 0x02)
             | ((m_chunkData[GetIndex(corner2)] >> 5) & 0x04)
             | ((m_chunkData[GetIndex(corner3)] >> 4) & 0x08)
             | ((m_chunkData[GetIndex(corner4)] >> 3) & 0x10)
             | ((m_chunkData[GetIndex(corner5)] >> 2) & 0x20)
             | ((m_chunkData[GetIndex(corner6)] >> 1) & 0x40)
             | (m_chunkData[GetIndex(corner7)] & 0x80));
     }

     private int GetIndex(Vector3Int localCoordinate)
     {
         return localCoordinate.y * m_cachedDataSize.z * m_cachedDataSize.x +
             localCoordinate.z * m_cachedDataSize.x +
             localCoordinate.x;
     }
     
    public bool HasVertices(Vector3Int cellCoordinate)
    {
        //perform a signed right shift on one of the sample values to fill a byte with
        //copies of its sign bit, and then exclusive OR that byte with the case index.
        //fails for values of 0 and 255
        byte caseCode = GetCaseCode(ref cellCoordinate);
        return (caseCode ^ ((GetCorner(cellCoordinate, 7) >> 7) & 0xFF)) != 0;
    }

    private sbyte GetCorner(Vector3Int cellCoordinate, int cornerIndex)
    {
        Vector3Int cornerOffset = new Vector3Int();
        cornerOffset.y = cornerIndex / 4;
        cornerOffset.x = (cornerIndex % 4) % 2;
        cornerOffset.z = (cornerIndex % 4) / 2;
        Vector3Int voxelCoordinate = cellCoordinate + cornerOffset;
        return m_chunkData[GetIndex(voxelCoordinate)];
    }

    private void GenerateVertices(Vector3Int cellCoordinate)
    {
        byte caseCode = GetCaseCode(ref cellCoordinate);
        byte equivalenceClassIndex = TransvoxelConsts.RegularCellClass[caseCode];
        RegularCellData equivalenceClass = TransvoxelConsts.RegularCellData[equivalenceClassIndex];
        
        for (int localVertexIndex = 0; localVertexIndex < equivalenceClass.GetVertexCount(); localVertexIndex++)
        {
            ushort vertexLocation = TransvoxelConsts.RegularVertexData[caseCode][localVertexIndex];

            byte code = (byte)(vertexLocation >> 8);

            ushort v0 = (ushort)((vertexLocation >> 4) & 0x0F);
            ushort v1 = (ushort)(vertexLocation & 0x0F);

            long d0 = GetCorner(cellCoordinate, v0);
            long d1 = GetCorner(cellCoordinate, v1);

            long t = (d1 << 8) / (d1 - d0);

            if ((t & 0x00FF) != 0)
            {
                //Vertex lies in the interior of the edge.
                Vector3Int p0 = new Vector3Int(
                    cellCoordinate.x + v0 % 2,
                    cellCoordinate.y + v0 / 4,
                    cellCoordinate.z + ((v0 % 4) / 2)
                );

                Vector3Int p1 = new Vector3Int(
                    cellCoordinate.x + v1 % 2,
                    cellCoordinate.y + v1 / 4,
                    cellCoordinate.z + ((v1 % 4) / 2)
                );

                long u = 0x0100 - t;
                Vector3Int vertexPos = (int)t * p0 + (int)u * p1;
                m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, vertexPos));
                continue;
            }
            else if (t == 0)
            {
                // Vertex lies at the higher-numbered endpoint (p0)
                if (v1 == 7)
                {
                    // This cell owns the vertex
                    Vector3Int vertexPos = new Vector3Int(
                        256 * (cellCoordinate.x + 1),
                        256 * (cellCoordinate.y + 1),
                        256 * (cellCoordinate.z + 1)
                    );

                    m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, vertexPos));
                    continue;
                }
            }

            if (t == 0 || t == 256)
            {
                int iv0 = v0 ^ 7;
                int xDiff = (iv0 >> 0) & 0x01;
                int zDiff = (iv0 >> 1) & 0x01;
                int yDiff = (iv0 >> 2) & 0x01;

                Vector3Int reuseCell = cellCoordinate;
                reuseCell.x -= xDiff;
                reuseCell.y -= yDiff;
                reuseCell.z -= zDiff;

                bool isInBounds = IsInBounds(reuseCell);
                if (!isInBounds || !HasVertices(reuseCell))
                {
                    Vector3Int p0 = new Vector3Int(
                        cellCoordinate.x + v0 % 2,
                        cellCoordinate.y + v0 / 4,
                        cellCoordinate.z + ((v0 % 4) / 2)
                    );

                    Vector3Int p1 = new Vector3Int(
                        cellCoordinate.x + v1 % 2,
                        cellCoordinate.y + v1 / 4,
                        cellCoordinate.z + ((v1 % 4) / 2)
                    );

                    long u = 0x0100 - t;
                    Vector3Int vertexPos = (int)t * p0 + (int)u * p1;
                    m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, vertexPos));
                }
                else
                {
                    m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, reuseCell, true));
                }
            }
            else
            {
                int xDiff = (vertexLocation >> 12) & 0x01;
                int zDiff = (vertexLocation >> 13) & 0x01;
                int yDiff = (vertexLocation >> 14) & 0x01;
                int reuseVertexIndex = (vertexLocation >> 8) & 0x0F;

                Vector3Int reuseCell = cellCoordinate;
                reuseCell.x -= xDiff;
                reuseCell.y -= yDiff;
                reuseCell.z -= zDiff;
                bool isInBounds = IsInBounds(reuseCell);

                // if (((vertexLocation >> 15) & 0x01) == 1) // Create new vertex for this cell?
                // {
                //     Vector3Int vertexPos = new Vector3Int(
                //         256 * (cellCoordinate.x + 1),
                //         256 * (cellCoordinate.y + 1),
                //         256 * (cellCoordinate.z + 1)
                //     );
                //
                //     vertices[localVertexIndex] = vertexPos;
                // }

                if (!isInBounds)
                {
                    Vector3Int p0 = new Vector3Int(
                        cellCoordinate.x + v0 % 2,
                        cellCoordinate.y + v0 / 4,
                        cellCoordinate.z + ((v0 % 4) / 2)
                    );

                    Vector3Int p1 = new Vector3Int(
                        cellCoordinate.x + v1 % 2,
                        cellCoordinate.y + v1 / 4,
                        cellCoordinate.z + ((v1 % 4) / 2)
                    );

                    long u = 0x0100 - t;
                    Vector3Int vertexPos = (int)t * p0 + (int)u * p1;
                    m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, vertexPos));
                }
                else
                {
                    m_vertices.Add(new VoxelCellVertex(localVertexIndex, code, reuseCell, reuseVertexIndex));
                }
            }

            // if (vertices[localVertexIndex] == null)
            // {
            //     Debug.Log("error null vertex");
            // }
        }
    }
}