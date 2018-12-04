using System;
using UnityEngine;


    [Serializable]
public class AudioClipWithVolume
{
    public AudioClip Clip;

    [Range(0f, 1.5f)]
    public float VolumeModifier = 1f;

    public float Pitch = 1f;
}

public static class AudioSourceExtensions
{
    public static void PlayClip(this AudioSource audioSource, AudioClipWithVolume clip, float additionalModifier = 1f)
    {
        if (clip == null || clip.Clip == null || clip.VolumeModifier < 1e-4)
            return;

        if (audioSource == null)
            return;

        audioSource.PlayOneShot(clip.Clip, clip.VolumeModifier * additionalModifier);
    }
}