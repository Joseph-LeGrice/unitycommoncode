using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static bool s_didFetch;

    public static T Instance
    {
        get
        {
            if (!s_didFetch)
            {
                s_instance = FindFirstObjectByType<T>();
                s_didFetch = true;
            }
            return s_instance;
        }
    }
}
