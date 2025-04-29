using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(VoxelConstantData))]
public class VoxelConstantDataEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        root.Add(new PropertyField(serializedObject.FindProperty("m_worldChunks")));
        root.Add(new PropertyField(serializedObject.FindProperty("m_chunkSize")));
        root.Add(new PropertyField(serializedObject.FindProperty("m_voxelScaleFactor")));
        root.Add(new PropertyField(serializedObject.FindProperty("m_fileName")));
        root.Add(new PropertyField(serializedObject.FindProperty("m_worldGenerator")));
        return root;
    }
}
