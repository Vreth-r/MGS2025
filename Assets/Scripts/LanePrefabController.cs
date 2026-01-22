using System.Collections.Generic;
using UnityEngine;

public interface ILaneController
{
    public int Index { get; }
    public Transform Zone { get; }
}

public class LanePrefabController : MonoBehaviour, ILaneController
{
    public enum Side
    {
        Left,
        Right,
    }

    public int laneIndex; // technically an ID
    public int Index { get => laneIndex; }

    // note spawn coords (inspector)
    public Transform spawnLeft;
    public Transform spawnRight;

    // hit zone during gameplay, insert zone during editing (inspector)
    public Transform specialZone;
    public Transform Zone { get => specialZone; }

    // Note prefabs (inspector)
    public GameObject deadNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject tapNotePrefab;

    private Dictionary<string, GameObject> notePrefabs;

    // Should be updated by parent objects to change the behaviour of the `Scroll` method
    public float step = 1;
    // Offset based on scrolling
    public float Offset { get; private set; }
    // Indiciates whether or not we are currently trying to spawn a HoldNote, and if so, what the
    // start time bound is
    public float? spawningHold = null;

    /// <summary>
    /// Awake() is a Monobehavior method, it is run before the first frame after object load and all Start() methods.
    /// </summary>
    void Awake()
    {
        notePrefabs = new Dictionary<string, GameObject>
        {
            {"Tap", tapNotePrefab },
            {"Hold", holdNotePrefab },
            {"Dead", deadNotePrefab }
        }; // this is a rare instance of hardcoding being ok do to for non dynamic references.
        // the reason why i am storing them in prefabs is because it allows for custom behavior and visual options.
        // you can do it with code yeah but theres a fine line between game programming and programming a game yk. TLDR, use the engine features they save time.
    }

    /// <summary>
    /// Start() is called ONCE on object enable.
    /// It is called BEFORE any Update() calls
    /// It is called AFTER the Awake() call
    /// Fun fact, has an overload for enumeration!
    /// </summary>
    void Start()
    {
        // ill give you a description in the comment this time but going forward its gonna look like: HandleLanePress() -> OnLanePressed [subscription]
        InputManager.Instance.OnLanePressed += HandleLanePress; // subscribes HandleLanePress() (method in this script) to OnLanePressed Event
        //InputManager.Instance.OnLaneReleased += HandleLaneRelease;
    }

    /// <summary>
    /// OnDestroy() is called when the object is removed from a scene, including when its scene is unloaded (important for later).
    /// </summary>
    private void OnDestroy()
    {
        InputManager.Instance.OnLanePressed -= HandleLanePress; // unsubs (see Start())
    }

    /// <summary>
    /// Move notes 1 'step' to the left or right. Responsible for deleting notes that move into the
    /// kill zones.
    /// </summary>
    public void Scroll(Side side)
    {
        var step = this.step * (side == Side.Left ? -1 : 1);
        if (spawningHold is not null && (Offset + step) <= spawningHold)
        {
            return;
        }
        foreach (var note in GetComponentsInChildren<NoteBase>())
        {
            note.transform.position += new Vector3(step, 0, 0);
            if (note.transform.position.x < spawnLeft.transform.position.x ||
                    note.transform.position.x > spawnRight.transform.position.x)
            {
                Destroy(note);
            }
        }
        Offset += step;
    }

    private static void InvalidNote(string type)
    {
        Debug.LogWarning($"Unknown note type '{type}': skip that shit");
    }

    /// <summary>
    /// SpawnNote(), spawns a note!
    /// Spawns note from prefab based on note type.
    /// </summary>
    public void SpawnNote(BeatmapData.NoteData data, Side side)
    {
        if (!notePrefabs.TryGetValue(data.type, out GameObject prefab))
        {
            InvalidNote(data.type);
            return;
        }

        // instantiate that shit
        var noteObj = Instantiate(
            prefab,
            (side == Side.Left ? spawnLeft : spawnRight).position,
            Quaternion.identity,
            transform
        ); // instantiate note prefab

        // see if the attatched script is either a NoteBase or a child class of NoteBase
        // THIS IS ONE OF THE FEW TIMES INHERITANCE IS USEFUL OUTSIDE OF WRITING API SOFTWARE.
        if (noteObj.TryGetComponent<NoteBase>(out var note))
        {
            note.Initialize(this, 0, data);
        }
    }

