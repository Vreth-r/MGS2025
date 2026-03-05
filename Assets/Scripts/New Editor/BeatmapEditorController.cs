using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FMODUnity;


public enum EditorTool { Tap, Hold, Dead }

public class BeatmapEditorController : MonoBehaviour
{
    [Header("Song Library UI")]
    [SerializeField] private GameObject songLibraryPanel;
    [SerializeField] private bool pauseWhenLibraryOpen = true;

    [Header("Song")]
    [SerializeField] private EditorSongTransport transport;
    [SerializeField] private Button loadSongButton;

    [Header("Metadata")]
    [SerializeField] private TMP_InputField songNameInput;
    [SerializeField] private TMP_InputField songEventInput;
    [SerializeField] private TMP_InputField bpmInput;

    [Header("Transport UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Slider seekSlider;
    [SerializeField] private TMP_Text timeLabel;

    [Header("Tools UI")]
    [SerializeField] private TMP_Dropdown toolDropdown;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button deleteButton;

    [Header("Timeline")]
    [SerializeField] private BeatmapTimelineView timelineView;

    // [Header("Preview (optional)")]
    // [SerializeField] private EditorPreviewSpawner previewSpawner;

    [Header("Input System")]
    [SerializeField] private InputActionAsset editorInputAsset;
    [SerializeField] private string editorActionMapName = "Editor";
    private InputAction aToggleLibrary;

    private bool LibraryOpen => songLibraryPanel != null && songLibraryPanel.activeSelf;


    public readonly List<EditorNote> notes = new();

    private EditorTool currentTool = EditorTool.Tap;
    private EditorNote selected;

    private bool isDraggingHold;
    private EditorNote holdDraft;

    private bool userDraggingSeekSlider;

    private float lastWindowStart;

    private InputActionMap editorMap;
    private InputAction aPlayPause, aStop, aToolTap, aToolHold, aToolDead, aDelete, aSeekLeft, aSeekRight;

    private static readonly CultureInfo C = CultureInfo.InvariantCulture;

    private void Awake()
    {
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        stopButton.onClick.AddListener(Stop);
        exportButton.onClick.AddListener(ExportBeatmap);
        deleteButton.onClick.AddListener(DeleteSelected);

        if (loadSongButton != null)
        loadSongButton.onClick.AddListener(ReloadFmodSong);

        seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);
        AddSliderDragHooks(seekSlider);

        toolDropdown.onValueChanged.AddListener(i => SetTool((EditorTool)i));

        timelineView.Initialize(
            laneCount: 5,
            onClick: OnTimelineClick,
            onDragStart: OnTimelineDragStart,
            onDragUpdate: OnTimelineDragUpdate,
            onDragEnd: OnTimelineDragEnd,
            onSelect: SelectNote
        );

        SetupInputActions();

        if (transport == null)
        {
            Debug.LogError("[BeatmapEditorController] Missing transport reference.");
        }
        else
        {
            if (songEventInput != null && !string.IsNullOrWhiteSpace(songEventInput.text))
                transport.SetEventPath(songEventInput.text);

            transport.Load(); // starts paused at 0
        }

        if (bpmInput != null)
        {
            bpmInput.onValueChanged.AddListener(_ =>
            {
                if (float.TryParse(bpmInput.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                    timelineView.SetBpm(b);
            });
        }

        if (bpmInput != null && float.TryParse(bpmInput.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float b0))
            timelineView.SetBpm(b0);

        timelineView.SetWindowStart(0f);
        timelineView.RenderWindow(notes);
    }

    private void OnEnable()
    {
        editorMap?.Enable();
        HookActions(true);
    }

    private void OnDisable()
    {
        HookActions(false);
        editorMap?.Disable();
    }

    private void ReloadFmodSong()
    {
        if (transport == null) return;

        transport.SetEventPath(songEventInput != null ? songEventInput.text : "");
        if (transport.Load())
        {
            SeekTo(0f);
        }
    }

    private void Update()
    {
        UpdateTransportUI();

        if (transport != null && transport.IsReady)
        {
            float t = transport.GetTimeSeconds();
            float len = transport.GetLengthSeconds();

            if (Mathf.Abs(t - lastWindowStart) > 0.02f)
            {
                timelineView.SetWindowStart(t);
                timelineView.RenderWindow(notes, len);
                lastWindowStart = t;
            }
        }
    }

    private void SetupInputActions()
    {
        if (editorInputAsset == null) return;

        editorMap = editorInputAsset.FindActionMap(editorActionMapName, throwIfNotFound: true);

        aPlayPause = editorMap.FindAction("PlayPause", throwIfNotFound: false);
        aStop = editorMap.FindAction("Stop", throwIfNotFound: false);

        aToolTap = editorMap.FindAction("ToolTap", throwIfNotFound: false);
        aToolHold = editorMap.FindAction("ToolHold", throwIfNotFound: false);
        aToolDead = editorMap.FindAction("ToolDead", throwIfNotFound: false);

        aDelete = editorMap.FindAction("Delete", throwIfNotFound: false);

        aSeekLeft = editorMap.FindAction("SeekSmallLeft", throwIfNotFound: false);
        aSeekRight = editorMap.FindAction("SeekSmallRight", throwIfNotFound: false);

        aToggleLibrary = editorMap.FindAction("ToggleLibrary", throwIfNotFound: false);
    }

    private void HookActions(bool hook)
    {
        if (hook)
        {
            if (aPlayPause != null) aPlayPause.performed += OnPlayPause;
            if (aStop != null) aStop.performed += OnStop;

            if (aToolTap != null) aToolTap.performed += _ => SetTool(EditorTool.Tap);
            if (aToolHold != null) aToolHold.performed += _ => SetTool(EditorTool.Hold);
            if (aToolDead != null) aToolDead.performed += _ => SetTool(EditorTool.Dead);

            if (aDelete != null) aDelete.performed += _ => DeleteSelected();

            if (aSeekLeft != null) aSeekLeft.performed += _ => NudgeSeek(-0.25f);
            if (aSeekRight != null) aSeekRight.performed += _ => NudgeSeek(+0.25f);
            if (aToggleLibrary != null) aToggleLibrary.performed += OnToggleLibrary;
        }
        else
        {
            if (aPlayPause != null) aPlayPause.performed -= OnPlayPause;
            if (aStop != null) aStop.performed -= OnStop;
            if (aToggleLibrary != null) aToggleLibrary.performed += OnToggleLibrary;
        }
    }

    public void OnSongLoadedFromLibrary()
    {
        SeekTo(0f);
    }

    private void OnPlayPause(InputAction.CallbackContext _)
    {
        if (LibraryOpen) return;
        if (transport == null || !transport.IsReady) return;
        if (transport.IsPlaying) Pause();
        else Play();
    }

    private void OnStop(InputAction.CallbackContext _)
    {
        if (LibraryOpen) return;
        Stop();
    }

    private void OnToggleLibrary(InputAction.CallbackContext _)
    {
        ToggleSongLibrary();
    }

    private void ToggleSongLibrary()
    {
        if (songLibraryPanel == null) return;

        bool newState = !songLibraryPanel.activeSelf;
        SetSongLibraryOpen(newState);
    }

    private void SetSongLibraryOpen(bool open)
    {
        if (songLibraryPanel == null) return;

        songLibraryPanel.SetActive(open);

        if (pauseWhenLibraryOpen && transport != null && transport.IsReady)
        {
            if (open && transport.IsPlaying)
                transport.Pause();
        }

        if (timelineView != null)
            timelineView.enabled = !open;

        if (editorMap != null)
        {
            if (open)
            {
                editorMap.Disable();
                editorMap.Enable();
            }
        }
    }

    private void SetTool(EditorTool tool)
    {
        currentTool = tool;
        if (toolDropdown != null)
            toolDropdown.value = (int)tool;
    }

    private void NudgeSeek(float deltaSeconds)
    {
        if (transport == null || !transport.IsReady) return;
        SeekTo(transport.GetTimeSeconds() + deltaSeconds);
    }

    private void Play()
    {
        if (transport == null || !transport.IsReady) return;
        transport.Play();

        // Rolling preview window uses current time
        float t = transport.GetTimeSeconds();
        timelineView.SetWindowStart(t);
        timelineView.RenderWindow(notes, transport.GetLengthSeconds());
        lastWindowStart = t;
    }

    private void Pause()
    {
        if (transport == null || !transport.IsReady) return;
        transport.Pause();
    }

    private void Stop()
    {
        if (transport == null || !transport.IsReady) return;

        transport.Stop();

        timelineView.SetWindowStart(0f);
        timelineView.RenderWindow(notes, transport.GetLengthSeconds());
        lastWindowStart = 0f;
    }

    private void SeekTo(float seconds)
    {
        if (transport == null || !transport.IsReady) return;

        float len = transport.GetLengthSeconds();
        seconds = Mathf.Clamp(seconds, 0f, len);

        transport.SeekSeconds(seconds);

        timelineView.SetWindowStart(seconds);
        timelineView.RenderWindow(notes, len);
        lastWindowStart = seconds;
    }

    private void OnSeekSliderChanged(float v)
    {
        if (transport == null || !transport.IsReady) return;
        if (!userDraggingSeekSlider) return;

        float len = transport.GetLengthSeconds();
        SeekTo(Mathf.Clamp01(v) * len);
    }

    private void UpdateTransportUI()
    {
        if (transport == null || !transport.IsReady) return;

        float len = transport.GetLengthSeconds();
        float t = transport.GetTimeSeconds();

        if (!userDraggingSeekSlider)
            seekSlider.value = (len <= 0f) ? 0f : (t / len);

        if (timeLabel != null)
            timeLabel.text = $"{t:0.00}s / {len:0.00}s";
    }

    private void OnTimelineClick(int lane, float time)
    {
        if (LibraryOpen) return;
        if (currentTool == EditorTool.Hold) return;

        var n = new EditorNote
        {
            type = currentTool == EditorTool.Tap ? EditorNoteType.Tap : EditorNoteType.Dead,
            lane = lane,
            time = time,
            endTime = time
        };

        notes.Add(n);
        SortNotes();
        timelineView.RenderWindow(notes, transport != null && transport.IsReady ? transport.GetLengthSeconds() : 0f);
        SelectNote(n);
    }

    private void OnTimelineDragStart(int lane, float time)
    {
        if (currentTool != EditorTool.Hold) return;

        isDraggingHold = true;
        holdDraft = new EditorNote
        {
            type = EditorNoteType.Hold,
            lane = lane,
            time = time,
            endTime = time
        };

        notes.Add(holdDraft);
        timelineView.RenderWindow(notes, transport != null && transport.IsReady ? transport.GetLengthSeconds() : 0f);
        SelectNote(holdDraft);
    }

    private void OnTimelineDragUpdate(int lane, float time)
    {
        if (!isDraggingHold || holdDraft == null) return;

        holdDraft.endTime = Mathf.Max(time, holdDraft.time);
        timelineView.RenderWindow(notes, transport != null && transport.IsReady ? transport.GetLengthSeconds() : 0f);
    }

    private void OnTimelineDragEnd(int lane, float time)
    {
        if (!isDraggingHold || holdDraft == null) return;

        isDraggingHold = false;
        holdDraft.endTime = Mathf.Max(time, holdDraft.time);

        SortNotes();
        timelineView.RenderWindow(notes, transport != null && transport.IsReady ? transport.GetLengthSeconds() : 0f);
    }

    private void SelectNote(EditorNote n)
    {
        selected = n;
        timelineView.SetSelected(n);
    }

    private void DeleteSelected()
    {
        if (selected == null) return;
        notes.Remove(selected);
        selected = null;

        timelineView.RenderWindow(notes, transport != null && transport.IsReady ? transport.GetLengthSeconds() : 0f);
        timelineView.SetSelected(null);
    }

    private void ExportBeatmap()
    {
        string csv = BeatmapCsv.ToCsv(notes);

        string songName = songNameInput != null ? songNameInput.text : "";
        string songEvent = songEventInput != null ? songEventInput.text : "";

        string songId = "";
        string userFileRel = "";

        if (transport != null && transport.CurrentEntry != null)
        {
            songId = transport.CurrentEntry.id;

            if (transport.CurrentEntry.sourceType == SongSourceType.FmodEvent)
            {
                songEvent = transport.CurrentEntry.fmodEventPath;
                userFileRel = "";
            }
            else
            {
                songEvent = "event:/Music/ProgrammerSong";
                userFileRel = transport.CurrentEntry.relativeFilePath;
            }
        }

        float bpm = 120f;
        if (bpmInput != null)
            float.TryParse(bpmInput.text, NumberStyles.Float, C, out bpm);

        var data = new BeatmapData
        {
            songName = songName,
            songPath = "", // unused i know fuck you
            songEvent = songEvent,
            songId = songId,
            userFileRelative = userFileRel,
            bpm = bpm,
            notesCsv = csv
        };

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

        string dir = Path.Combine(Application.streamingAssetsPath, "Beatmaps");
        Directory.CreateDirectory(dir);

        string file = Path.Combine(dir, $"beatmap_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(file, json);

        Debug.Log($"[BeatmapEditor] Saved beatmap to: {file}\n\nCSV:\n{csv}");
    }

    private void SortNotes()
    {
        notes.Sort((a, b) =>
        {
            int t = a.time.CompareTo(b.time);
            return t != 0 ? t : a.lane.CompareTo(b.lane);
        });
    }

    private void AddSliderDragHooks(Slider slider)
    {
        if (slider == null) return;

        var trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

        trigger.triggers ??= new List<EventTrigger.Entry>();

        var begin = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        begin.callback.AddListener(_ => userDraggingSeekSlider = true);

        var end = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        end.callback.AddListener(_ => userDraggingSeekSlider = false);

        trigger.triggers.Add(begin);
        trigger.triggers.Add(end);
    }
}