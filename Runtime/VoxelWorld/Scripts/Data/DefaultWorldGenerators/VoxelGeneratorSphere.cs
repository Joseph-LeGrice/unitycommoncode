using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;


[System.Serializable]
public class VoxelGeneratorSphere : IVoxelGeneratorLayer
{
    [BurstCompile]
    public struct Worker : IJob
    {
        public float3 m_sphereOrigin;
        public float m_sphereRadius;
        public ChunkBuildData m_buildData;
        
        public void Execute()
        {
            for (int x = 0; x < m_buildData.m_chunkSize.x; x++)
            {
                for (int z = 0; z < m_buildData.m_chunkSize.z; z++)
                {
                    for (int y = 0; y < m_buildData.m_chunkSize.y; y++)
                    {
                        float3 worldPoint = m_buildData.LocalToGlobalVoxelCoordinate(x, y, z);
                        float distance = 1.0f - math.clamp(math.distance(worldPoint, m_sphereOrigin) - m_sphereRadius, 0.0f, 1.0f);
                        sbyte voxelValue = (sbyte)(int)math.lerp(-128, 127, distance);
                        m_buildData.SetValue(voxelValue, x, y, z);
                    }
                }
            }
        }
    }

    [SerializeField]
    private Vector3 m_origin;
    [SerializeField]
    private float m_radius;

    public VoxelGeneratorSphere()
    {
        m_origin = Vector3.zero;
        m_radius = 1.0f;
    }

    public VoxelGeneratorSphere(Vector3 origin, float radius)
    {
        m_origin = origin;
        m_radius = radius;
    }
    
    public override JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency)
    {
        Worker worker = new Worker()
        {
            m_buildData = buildData,
            m_sphereOrigin = this.m_origin,
            m_sphereRadius = this.m_radius,
        };
        return worker.Schedule(dependency);
    }

    protected override void CreateGUIInternal(VisualElement root)
    {
        Vector3Field sphereOrigin = new Vector3Field("Sphere Origin");
        sphereOrigin.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_origin))
        });
        root.Add(sphereOrigin);
        FloatField sphereRadius = new FloatField("Sphere Radius");
        sphereRadius.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_radius))
        });
        root.Add(sphereRadius);
    }
}
