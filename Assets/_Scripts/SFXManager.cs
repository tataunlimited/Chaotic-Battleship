using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource destroyerAttack;
    [SerializeField] private AudioSource battleshipAttack;
    [SerializeField] private AudioSource cruiserAttack;
    [SerializeField] private AudioSource submarineAttack;

    [Header("Mixer (exposed: MasterVolume, SFXVolume, BGMVolume)")]
    [SerializeField] private AudioMixer MasterVolumeAudioMixer;

    // Shared PlayerPrefs keys (used by Main Menu & In-Game)
    private const string PREF_MASTER = "opt_audio_master";
    private const string PREF_SFX    = "opt_audio_sfx";
    private const string PREF_BGM    = "opt_audio_bgm";

    private const float DEFAULT_MASTER = 1f;
    private const float DEFAULT_SFX    = 1f;
    private const float DEFAULT_BGM    = 1f;

    private void Awake()
    {
        // Singleton across scenes; destroy duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ApplyFromPrefs();
    }

    // Called by UI sliders (linear 0..1)
    public void SetMasterVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("MasterVolume", LinearToDb(sliderValue));
        PlayerPrefs.SetFloat(PREF_MASTER, Mathf.Clamp01(sliderValue));
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("SFXVolume", LinearToDb(sliderValue));
        PlayerPrefs.SetFloat(PREF_SFX, Mathf.Clamp01(sliderValue));
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("BGMVolume", LinearToDb(sliderValue));
        PlayerPrefs.SetFloat(PREF_BGM, Mathf.Clamp01(sliderValue));
        PlayerPrefs.Save();
    }

    // Gameplay hooks
    public void PlayDestroyerAttack()  { if (destroyerAttack)  destroyerAttack.Play(); }
    public void PlayBattleshipAttack() { if (battleshipAttack) battleshipAttack.Play(); }
    public void PlayCruiserAttack()    { if (cruiserAttack)    cruiserAttack.Play(); }
    public void PlaySubmarineAttack()  { if (submarineAttack)  submarineAttack.Play(); }

    private void ApplyFromPrefs()
    {
        float m = PlayerPrefs.GetFloat(PREF_MASTER, DEFAULT_MASTER);
        float s = PlayerPrefs.GetFloat(PREF_SFX,    DEFAULT_SFX);
        float b = PlayerPrefs.GetFloat(PREF_BGM,    DEFAULT_BGM);

        MasterVolumeAudioMixer.SetFloat("MasterVolume", LinearToDb(m));
        MasterVolumeAudioMixer.SetFloat("SFXVolume",    LinearToDb(s));
        MasterVolumeAudioMixer.SetFloat("BGMVolume",    LinearToDb(b));
    }

    private static float LinearToDb(float linear)
    {
        if (linear <= 0.0001f) return -80f; // floor to avoid -Inf
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }
}
