using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

[CustomEditor(typeof(VoxelWorld))]
public class TransvoxelWorldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate World"))
        {
            ((VoxelWorld)target).RegenerateWorld();
            EditorUtility.SetDirty(target);
        }

        // ShiftCentreChunk("Shift Centre Chunk Forward", Vector3Int.forward);
        // ShiftCentreChunk("Shift Centre Chunk Left", Vector3Int.left);
        // ShiftCentreChunk("Shift Centre Chunk Back", Vector3Int.back);
        // ShiftCentreChunk("Shift Centre Chunk Right", Vector3Int.right);
        // ShiftCentreChunk("Shift Centre Chunk Up", Vector3Int.up);
        // ShiftCentreChunk("Shift Centre Chunk Down", Vector3Int.down);
    }

    // private void ShiftCentreChunk(string buttonName, Vector3Int displacement)
    // {
    //     if (GUILayout.Button(buttonName))
    //     {
    //         Vector3Int centreChunk = ((VoxelWorld)target).GetCentreChunk();
    //         centreChunk += displacement;
    //         ((VoxelWorld)target).LoadChunks(centreChunk);
    //         EditorUtility.SetDirty(target);
    //     }
    // }
}
