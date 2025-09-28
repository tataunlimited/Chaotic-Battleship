using UnityEngine;
using UnityEngine.UI;

public class OptionsAudioUIBinder : MonoBehaviour
{
    [SerializeField] private Slider masterSlider; // MV_Slider (Main) / MV_Slider (Game)
    [SerializeField] private Slider sfxSlider;    // SFXV_Slider
    [SerializeField] private Slider bgmSlider;    // BGMV_Slider

    private const string PREF_MASTER = "opt_audio_master";
    private const string PREF_SFX    = "opt_audio_sfx";
    private const string PREF_BGM    = "opt_audio_bgm";

    private void Awake()
    {
        float m = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        float s = PlayerPrefs.GetFloat(PREF_SFX,    1f);
        float b = PlayerPrefs.GetFloat(PREF_BGM,    1f);

        if (masterSlider) masterSlider.SetValueWithoutNotify(m);
        if (sfxSlider)    sfxSlider.SetValueWithoutNotify(s);
        if (bgmSlider)    bgmSlider.SetValueWithoutNotify(b);

        if (masterSlider) masterSlider.onValueChanged.AddListener(v => { if (SFXManager.Instance) SFXManager.Instance.SetMasterVolume(v); });
        if (sfxSlider)    sfxSlider.onValueChanged.AddListener(v => { if (SFXManager.Instance) SFXManager.Instance.SetSFXVolume(v); });
        if (bgmSlider)    bgmSlider.onValueChanged.AddListener(v => { if (SFXManager.Instance) SFXManager.Instance.SetBGMVolume(v); });
    }

    private void OnEnable()
    {
        // Refresh knob positions when panel opens
        float m = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        float s = PlayerPrefs.GetFloat(PREF_SFX,    1f);
        float b = PlayerPrefs.GetFloat(PREF_BGM,    1f);

        if (masterSlider) masterSlider.SetValueWithoutNotify(m);
        if (sfxSlider)    sfxSlider.SetValueWithoutNotify(s);
        if (bgmSlider)    bgmSlider.SetValueWithoutNotify(b);
    }
}
