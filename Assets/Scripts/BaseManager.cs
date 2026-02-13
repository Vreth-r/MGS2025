using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton exposing GameObjects in the `Base` scene to the Game and Editor Managers,
/// reponsible for loading Note prefabs and making those available as well
/// </summary>
public class BaseManager : MonoBehaviour
{
    public static BaseManager Instance { get; private set; }

    [Header("Beatmap")]
    public string beatmapFileName = "fuckoff.json"; // A file name in /StreamingAssets/Beatmaps
    public BeatmapData beatmap; // the beatmap to run

    [Header("Hooks")]
    public LaneController[] lanes; // Lane hooks (inspector)
    public AudioSource audioSource; // audio hook (inspector)


    private float offset;
    public float Offset
    {
        get => offset;
        set
        {
            offset = (float)Math.Round((double)value, 6);
            foreach (var lane in lanes)
            {
                lane.Offset = offset;
            }
        }
    }
    private int scale = 5;
    public int Scale
    {
        get => scale;
        set
        {
            scale = value;
            Offset = offset;
            foreach (var lane in lanes)
            {
                lane.Scale = value;
            }
        }
    }

    public float step = 1f;

    // Note prefabs (inspector)
    public GameObject deadNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject tapNotePrefab;
    public Dictionary<string, GameObject> NotePrefabs { get; private set; }

    private void Awake()
    {

        NotePrefabs = new Dictionary<string, GameObject>
        {
            {"Tap", tapNotePrefab },
            {"Hold", holdNotePrefab },
{"Dead", deadNotePrefab }
        }; // this is a rare instance of hardcoding being ok do to for non dynamic references.
           // the reason why i am storing them in prefabs is because it allows for custom behavior and visual options.
           // you can do it with code yeah but theres a fine line between game programming and programming a game yk. TLDR, use the engine features they save time.

        Instance = this;

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Beatmaps", beatmapFileName); // makes a nice readble path to the beatmap
        beatmap = BeatmapData.Load(path);
        if (beatmap == null)
        {
            Debug.LogError("Failed to load beatmap. Abort!");
            enabled = false;
            return; // basically just tell it to break to avoid any loops
        }

        foreach (var note in beatmap.notes)
        {
            if (lanes[note.lane].notes == null)
            {
                Debug.LogError(note.lane);
            }
            lanes[note.lane].SpawnNote(note);
        }

    }

    public void Start()
    {
        // Deliberately, explicitly invoke setter
        Scale = scale;
    }

    public void Scroll(LaneController.Side side = LaneController.Side.Right)
    {

        var step = this.step * (side == LaneController.Side.Right ? -1 : 1);
        Offset += step;
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("MainMenu-Copy", LoadSceneMode.Single);
    }
}
