using System;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Beat Settings")]
    public SongCompleteScreen screen;
    
    public float noteSpeed = 5f;
    public Transform hitZone;

    [Header("Beatmap")]
    public string beatmapFileName = "beatmap.json";

    [Header("Hooks")]
    public LaneController[] lanes;

    [Header("Note Prefabs")]
    public GameObject tapNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject deadNotePrefab;

    [Header("Pulse")]
    public float bpm;
    public float secondsPerBeat;
    public event Action OnPulse;

    [Header("Ultimate Note Saturation (optional)")]
    public float noteSaturation = 0.5f;

    
    private EventInstance songInstance;
    private bool songStarted;
    private string currentSongEventPath;

    
    private BeatmapData _beatmap;
    private BeatmapPlayer _player;
    private int _beatmapNoteCount;
    private float _nextPulseTime;
    private float _lastSongTime;
    private bool finishedSong = false;

    public int noteCount => _beatmapNoteCount;

    public float SongTimeSeconds => GetSongTimeSeconds();

    
    public Dictionary<string, GameObject> notePrefabs { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        
        if (lanes != null)
        {
            foreach (var lane in lanes)
                if (lane != null) lane.hitZone = hitZone;
        }

        notePrefabs = new Dictionary<string, GameObject>
        {
            { "Tap",  tapNotePrefab  },
            { "Hold", holdNotePrefab },
            { "Dead", deadNotePrefab }
        };

        LoadBeatmapFromFile(beatmapFileName);
    }

    private void Start()
    {
        FMODUnity.RuntimeManager.LoadBank("Master", true);
        FMODUnity.RuntimeManager.LoadBank("Master.strings", true);
        InputManager.Instance.EnableGameplay();
        StartBeatmapPlayback();
    }

    private void Update()
    {
        if (_player == null) return;

        float songTime = GetSongTimeSeconds();

        
        if (songTime < _lastSongTime - 0.05f)
            ResetBeatmapPlayer(songTime);

        _lastSongTime = songTime;

        _player.Update(songTime);

        if (Time.time >= _nextPulseTime)
        {
            _nextPulseTime += secondsPerBeat;
            OnPulse?.Invoke();
        }
        //temp button to open pause menu
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        MenuManager.Instance.OpenPause();
    }

    public void PauseSong()
    {
        if (songInstance.isValid())
            songInstance.setPaused(true);
    }

    public void ResumeSong()
    {
        if (songInstance.isValid())
            songInstance.setPaused(false);
    }

    public bool GameIsDone()
    {        
        if (Health.IsDead()) return true;
        
        if (!finishedSong && _player != null && _player.IsFinished())
        {
            // Displays track finish screen
            finishedSong = true;
            screen.TrackFinish();
        }
        return _player != null && _player.IsFinished();
    }

    public float BeatmapEndTimeSeconds
    {
        get
        {
            if (_beatmap == null || _beatmap.notes == null || _beatmap.notes.Count == 0) return 0f;

            float end = 0f;
            foreach (var n in _beatmap.notes)
            {
                float t = n.time;
                if (n.parameters != null && n.parameters.TryGetValue("endTime", out float e))
                    t = Mathf.Max(t, e);
                end = Mathf.Max(end, t);
            }
            return end;
        }
    }

    public void LoadBeatmapFromFile(string fileName)
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Beatmaps", fileName);
        var loaded = BeatmapLoader.LoadFromJson(path);

        if (loaded == null)
        {
            Debug.LogError($"[GameManager] Failed to load beatmap: {path}");
            enabled = false;
            return;
        }

        _beatmap = loaded;
        beatmapFileName = fileName;
        
        _beatmap.ParseCsv();

        bpm = _beatmap.bpm;
        secondsPerBeat = 60f / Mathf.Max(1f, bpm);

        _beatmapNoteCount = _beatmap.notes?.Count ?? 0;

        currentSongEventPath = _beatmap.songEvent;

        _player = new BeatmapPlayer(_beatmap, lanes, noteSpeed);
    }

    public void ReloadCurrentBeatmap()
    {
        LoadBeatmapFromFile(beatmapFileName);
        StartBeatmapPlayback();
    }

    public void SwitchSong(string newSongEventPath)
    {
        currentSongEventPath = newSongEventPath;
        RestartSong();
    }

    private void StartBeatmapPlayback()
    {
        StopSongImmediate();

        StartSong(currentSongEventPath);

        ResetBeatmapPlayer(0f);
        _nextPulseTime = Time.time + secondsPerBeat;
        _lastSongTime = 0f;

        ClearActiveNotes();
    }

    private void ResetBeatmapPlayer(float songTimeNow)
    {
        
        _player = new BeatmapPlayer(_beatmap, lanes, noteSpeed);
        _player.Update(songTimeNow);
    }

    private void ClearActiveNotes()
    {
        if (lanes == null) return;

        foreach (var lane in lanes)
        {
            if (lane == null) continue;

            for (int i = lane.transform.childCount - 1; i >= 0; i--)
            {
                var child = lane.transform.GetChild(i);
                if (child == null) continue;

                if (child.TryGetComponent<NoteBase>(out _))
                    Destroy(child.gameObject);
            }
        }
    }

    private void StartSong(string eventPath)
    {
        if (string.IsNullOrWhiteSpace(eventPath))
        {
            Debug.LogError("[GameManager] Beatmap songEvent is empty/null.");
            songStarted = false;
            return;
        }

        songInstance = RuntimeManager.CreateInstance(eventPath);

        RuntimeManager.AttachInstanceToGameObject(songInstance, transform, GetComponent<Rigidbody>());

        songInstance.start();
        songStarted = true;
    }

    public void RestartSong()
    {
        StartBeatmapPlayback();
    }

    public void SeekSong(float seconds)
    {
        if (!songStarted || !songInstance.isValid()) return;

        int ms = Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f));
        songInstance.setTimelinePosition(ms);

        
        ResetBeatmapPlayer(seconds);
        ClearActiveNotes(); 
        _lastSongTime = seconds;
    }

    private void StopSongImmediate()
    {
        if (!songInstance.isValid()) return;

        songInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        songInstance.release();
        songStarted = false;
    }

    private float GetSongTimeSeconds()
    {
        if (!songStarted || !songInstance.isValid()) return 0f;

        songInstance.getTimelinePosition(out int ms);
        return ms / 1000f;
    }

    private void OnDestroy()
    {
        StopSongImmediate();
    }

    private void OnEnable()
    {
        UltimateSystem.OnUltimateStarted += ApplyUltimateSaturation;
        UltimateSystem.OnUltimateFinished += ResetSaturation;
    }

    private void OnDisable()
    {
        UltimateSystem.OnUltimateStarted -= ApplyUltimateSaturation;
        UltimateSystem.OnUltimateFinished -= ResetSaturation;
    }

    private void ApplyUltimateSaturation()
        => ForEachActiveNoteSprite(r => r.color = ChangeSaturation(r.color, noteSaturation));

    private void ResetSaturation()
        => ForEachActiveNoteSprite(r => r.color = ChangeSaturation(r.color, 1f));

    private void ForEachActiveNoteSprite(Action<SpriteRenderer> act)
    {
        if (lanes == null) return;

        foreach (var lane in lanes)
        {
            if (lane == null) continue;

            for (int i = 0; i < lane.transform.childCount; i++)
            {
                var t = lane.transform.GetChild(i);
                if (!t.TryGetComponent<NoteBase>(out _)) continue;

                var r = t.GetComponentInChildren<SpriteRenderer>();
                if (r != null) act(r);
            }
        }
    }

    public Color ChangeSaturation(Color c, float newS)
    {
        Color.RGBToHSV(c, out float h, out _, out float v);
        return Color.HSVToRGB(h, newS, v);
    }
}