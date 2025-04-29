using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public struct ChunkBuildData
{
    public float4x4 m_localToWorldMatrix;
    public int3 m_worldDimensions;
    public int3 m_chunkSize;
    public int3 m_chunkCoordinate;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> m_data;

    public static readonly int VOXEL_OUTSIDE_TERRAIN = -128;
    public static readonly int VOXEL_INSIDE_TERRAIN = 127;
    
    public float3 LocalVoxelToWorldPosition(int3 localVoxelCoordinate)
    {
        return LocalVoxelToWorldPosition(localVoxelCoordinate.x, localVoxelCoordinate.y, localVoxelCoordinate.z);
    }

    public float3 LocalVoxelToWorldPosition(int xLocal, int yLocal, int zLocal)
    {
        float4 globalCoordinate = new float4(LocalToGlobalVoxelCoordinate(xLocal, yLocal, zLocal), 0.0f);
        return math.mul(m_localToWorldMatrix, globalCoordinate).xyz;
    }

    public int3 LocalToGlobalVoxelCoordinate(int3 localVoxelCoordinate)
    {
        return LocalToGlobalVoxelCoordinate(localVoxelCoordinate.x, localVoxelCoordinate.y, localVoxelCoordinate.z);
    }
    
    public int3 LocalToGlobalVoxelCoordinate(int xLocal, int yLocal, int zLocal)
    {
        return new int3(
            m_chunkCoordinate.x * m_chunkSize.x + xLocal,
            m_chunkCoordinate.y * m_chunkSize.y + yLocal,
            m_chunkCoordinate.z * m_chunkSize.z + zLocal
        );
    }
    
    public void SetValue(sbyte voxelValue, int xLocal, int yLocal, int zLocal)
    {
        int i = yLocal * m_chunkSize.x * m_chunkSize.z + zLocal * m_chunkSize.x + xLocal;
        m_data[i] = (byte)voxelValue;
    }
}

[System.Serializable]
public abstract class IVoxelGeneratorLayer
{
    [SerializeField]
    private bool m_enabled = true;

    public bool IsEnabled()
    {
        return m_enabled;
    }

    public abstract JobHandle ScheduleJob(ChunkBuildData buildData, JobHandle dependency);

    public VisualElement CreateGUI()
    {
        VisualElement root = new VisualElement();
        Toggle enabledField = new Toggle("Enabled");
        enabledField.SetBinding("value", new DataBinding()
        {
            dataSource = this,
            dataSourcePath = PropertyPath.FromName(nameof(m_enabled)),
        });
        root.Add(enabledField);
        CreateGUIInternal(root);
        return root;
    }

    protected virtual void CreateGUIInternal(VisualElement root)
    {
    }
}