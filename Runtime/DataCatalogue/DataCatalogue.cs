using System.Collections.Generic;
using UnityEngine;

public class DataCatalogue<T> : ScriptableObject where T : ScriptableObject
{
    [SerializeField]
    private List<T> m_data;

    public IReadOnlyList<T> GetAllData()
    {
        return m_data;
    }

    public void RegisterData(T data)
    {
        m_data.Add(data);
    }
}