    /// <summary>
    /// Adds a new note to the beatmap at the position of the 'Special Zone'. Only for use in the
    /// editor.
    ///
    /// If `type` is 'Hold', will stay in 'adding note' mode until `EndSpawnSpecial` is called
    /// </summary>
    public void BeginSpawnSpecial(string type, ref BeatmapData data)
    {
        if (spawningHold is not null)
        {
            Debug.LogWarning($"attempt to start adding a new note to the beat map while in the middle of adding a HoldNote (start: {{ {spawningHold} }}) already");
        }
        if (type == "Tap" || type == "Dead")
        {
            BeatmapData.NoteData noteData = new()
            {
                type = type,
                lane = laneIndex,
                time = Offset,
            };
            data.notes.Add(noteData);
            data.Save();
            var noteObj = Instantiate(notePrefabs[type], specialZone.position, Quaternion.identity, transform);
            // see if the attatched script is either a NoteBase or a child class of NoteBase
            // THIS IS ONE OF THE FEW TIMES INHERITANCE IS USEFUL OUTSIDE OF WRITING API SOFTWARE.
            if (noteObj.TryGetComponent<NoteBase>(out var note))
            {
                note.Initialize(this, 0, noteData);
            }

        }
        else if (type == "Hold")
        {
            spawningHold = Offset;
            // Can't put the start and the end in the same spot
            Scroll(Side.Right);
        }
        else
        {
            InvalidNote(type);
        }
    }
    public void EndSpawnSpecial(ref BeatmapData data)
    {
        if (spawningHold is null)
        {
            Debug.LogError("attempt to finish adding a HoldNote without starting");
            return;
        }
        BeatmapData.NoteData noteData = new()
        {
            type = "Hold",
            lane = laneIndex,
            time = (float)spawningHold,
            parameters = new Dictionary<string, float>
            {
                {"endTime", Offset},
            },
        };
        data.Save();

        // reset the position back to the start of the hold note
        while (Offset > spawningHold)
        {
            Scroll(Side.Left);
        }
        spawningHold = null;

        var noteObj = Instantiate(notePrefabs["Hold"], specialZone.position, Quaternion.identity, transform);
        // see if the attatched script is either a NoteBase or a child class of NoteBase
        // THIS IS ONE OF THE FEW TIMES INHERITANCE IS USEFUL OUTSIDE OF WRITING API SOFTWARE.
        if (noteObj.TryGetComponent<NoteBase>(out var note))
        {
            note.Initialize(this, 0, noteData);
        }
    }

    /// <summary>
    /// HandleLanePress(int) is called whenever a key is pressed BY THE INPUTMANAGER USING EVENTS.
    /// See the InputManager for more info!
    /// </summary>
    /// <param name="lane">The lane that got proced</param>
    private void HandleLanePress(int lane)
    {
        bool justHit = false;
        if (lane != laneIndex) return; // fuck off if its not the lane we care about [checkCond]

        // Detect closest note in hit zone
        foreach (Transform child in transform) // for every note
        {
            if (child.TryGetComponent<NoteBase>(out var note) && note.IsInHitZone(specialZone)) // if the note is not nothing (it happens) and the note thinks its in the hitzone
            {
                justHit = true;
                note.OnKeyPressed(); // tell the note it hath been pressed
                note.ResolveNote();
                break; // dont need to check the rest, semantically (and design wise) it is impossible for two notes to be in the same place.
            }
        }
        if (!justHit)
        {
            ScoreManager.Instance.AddScore(1f); //miss
        }
    }
}
