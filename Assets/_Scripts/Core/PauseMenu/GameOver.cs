using UnityEngine;

public class GameOver: MonoBehaviour
{
    public GameObject GameOverPanel;

    // Update is called once per frame
    void Update()
    {
        RoundLost();
    }

    public void RoundLost()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Restart()
    {
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
