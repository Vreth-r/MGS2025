using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    [Header("Sound Effects")]
    public SoundEffectsBank soundEffectsBank;
    public bool UseFleshVersion = true;
    private Dictionary<string, SoundEffectsBank.SoundEffect> soundEffectDict;
    private Dictionary<string, EventInstance> currentLoopingSounds = new Dictionary<string, EventInstance>();

    private void Awake()
    {
        soundEffectDict = new Dictionary<string, SoundEffectsBank.SoundEffect>();

        foreach (var soundEffect in soundEffectsBank.soundEffects)
        {
            soundEffectDict[soundEffect.name] = soundEffect;
        }
    }

    public void PlaySoundEffect(string name)
    {
        if (soundEffectDict.TryGetValue(name, out var soundEffect))
        {
            if (UseFleshVersion == true)
            {
                RuntimeManager.PlayOneShot(soundEffect.flesh);
            }

            else
            {
                RuntimeManager.PlayOneShot(soundEffect.normal);
            }
        }

        else
        {
            Debug.Log($"{name} sound effect not found");
        }
    }

    public void PlayLoopingSoundEffect(string name)
    {
        if (!currentLoopingSounds.ContainsKey(name))
        {
            if (soundEffectDict.TryGetValue(name, out var soundEffect))
            {
                if (UseFleshVersion == true)
                {
                    EventInstance instance = RuntimeManager.CreateInstance(soundEffect.flesh);
                    instance.start();
                    currentLoopingSounds[name] = instance;
                }

                else
                {
                    EventInstance instance = RuntimeManager.CreateInstance(soundEffect.normal);
                    instance.start();
                    currentLoopingSounds[name] = instance;
                }
            }

            else
            {
                Debug.Log($"{name} sound effect not found");
            }
        }
    }

    public void StopLoopingSoundEffect(string name)
    {
        if (currentLoopingSounds.TryGetValue(name, out var instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            currentLoopingSounds.Remove(name);
        }
    }

    private void OnDestroy()
    {
        foreach (var soundEffect in currentLoopingSounds.Values)
        {
            soundEffect.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            soundEffect.release();
        }

        currentLoopingSounds.Clear();
    }

}
