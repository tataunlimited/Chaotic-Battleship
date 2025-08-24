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
        // This was meant to replace just doing LoadScene, so that it could preserve player information like score
        // but it's still buggy (one of GameManager's coroutines is not getting reset).
        // Anyways, not needed for this prototype yet
        //
        //GameManager game = GameManager.Get();
        //game.Restart();
        //GameOverPanel.SetActive(false);

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
