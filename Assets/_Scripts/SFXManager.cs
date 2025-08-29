using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public AudioMixer myAudioMixer;
    //public Slider volumeSlider;
    //public AudioSource hitSoundSource;
    //public AudioSource shipSunkSource;

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

    public void SetVolume(float sliderValue)
    {
        myAudioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }

    //public void SetVolume(float volume)
    //{
    //    if (hitSoundSource != null && shipSunkSource != null)
    //    {
    //        hitSoundSource.volume = volume;
    //        shipSunkSource.volume = volume;
    //    }
    //}
}
