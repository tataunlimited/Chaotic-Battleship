using UnityEngine;

public static class PlayerDataBootstrap
{
    // Runs before the first scene is loaded
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsurePlayerData()
    {
        if (PlayerData.Instance != null) return;

        var go = new GameObject("PlayerData_Auto");
        go.AddComponent<PlayerData>();     // This runs PlayerData.Awake()
        go.AddComponent<ScoreAutoSave>();  // NEW: autosave on score change + pause/quit
        Object.DontDestroyOnLoad(go);
    }
}
