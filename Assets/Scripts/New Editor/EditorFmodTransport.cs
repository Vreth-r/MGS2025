using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class EditorFmodTransport : MonoBehaviour
{
    [Header("FMOD")]
    [Tooltip("FMOD event path like event:/BGM/TestBGM OR leave empty and provide via code/UI.")]
    [SerializeField] private string eventPath;

    private EventInstance instance;
    private EventDescription desc;
    private bool hasDesc;
    private bool isValid;
    private int lengthMs;

    public bool IsReady => isValid && hasDesc;
    public bool IsPlaying { get; private set; }

    public string EventPath => eventPath;

    public void SetEventPath(string path)
    {
        eventPath = path;
    }

    public bool Load()
    {
        Unload();

        if (string.IsNullOrWhiteSpace(eventPath))
        {
            Debug.LogError("[EditorFmodTransport] eventPath is empty.");
            return false;
        }

        instance = RuntimeManager.CreateInstance(eventPath);
        isValid = instance.isValid();

        if (!isValid)
        {
            Debug.LogError($"[EditorFmodTransport] Failed to create instance for '{eventPath}'");
            return false;
        }

        instance.getDescription(out desc);
        hasDesc = desc.isValid();

        if (hasDesc)
        {
            desc.getLength(out lengthMs);
        }
        else
        {
            lengthMs = 0;
        }

        
        instance.setTimelinePosition(0);
        instance.start();
        instance.setPaused(true);
        IsPlaying = false;

        return true;
    }

    public void Unload()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }

        instance = default;
        desc = default;
        hasDesc = false;
        isValid = false;
        lengthMs = 0;
        IsPlaying = false;
    }

    public void Play()
    {
        if (!IsReady) return;
        instance.setPaused(false);
        IsPlaying = true;
    }

    public void Pause()
    {
        if (!IsReady) return;
        instance.setPaused(true);
        IsPlaying = false;
    }

    public void Stop()
    {
        if (!IsReady) return;
        instance.setPaused(true);
        instance.setTimelinePosition(0);
        IsPlaying = false;
    }

    public void SeekSeconds(float seconds)
    {
        if (!IsReady) return;
        int ms = Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f));
        instance.setTimelinePosition(ms);
    }

    public float GetTimeSeconds()
    {
        if (!IsReady) return 0f;
        instance.getTimelinePosition(out int ms);
        return ms / 1000f;
    }

    public float GetLengthSeconds()
    {
        if (!IsReady) return 0f;
        return lengthMs / 1000f;
    }

    private void OnDestroy()
    {
        Unload();
    }
}