using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using ProtoBuf;

[System.Serializable]
[ProtoContract(SkipConstructor = true)]
public class CompressedVoxelData
{
    public static readonly int LZ4_COMPRESSION_LEVEL = 1;
    
    [SerializeField]
    [ProtoMember(1)]
    private byte[] m_encodedBytes;
    [SerializeField]
    [ProtoMember(2)]
    private int[] m_bytesWritten;
    [SerializeField]
    [ProtoMember(3)]
    private int[] m_bytesAccumulated;
    [SerializeField]
    [ProtoMember(4)]
    private Vector3Int m_worldDimensions;
    [SerializeField]
    [ProtoMember(5)]
    private int m_voxelChunkSize;

    public static CompressedVoxelData Load(string path)
    {
        ProtoVector3Int.Register();
        using FileStream fs = new FileStream(path, FileMode.Open);
        return Serializer.Deserialize<CompressedVoxelData>(fs);
    }

    public static CompressedVoxelData CreateNew(VoxelConstantData constantData)
    {
        int3 worldDimensions = new int3(constantData.GetWorldDimensions().x, constantData.GetWorldDimensions().y, constantData.GetWorldDimensions().z);
        int numChunks = worldDimensions.x * worldDimensions.y * worldDimensions.z;

        int3 chunkSize = new int3(constantData.GetChunkSize().x, constantData.GetChunkSize().y, constantData.GetChunkSize().z);
        int chunkSizeTotal = chunkSize.x * chunkSize.y * chunkSize.z;

        List<byte> allEncodedBytes = new List<byte>();
        int[] bytesAccumulated = new int[numChunks];

        const int batchCount = 64;
        int batchStartIndex = 0;
        int bytesEncodedLength = lz4.GetMaxSize(chunkSizeTotal, LZ4_COMPRESSION_LEVEL);

        NativeArray<byte> bytesRaw = new NativeArray<byte>(batchCount * chunkSizeTotal, Allocator.Persistent);
        NativeArray<byte> bytesEncoded = new NativeArray<byte>(batchCount * bytesEncodedLength, Allocator.Persistent);
        NativeArray<int> bytesWritten = new NativeArray<int>(numChunks, Allocator.Persistent);

        NativeList<JobHandle> currentBatch = new NativeList<JobHandle>(Allocator.Persistent);

        for (int i = 0; i < numChunks; i++)
        {
            int3 chunkCoordinate = new int3();
            chunkCoordinate.y = i / (worldDimensions.x * worldDimensions.z);
            chunkCoordinate.z = i % (worldDimensions.x * worldDimensions.z) / worldDimensions.x;
            chunkCoordinate.x = i % (worldDimensions.x * worldDimensions.z) % worldDimensions.x;

            int bytesRawStart = currentBatch.Length * chunkSizeTotal;
            NativeArray<byte> bytesRawSlice = bytesRaw.GetSubArray(bytesRawStart, chunkSizeTotal);
            
            ChunkBuildData chunkBuildData = new ChunkBuildData()
            {
                m_localToWorldMatrix = constantData.transform.localToWorldMatrix,
                m_worldDimensions = worldDimensions,
                m_chunkSize = chunkSize,
                m_chunkCoordinate = chunkCoordinate,
                m_data = bytesRawSlice
            };
            
            int bytesEncodedStart = currentBatch.Length * bytesEncodedLength;
            NativeArray<byte> bytesEncodedSlice = bytesEncoded.GetSubArray(bytesEncodedStart, bytesEncodedLength);

            WorldChunkCompressionJob compressionJob = new WorldChunkCompressionJob()
            {
                m_chunkIndex = i,
                m_bytesRaw = bytesRawSlice,
                m_bytesEncoded = bytesEncodedSlice,
                m_bytesWritten = bytesWritten,
            };
            
            JobHandle buildHandle = constantData.GetWorldGenerator().ScheduleBuild(chunkBuildData);
            buildHandle = compressionJob.Schedule(buildHandle);
            
            currentBatch.Add(buildHandle);

            if (currentBatch.Length == batchCount)
            {
                JobHandle.CompleteAll(currentBatch.AsArray());

                for (int batchIndex = 0; batchIndex < currentBatch.Length; batchIndex++)
                {
                    int chunkIndex = batchStartIndex + batchIndex;

                    if (bytesWritten[chunkIndex] > 0)
                    {
                        NativeArray<byte> bytesEncodedSubArray = bytesEncoded.GetSubArray(batchIndex * bytesEncodedLength, bytesWritten[chunkIndex]);
                        allEncodedBytes.AddRange(bytesEncodedSubArray.ToArray());
                    }

                    if (chunkIndex > 0)
                    {
                        bytesAccumulated[chunkIndex] = bytesAccumulated[chunkIndex - 1] + bytesWritten[chunkIndex - 1];
                    }
                }

                currentBatch.Clear();

                batchStartIndex = i + 1;
            }
        }

        if (currentBatch.Length > 0)
        {
            JobHandle.CompleteAll(currentBatch.AsArray());

            for (int batchIndex = 0; batchIndex < currentBatch.Length; batchIndex++)
            {
                int chunkIndex = batchStartIndex + batchIndex;
                if (chunkIndex > 0)
                {
                    bytesAccumulated[chunkIndex] = bytesAccumulated[chunkIndex - 1] + bytesWritten[chunkIndex - 1];
                }

                NativeArray<byte> bytesEncodedSubArray = bytesEncoded.GetSubArray(batchIndex * bytesEncodedLength, bytesWritten[chunkIndex]);
                allEncodedBytes.AddRange(bytesEncodedSubArray);
            }

            currentBatch.Clear();
        }

        CompressedVoxelData result = new CompressedVoxelData()
        {
            m_encodedBytes = allEncodedBytes.ToArray(),
            m_bytesWritten = bytesWritten.ToArray(),
            m_bytesAccumulated = bytesAccumulated,
            m_voxelChunkSize = chunkSizeTotal,
            m_worldDimensions = constantData.GetWorldDimensions(),
        };

        bytesRaw.Dispose();
        bytesEncoded.Dispose();
        bytesWritten.Dispose();
        currentBatch.Dispose();

        return result;
    }

