using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A BeatmapPlayer handles the logic to run a beatmap.
/// TIS NOT A MONOBEHAVIOR SHOULD ONE LOOK CLOSELY
/// IT IS DONE LIKE THIS FOR ORGANIZATION AND INSTANCING.
/// IF YOU THINK THATS UNNEEDED, YOU ARE FIRED AND LIKELY WILL BE ALSO FROM ANY JOB YOU HAVE IN THE FUTURE.
/// </summary>
public class BeatmapPlayer
{
    // listen, comments get to a point.
    private BeatmapData beatmap; // the beatmap to run (GM)
    private LaneController[] lanes; // lanes (GM)
    private float noteSpeed; // (GM)
    private float noteTravelDistance; // (GM)
    private int nextNoteIndex = 0; // what note we're gonna be on


    /// <summary>
    /// Good old constructor
    /// </summary>
    public BeatmapPlayer(BeatmapData beatmap, LaneController[] lanes, float noteSpeed)
    {
        this.beatmap = beatmap;
        this.lanes = lanes;
        this.noteSpeed = noteSpeed;

        foreach (var lane in lanes)
        {
            lane.noteSpeed = noteSpeed;
            this.noteTravelDistance = lane.hitZone.position.x - lane.spawnPoint.position.x;
        }
    }


    public void Update(float songTime)
    {
        float spawnLeadTime = noteTravelDistance / noteSpeed; // calc the time it takes for the note to cross the distance at its speed.

        // while there are still beats to be made AND the next beat time is less than the current song time plus the freshly calculated lead time
        while (nextNoteIndex < beatmap.notes.Count && beatmap.notes[nextNoteIndex].time <= songTime + spawnLeadTime)
        {
            var data = beatmap.notes[nextNoteIndex]; // grab note data
            if (data.lane >= 0 && data.lane < lanes.Length) // if the note is in a valid lane
            {
                //Debug.Log($"Spawning {data.type} note");
                lanes[data.lane].SpawnTypedNote(data, data.type); // spawn that shit
                // note that while C# method calls do pass values and not references (so it gets duplicated),
                // C#'s garbage collection system unwinds the stack after method execution and clears it, so there is 
                // very little overhead.
            }
            nextNoteIndex++; // move on to the next one
        }
    }

    // The endgame screen should only show after all notes have been 
    // resolved (hit or missed), not just when the last note spawns
    public bool IsFinished()
    {
        // For debuggging
        // for (int i = 0; i < beatmap.notes.Count; i++)
        // {
        //     var note = beatmap.notes[i];
        //     Debug.Log($"Note {i}: time={note.time}, lane={note.lane}, type={note.type}, resolved={note.resolved}");
        // }

        // Hold notes don't work right now so just check if the last note is resolved
        // return beatmap.notes.TrueForAll(note => note.resolved);

        return beatmap.notes[beatmap.notes.Count - 1].resolved;
    }
}
