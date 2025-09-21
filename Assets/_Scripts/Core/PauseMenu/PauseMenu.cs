using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuPanel;
    public GameObject OptionsMenuPanel;
    public GameObject QuitConfirmPanel;

    // Update is called once per frame
    void Update()
    {

    }
    // PAUSE MENU
    public void Pause()
    {
        PauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Continue()
    {
        PauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitConfirm()
    {
        PauseMenuPanel.SetActive(false);
        QuitConfirmPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Quit()
    {
        SceneManager.Instance.LoadScene(SceneTypes.SceneType.MainMenu);
    }

    // OPTIONS MENU
    public void Options()
    {
        PauseMenuPanel.SetActive(false);
        OptionsMenuPanel.SetActive(true);
    }

    public void Return()
    {
        PauseMenuPanel.SetActive(true);
        OptionsMenuPanel.SetActive(false);
        QuitConfirmPanel.SetActive(false);
    }
}
