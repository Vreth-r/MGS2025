using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SongLibraryData
{
    public List<SongEntry> songs = new();
}

public enum SongSourceType
{
    FmodEvent,   
    UserFile     
}

[Serializable]
public class SongEntry
{
    public string id;              
    public string displayName;     
    public SongSourceType sourceType;

    
    public string fmodEventPath;

    
    public string relativeFilePath;

    
    public float bpmHint;
}

public static class SongLibraryStore
{
    private const string FileName = "song_library.json";
    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SongLibraryData Load()
    {
        try
        {
            if (!File.Exists(SavePath))
                return new SongLibraryData();

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SongLibraryData>(json) ?? new SongLibraryData();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SongLibraryStore] Load failed: {e}");
            return new SongLibraryData();
        }
    }

    public static void Save(SongLibraryData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SongLibraryStore] Save failed: {e}");
        }
    }
}