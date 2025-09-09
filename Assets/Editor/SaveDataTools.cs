#if UNITY_EDITOR
using UnityEditor;

public static class SaveDataTools
{
    [MenuItem("Tools/Save Data/Reset All PlayerPrefs")]
    public static void ResetAll()
    {
        SaveManager.ResetAllData();
    }
}
#endif
