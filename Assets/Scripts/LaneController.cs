using UnityEngine;

/// <summary>
/// A LaneController manages it's lane, including note spawning and hit registration.
/// Multi instance (non singleton)
/// </summary>
public class LaneController : MonoBehaviour
{
    public int laneIndex; // technically an ID
    //public GameObject notePrefab; // prefab (inspector) [hook] {artifact}
    public Transform spawnPoint; // note spawn coords (inspector)
    public Transform hitZone; // note hit coords (GM)
    public float noteSpeed = 5f; // the speed at which notes move because accesibility.
    // yk they never talk about MY accesibility needs of not giving a fuck about this

    /// <summary>
    /// Start() is called ONCE on object enable.
    /// It is called BEFORE any Update() calls
    /// It is called AFTER the Awake() call
    /// Fun fact, has an overload for enumeration!
    /// </summary>
    private void Start()
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
    /// SpawnNote(), spawns a note!
    /// Spawns note from prefab based on note type.
    /// </summary>
    public void SpawnTypedNote(BeatmapData.NoteData data, string type)
    {
        // ask GM pwetty pwease for the note prefab according to the type
        if (!GameManager.Instance.notePrefabs.TryGetValue(type, out GameObject prefab))
        {
            Debug.LogWarning($"Unknown note type '{type}': skip that shit");
            return;
        }

        // instantiate that shit
        GameObject noteObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform); // instantiate note prefab

        // see if the attatched script is either a NoteBase or a child class of NoteBase
        // THIS IS ONE OF THE FEW TIMES INHERITANCE IS USEFUL OUTSIDE OF WRITING API SOFTWARE.
        if (noteObj.TryGetComponent<NoteBase>(out var note))
        {
            note.Initialize(this, noteSpeed, data);
        }
        //noteObj.GetComponent<Note>().Initialize(this, noteSpeed); // this grabs the script set in the prefab and tells it some info to keep track of {artifact}

        //when the ult is active, tells the lanes to change the note saturation before they spawn new notes
        //(does not apply for already existing notes on the lanes, that happens in GameManager)
        if (UltimateSystem.UltimateActive)
        {
            //grabs the SpriteRenderer of the note instance and changes the saturation (see GameManager for the functions)
            SpriteRenderer renderer = noteObj.GetComponentInChildren<SpriteRenderer>();
            renderer.color = GameManager.Instance.changeSaturation(renderer.color, GameManager.Instance.noteSaturation);
        }
    }

    /// <summary>
    /// HandleLanePress(int) is called whenever a key is pressed BY THE INPUTMANAGER USING EVENTS.
    /// See the InputManager for more info!
    /// </summary>
    /// <param name="lane">The lane that got proced</param>
    private void HandleLanePress(int lane)
    {
        if (lane != laneIndex) return;

        NoteBase best = null;
        Judgement bestJudge = Judgement.Miss;
        float bestTiming = float.MaxValue;

        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent<NoteBase>(out var note)) continue;

            var j = note.GetJudgement(hitZone);
            if (j == Judgement.Miss) continue;

            float t = note.GetTimingSeconds(hitZone);
            if (t < bestTiming)
            {
                best = note;
                bestTiming = t;
                bestJudge = j;
            }
        }

        if (best != null)
        {
            // scoring once, on actual hit
            ScoreManager.Instance.AddScore(bestTiming);

            best.OnHit(bestJudge);
            best.ResolveNote();
        }
        else
        {
            // ghost tap
            ScoreManager.Instance.AddScore(1f);
            // optionally: Health.TakeDamage(smallGhostTapPenalty);
        }
    }
}
