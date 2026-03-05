using System.IO;
using UnityEngine;

public class EditorSongTransport : MonoBehaviour
{
    [Header("Built-in Event Playback")]
    [SerializeField] private EditorFmodTransport builtInEventTransport;

    [Header("User File Playback (Programmer Instrument)")]
    [SerializeField] private FmodProgrammerSongPlayer programmerPlayer;

    private string manualEventPath;
    private SongEntry currentEntry;

    public bool IsReady
    {
        get
        {
            if (currentEntry == null)
            {
                
                return builtInEventTransport != null && builtInEventTransport.IsReady;
            }

            if (currentEntry.sourceType == SongSourceType.FmodEvent)
                return builtInEventTransport != null && builtInEventTransport.IsReady;

            return programmerPlayer != null && programmerPlayer.IsReady && programmerPlayer.ReadyForSeek;
        }
    }

    public bool IsPlaying
    {
        get
        {
            if (currentEntry == null)
                return builtInEventTransport != null && builtInEventTransport.IsPlaying;

            if (currentEntry.sourceType == SongSourceType.FmodEvent)
                return builtInEventTransport != null && builtInEventTransport.IsPlaying;

            return programmerPlayer != null && programmerPlayer.IsReady && programmerPlayer.ReadyForSeek;
        }
    }

    
    public SongEntry CurrentEntry => currentEntry;

    
    public void SetEventPath(string eventPath)
    {
        manualEventPath = eventPath;
        
        currentEntry = null;
    }

    public bool Load()
    {
        
        StopAll();

        if (builtInEventTransport == null) return false;
        builtInEventTransport.SetEventPath(manualEventPath);
        return builtInEventTransport.Load();
    }

    
    public bool LoadSong(SongEntry entry)
    {
        StopAll();
        currentEntry = entry;
        if (currentEntry == null) return false;

        if (currentEntry.sourceType == SongSourceType.FmodEvent)
        {
            if (builtInEventTransport == null) return false;
            builtInEventTransport.SetEventPath(currentEntry.fmodEventPath);
            return builtInEventTransport.Load();
        }
        else
        {
            if (programmerPlayer == null) return false;
            string abs = Path.Combine(Application.persistentDataPath, currentEntry.relativeFilePath);
            return programmerPlayer.LoadUserFile(abs);
        }
    }

    
    public void Play()
    {
        if (!IsReady) return;
        if (UsingBuiltIn()) builtInEventTransport.Play();
        else programmerPlayer.Play();
    }

    public void Pause()
    {
        if (!IsReady) return;
        if (UsingBuiltIn()) builtInEventTransport.Pause();
        else programmerPlayer.Pause();
    }

    public void Stop()
    {
        if (!IsReady) return;
        if (UsingBuiltIn()) builtInEventTransport.Stop();
        else programmerPlayer.Stop();
    }

    public void SeekSeconds(float seconds)
    {
        if (!IsReady) return;
        if (UsingBuiltIn()) builtInEventTransport.SeekSeconds(seconds);
        else programmerPlayer.SeekSeconds(seconds);
    }

    public float GetTimeSeconds()
    {
        if (!IsReady) return 0f;
        return UsingBuiltIn() ? builtInEventTransport.GetTimeSeconds() : programmerPlayer.GetTimeSeconds();
    }

    public float GetLengthSeconds()
    {
        if (!IsReady) return 0f;
        return UsingBuiltIn() ? builtInEventTransport.GetLengthSeconds() : programmerPlayer.GetLengthSeconds();
    }

    private bool UsingBuiltIn()
    {
        if (currentEntry == null) return true; 
        return currentEntry.sourceType == SongSourceType.FmodEvent;
    }

    private void StopAll()
    {
        if (builtInEventTransport != null) builtInEventTransport.Unload();
        if (programmerPlayer != null) programmerPlayer.StopImmediate();
    }

    private void OnDestroy()
    {
        StopAll();
    }
}