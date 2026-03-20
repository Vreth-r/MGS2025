using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class BeatmapCsv
{
    private static readonly CultureInfo C = CultureInfo.InvariantCulture;

    public static string ToCsv(List<EditorNote> notes)
    {
        var sb = new StringBuilder();

        notes.Sort((a, b) =>
        {
            int t = a.time.CompareTo(b.time);
            return t != 0 ? t : a.lane.CompareTo(b.lane);
        });

        for (int i = 0; i < notes.Count; i++)
        {
            var n = notes[i];
            if (i > 0) sb.Append('\n');

            if (n.type == EditorNoteType.Hold)
            {
                sb.Append($"Hold,{n.lane},{n.time.ToString(C)},endTime={n.endTime.ToString(C)}");
            }
            else
            {
                sb.Append($"{n.type},{n.lane},{n.time.ToString(C)}");
            }
        }

        return sb.ToString();
    }

    public static List<EditorNote> FromCsv(string csv)
    {
        var result = new List<EditorNote>();

        if (string.IsNullOrWhiteSpace(csv))
            return result;

        string[] lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] parts = line.Split(',');
            if (parts.Length < 3)
            {
                UnityEngine.Debug.LogWarning($"[BeatmapCsv] Skipping malformed line: {line}");
                continue;
            }

            if (!Enum.TryParse(parts[0], true, out EditorNoteType type))
            {
                UnityEngine.Debug.LogWarning($"[BeatmapCsv] Unknown note type in line: {line}");
                continue;
            }

            if (!int.TryParse(parts[1], NumberStyles.Integer, C, out int lane))
            {
                UnityEngine.Debug.LogWarning($"[BeatmapCsv] Invalid lane in line: {line}");
                continue;
            }

            if (!float.TryParse(parts[2], NumberStyles.Float, C, out float time))
            {
                UnityEngine.Debug.LogWarning($"[BeatmapCsv] Invalid time in line: {line}");
                continue;
            }

            float endTime = time;

            if (type == EditorNoteType.Hold)
            {
                for (int i = 3; i < parts.Length; i++)
                {
                    string token = parts[i].Trim();
                    if (token.StartsWith("endTime=", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = token.Substring("endTime=".Length);
                        if (float.TryParse(value, NumberStyles.Float, C, out float parsedEnd))
                            endTime = parsedEnd;
                    }
                }
            }

            result.Add(new EditorNote
            {
                type = type,
                lane = lane,
                time = time,
                endTime = Mathf.Max(time, endTime)
            });
        }

        result.Sort((a, b) =>
        {
            int t = a.time.CompareTo(b.time);
            return t != 0 ? t : a.lane.CompareTo(b.lane);
        });

        return result;
    }
}