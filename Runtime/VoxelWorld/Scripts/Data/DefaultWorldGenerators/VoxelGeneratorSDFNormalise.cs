using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

// https://mshgrid.com/2021/02/04/the-fast-sweeping-algorithm/

[System.Serializable]
public class VoxelGeneratorSDFNormalise : IVoxelGeneratorLayer
{
    [BurstCompile]
    public struct Worker : IJob
    {
        public ChunkBuildData m_buildData;
        
        public void Execute()
        {
            // for (int x = 0; x < m_buildData.m_chunkSize.x; x++)
            // {
            //     for (int z = 0; z < m_buildData.m_chunkSize.z; z++)
            //     {
            //         for (int y = 0; y < m_buildData.m_chunkSize.y; y++)
            //         {
            //             float3 point = new float3(
            //                 m_noiseScale.x * ((float)(m_buildData.m_chunkCoordinate.x * m_buildData.m_chunkSize.x + x) / (m_buildData.m_worldDimensions.x * m_buildData.m_chunkSize.x)),
            //                 m_noiseScale.y * ((float)(m_buildData.m_chunkCoordinate.y * m_buildData.m_chunkSize.y + y) / (m_buildData.m_worldDimensions.y * m_buildData.m_chunkSize.y)),
            //                 m_noiseScale.z * ((float)(m_buildData.m_chunkCoordinate.z * m_buildData.m_chunkSize.z + z) / (m_buildData.m_worldDimensions.z * m_buildData.m_chunkSize.z))
            //             );
            //             float noiseValue = math.abs(0.5f * (noise.snoise(point, out float3 gradient) + 1.0f));
            //             // noiseValue = math.round(noiseValue);
            //             sbyte voxelValue = (sbyte)(int)math.lerp(-128, 127, noiseValue);
            //             
            //             int i = y * m_buildData.m_chunkSize.x * m_buildData.m_chunkSize.z + z * m_buildData.m_chunkSize.x + x;
            //             m_buildData.m_data[i] = (byte)voxelValue;
            //         }
            //     }
            // }
            
            // const int height = imgPar->height, width = imgPar->width;
            // const int row = imgPar->row;
            // 
            // const int NSweeps = 4;
            // // sweep directions { start, end, step }
            // const int dirX[NSweeps][3] = { {0, width - 1, 1} , {width - 1, 0, -1}, {width - 1, 0, -1}, {0, width - 1, 1} };
            // const int dirY[NSweeps][3] = { {0, height - 1, 1}, {0, height - 1, 1}, {height - 1, 0, -1}, {height - 1, 0, -1} };
            // double aa[2], eps = 1e-6;
            // double d_new, a, b;
            // int s, ix, iy, gridPos;
            // const double h = 1.0, f = 1.0;
            // 
            // for (s = 0; s < NSweeps; s++) {
            // 
            //     for (iy = dirY[s][0]; dirY[s][2] * iy <= dirY[s][1]; iy += dirY[s][2]) {
            //         for (ix = dirX[s][0]; dirX[s][2] * ix <= dirX[s][1]; ix += dirX[s][2]) {
            // 
            //             gridPos = iy * row + ix;
            // 
            //             if (!frozenCells[gridPos]) {
            // 
            //                 // === neighboring cells (Upwind Godunov) ===
            //                 if (iy == 0 || iy == (height - 1)) {
            //                     if (iy == 0) {
            //                         aa[1] = distGrid[gridPos] < distGrid[(iy + 1) * row + ix] ? distGrid[gridPos] : distGrid[(iy + 1) * row + ix];
            //                     }
            //                     if (iy == (height - 1)) {
            //                         aa[1] = distGrid[(iy - 1) * row + ix] < distGrid[gridPos] ? distGrid[(iy - 1) * row + ix] : distGrid[gridPos];
            //                     }
            //                 }
            //                 else {
            //                     aa[1] = distGrid[(iy - 1) * row + ix] < distGrid[(iy + 1) * row + ix] ? distGrid[(iy - 1) * row + ix] : distGrid[(iy + 1) * row + ix];
            //                 }
            // 
            //                 if (ix == 0 || ix == (width - 1)) {
            //                     if (ix == 0) {
            //                         aa[0] = distGrid[gridPos] < distGrid[iy * row + (ix + 1)] ? distGrid[gridPos] : distGrid[iy * row + (ix + 1)];
            //                     }
            //                     if (ix == (width - 1)) {
            //                         aa[0] = distGrid[iy * row + (ix - 1)] < distGrid[gridPos] ? distGrid[iy * row + (ix - 1)] : distGrid[gridPos];
            //                     }
            //                 }
            //                 else {
            //                     aa[0] = distGrid[iy * row + (ix - 1)] < distGrid[iy * row + (ix + 1)] ? distGrid[iy * row + (ix - 1)] : distGrid[iy * row + (ix + 1)];
            //                 }
            // 
            //                 a = aa[0]; b = aa[1];
            //                 d_new = (fabs(a - b) < f * h ? (a + b + sqrt(2.0 * f * f * h * h - (a - b) * (a - b))) * 0.5 : std::fminf(a, b) + f * h);
            // 
            //                 distGrid[gridPos] = distGrid[gridPos] < d_new ? distGrid[gridPos] : d_new;
            //             }
            //         }
            //     }
            // }
        }
    }

    [SerializeField]
    private Vector3 m_noiseScale;

    public override JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency)
    {
        Worker worker = new Worker()
        {
            m_buildData = buildData,
        };
        return worker.Schedule(dependency);
    }
}
