using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "SoundEffectsBank", menuName = "Scriptable Objects/SoundEffectsBank")]
public class SoundEffectsBank : ScriptableObject
{
    [System.Serializable]
    public struct SoundEffect
    {
        public string name;
        public EventReference normal;
        public EventReference flesh;
    }

    public SoundEffect[] soundEffects;
}
