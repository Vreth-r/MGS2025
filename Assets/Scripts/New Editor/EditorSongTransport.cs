using UnityEngine;

public class EditorSongTransport : MonoBehaviour
{
    [SerializeField] private EditorFmodTransport transport;

    private string eventPath;

    public bool IsReady => transport != null && transport.IsReady;
    public bool IsPlaying => transport != null && transport.IsPlaying;

    public void SetEventPath(string path) => eventPath = path;

    public bool Load()
    {
        if (transport == null) return false;
        transport.Unload();
        //transport.SetEventPath(eventPath);
        return transport.Load();
    }

    public void Play()
    {
        if (!IsReady) return;
        transport.Play();
    }

    public void Pause()
    {
        if (!IsReady) return;
        transport.Pause();
    }

    public void Stop()
    {
        if (!IsReady) return;
        transport.Stop();
    }

    public void SeekSeconds(float seconds)
    {
        if (!IsReady) return;
        transport.SeekSeconds(seconds);
    }

    public float GetTimeSeconds()
    {
        if (!IsReady) return 0f;
        return transport.GetTimeSeconds();
    }

    public float GetLengthSeconds()
    {
        if (!IsReady) return 0f;
        return transport.GetLengthSeconds();
    }

    private void OnDestroy()
    {
        if (transport != null) transport.Unload();
    }
}