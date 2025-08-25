using UnityEngine;
using static GameManager;

public class NextWave: MonoBehaviour
{
    public GameObject NextWavePanel;


    public void StartNextWave()
    {
        Debug.Log("Starting next wave...");
        GameManager.Get().StartNextWave();
        NextWavePanel.SetActive(false);
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
