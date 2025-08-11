using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuPanel;

    // Update is called once per frame
    void Update()
    {

    }

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
}
