using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume");
        if (volumeSlider.value == 0) volumeSlider.value = 0.5f;
        SetVolume(volumeSlider.value);
    }
    public AudioMixer audioMixer;
    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("Volume", volume);
        volume = volume <= .001f ?  1e-50f: 
            volume >= .998f ? .998f : 
            volume;
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 15);
    }

    public void ResetData()
    {
        GameManager.Instance.ResetSaveData();
    }
}
