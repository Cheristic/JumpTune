using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct ActiveNote
{
    public float timeProgressedInPhase;
    public ADSR.ADSR_Phase phase;

    public float frequency;
    public AudioSource AudioSource;
    public int bufferPosition;
    public int totalSamples;
    public double startTime;

    public float Evaluate()
    {
        return 1;
    }
    internal readonly bool IsPlaying => phase != ADSR.ADSR_Phase.NotPlaying;
}

[Serializable]
public struct ADSR
{
    public float Attack_MS;
    public static float ATTACK_DB_Percent = 1;
    public float Decay_MS;
    public float Sustain_MS;
    public float Sustain_DB_Percent;
    public float Release_MS;

    internal readonly float Attack_S => Attack_MS / 1000f;
    internal readonly float Decay_S => Decay_MS / 1000f;
    internal readonly float Sustain_S => Sustain_MS / 1000f;
    internal readonly float Release_S => Release_MS / 1000f;
    public enum ADSR_Phase
    {
        NotPlaying,
        Attack,
        Decay,
        Sustain,
        Release,
        QueueToStop
    }
    internal readonly float ADSR_Time => Attack_S + Decay_S + Sustain_S + Release_S;
}

public class ToneManager : MonoBehaviour
{

    public static ToneManager Instance { get; private set; }
    // --------------------------------------
    // Public
    public float gain;
    public float frequencyTransitionRate = .001f;

    public float[] harmonicStrengths = new float[12];

    public int SampleRate = 44100;     // this is the number of samples we use per second,to construct the sound waveforms.
                                       // default is 48,000 samples. This means if your frame rate is 60 fps, in each frame you need to provide 48k/60 samples. 

    public ADSR ADSR;
    int BufferedSamples { get => Mathf.CeilToInt(ADSR.ADSR_Time * SampleRate); } // this is the max each OnAudioRead's data[] will store

    private ActiveNote[] notes = new ActiveNote[12];


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        for (int i = 0; i < notes.Length; i++)
        {
            notes[i].AudioSource = gameObject.AddComponent<AudioSource>();
            notes[i].phase = ADSR.ADSR_Phase.NotPlaying;
        }
    }

    private void Update()
    {
        for (int i = 0; i < notes.Length; i++)
        {
            if (notes[i].phase == ADSR.ADSR_Phase.QueueToStop)
            {
                double progress = AudioSettings.dspTime - notes[i].startTime;
                double goal = 1.0f * BufferedSamples / SampleRate;
                if (progress >= goal)
                {
                    Debug.Log("stopping " + i);
                    notes[i].AudioSource.Stop();
                    notes[i].phase = ADSR.ADSR_Phase.NotPlaying;
                }
            }
        }
    }


    public int PlayNote(float frequency)
    {
        for (int i = 0; i < notes.Length; i++)
        {
            if (!notes[i].IsPlaying)
            {
                notes[i].frequency = frequency;
                notes[i].phase = ADSR.ADSR_Phase.Attack;
                notes[i].totalSamples = 0;
                AudioClip ac = AudioClip.Create("", BufferedSamples, 1, SampleRate, false, (data) => OnAudioRead(data, i), (pos) => OnAudioSetPosition(pos, i));
                notes[i].AudioSource.Stop();
                notes[i].AudioSource.clip = ac;
                notes[i].AudioSource.time = 0.0f;
                notes[i].startTime = AudioSettings.dspTime;
                notes[i].AudioSource.Play();

                return i;
            }
        }
        return -1;
    }

    void OnAudioRead(float[] data, int i)
    {
        //if (!notes[i].IsPlaying) return;
        Debug.Log(i + " " + notes[i].phase + ": " + notes[i].bufferPosition + " - " + notes[i].totalSamples);

        for (int j = 0; j < data.Length; j++)
        {
            //float volumeModifier = ADSR.Evaluate(1.0f * activeNotes[j].bufferPosition / BufferedSamples);
            float adsrVolumeModifier = EvaluateADSR(i);
            //if (j % 1000 == 0) Debug.Log("playing " + i + " " + notes[i].bufferPosition + " " + notes[i].timeProgressedInPhase + " " + notes[i].phase + " vol=" + adsrVolumeModifier);

            data[j] = CreateSineOscillator(notes[i].frequency, notes[i].totalSamples) * gain * adsrVolumeModifier;


            notes[i].bufferPosition++;
            notes[i].totalSamples++;
        }

       // Debug.Log("first " + data[0] + ", last " + data[data.Length - 1] + " - " + notes[i].totalSamples);
    }

    float CreateSineOscillator(float frequency, int position)
    {
        return Mathf.Sin(2 * Mathf.PI * frequency * position / SampleRate);
    }

    float EvaluateADSR(int i)
    {
        ActiveNote n = notes[i];
        float nTime = n.timeProgressedInPhase + 1.0f / SampleRate;
        float vol = 0;
        if (n.phase == ADSR.ADSR_Phase.Attack)
        {
            vol = Mathf.Lerp(0, ADSR.ATTACK_DB_Percent, nTime / ADSR.Attack_S);
            if (nTime >= ADSR.Attack_S)
            {
                n.timeProgressedInPhase = nTime % ADSR.Attack_S;
                n.phase = ADSR.ADSR_Phase.Decay;
            }
            else n.timeProgressedInPhase = nTime;
        } else if (n.phase == ADSR.ADSR_Phase.Decay)
        {
            vol = Mathf.Lerp(ADSR.ATTACK_DB_Percent, ADSR.Sustain_DB_Percent, nTime / ADSR.Decay_S);
            if (nTime >= ADSR.Decay_S)
            {
                n.timeProgressedInPhase = nTime % ADSR.Decay_S; ;
                n.phase = ADSR.ADSR_Phase.Sustain;
            }
            else n.timeProgressedInPhase = nTime;
        } else if (n.phase == ADSR.ADSR_Phase.Sustain)
        {
            vol = ADSR.Sustain_DB_Percent;
            if (nTime >= ADSR.Sustain_S)
            {
                n.timeProgressedInPhase = nTime % ADSR.Sustain_S;
                n.phase = ADSR.ADSR_Phase.Release;
            }
            else n.timeProgressedInPhase = nTime;
        } else if (n.phase == ADSR.ADSR_Phase.Release)
        {
            vol = Mathf.Lerp(ADSR.Sustain_DB_Percent, 0, nTime / ADSR.Release_S);
            if (nTime >= ADSR.Release_S)
            {
                n.phase = ADSR.ADSR_Phase.QueueToStop;
                n.timeProgressedInPhase = 0;
                vol = 0;
            }
            else n.timeProgressedInPhase = nTime;
        }
        notes[i] = n;
        return vol;
    }

    void OnAudioSetPosition(int newPosition, int i)
    {
        if (notes[i].phase == ADSR.ADSR_Phase.NotPlaying) return;
        notes[i].bufferPosition = newPosition;
    }


    public float ReturnSuperimposedHarmonicsSeries(int dataIndex)
    {
        float superImposed = 0.0f;

        //for (int i = 1; i <= 12; i++)
        //{
        //    float harmonicFrequency = fundementalToneFrequency * i;

        //    float timeAtTheBeginig = (float)(AudioSettings.dspTime % (1.0 / (double)harmonicFrequency)); // very important to deal with percision issue as dspTime gets large

        //    float exactTime = timeAtTheBeginig + (float)dataIndex / SampleRate;

        //    superImposed += Mathf.Sin(exactTime * harmonicFrequency * 2f * Mathf.PI) * harmonicStrengths[i - 1];
        //}

        return superImposed;
    }
}