    public sbyte[] GetCompressedChunkData(Vector3Int chunkCoordinate)
    {
        if (chunkCoordinate.x < 0 || chunkCoordinate.x >= m_worldDimensions.x  ||
            chunkCoordinate.y < 0 || chunkCoordinate.y >= m_worldDimensions.y ||
            chunkCoordinate.z < 0 || chunkCoordinate.z >= m_worldDimensions.z)
        {
            throw new IndexOutOfRangeException($"chunkCoord ({chunkCoordinate}) out of m_worldDimensions ({m_worldDimensions}) range");
        }
        
        int chunkIndex = chunkCoordinate.y * m_worldDimensions.x * m_worldDimensions.z + chunkCoordinate.z * m_worldDimensions.x + chunkCoordinate.x;
        
        sbyte[] result = new sbyte[m_voxelChunkSize];
        if (m_bytesWritten[chunkIndex] > 0 && m_bytesAccumulated[chunkIndex] < m_encodedBytes.Length)
        {
            byte[] bytesRaw = lz4.Decompress(m_encodedBytes.AsSpan(m_bytesAccumulated[chunkIndex], m_bytesWritten[chunkIndex]).ToArray());
            Buffer.BlockCopy(bytesRaw, 0, result, 0, m_voxelChunkSize);
        }
        
        return result;
    }
    
    public void Save(string path)
    {
        ProtoVector3Int.Register();
        using FileStream fs = new FileStream(path, FileMode.Create);
        Serializer.Serialize(fs, this);
    }
}
