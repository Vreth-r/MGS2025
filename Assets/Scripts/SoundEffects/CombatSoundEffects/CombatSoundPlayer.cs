using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CombatSoundPlayer : MonoBehaviour
{
    [Header("Combat Sound Effects")]
    public CombatSoundBank combatSoundBank; //the scriptable object that holds the sound event references

    private Dictionary<string, CombatSoundBank.CombatSoundEffect> soundEffectDict;
    private Dictionary<string, EventInstance> currentLoopingSounds = new Dictionary<string, EventInstance>(); //for the ult, cause the sounds needa loop, which needs extra stuff

    public static Action<int, Judgement> OnSuccessfulHit; //to signal the sound to actually play

    private void Awake()
    {
        soundEffectDict = new Dictionary<string, CombatSoundBank.CombatSoundEffect>();

        foreach (var soundEffect in combatSoundBank.combatSoundEffects)
        {
            soundEffectDict[soundEffect.name] = soundEffect; //pair the sound name with the effect for easy callings in a dictionary
        }
    }

    public void PlaySoundEffect(string name)
    {
        if (soundEffectDict.TryGetValue(name, out var soundEffect)) //sound exists in the dictionary
        {
            RuntimeManager.PlayOneShot(soundEffect.eventReference); //play sound
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
            if (soundEffectDict.TryGetValue(name, out var soundEffect)) //sound exists in the dictionary
            {
                EventInstance instance = RuntimeManager.CreateInstance(soundEffect.eventReference); //play sound that needs looping (currently for the ult)
                instance.start();
                currentLoopingSounds[name] = instance; //add the current instance of the looping sound to the dict, paired by name
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
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); //stops looping sounds and frees the resources
            instance.release();
            currentLoopingSounds.Remove(name);
        }
    }

    private void OnDestroy()
    {
        foreach (var soundEffect in currentLoopingSounds.Values) 
        {
            soundEffect.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); //stops sounds and frees the resources
            soundEffect.release();
        }

        currentLoopingSounds.Clear();
    }
}
