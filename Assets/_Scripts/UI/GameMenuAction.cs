using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuActions : MonoBehaviour
{
    // Clear ONLY the mid-wave board snapshot so the next load spawns a fresh board.
    public void ClearSnapshot()
    {
        SaveManager.ResetAllData();
        RestartWave();
        Debug.Log("[Menu] Cleared board snapshot.");
    }

    // Restart the current scene/wave (fresh board), keeping meta (wave number) but
    // rolling back any points earned during THIS wave to the start-of-wave baseline.
    public void RestartWave()
    {
        // Always unpause before reload to avoid "stuck paused" after scene load.
        Time.timeScale = 1f;

        // Roll back score to baseline captured at the start of this wave.
        var pd = PlayerData.Instance;
        if (pd != null)
        {
            int before = pd.currentScore;
            pd.currentScore = pd.scoreAtWaveStart;  // <-- core requirement
            Debug.Log($"[Menu] RestartWave: score {before} → {pd.currentScore} (baseline). Wave stays {pd.waveNumber}.");
        }
        else
        {
            Debug.LogWarning("[Menu] RestartWave: PlayerData.Instance was null; could not roll back score.");
        }

        // Ensure we don't reload into a stale mid-wave save.
        SaveManager.ClearBoardState();

        // Persist meta (wave number unchanged), restored score, upgrades, and baseline.
        SaveManager.SaveGame();

        // Reload the current scene. Wave number is NOT changed here.
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //had to make it work with mine
        SceneManager.Instance.ReloadActiveScene();
    }

    // Full wipe: PlayerPrefs + PlayerData defaults + snapshot; then reload scene.
    public void ResetProgress()
    {
        Time.timeScale = 1f;               // important: unpause before reload
        SaveManager.ResetAllData();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // same here
        SceneManager.Instance.ReloadActiveScene();
    }
}
