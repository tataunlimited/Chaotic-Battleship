using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    [SerializeField] private AudioSource destroyerAttack;
    [SerializeField] private AudioSource battleshipAttack;
    [SerializeField] private AudioSource cruiserAttack;
    [SerializeField] private AudioSource submarineAttack;
    [SerializeField] private AudioMixer MasterVolumeAudioMixer;
    //public Slider volumeSlider;
    //public AudioSource hitSoundSource;
    //public AudioSource shipSunkSource;

    private void Awake()
    {
        Instance = this;
    }

    //    void Start()
    //{
    //    // Optional: Set initial slider value to current audio source volume
    //    if (volumeSlider != null && hitSoundSource != null && shipSunkSource != null)
    //    {
    //        volumeSlider.value = hitSoundSource.volume;
    //        volumeSlider.value = shipSunkSource.volume;
    //    }

    //    // Add listener to update volume when slider value changes
    //    if (volumeSlider != null)
    //    {
    //        volumeSlider.onValueChanged.AddListener(SetVolume);
    //    }
    //}

    public void SetMasterVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }
    
    public void SetSFXVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
    }
    
    public void SetBGMVolume(float sliderValue)
    {
        MasterVolumeAudioMixer.SetFloat("BGMVolume", Mathf.Log10(sliderValue) * 20);
    }

    //public void SetVolume(float volume)
    //{
    //    if (hitSoundSource != null && shipSunkSource != null)
    //    {
    //        hitSoundSource.volume = volume;
    //        shipSunkSource.volume = volume;
    //    }
    //}

    public void PlayDestroyerAttack()
    {
        destroyerAttack.Play();

    }
    public void PlayBattleshipAttack()
    {
        battleshipAttack.Play();

    }
    public void PlayCruiserAttack()
    {
        cruiserAttack.Play();

    }
    public void PlaySubmarineAttack()
    {
        submarineAttack.Play();

    }
}
