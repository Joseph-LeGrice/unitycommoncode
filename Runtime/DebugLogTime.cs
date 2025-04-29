using UnityEngine;

public static class DebugLogTime
{
    private static double s_t1;
    private static double s_t2;
    
    public static void Log(string messageFormat)
    {
            s_t2 = Time.time;
#if UNITY_EDITOR
            s_t2 = UnityEditor.EditorApplication.timeSinceStartup;
#endif
        
        Debug.Log(string.Format(messageFormat, (s_t2 - s_t1)));

        s_t1 = Time.time;
#if UNITY_EDITOR
            s_t1 = UnityEditor.EditorApplication.timeSinceStartup;
#endif
    }

    public static void ResetTimers()
    {
        s_t1 = Time.time;
        s_t2 = Time.time;
#if UNITY_EDITOR
        s_t1 = UnityEditor.EditorApplication.timeSinceStartup;
        s_t2 = UnityEditor.EditorApplication.timeSinceStartup;
#endif
    }
}
