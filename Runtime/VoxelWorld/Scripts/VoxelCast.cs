using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

//TODO: Voxel Casts
public static class VoxelCast
{
    // public void DoVoxelCast(Vector3 worldPosition, Vector3 direction)
    // {
    //     Vector3 voxelOrigin = WorldToVoxel(worldPosition);
    //     
    //     RaycastHit hitInfo;
    //     bool didHit = Physics.Raycast(voxelOrigin, direction, out hitInfo);
    //     if (didHit)
    //     {
    //         VoxelWorldChunk worldChunk = hitInfo.collider.GetComponent<VoxelWorldChunk>();
    //         if (worldChunk != null)
    //         {
    //             // hitInfo.point
    //             Debug.Log("Hit chunk");
    //         }
    //     }
    // }
    //
    // public List<VoxelRange> GetVoxelsInSphere(Vector3 worldPosition, float radius)
    // {
    //     Vector3 voxelOrigin = WorldToVoxel(worldPosition);
    //
    //     List<VoxelRange> result = new List<VoxelRange>();
    //     if (radius < 1.0f)
    //     {
    //         return result;
    //     }
    //     
    //     int minY = Mathf.CeilToInt(voxelOrigin.y - radius);
    //     int maxY = Mathf.FloorToInt(voxelOrigin.y + radius);
    //  
    //     for (int y = minY; y <= maxY; y++)
    //     {
    //         float a = Mathf.Acos((y - voxelOrigin.y) / radius);
    //         float opp = radius * Mathf.Sin(a);
    //
    //         int offsetXZ = Mathf.FloorToInt(opp);
    //         
    //         Vector3Int min = new Vector3Int();
    //         min.x = Mathf.CeilToInt(voxelOrigin.x - offsetXZ);
    //         min.y = y;
    //         min.z = Mathf.CeilToInt(voxelOrigin.z - offsetXZ);
    //         Vector3Int max = new Vector3Int();
    //         max.x = Mathf.FloorToInt(voxelOrigin.x + offsetXZ);
    //         max.y = y;
    //         max.z = Mathf.FloorToInt(voxelOrigin.z + offsetXZ);
    //
    //         VoxelRange vr = new VoxelRange(min, max - min);
    //         if (vr.IsValidRange())
    //         {
    //             result.Add(vr);
    //         }
    //     }
    //     return result;
    // }
    //
    // public VoxelRange GetVoxelsInBox(Vector3 worldPosition, Vector3 size)
    // {
    //     Vector3 voxelOrigin = WorldToVoxel(worldPosition);
    //     Vector3Int min = new Vector3Int();
    //     min.x = Mathf.CeilToInt(voxelOrigin.x);
    //     min.y = Mathf.CeilToInt(voxelOrigin.z);
    //     min.z = Mathf.CeilToInt(voxelOrigin.z);
    //     Vector3Int max = new Vector3Int();
    //     max.x = Mathf.FloorToInt(voxelOrigin.x + size.x);
    //     max.y = Mathf.FloorToInt(voxelOrigin.y + size.y);
    //     max.z = Mathf.FloorToInt(voxelOrigin.z + size.z);
    //     return new VoxelRange(min, max - min);
    // }
    //
    // public void ModifyRanges(List<VoxelRange> ranges, sbyte value)
    // {
    //     foreach (VoxelRange r in ranges)
    //     {
    //         // m_data.SetBlock(r, -127);
    //         // m_blockVoxelWorldData.ModifyRange(r, value);
    //     }
    // }
}
