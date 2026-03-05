using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
}