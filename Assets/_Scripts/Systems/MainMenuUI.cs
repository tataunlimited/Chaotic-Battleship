using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using static SceneTypes;

public class MainMenuUI : MonoBehaviour
{
    public void OnMainMenu()
    {
        SceneManager.Instance.LoadScene(SceneType.MainMenu);
    }
    public void OnPlay()
    {
        SceneManager.Instance.LoadScene(SceneType.Game);
    }

    public void onContinue()
    {
        SceneManager.Instance.LoadScene(SceneType.Continue);
    }
    public void OnStory()
    {
        SceneManager.Instance.LoadScene(SceneType.Story);
    }
    public void OnCredits()
    {
        SceneManager.Instance.LoadScene(SceneType.Credits);
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