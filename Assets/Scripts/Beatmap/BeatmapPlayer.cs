using System;
using UnityEngine;

public class BeatmapPlayer
{
    private readonly BeatmapData beatmap;
    private readonly LaneController[] lanes;
    private readonly float noteSpeed;

    private readonly float[] leadTimeByLane;
    private int nextNoteIndex;

    public BeatmapPlayer(BeatmapData beatmap, LaneController[] lanes, float noteSpeed)
    {
        this.beatmap = beatmap;
        this.lanes = lanes;
        this.noteSpeed = Mathf.Max(0.001f, noteSpeed);

        beatmap.notes.Sort((a, b) => a.time.CompareTo(b.time));

        leadTimeByLane = new float[lanes.Length];
        for (int i = 0; i < lanes.Length; i++)
        {
            var lane = lanes[i];
            if (lane == null) continue;

            lane.noteSpeed = this.noteSpeed;

            float dist = Mathf.Abs(lane.spawnPoint.position.x - lane.hitZone.position.x);
            leadTimeByLane[i] = dist / this.noteSpeed;
        }

        nextNoteIndex = 0;
    }

    public void ResetToTime(float songTime)
    {
        nextNoteIndex = 0;
        Update(songTime);
    }

    public void Update(float songTime)
    {
        var notes = beatmap.notes;

        while (nextNoteIndex < notes.Count)
        {
            var n = notes[nextNoteIndex];

            int laneIdx = Mathf.Clamp(n.lane, 0, lanes.Length - 1);
            float lead = leadTimeByLane[laneIdx];
            
            float spawnTime = n.time - lead;
            if (songTime < spawnTime)
                break;

            lanes[laneIdx].SpawnTypedNote(n, n.type);
            nextNoteIndex++;
        }
    }

    public bool IsFinished()
    {
        if (beatmap.notes.Count == 0) return true;
        if (nextNoteIndex < beatmap.notes.Count) return false;

        for (int i = 0; i < beatmap.notes.Count; i++)
        {
            if (!beatmap.notes[i].resolved)
                return false;
        }
        return true;
    }
}