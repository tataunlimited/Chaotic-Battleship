using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using static SceneTypes;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.Instance.LoadScene(SceneType.Game);
    }
    public void OnMainMenu()
    {
        SceneManager.Instance.LoadScene(SceneType.MainMenu);
    }
    public void OnCredits()
    {
        SceneManager.Instance.LoadScene(SceneType.Credits);
    }
    public void OnHarbor()
    {
        SceneManager.Instance.LoadScene(SceneType.Harbor);
    }
    public void OnStory()
    {
        SceneManager.Instance.LoadScene(SceneType.Story);
    }

    public void OnOptions()
    {
        SceneManager.Instance.LoadScene(SceneType.Options);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}