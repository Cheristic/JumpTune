using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class Stem
    {
        public string name;
        public AudioClip audioClip;
        //[Range(0f, 1f)] internal float volume;
    }

    AudioSource audioSourceA;
    AudioSource audioSourceB;
    AudioSource endSource;

    public Stem[] stems;
    public Stem finalStem;

    bool currentA;
    int previousIdx;
    bool playedEnd;

    internal LevelManager levelManager;
    internal PlayerControls player;

    public float fadeLength;

    void Start()
    {
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
        
        audioSourceA = audioSources[0];
        audioSourceB = audioSources[1];
        endSource = audioSources[2];

        audioSourceA.clip = stems[0].audioClip;
        audioSourceA.volume = 0f;
        audioSourceA.loop = true;
        audioSourceA.Play();

        audioSourceB.clip = stems[1].audioClip;
        audioSourceB.volume = 0f;
        audioSourceB.loop = true;
        audioSourceB.Play();

        //currentStem = 0;
        currentA = true;
        previousIdx = -1;
        playedEnd = false;

        levelManager = FindFirstObjectByType<LevelManager>();
        player = FindFirstObjectByType<PlayerControls>();

        //increment = (levelManager.topY - levelManager.bottomY) / (stems.Length-1);
    }

    void Update()
    {
        if (playedEnd) return;

        if (!playedEnd && player.transform.position.y >= levelManager.topY)
        {
            // play end sound
            // could also be triggered from the scoring logic?
            playedEnd = true;

            //PlayAudio(currentA, finalStem.audioClip);

            audioSourceA.Stop();
            audioSourceB.Stop();

            endSource.loop = false;
            endSource.volume = 1f;

            endSource.PlayOneShot(finalStem.audioClip);

            return;
        }

        // progress is between 0 and 1
        float progress = Mathf.InverseLerp(levelManager.bottomY, levelManager.topY, player.transform.position.y);

        // map progress to index
        int progressIdx = Mathf.Clamp(Mathf.FloorToInt(progress * stems.Length), 0, stems.Length - 1);

        if (progressIdx != previousIdx)
        {
            PlayAudio(stems[progressIdx].audioClip);

            previousIdx = progressIdx;
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (currentA)
        {
            currentA = false;

            audioSourceB.clip = clip;
            audioSourceB.volume = 1f;

            audioSourceB.Play();

            audioSourceA.volume = 0f;

            StartCoroutine(FadeStem(audioSourceA, audioSourceB, fadeLength));
        }

        else
        {
            currentA = true;

            audioSourceA.clip = clip;
            audioSourceA.volume = 1f;

            audioSourceA.Play();

            audioSourceB.volume = 0f;

            StartCoroutine(FadeStem(audioSourceB, audioSourceA, fadeLength));
        }
    }

    IEnumerator FadeStem(AudioSource from, AudioSource to, float fadeLength)
    {
        float t = 0;

        while(t < fadeLength)
        {
            t += Time.deltaTime;

            float k = t / fadeLength;

            from.volume = 1 - k;
            to.volume = k;

            yield return null;
        }
    }
}
