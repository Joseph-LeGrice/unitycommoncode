using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[System.Serializable]
public class VoxelGeneratorFlatTerrain : IVoxelGeneratorLayer
{
    [BurstCompile]
    public struct Worker : IJob
    {
        public float m_groundHeightNormalised;
        public ChunkBuildData m_buildData;

        public void Execute()
        {
            int groundY = (int)math.floor(m_groundHeightNormalised * m_buildData.m_worldDimensions.y);
            int3 voxelWorldOffset = m_buildData.m_chunkCoordinate * m_buildData.m_chunkSize;

            if (voxelWorldOffset.y < groundY && voxelWorldOffset.y + m_buildData.m_chunkSize.y >= groundY)
            {
                int toGroundHeightOffset = (groundY - voxelWorldOffset.y) * m_buildData.m_chunkSize.x * m_buildData.m_chunkSize.z;
                int remainder = ((voxelWorldOffset.y + m_buildData.m_chunkSize.y) - groundY) * m_buildData.m_chunkSize.x * m_buildData.m_chunkSize.z;
                m_buildData.m_data.FillArray((byte)0x7F, 0, toGroundHeightOffset);
                m_buildData.m_data.FillArray((byte)0x81, toGroundHeightOffset, remainder);
            }
            else if (voxelWorldOffset.y + m_buildData.m_chunkSize.y < groundY)
            {
                m_buildData.m_data.FillArray((byte)0x7F); // 127, below ground
            }
            else if (voxelWorldOffset.y >= groundY)
            {
                m_buildData.m_data.FillArray((byte)0x81); // -128, above ground
            }
        }
    }

    [SerializeField]
    private float m_groundHeightNormalised;

    public override JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency)
    {
        Worker worker = new Worker()
        {
            m_groundHeightNormalised = m_groundHeightNormalised,
            m_buildData = buildData,
        };
        return worker.Schedule(dependency);
    }

    protected override void CreateGUIInternal(VisualElement root)
    {
        FloatField groundHeight = new FloatField("Ground Height Normalised");
        groundHeight.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_groundHeightNormalised)),
        });
        root.Add(groundHeight);
    }
}
