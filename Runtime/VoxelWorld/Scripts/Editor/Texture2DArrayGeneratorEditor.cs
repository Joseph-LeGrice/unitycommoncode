using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
    
[CustomEditor(typeof(Texture2DArrayGenerator))]
public class Texture2DArrayGeneratorEditor : Editor
{
    public Texture2DArrayGenerator Target { get { return (Texture2DArrayGenerator)target; } }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Texture2DArray"))
        {
            UpdateTextureArrays();
        }
    }
    
    public void UpdateTextureArrays()
    {
        
        List<Texture2DArrayGenerator.TerrainType> allTerrainTypes = ((Texture2DArrayGenerator)target).GetAllTerrainTypes();
        
        if (allTerrainTypes.Count == 0)
        {
            Debug.LogError("Cannot create Texture2DArrays, m_allTerrainTypes.Count == 0");
            return;
        }

        string path = EditorUtility.OpenFolderPanel("Select output path", Target.GetDefaultPath(), "");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        path = Target.GetDefaultPath().Substring(Target.GetDefaultPath().IndexOf("Assets"));
        Target.SetDefaultPath(path);
        EditorUtility.SetDirty(target);
        
        List<Texture2D> mainTextures = allTerrainTypes.ConvertAll(tt => tt.m_mainTexture);
        Texture2DArray mainTextureArray = GenerateArray(mainTextures, true);
        SaveTextureArray(mainTextureArray, "TerrainTextureArray_MainTextures");
        
        List<Texture2D> normalMaps = allTerrainTypes.ConvertAll(tt => tt.m_normalMap);
        Texture2DArray normalMapTextureArray = GenerateArray(normalMaps, true);
        SaveTextureArray(normalMapTextureArray, "TerrainTextureArray_NormalMaps");
        
        List<Texture2D> aoMaps = allTerrainTypes.ConvertAll(tt => tt.m_ambientOcclusionMap);
        Texture2DArray aoMapTextureArray = GenerateArray(aoMaps, true);
        SaveTextureArray(aoMapTextureArray, "TerrainTextureArray_AOMaps");
        
        List<Texture2D> specularMaps = allTerrainTypes.ConvertAll(tt => tt.m_specularMap);
        Texture2DArray specularMapTextureArray = GenerateArray(specularMaps, true);
        SaveTextureArray(specularMapTextureArray, "TerrainTextureArray_SpecularMaps");
    }

    private void SaveTextureArray(Texture2DArray textureArray, string assetName)
    {
        if (textureArray == null)
        {
            Debug.LogError("No texture array specified for " + assetName);
            return;
        }

        textureArray.name = assetName;
        
        string path = Path.Join(Target.GetDefaultPath(), assetName) + ".asset";
        Texture2DArray existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
        
        if (existing != null)
        {
            EditorUtility.CopySerialized(textureArray, existing);
            AssetDatabase.SaveAssetIfDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(textureArray, path);
        }
    }

    private Texture2DArray GenerateArray(List<Texture2D> fromTextures, bool generateMipChain)
    {
        if (fromTextures.Count == 0)
        {
            Debug.LogError("Cannot create Texture2DArray, fromTextures.Count == 0");
            return null;
        }
        
        int width = fromTextures[0].width;
        int height = fromTextures[0].height;
        TextureFormat format = fromTextures[0].format;
        bool isLinear = !fromTextures[0].isDataSRGB;

        foreach (Texture2D t in fromTextures)
        {
            if (width != t.width || height != t.height)
            {
                Debug.LogError("Cannot create Texture2DArray, " + t.name + " dimensions mismatch");
                return null;
            }
            
            if (format != t.format)
            {
                Debug.LogError("Cannot create Texture2DArray, " + t.name + " format mismatch");
                return null;
            }

            if (isLinear == t.isDataSRGB)
            {
                Debug.LogError("Cannot create Texture2DArray, isDataSRGB mismatch");
                return null;
            }
        }

        int slices = fromTextures.Count;
        Texture2DArray textureArray = new Texture2DArray(width, height, slices, format, generateMipChain, isLinear);
        
        for (int slice = 0; slice < slices; slice++)
        {
            Graphics.CopyTexture(fromTextures[slice], 0, 0, textureArray, slice, 0);
        }
        
        textureArray.Apply();

        return textureArray;
    }
}
