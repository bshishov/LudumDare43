using System;
using UnityEngine;
using UnityEngine.Audio;


[Serializable]
public class Sound
{
    public AudioClip Clip;
    [Range(0f, 1.5f)] public float VolumeModifier = 1f;
    [Range(0.5f, 1.5f)] public float Pitch = 1f;
    public AudioMixerGroup MixerGroup;
}