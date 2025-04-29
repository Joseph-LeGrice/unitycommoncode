using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Texture2DArray Generator")]
public class Texture2DArrayGenerator : ScriptableObject
{
    [System.Serializable]
    public class TerrainType
    {
        public Texture2D m_mainTexture;
        public Texture2D m_normalMap;
        public Texture2D m_ambientOcclusionMap;
        public Texture2D m_specularMap;
    }
    
    [SerializeField]
    private string m_textureArrayPath;
    [SerializeField]
    private List<TerrainType> m_allTerrainTypes;
    
    public void SetDefaultPath(string textureArrayPath)
    {
        m_textureArrayPath = textureArrayPath;
    }

    public string GetDefaultPath()
    {
        return m_textureArrayPath;
    }

    public List<TerrainType> GetAllTerrainTypes()
    {
        return m_allTerrainTypes;
    }
}
