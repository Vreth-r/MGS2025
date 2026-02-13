using UnityEngine;
using System.Linq;
using System;

public class EditorManager : MonoBehaviour
{
    public static EditorManager Instance { get; private set; }

    public string beatmapFileName;
    private BeatmapData beatmap;

    public LaneController[] lanes;
    public AudioSource audioSource;

    private int lane = 0;
    private float? addingHoldNote;
    private string selectedNoteType = "Tap";

    void Awake()
    {
        if (BaseManager.Instance != null)
        {
            beatmapFileName = BaseManager.Instance.beatmapFileName;
            beatmap = BaseManager.Instance.beatmap;
            lanes = BaseManager.Instance.lanes;
            audioSource = BaseManager.Instance.audioSource;
            BaseManager.Instance.step = 1f;
        }
        else
            Debug.LogError("BaseManager singleton has not been instantiated. Did you forget to load the Base scene?");

        if (Instance == null)
            Instance = this;

        foreach (var lane in lanes)
        {
            var special = GameObject.CreatePrimitive(PrimitiveType.Plane);
            special.transform.SetParent(lane.specialZone, false);
            special.transform.rotation = Quaternion.Euler(0, -90, 0);
            special.transform.localScale = Vector3.one * 0.1f;
            special.transform.position += Vector3.up * 0.1f;
        }
    }


    public void PlayTrack() { }
    public void PauseTrack() { }
    /*
    public void SetTrackTime(float playbackPercent)
    {
        trackTime = playbackPercent;
    }
    */
    // add functions here!

    public void SelectLane(int index)
    {
        if (index < 0 || index > 4)
        {
            Debug.LogError($"Refusing to select non-existent lane with index {index}");
            return;
        }
        lane = index;
    }

    public void SelectNoteType(string noteType)
    {
        switch (noteType)
        {
            case "Tap":
            case "Hold":
            case "Dead":
                selectedNoteType = noteType;
                break;
            default:
                Debug.LogError($"Invalid note type: {noteType}");
                break;
        }
    }

    public void Edit()
    {
        var selected = lanes[lane];


        if (addingHoldNote is not null)
        {
            // If we are in the middle of adding a HoldNote, end and add the HoldNote
            var note = new BeatmapData.NoteData
            {
                lane = selected.laneIndex,
                time = -(float)addingHoldNote,
                type = "Hold",
                parameters = new()
                {
                    {"endTime", -BaseManager.Instance.Offset}
                },
            };
            beatmap.notes.Add(note);
            beatmap.Save();
            selected.SpawnNote(note);
            addingHoldNote = null;
            return;
        }

        try
        {
            // This logic will probably require improvement later
            beatmap.notes.Remove(selected.GetComponentsInChildren<NoteBase>()
                    .Where(note =>
                    {
                        Debug.Log($"Note {note.transform.position}");
                        Debug.Log($"Lane {selected.specialZone.position}");
                        return note.transform.position.x == selected.specialZone.position.x;
                    })
                    .First()
                    .Decay());
            beatmap.Save();
            return;
        }
        catch (Exception e) when (e is InvalidOperationException || e is NullReferenceException) // if this exception fired, theres no note
        {
            _ = e;
        }

        // If we're still running, neither of the two blocks succeeded, so we should add a note
        if (selectedNoteType == "Hold")
            addingHoldNote = BaseManager.Instance.Offset;
        else
        {
            var note = new BeatmapData.NoteData
            {
                lane = selected.laneIndex,
                time = -BaseManager.Instance.Offset,
                type = selectedNoteType,
            };
            beatmap.notes.Add(note);
            beatmap.Save();
            selected.SpawnNote(note);
        }
    }

    public void Scroll(LaneController.Side side)
    {
        var step = BaseManager.Instance.step * (side == LaneController.Side.Right ? -1 : 1);
        // Don't allow the timeline to go further left of where we've already started placing a hold
        // note
        if (addingHoldNote is not null && BaseManager.Instance.Offset + step >= addingHoldNote)
        {
            Debug.Log("refusing to scroll past start of pending HoldNote");
            return;
        }

        BaseManager.Instance.Scroll(side);
    }

    // For referencing in the inspector
    public void ScrollLeft()
    {
        Scroll(LaneController.Side.Left);
    }
    public void ScrollRight()
    {
        Scroll(LaneController.Side.Right);
    }
    public void Back()
    {
        BaseManager.MainMenu();
    }


    // For inspector
    public static void SetScale(int index)
    {
        BaseManager.Instance.Scale = index + 1;
    }
    public static void SetStep(int index)
    {
        var factor = 0;
        //
        switch (index)
        {
            case 0:
                factor = 48;
                break;
            case 1:
                factor = 32;
                break;
            case 2:
                factor = 24;
                break;
            case 3:
                factor = 16;
                break;
            case 4:
                factor = 12;
                break;
            case 5:
                factor = 8;
                break;
            case 6:
                factor = 6;
                break;
            case 7:
                factor = 4;
                break;
            case 8:
                factor = 3;
                break;
            case 9:
                factor = 2;
                break;
            case 10:
                factor = 1;
                break;
        }
        BaseManager.Instance.step = 1f / factor;
    }
}
