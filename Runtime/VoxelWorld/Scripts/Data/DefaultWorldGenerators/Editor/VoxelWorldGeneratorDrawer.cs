using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(VoxelWorldGenerator))]
public class VoxelWorldGeneratorDrawer : PropertyDrawer
{
    private List<Type> m_layerTypes;
    private List<string> m_layerTypeNames;

    public VoxelWorldGeneratorDrawer()
    {
        m_layerTypes = new List<Type>();
        m_layerTypeNames = new List<string>();
        AddLayer(typeof(VoxelGeneratorFlatTerrain));
        AddLayer(typeof(VoxelGeneratorHeightMapTerrain));
        AddLayer(typeof(VoxelGeneratorNoise3D));
        AddLayer(typeof(VoxelGeneratorSDFNormalise));
        AddLayer(typeof(VoxelGeneratorSphere));
    }

    private void AddLayer(Type layerType)
    {
        m_layerTypes.Add(layerType);
        m_layerTypeNames.Add(layerType.Name);
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();
        ListView listView = new ListView();
        listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        listView.reorderable = true;
        listView.allowAdd = false;
        listView.allowRemove = false;
        listView.showBoundCollectionSize = false;
        listView.makeItem = () => CreateElement(listView, property);
        listView.bindItem = (element, i) => BindElement(element, i, property);
        listView.bindingPath = property.FindPropertyRelative("m_layers").propertyPath;
        root.Add(listView);
        Button addLayerButton = new Button(() => AddLayer(property));
        addLayerButton.text = "Add Layer";
        root.Add(addLayerButton);
        return root;
    }

    private VisualElement CreateElement(ListView listView, SerializedProperty property)
    {
        VisualElement layer = new VisualElement();
        
        DropdownField layerTypeDropdown = new DropdownField("Layer Type", m_layerTypeNames, 0);
        layerTypeDropdown.RegisterValueChangedCallback((evt) => OnLayerTypeChanged(listView, property, evt, layer));
        layer.Add(layerTypeDropdown);

        VisualElement dataContainer = new VisualElement();
        dataContainer.name = "data";
        layer.Add(dataContainer);
        
        Button removeLayerButton = new Button(() => RemoveLayer(property, layer));
        removeLayerButton.text = "Remove";
        layer.Add(removeLayerButton);
        return layer;
    }

    private void BindElement(VisualElement element, int listIndex, SerializedProperty property)
    {
        property.serializedObject.Update();

        element.userData = listIndex;
        
        SerializedProperty existingLayers = property.FindPropertyRelative("m_layers");
        SerializedProperty layer = existingLayers.GetArrayElementAtIndex(listIndex);

        VisualElement dataContainer = element.Q<VisualElement>("data");
        dataContainer.Clear();

        if (layer.managedReferenceValue != null)
        {
            VisualElement dataGui = ((IVoxelGeneratorLayer)layer.managedReferenceValue).CreateGUI();
            dataContainer.Add(dataGui);
            
            DropdownField layerTypeDropdown = element.Q<DropdownField>();
            int layerTypeIndex = m_layerTypes.IndexOf(layer.managedReferenceValue.GetType());
            layerTypeDropdown.value = m_layerTypeNames[layerTypeIndex];
        }
    }

    private void AddLayer(SerializedProperty property)
    {
        SerializedProperty existingLayers = property.FindPropertyRelative("m_layers");
        existingLayers.arraySize++;
        existingLayers.GetArrayElementAtIndex(existingLayers.arraySize - 1).managedReferenceValue = (IVoxelGeneratorLayer)Activator.CreateInstance(m_layerTypes[0]);
        property.serializedObject.ApplyModifiedProperties();
    }

    private void RemoveLayer(SerializedProperty property, VisualElement element)
    {
        int layerIndex = (int)element.userData;
        SerializedProperty existingLayers = property.FindPropertyRelative("m_layers");
        existingLayers.DeleteArrayElementAtIndex(layerIndex);
        property.serializedObject.ApplyModifiedProperties();
    }

    private void OnLayerTypeChanged(ListView listView, SerializedProperty property, ChangeEvent<string> evt, VisualElement element)
    {
        int layerIndex = (int)element.userData;
        int layerTypeIndex = m_layerTypeNames.IndexOf(evt.newValue);
        
        SerializedProperty existingLayers = property.FindPropertyRelative("m_layers");
        SerializedProperty layerElement = existingLayers.GetArrayElementAtIndex(layerIndex);
        
        if (layerElement.managedReferenceValue == null || layerElement.managedReferenceValue.GetType() != m_layerTypes[layerTypeIndex])
        {
            layerElement.managedReferenceValue = (IVoxelGeneratorLayer)Activator.CreateInstance(m_layerTypes[layerTypeIndex]);
            property.serializedObject.ApplyModifiedProperties();
            listView.RefreshItem(layerIndex);
        }
    }
}
