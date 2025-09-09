using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverPanel;

    void Update()
    {
        RoundLost();
    }

    public void RoundLost()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameOverPanel.SetActive(!GameOverPanel.activeSelf);
            Time.timeScale = 0f;
        }
    }

    public void Restart()
    {
        Debug.Log("Restart On Game Over");
        Time.timeScale = 1f;
        SaveManager.ClearBoardState(); // ensure no stale mid-wave board
        SaveManager.SaveGame();        // keep current meta (wave/score) consistent
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
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
