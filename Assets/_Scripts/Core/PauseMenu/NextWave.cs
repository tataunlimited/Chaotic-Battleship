using UnityEngine;

public class NextWave : MonoBehaviour
{
    [SerializeField] private GameObject NextWavePanel;

    public void StartNextWave()
    {
        Debug.Log("Starting Next Wave");

        var gm = GameManager.instance ?? GameManager.Get();
        if (gm != null)
        {
            gm.StartNextWave();                 // increments wave & saves; also resets placement UI
            // NEW: Set baseline for the new wave so restarts roll back correctly
            if (PlayerData.Instance != null)
            {
                PlayerData.Instance.scoreAtWaveStart = PlayerData.Instance.currentScore;
                SaveManager.SaveGame();
            }
            if (NextWavePanel) NextWavePanel.SetActive(false);
            
            SceneManager.Instance.LoadScene(SceneTypes.SceneType.Harbor);
            return;
        }

        // Safety: if GameManager isn't found for some reason, at least bump wave meta
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.waveNumber += 1;
            PlayerData.Instance.scoreAtWaveStart = PlayerData.Instance.currentScore; // NEW
        }
        SaveManager.ClearBoardState();
        SaveManager.SaveGame();
        if (NextWavePanel) NextWavePanel.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
