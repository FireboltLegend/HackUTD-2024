using System;
using UnityEngine;
using System.Collections;


public enum AudioProviderType
{
    Microphone = 0,
    AudioClip = 1
}

[DisallowMultipleComponent]
[AddComponentMenu("Ready Player Me/Voice Handler", 0)]
public class VoiceHandler : MonoBehaviour
{
    private const string MouthOpenBlendShapeName = "mouthOpen";
    private const int AmplituteMultiplier = 10;
    private const int AudioSampleLength = 4096;

    private float[] audioSample = new float[AudioSampleLength];

    public AudioClip AudioClip = null;
    public AudioSource AudioSource = null;
    public AudioProviderType AudioProvider = AudioProviderType.Microphone;

    public float Volume;

    private void Start()
    {
        InitializeAudio();
    }

    private void Update()
    {
        Volume = GetAmplitude();
        //Debug.Log(Volume);
    }

    public void InitializeAudio()
    {
        try
        {
            if (AudioSource == null)
            {
                AudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (AudioProvider.Equals(AudioProviderType.Microphone))
            {
                SetMicrophoneSource();
            }
            else if (AudioProvider.Equals(AudioProviderType.AudioClip))
            {
                SetAudioClipSource();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"VoiceHandler.Initialize:/n" + e);
        }
    }

    private void SetMicrophoneSource()
    {
        #if !UNITY_WEBGL
        AudioSource.clip = Microphone.Start(null, true, 1, 44100);
        AudioSource.loop = true;
        AudioSource.mute = true;
        AudioSource.Play();
        #endif
    }

    private void SetAudioClipSource()
    {
        AudioSource.clip = AudioClip;
        AudioSource.loop = false;
        AudioSource.mute = false;
        AudioSource.Stop();
    }

    [Button]
    public void PlayCurrentAudioClip()
    {
        AudioSource.Play();
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        AudioClip = AudioSource.clip = audioClip;
        PlayCurrentAudioClip();
    }

    private float GetAmplitude()
    {
        if (AudioSource != null && AudioSource.clip != null && AudioSource.isPlaying)
        {
            float amplitude = 0f;
            AudioSource.clip.GetData(audioSample, AudioSource.timeSamples);

            foreach (var sample in audioSample)
            {
                amplitude += Mathf.Abs(sample);
            }

            return Mathf.Clamp01(amplitude / audioSample.Length * AmplituteMultiplier);
        }

        return 0;
    }

    private void OnDestroy()
    {
        audioSample = null;
    }
}