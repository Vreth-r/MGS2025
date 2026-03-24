using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public EventReference eventReference;

    private EventInstance musicInstance;

    private void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(eventReference);
        musicInstance.start();
    }

    private void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}
