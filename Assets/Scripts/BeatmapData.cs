using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A BeatmapData contains information about a beatmap!
/// For those who don't know JSON:
/// 1. Learn it! https://www.json.org/json-en.html
/// 2. JSON is a *FORMAT*, not a language. There are multiple frameworks that read the json format and translates that shit directly
/// into a pre set class that is api readable. This is one such class.
/// </summary>
[Serializable] // Serialization is a bit of a more advanced concept. basically, a serialized object is marked by unity to be in a format it can edit.
// theres way more, but for now, mark things as serializable if you need to edit them in the inspector and they don't pop up automatically like certain vars.
public class BeatmapData
{
    public string songName; // the name of the song [metadata]
    public string songPath; // directory path to the audio. Should be in Assets/StreamingAssets but that could change
    public string songEvent; // fmod thing
    public float bpm; // the predetermined BPM of the song. Sound Design should be providing these to you.
    public string notesCsv; // Notes and timings [encoded(csv)]

    [NonSerialized] public List<NoteData> notes = new(); // raw note data [decodeTarget]

    [Serializable]
    public class NoteData // this is a POCO (Plain Ol' C# Object) class that just ties together info for notes from a CSV (Comma Seperated Values) string in the json
    {
        public string type; // the type of note like hold, tap, etc [parsingKey]
        public int lane; // the lane the note goes to
        public float time; // the time the note should be spawned

        // optionals (params for holding, etc)
        public Dictionary<string, float> parameters = new();
        // I admit this is over the top but like, its an easy way to allow for prototyping for other notes types later and keeps the csv readable,
        // which is normally something i would not care about but we have a lot of non-experienced people both in and out of programming so
        // a clearer understanding of architecture wherever it can be implemented is worth its weight in gold in large cooperative environments.
        // it also forces mandatory parameters while allowing any number of optional parameters.

        // whether the note has been hit or missed
        // needed to determine when game ends
        public bool resolved = false; 
    }

    /// <summary>
    /// Parses the CSV-like string into structured note data.
    /// Expected format: 
    ///     type,lane,time[,endtime,param1=val,param2=val,...]
    ///    ex: Hold,2,3.0,endTime=5.0,length=2.0,intensity=0.8
    /// </summary>
    public void ParseCsv()
    {
        notes.Clear(); // wipe the list if its been used or data carried over for whatever reason
        if (string.IsNullOrEmpty(notesCsv)) // if theres nothing to decode, fuck out of here
            return;

        string[] lines = notesCsv.Split('\n', StringSplitOptions.RemoveEmptyEntries); // split by a delimiter \n and dont track anything empty
        foreach (string line in lines) // for every encoded note
        {
            string[] parts = line.Trim().Split(','); // get rid of any leading/trailing whitespace and split by delimiter ,
            if (parts.Length < 3) continue; // format checking

            var data = new NoteData
            {
                type = parts[0],
                lane = int.Parse(parts[1]),
                time = float.Parse(parts[2]),
                parameters = new Dictionary<string, float>()
            };

            // parse optional params (always from index 3)
            for (int i = 3; i < parts.Length; i++)
            {
                // if you can't read this block I would just work on something else.
                string[] kv = parts[i].Split('='); // kv means "key-value"
                if (kv.Length == 2 && float.TryParse(kv[1], out float val))
                {
                    data.parameters[kv[0]] = val;
                }
            }
            notes.Add(data);
        }
        notes.Sort((a, b) => a.time.CompareTo(b.time)); // sorts notes based on time to appear so single iteration is possible
        // for those who dont know this is a lambda ^ (look it up)
        // beatmaps should already be formatted as such but you just never know.
    }
}
