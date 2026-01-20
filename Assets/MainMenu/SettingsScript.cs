using UnityEngine;
using UnityEngine.Audio;

public class SettingsScript : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void SetVolume(float volume)
    {
        volume *=  -80;
        Debug.Log(volume);
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void ResetData()
    {
        GameManager.Instance.ResetSaveData();
    }
}
