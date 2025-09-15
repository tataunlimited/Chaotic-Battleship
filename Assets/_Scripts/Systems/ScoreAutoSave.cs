using UnityEngine;

// Listens for score changes and persists meta immediately.
// Also saves on pause/quit (covers Editor Stop).
public class ScoreAutoSave : MonoBehaviour
{
    private void OnEnable()
    {
        GameManagerScore.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        GameManagerScore.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int _)
    {
        SaveManager.SaveGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveManager.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveManager.SaveGame();
    }
}
