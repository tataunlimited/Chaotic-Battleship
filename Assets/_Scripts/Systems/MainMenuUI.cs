using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using static SceneTypes;

public class MainMenuUI : MonoBehaviour
{
    [Header("Feature Flags")] [SerializeField]
    bool optionsEnabled = false;

    [Header("Panels (optional)")] [SerializeField]
    GameObject optionsPanel;

    [Header("Options UI (optional)")] [SerializeField]
    Slider masterVolumeSlider;

    [SerializeField] Toggle fullscreenToggle;

    [Header("Audio (optional)")] [SerializeField]
    AudioSource uiAudio;

    [SerializeField] AudioClip clickSfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Safe
        if (optionsPanel) optionsPanel.SetActive(false);

        if (optionsEnabled)
        {
            if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;
            float vol = PlayerPrefs.GetFloat("masterVol", 0.8f);
            if (masterVolumeSlider) masterVolumeSlider.value = vol;
            ApplyVolume(vol);
        }
    }

    public void OnPlay()
    {
        PlayClick();
        SceneManager.Instance.LoadScene(SceneType.Game);
    }

    public void OnCredits()
    {
        PlayClick();
        SceneManager.Instance.LoadScene(SceneType.Credits);
    }

    public void OnOptions()
    {
        PlayClick();
        if (!optionsEnabled)
        {
            Debug.Log("Options are coming soon.");
            return;
        }

        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void OnQuit()
    {
        PlayClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }

    public void OnBackFromOptions()
    {
        PlayClick();
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    public void OnVolumeChanged(float v)
    {
        if (!optionsEnabled) return;
        ApplyVolume(v);
        PlayerPrefs.SetFloat("masterVol", v);
    }

    public void OnFullscreenToggled(bool on)
    {
        if (!optionsEnabled) return;
        Screen.fullScreen = on;
    }

    void ApplyVolume(float v) => AudioListener.volume = Mathf.Clamp01(v);

    void PlayClick()
    {
        if (uiAudio && clickSfx) uiAudio.PlayOneShot(clickSfx);
    }
}