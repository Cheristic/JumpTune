﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct ActiveNote
{
    public float startPlayTime;
    public float fundementalFrequency;
    public float percentageProgresssed;
    public AudioSource AudioSource;
    public int position;
}

public class ToneManager : MonoBehaviour
{

    public static ToneManager Instance { get; private set; }
    // --------------------------------------
    // Public
    public float gain;

    public float[] harmonicStrengths = new float[12];

    public int SampleRate = 44100;     // this is the number of samples we use per second,to construct the sound waveforms.
                                         // default is 48,000 samples. This means if your frame rate is 60 fps, in each frame you need to provide 48k/60 samples. 

    int BufferedSamples { get => Mathf.CeilToInt(SampleRate * ADSRtime); }
    private float fundementalToneFrequency;

    private ActiveNote[] currentlyBeingPlayed = new ActiveNote[12];

    [SerializeField] AnimationCurve ADSR;
    [SerializeField] float ADSRtime;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        for (int i = 0; i < currentlyBeingPlayed.Length; i++)
        {
            currentlyBeingPlayed[i].AudioSource = gameObject.AddComponent<AudioSource>();
        }

    }
    

    public int ClaimActiveNote()
    {
        for (int i = 0; i < currentlyBeingPlayed.Length; i++)
        {
            if ((float)AudioSettings.dspTime - currentlyBeingPlayed[i].startPlayTime > ADSRtime)
            {
                return i;
            }
        }
        return -1;
    }

    public void PlayNote(int index, float frequency)
    {
        currentlyBeingPlayed[index].fundementalFrequency = frequency;
        currentlyBeingPlayed[index].startPlayTime = (float)AudioSettings.dspTime;
        AudioClip ac = AudioClip.Create("", BufferedSamples, 1, SampleRate, false, OnAudioRead, OnAudioSetPosition);
        currentlyBeingPlayed[index].AudioSource.Stop();
        currentlyBeingPlayed[index].AudioSource.clip = ac;
        currentlyBeingPlayed[index].AudioSource.time = 0.0f;
        currentlyBeingPlayed[index].AudioSource.Play();

        //float tau = Mathf.PI * 2.0f;

        //float[] ret = new float[SampleRate];

        //for (int i = 0; i < SampleRate; ++i)
        //    ret[i] = Mathf.Sin((float)i / (float)SampleRate * frequency * tau) * gain;

        //ac.SetData(ret, 0);

        //this.audioSource.Stop();
        //this.audioSource.clip = ac;
        //this.audioSource.time = 0.0f;
        //this.audioSource.Play();
        //float timeSinceNoteStartedPlaying = (float)AudioSettings.dspTime - currentlyBeingPlayed[index].startPlayTime;

        //if (timeSinceNoteStartedPlaying < keysADSR.DecayT)
        //{
        //    // nothing
        //}
        //else if (timeSinceNoteStartedPlaying < keysADSR.SustainT)
        //{
        //    currentlyBeingPlayed[index].startPlayTime = (float)AudioSettings.dspTime - keysADSR.DecayT;
        //}
        //else if (timeSinceNoteStartedPlaying < keysADSR.ReleaseT)
        //{
        //    float between = Mathf.InverseLerp(keysADSR.SustainT, keysADSR.ReleaseT, timeSinceNoteStartedPlaying);
        //    float volumeModifier = Mathf.Lerp(keysADSR.Sustain, 0.0f, between);
        //    currentlyBeingPlayed[index].startPlayTime = (float)AudioSettings.dspTime - Mathf.InverseLerp(0, keysADSR.Decay, volumeModifier);
        //}
        //else
        //{
        //    currentlyBeingPlayed[index].startPlayTime = (float)AudioSettings.dspTime;

        //}
        //currentlyBeingPlayed[index].startPlayTime = (float)AudioSettings.dspTime;
        //Debug.Log("Playing " + frequency + " at " + currentlyBeingPlayed[index].startPlayTime);

    }

    void OnAudioRead(float[] data)
    {
        for (int j = 0; j < currentlyBeingPlayed.Length; j++)
        {
            if ((float)AudioSettings.dspTime - currentlyBeingPlayed[j].startPlayTime > ADSRtime) continue;

            fundementalToneFrequency = currentlyBeingPlayed[j].fundementalFrequency;
            //Debug.Log("START " + currentlyBeingPlayed[j].position);
            for (int i = 0; i < data.Length; i++)
            {
                float volumeModifier = ADSR.Evaluate(currentlyBeingPlayed[j].position / BufferedSamples);

                data[i] = Mathf.Sin(2 * Mathf.PI * fundementalToneFrequency * currentlyBeingPlayed[j].position / SampleRate) * gain * volumeModifier;
                currentlyBeingPlayed[j].position++;
            }
           // Debug.Log("END " + currentlyBeingPlayed[j].position);
        }
            
    }

    void OnAudioSetPosition(int newPosition)
    {
        for (int j = 0; j < currentlyBeingPlayed.Length; j++)
        {
            if ((float)AudioSettings.dspTime - currentlyBeingPlayed[j].startPlayTime > ADSRtime) continue;
            currentlyBeingPlayed[j].position = newPosition;
        }
    }


    public float ReturnSuperimposedHarmonicsSeries(int dataIndex)
    {
        float superImposed = 0.0f;

        for (int i = 1; i <= 12; i++)
        {
            float harmonicFrequency = fundementalToneFrequency * i;

            float timeAtTheBeginig = (float)(AudioSettings.dspTime % (1.0 / (double)harmonicFrequency)); // very important to deal with percision issue as dspTime gets large

            float exactTime = timeAtTheBeginig + (float)dataIndex / SampleRate;

            superImposed += Mathf.Sin(exactTime * harmonicFrequency * 2f * Mathf.PI) * harmonicStrengths[i - 1];
        }

        return superImposed;
    }
}
