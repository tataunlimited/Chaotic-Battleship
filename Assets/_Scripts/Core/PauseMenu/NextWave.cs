using UnityEngine;
using static GameManager;

public class NextWave: MonoBehaviour
{
    public GameObject NextWavePanel;


    public void StartNextWave()
    {
        // GameManager.Get().StartNextWave();
        // NextWavePanel.SetActive(false);
        Debug.Log("Starting Next Wave");
        PlayerData.Instance.waveNumber ++;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

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
