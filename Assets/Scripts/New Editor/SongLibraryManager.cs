using System;
using System.Linq;
using UnityEngine;

public class SongLibraryManager : MonoBehaviour
{
    public static SongLibraryManager I { get; private set; }

    public SongLibraryData Data { get; private set; }
    public SongEntry Selected { get; private set; }

    public event Action OnChanged;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        //DontDestroyOnLoad(gameObject);

        Data = SongLibraryStore.Load();
    }

    public void AddBuiltInEvent(string eventPath, string displayName = null)
    {
        if (string.IsNullOrWhiteSpace(eventPath)) return;

        Data.songs.Add(new SongEntry
        {
            id = Guid.NewGuid().ToString("N"),
            displayName = string.IsNullOrWhiteSpace(displayName) ? eventPath : displayName,
            sourceType = SongSourceType.FmodEvent,
            fmodEventPath = eventPath
        });

        SongLibraryStore.Save(Data);
        OnChanged?.Invoke();
    }
    

    public void Remove(string id)
    {
        var s = Data.songs.FirstOrDefault(x => x.id == id);
        if (s == null) return;

        Data.songs.Remove(s);
        if (Selected != null && Selected.id == id) Selected = null;

        SongLibraryStore.Save(Data);
        OnChanged?.Invoke();
    }

    public void AddImportedUserSong(string relativePath, string displayName)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        Data.songs.Add(new SongEntry
        {
            id = Guid.NewGuid().ToString("N"),
            displayName = string.IsNullOrWhiteSpace(displayName) ? relativePath : displayName,
            sourceType = SongSourceType.UserFile,
            relativeFilePath = relativePath
        });

        SongLibraryStore.Save(Data);
        OnChanged?.Invoke();
    }

    public void Select(string id)
    {
        Selected = Data.songs.FirstOrDefault(x => x.id == id);
        OnChanged?.Invoke();
    }
}