using System.IO;
using UnityEngine;

/// <summary>
/// THE (ITS STATIC ITS A SINGLETON) BeatmapLoader will load JSON files into engine usable BeatmapData objects
/// </summary>
public static class BeatmapLoader
{
    /// <summary>
    /// Loads a BeatmapData object from a JSON file and tell it to parse itself.
    /// </summary>
    /// <param name="filePath">Path of the JSON to load</param>
    public static BeatmapData LoadFromJson(string filePath)
    {
        try // "try to run this code"
        {
            string json = File.ReadAllText(filePath); // grab that shit
            BeatmapData data = JsonUtility.FromJson<BeatmapData>(json); // instatiate a beatmapdata object from the json utility
            data.ParseCsv(); // You should parse yourself, NOW!
            return data;
        }
        catch (System.Exception e) // "and if anything goes wrong, don't crash, just run this code instead"
        {
            Debug.LogError($"Failed to load beatmap: {filePath}\n{e}");
            return null;
        }
    }
}
