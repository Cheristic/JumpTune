using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] int soundsToPool;
    [SerializeField] AudioClip selectSFX;
    [SerializeField] float selectSFXPitchVariance = 0.05f;
    [SerializeField] AudioClip startSFX;

    List<AudioSource> sources;
    public void Init()
    {
        sources = new();
        for (int i = 0; i < soundsToPool; i++)
        {
            sources.Add(gameObject.AddComponent<AudioSource>());
            sources[i].outputAudioMixerGroup = ToneManager.Instance.audioMixer;
        }
        GameManager.StartGame += PlayStartSound;
    }

    void PlayStartSound() => PlayStartSound(0);

    private void OnDisable()
    {
        GameManager.StartGame -= PlayStartSound;
    }

    AudioSource GetSource()
    {
        foreach (var source in sources)
        {
            if (!source.isPlaying) return source;
        }
        sources.Add(gameObject.AddComponent<AudioSource>());
        sources[^1].outputAudioMixerGroup = ToneManager.Instance.audioMixer;
        return sources[^1];
    }
    public void PlaySelectSound(float pitchChange = 0)
    {
        AudioSource source = GetSource();
        source.clip = selectSFX;
        source.time = 0;
        source.pitch = Random.Range(-selectSFXPitchVariance, selectSFXPitchVariance) + 1f + pitchChange;
        source.Play();  
    }

    public void PlayStartSound(float pitchChange = 0)
    {
        AudioSource source = GetSource();
        source.clip = startSFX;
        source.time = 0;
        source.pitch = 1f + pitchChange;
        source.Play();
    }

}
