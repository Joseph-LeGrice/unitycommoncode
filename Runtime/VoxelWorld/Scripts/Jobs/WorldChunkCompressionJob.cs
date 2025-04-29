using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct WorldChunkCompressionJob : IJob
{
    public int m_chunkIndex;
    
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<int> m_bytesWritten;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> m_bytesRaw;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> m_bytesEncoded;
    
    public void Execute()
    {
        m_bytesWritten[m_chunkIndex] = lz4.Compress(m_bytesRaw, m_bytesEncoded, CompressedVoxelData.LZ4_COMPRESSION_LEVEL);
    }
}

