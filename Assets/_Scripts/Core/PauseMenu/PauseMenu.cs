using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuPanel;
    public GameObject OptionsMenuPanel;

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

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
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
    }
}
