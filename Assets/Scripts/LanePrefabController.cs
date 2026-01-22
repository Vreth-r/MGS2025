using System.Collections.Generic;
using UnityEngine;

public class LanePrefabController : MonoBehaviour
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

    // Note prefabs (inspector)
    public GameObject deadNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject tapNotePrefab;

    private Dictionary<string, GameObject> notePrefabs;

    // Should be updated by parent objects to change the behaviour of the `Scroll` method
    public float step = 1;
    // Offset based on scrolling
    public float Offset { get; private set; }

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
