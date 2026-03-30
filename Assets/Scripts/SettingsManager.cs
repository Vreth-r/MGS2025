using UnityEngine;
using System;
using FMODUnity;
using FMOD.Studio;

public static class SettingsManager
{
    [Range(0f, 1f)] public static float musicVolume = 1f;
    [Range(0f, 1f)] public static float sfxVolume = 1f;
    private static Bus musicBus;
    private static Bus sfxBus;

    private static bool initialized = false;

    private static void Init()
    {
        if (initialized) return;

        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        initialized = true;
    }

    // audio settings
    public static void SetMusicVolume(float newVolume)
    {
        Init();
        musicVolume = Mathf.Clamp01(newVolume);
        musicBus.setVolume(musicVolume);
        musicVolume = newVolume;
    }
    public static void SetSFXVolume(float newVolume)
    {
        Init();
        sfxVolume = Mathf.Clamp01(newVolume);
        sfxBus.setVolume(sfxVolume);
        sfxVolume = newVolume;
    }
}