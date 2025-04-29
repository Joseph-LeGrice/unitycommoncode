using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoxelDebugger))]
public class VoxelDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Vector3Int rootVoxel = Vector3Int.zero;
        Vector3Int chunkCoordinate = Vector3Int.zero;
        Vector3Int localVoxel = Vector3Int.zero;

        VoxelWorld world = (VoxelWorld)serializedObject.FindProperty("m_world").objectReferenceValue;
        if (world != null)
        {
            rootVoxel = world.WorldPositionToVoxel(((VoxelDebugger)target).transform.position);
            chunkCoordinate = world.GetWorldData().GetChunkCoordinate(rootVoxel);
            localVoxel = world.GetWorldData().WorldToLocalVoxel(rootVoxel);
        }

        EditorGUILayout.LabelField("Current Voxel: " + rootVoxel);
        EditorGUILayout.LabelField("Current Chunk: " + chunkCoordinate);
        EditorGUILayout.LabelField("Current Voxel (Local): " + localVoxel);
        
        DrawDefaultInspector();

        if (GUILayout.Button("Sample Position"))
        {
            ((VoxelDebugger)target).SampleData();
        }
    }
}
