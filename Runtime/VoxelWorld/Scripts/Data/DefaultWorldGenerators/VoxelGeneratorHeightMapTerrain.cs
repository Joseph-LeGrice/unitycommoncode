using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class VoxelGeneratorHeightMapTerrain : IVoxelGeneratorLayer
{
    [BurstCompile]
    public struct Worker : IJob
    {
        public float m_noiseAmplitude;
        public float m_noiseOffset;
        public float2 m_noiseScale;
        public ChunkBuildData m_buildData;
        
        public void Execute()
        {
            for (int x = 0; x < m_buildData.m_chunkSize.x; x++)
            {
                for (int z = 0; z < m_buildData.m_chunkSize.z; z++)
                {
                    float noiseX = m_noiseScale.x * ((float)(m_buildData.m_chunkCoordinate.x * m_buildData.m_chunkSize.x + x) / (m_buildData.m_worldDimensions.x * m_buildData.m_chunkSize.x));
                    float noiseZ = m_noiseScale.y * ((float)(m_buildData.m_chunkCoordinate.z * m_buildData.m_chunkSize.z + z) / (m_buildData.m_worldDimensions.z * m_buildData.m_chunkSize.z));

                    float n = 0.5f * (noise.cnoise(new float2(noiseX, noiseZ)) + 1.0f);
                    float xzHeightValue = m_noiseAmplitude * n + m_noiseOffset;

                    for (int y = 0; y < m_buildData.m_chunkSize.y; y++)
                    {
                        int yWorld = m_buildData.m_chunkCoordinate.y * m_buildData.m_chunkSize.y + y;
                        float heightDiff = xzHeightValue - yWorld;

                        float fractional = math.sign(heightDiff) * math.clamp(math.abs(heightDiff), 0.0f, 1.0f);
                        fractional = 0.5f * (fractional + 1.0f);
                        sbyte voxelValue = (sbyte)(int)math.round(math.lerp(-128, 127, fractional));

                        int i = y * m_buildData.m_chunkSize.x * m_buildData.m_chunkSize.z + z * m_buildData.m_chunkSize.x + x;
                        m_buildData.m_data[i] = (byte)voxelValue;
                    }
                }
            }
        }
    }
    
    [SerializeField]
    private float m_noiseAmplitude;
    [SerializeField]
    private float m_noiseOffset;
    [SerializeField]
    private Vector2 m_noiseScale;

    public override JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency)
    {
        Worker worker = new Worker()
        {
            m_buildData = buildData,
            m_noiseAmplitude = this.m_noiseAmplitude,
            m_noiseOffset = this.m_noiseOffset,
            m_noiseScale = this.m_noiseScale,
        };
        return worker.Schedule(dependency);
    }

    protected override void CreateGUIInternal(VisualElement root)
    {
        FloatField noiseAmplitude = new FloatField("Noise Amplitude");
        noiseAmplitude.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_noiseAmplitude))
        });
        root.Add(noiseAmplitude);
        FloatField noiseOffset = new FloatField("Noise Offset");
        noiseOffset.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_noiseOffset))
        });
        root.Add(noiseOffset);
        Vector2Field noiseScale = new Vector2Field("Noise Scale");
        noiseScale.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_noiseScale))
        });
        root.Add(noiseScale);
    }
}
