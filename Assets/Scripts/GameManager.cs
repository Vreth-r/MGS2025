using System;
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

    private int beatmapNoteCount;
    public int noteCount => beatmapNoteCount;

    [Header("Pulse Settings")]
    public float bpm; // Taken from beatmap data
    public float secondsPerBeat; // (Awake()) due to calculation
    private float lastPulseTime = 0; // The time of which the last pulse was triggered

    public event Action OnPulse; // Pulse Action

    [Header("Hooks")]
    public LaneController[] lanes; // Lane hooks (inspector)
    public AudioSource audioSource; // audio hook (inspector)
    public Dictionary<string, GameObject> notePrefabs; // reference storage for parsing from csv (GM)
    // "erm 🤓 what about enums?" fuck off like actually.

    public GameObject tapNotePrefab; // (inspector)
    public GameObject holdNotePrefab; // (inspector)
    public GameObject deadNotePrefab; // (inspector)

    [Header("Ultimate Note Saturation Settings")]
    public float noteSaturation = 0.5f; //to changes note saturation during the ultimate

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
        beatmap = BeatmapLoader.LoadFromJson(path);
        foreach (var lane in lanes) // set hitzone for each lane
        {
            lane.hitZone = hitZone;
        }
        if (beatmap == null)
        {
            Debug.LogError("Failed to load beatmap. Abort!");
            enabled = false;
            return; // basically just tell it to break to avoid any loops
        }
        beatmapPlayer = new BeatmapPlayer(beatmap, lanes, noteSpeed);
        //secondsPerBeat = 60f / bpm; // Seconds in each beat is just the bpm converted to seconds reciprocal.

        beatmapNoteCount = beatmap.notes.Count;
        bpm = beatmap.bpm;
        secondsPerBeat = 60f / bpm; // Seconds in each beat is just the bpm converted to seconds reciprocal.
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
        if (Time.time - secondsPerBeat >= lastPulseTime)
        {
            lastPulseTime += secondsPerBeat;
            // PULSE
            OnPulse?.Invoke();
        }
        //}
    }
    

    // The GameManager is in the scene, but not beatmap player, so beatmap player can't be accessed outside of here
    // beatmapPlayer is private so we need a public method to check if game is done
    public bool GameIsDone()
    {
        if (Health.IsDead()) return true;

        if (beatmapPlayer == null)
            return false;

        return beatmapPlayer.IsFinished();
    }

    //general custom function to change note saturation (currently used for the ultimate)
    public Color changeSaturation(Color currentColor, float newSaturation)
    {
        float h, s, v; //hue, saturation, value

        Color.RGBToHSV(currentColor, out h, out s, out v); //grabs the converted hsv values from the rgb colours (so we can access saturation values)

        return Color.HSVToRGB(h, newSaturation, v); //reconverts the colour values from hsv to rgb, but with a new saturation
    }

    //ultimate specific func to change all the saturations of the currently active notes in the lanes
    private void UltimateNoteSaturation()
    {
        for (int i = 0; i < lanes.Length; i = i + 1) //loops thru all lanes, grabs each lane and does stuff with them
        {
            LaneController lane = lanes[i]; //grab a single lane

            for (int x = 0; x < lane.transform.childCount; x = x + 1) //grabs all children in the lanes (we looking for the notes)
            {
                Transform child = lane.transform.GetChild(x); //grabs a child object

                if (child.TryGetComponent<NoteBase>(out var note)) //checks if the object type is a note (safeguards to prevent trying to do note stuff on non-notes)
                {
                    //changes the note saturation because the ultimate is active
                    note.GetComponentInChildren<SpriteRenderer>().color = changeSaturation(note.GetComponentInChildren<SpriteRenderer>().color, noteSaturation);
                }
            }
        }
    }

    //ultimate specific func to revert all the saturations of the currently active notes in the lanes
    private void ResetNoteSaturation()
    {
        for (int i = 0; i < lanes.Length; i = i + 1) //loops thru all lanes, grabs each lane and does stuff with them
        {
            LaneController lane = lanes[i];

            for (int x = 0; x < lane.transform.childCount; x = x + 1) //grabs all children in the lanes (we looking for the notes)
            {
                Transform child = lane.transform.GetChild(x); //grabs a child object

                if (child.TryGetComponent<NoteBase>(out var note)) //checks if the object type is a note (safeguards to prevent trying to do note stuff on non-notes)
                {
                    //reverts the note saturation because the ultimate is done
                    //currently hardcoded to revert to full saturation as all the notes are normally like this currently, might have to change later with some unique inspector value or something 
                    note.GetComponentInChildren<SpriteRenderer>().color = changeSaturation(note.GetComponentInChildren<SpriteRenderer>().color, 1);
                }
            }
        }
    }

    //subscribes both note saturation functions to be called to specific events in the UltimateSystem when its enabled
    private void OnEnable()
    {
        UltimateSystem.OnUltimateStarted += UltimateNoteSaturation; //when "OnUltimateStarted" is called, also call the func "UltimateNoteSaturation"
        UltimateSystem.OnUltimateFinished += ResetNoteSaturation; //when "OnUltimateFinished" is called, also call the func "ResetNoteSaturation"
    }

    //unsubscribes both note saturation functions from specific events in the UltimateSystem when its disabled
    private void OnDisable()
    {
        //OnDisable happens when the ult is inactive -> this is to prevent unwanted functions accidentally being called when they shouldnt be (like after the ult is finished)
        UltimateSystem.OnUltimateStarted -= UltimateNoteSaturation;
        UltimateSystem.OnUltimateFinished -= ResetNoteSaturation;
    }
}
