using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MicrophoneListener : MonobehaviourSingleton<MicrophoneListener>
{
    [SerializeField]
    private Image soundImg;
    [SerializeField]
    private Text TextVol;

    public float Sensitivity = 100;
    public float Loudness = 0;
    public float Pitch = 0;
    AudioSource audio;

    public float RmsValue;
    public float DbValue;
    public float PitchValue;

    private const int QSamples = 1024;
    private const float RefValue = 0.1f;
    private float Threshhold = 0.02f;

    float[] samples;
    private float[] spectrum;
    private float _fSample;

    public bool StartMicOnStartUp = true;
    public bool StopMicrophoneListener = false;
    public bool StartMicrophoneListener = false;

    private bool microphoneListenerOn = false;

    public bool DisableOutputSound = false;

    AudioSource src;

    public AudioMixer masterMixer;

    float timeSinceRestart = 0;

    void Start()
       {
        
    }
}
