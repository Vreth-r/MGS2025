using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "MGS2025/AudioDatabase")]
public class AudioDatabase : ScriptableObject {
    [Serializable]
    public struct AudioElement {
        public string name;
        public AudioClip audioClip;
    }
    public List<AudioElement> audioElements = new();
}
