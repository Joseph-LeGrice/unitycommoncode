using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;


[System.Serializable]
public class VoxelWorldGenerator
{
    [SerializeReference]
    private List<IVoxelGeneratorLayer> m_layers = new List<IVoxelGeneratorLayer>();

    public void ClearLayers()
    {
        m_layers.Clear();
    }

    public void AddLayer<T>(T layer) where T : IVoxelGeneratorLayer
    {
        m_layers.Add(layer);
    }
    
    public JobHandle ScheduleBuild(ChunkBuildData buildData)
    {
        // TODO: Figure out additive/subtractive blending between layers
        // TODO: Custom drawer to allow selection of various types in the m_layers array
        
        JobHandle dependency = default(JobHandle);
        foreach (IVoxelGeneratorLayer layer in m_layers)
        {
            if (layer.IsEnabled())
            {
                dependency = layer.ScheduleJob(buildData, dependency);
            }
        }
        return dependency;
    }
}

