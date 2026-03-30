using UnityEngine;
using System;

public static class SettingsManager
{
    [Range(0f, 1f)] public static float musicVolume = 1f;
    [Range(0f, 1f)] public static float sfxVolume = 1f;

    // audio settings
    public static void SetMusicVolume(float newVolume)
    {
        musicVolume = newVolume;
    }
    public static void SetSFXVolume(float newVolume)
    {
        sfxVolume = newVolume;
    }
}