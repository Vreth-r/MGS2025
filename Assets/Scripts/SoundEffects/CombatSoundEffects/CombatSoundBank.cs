using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "CombatSoundBank", menuName = "Scriptable Objects/CombatSoundBank")]
public class CombatSoundBank : ScriptableObject
{
    //a sound asset file that acts as a bank of sounds (aka holds the references to the sound events and stuff)
    //can create these asset files via Create -> Scriptable Objects -> CombatSoundBanks

    [System.Serializable]
    public struct CombatSoundEffect
    {
        public string name; //name of sound
        public EventReference eventReference; //FMOD event reference to said sound
    }

    public CombatSoundEffect[] combatSoundEffects;
}
