using System.Linq;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right,
    }

    public int laneIndex; // technically an ID

    // note spawn coords (inspector)
    public Transform spawnLeft;
    public Transform spawnRight;

    // hit zone during gameplay, insert zone during editing (inspector)
    public Transform specialZone;
    public Transform notes;

    // Offset based on scrolling
    public float Offset
    {
        get
        {
            return notes.transform.localPosition.x / BaseManager.Instance.Scale;
        }
        set
        {
            notes.transform.localPosition = BaseManager.Instance.Scale * value * Vector3.right;
        }
    }

    private int scale = 5;
    public int Scale
    {
        get => scale;
        set
        {
            scale = value;
            notes.localScale = new(value, 1, 1);
            foreach (var note in notes.GetComponentsInChildren<NoteBase>())
            {
                note.gameObject.transform.localScale = new(1f / value, 1, 1);
                if (note is HoldNote hold)
                    hold.UpdateTail();
            }
        }
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

    private static void InvalidNote(string type)
    {
        Debug.LogWarning($"Unknown note type '{type}': skip that shit");
    }

    /// <summary>
    /// SpawnNote(), spawns a note!
    /// Spawns note from prefab based on note type.
    /// </summary>
    public void SpawnNote(BeatmapData.NoteData data)
    {
        if (!BaseManager.Instance.NotePrefabs.TryGetValue(data.type, out GameObject prefab))
        {
            InvalidNote(data.type);
            return;
        }

        if (data.lane != laneIndex)
        {
            Debug.LogWarning($"spawning note meant for lane {data.lane} on lane {laneIndex}");
        }

        // instantiate that shit
        var noteObj = Instantiate(
            prefab,
            Vector3.right * data.time,
            Quaternion.identity
        ); // instantiate note prefab
        noteObj.transform.SetParent(notes.transform, false);
        noteObj.transform.localScale = new(1f / scale, 1, 1);


        // see if the attatched script is either a NoteBase or a child class of NoteBase
        // THIS IS ONE OF THE FEW TIMES INHERITANCE IS USEFUL OUTSIDE OF WRITING API SOFTWARE.
        if (noteObj.TryGetComponent<NoteBase>(out var note))
        {
            if (note is HoldNote hold)
                hold.UpdateTail();
            note.Initialize(this, data);
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
