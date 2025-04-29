using UnityEditor;
using UnityEngine;

public class CommonEditorUtil
{
    [MenuItem("Project/Open Persistent Data Folder")]
    private static void OpenDataFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
