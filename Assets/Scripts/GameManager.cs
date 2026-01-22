using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// "That is my power. The Almighty." - Yhwach
/// The Game Manager does an assortment of things, mainly variable tracking and hooking.
/// 
/// The Game Manager (also the other files) are intended to work as if they are recreated for each level, you'll see what I mean
/// when we get to scene transitions
/// 
/// YES! I even documented my code!
/// </summary>
public class GameManager : MonoBehaviour
{
    // my commenting style is a bit unconventional, so heres an example of formatting i tend to stick to:
    // public string fuckOff = "Fuck You"; // Context for the variable and/or reason it exists (where it's assigned if thats not obvious) [tags]
    // [tags] function like keywords in Magic the Gathering, its a one work descriptor for a concept that should be well known.
    // I also sometimes add {artifact} on commented lines, as past interations can provide some useful info for why things are done the way they are now.
    // Anything with {artifact} is NOT to be uncommented, it likely does not have any supporting infrastructure and may break compilation
    public static GameManager Instance { get; private set; } // There can only be one. [singleton]

    [Header("Beat Settings")]
    // public float bpm = 120f; // the BPM to a given song, for the MVP, its 120 default {artifact}
    // private float secondsPerBeat; // (Awake()) due to calculation {artifact}
    //public float noteTravelDistance = 10f; // i need this for later {artifact}
    public float noteSpeed = 5f; // i need this for later!
    public Transform hitZone; // hit zone for notes (inspector)
    // the killzone is set a trigger collider and handles note deletion on its own.

    [Header("Beatmap")]
    public string beatmapFileName = "fuckoff.json"; // A file name in /StreamingAssets/Beatmaps
    private BeatmapData beatmap; // the beatmap to run
    private BeatmapPlayer beatmapPlayer; // the beatmap runner

    [Header("Hooks")]
    public LanePrefabController[] lanes; // Lane hooks (inspector)
    public AudioSource audioSource; // audio hook (inspector)
    public Dictionary<string, GameObject> notePrefabs; // reference storage for parsing from csv (GM)
    // "erm 🤓 what about enums?" fuck off like actually.

    public GameObject tapNotePrefab; // (inspector)
    public GameObject holdNotePrefab; // (inspector)
    public GameObject deadNotePrefab; // (inspector)

    /// <summary>
    /// Awake() is a Monobehavior method, it is run before the first frame after object load and all Start() methods.
    /// </summary>
    private void Awake()
    {
        
        Instance = this; // Assign singleton reference
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Beatmaps", beatmapFileName); // makes a nice readble path to the beatmap
        notePrefabs = new Dictionary<string, GameObject>
        {
            {"Tap", tapNotePrefab },
            {"Hold", holdNotePrefab },
            {"Dead", deadNotePrefab }
        }; // this is a rare instance of hardcoding being ok do to for non dynamic references.
        // the reason why i am storing them in prefabs is because it allows for custom behavior and visual options.
        // you can do it with code yeah but theres a fine line between game programming and programming a game yk. TLDR, use the engine features they save time.
        beatmap = BeatmapData.Load(path);
        foreach (var lane in lanes) // set hitzone for each lane
        {
            lane.specialZone = hitZone;
        }
        if (beatmap == null)
        {
            Debug.LogError("Failed to load beatmap. Abort!");
            enabled = false;
            return; // basically just tell it to break to avoid any loops
        }
        beatmapPlayer = new BeatmapPlayer(beatmap, lanes, noteSpeed);
        //secondsPerBeat = 60f / bpm; // Seconds in each beat is just the bpm converted to seconds reciprocal.
    }

    private void Start()
    {
        // uncomment this when you add some audioclips in the proper path
        // which is Assets/Resources/Audio/
        /*
        AudioClip clip = Resources.Load<AudioClip>(beatmap.songPath); // grab the associated beatmap audio
        if (clip == null) // error catching
        {
            Debug.LogError($"No song at Resources/{beatmap.songPath}");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        */

        // Enable gameplay input map
        InputManager.Instance.EnableGameplay();
    }

    /// <summary>
    /// Update() is called every frame, all Update() calls from every object complete before the next frame is started.
    /// </summary>
    private void Update()
    {
        //if (audioSource.isPlaying)
        //{
        // swap for audioSource.time when the audioSource is fully implemented
        beatmapPlayer.Update(Time.time);
        //}
    }
    

    // The GameManager is in the scene, but not beatmap player, so beatmap player can't be accessed outside of here
    // beatmapPlayer is private so we need a public method to check if game is done
    public bool GameIsDone()
    {
        if (beatmapPlayer == null)
            return false;

        return beatmapPlayer.IsFinished();
    }
}
