using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsPlayer : MonoBehaviour
{
    [Header("Sound Effects")]
    public SoundEffectsBank soundEffectsBank;
    public bool UseFleshVersion = true;
    private Dictionary<string, SoundEffectsBank.SoundEffect> soundEffectDict;

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
}
