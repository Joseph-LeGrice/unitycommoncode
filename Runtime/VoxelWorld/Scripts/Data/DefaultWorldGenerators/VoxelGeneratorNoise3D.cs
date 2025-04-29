using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;


// https://gamedev.stackexchange.com/questions/85873/implicit-functions-and-extracting-an-isosurface
// https://www.google.com/search?client=firefox-b-d&q=sdf+from+simplex+noise
// https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu
// https://github.com/Zylann/godot_voxel/blob/master/generators/simple/voxel_generator_noise.cpp (line 234 ish)
// https://voxel-tools.readthedocs.io/en/latest/smooth_terrain/
// https://en.wikipedia.org/wiki/Gaussian_blur
// https://hackernoon.com/how-to-implement-gaussian-blur-zw28312m

[System.Serializable]
public class VoxelGeneratorNoise3D : IVoxelGeneratorLayer
{
    [BurstCompile]
    public struct Worker : IJob
    {
        public float3 m_noiseScale;
        public ChunkBuildData m_buildData;
        
        public void Execute()
        {
            for (int x = 0; x < m_buildData.m_chunkSize.x; x++)
            {
                for (int z = 0; z < m_buildData.m_chunkSize.z; z++)
                {
                    for (int y = 0; y < m_buildData.m_chunkSize.y; y++)
                    {
                        float3 point = new float3(
                            m_noiseScale.x * ((float)(m_buildData.m_chunkCoordinate.x * m_buildData.m_chunkSize.x + x) / (m_buildData.m_worldDimensions.x * m_buildData.m_chunkSize.x)),
                            m_noiseScale.y * ((float)(m_buildData.m_chunkCoordinate.y * m_buildData.m_chunkSize.y + y) / (m_buildData.m_worldDimensions.y * m_buildData.m_chunkSize.y)),
                            m_noiseScale.z * ((float)(m_buildData.m_chunkCoordinate.z * m_buildData.m_chunkSize.z + z) / (m_buildData.m_worldDimensions.z * m_buildData.m_chunkSize.z))
                        );
                        
                        // TODO: Try noise.cellular2x2x2(); !!
                        // ng Danielsson’s distance transform 
                        // https://www.sciencedirect.com/science/article/abs/pii/0146664X80900544
                        // https://github.com/proog128/raymarching/blob/master/src/World.cpp
                        float noiseValue = math.abs(0.5f * (noise.snoise(point, out float3 gradient) + 1.0f));
                        noiseValue = math.round(noiseValue);
                        sbyte voxelValue = (sbyte)(int)math.lerp(-128, 127, noiseValue);
                        
                        int i = y * m_buildData.m_chunkSize.x * m_buildData.m_chunkSize.z + z * m_buildData.m_chunkSize.x + x;
                        m_buildData.m_data[i] = (byte)voxelValue;
                    }
                }
            }
        }
    }

    [SerializeField]
    private Vector3 m_noiseScale;

    public override JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency)
    {
        Worker worker = new Worker()
        {
            m_buildData = buildData,
            m_noiseScale = this.m_noiseScale,
        };
        return worker.Schedule(dependency);
    }

    protected override void CreateGUIInternal(VisualElement root)
    {
        Vector3Field noiseScale = new Vector3Field("Noise Scale");
        noiseScale.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_noiseScale))
        });
        root.Add(noiseScale);
    }
}
