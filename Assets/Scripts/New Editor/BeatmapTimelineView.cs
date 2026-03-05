using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BeatmapTimelineView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Layout")]
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform[] laneRows;

    [Header("Layers (optional but recommended)")]
    [SerializeField] private RectTransform gridLayer;    
    [SerializeField] private RectTransform markerLayer;  

    [SerializeField] private GameObject noteMarkerPrefab;

    [Header("Scale")]
    [SerializeField] private float pixelsPerSecond = 120f;
    [SerializeField] private float laneHeight = 60f;

    [Header("Window Mode")]
    [SerializeField] private float windowSeconds = 8f;   
    [SerializeField] private float leftPadSeconds = 0f;  

    [SerializeField] private bool autoScaleToViewport = true;
    private float windowStartTime;

    private int laneCount;

    
    [Header("Beat Grid")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private float bpm = 120f;
    [Tooltip("How many subdivisions per beat (e.g., 4 = quarter-beat grid)")]
    [SerializeField] private int subdivisionsPerBeat = 4;
    [Tooltip("How many beats per bar (e.g., 4 for 4/4)")]
    [SerializeField] private int beatsPerBar = 4;

    [Header("Grid Prefabs")]
    [SerializeField] private Image gridLinePrefab;            
    [SerializeField] private TextMeshProUGUI gridLabelPrefab; 

    [Header("Labeling")]
    [SerializeField] private bool showLabels = true;
    [Tooltip("Label every N bars (1 = every bar, 2 = every other bar)")]
    [SerializeField] private int labelEveryBars = 1;

    
    private readonly List<Image> linePool = new();
    private readonly List<TextMeshProUGUI> labelPool = new();
    private int lineUsed;
    private int labelUsed;

    
    private readonly Dictionary<EditorNote, EditorNoteMarker> markerByNote = new();
    private readonly List<EditorNote> _toRemove = new();

    
    private bool pointerDown;
    private Vector2 pointerDownScreen;
    private bool didDrag;
    [SerializeField] private float dragThresholdPixels = 6f;

    private Action<int, float> onClick;
    private Action<int, float> onDragStart;
    private Action<int, float> onDragUpdate;
    private Action<int, float> onDragEnd;
    private Action<EditorNote> onSelect;

    public void Initialize(
        int laneCount,
        Action<int, float> onClick,
        Action<int, float> onDragStart,
        Action<int, float> onDragUpdate,
        Action<int, float> onDragEnd,
        Action<EditorNote> onSelect)
    {
        this.laneCount = laneCount;
        this.onClick = onClick;
        this.onDragStart = onDragStart;
        this.onDragUpdate = onDragUpdate;
        this.onDragEnd = onDragEnd;
        this.onSelect = onSelect;
    }

    public void SetWindowStart(float timeSeconds)
    {
        windowStartTime = Mathf.Max(0f, timeSeconds);
    }

    
    public void SetBpm(float newBpm)
    {
        bpm = Mathf.Max(1f, newBpm);
    }

    private static void PinTopLeft(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    public void RenderWindow(List<EditorNote> notes, float songLengthSeconds = 0f)
    {
        float viewportWidth = viewport != null ? viewport.rect.width : (windowSeconds * pixelsPerSecond);
        if (autoScaleToViewport && viewportWidth > 1f)
            pixelsPerSecond = viewportWidth / Mathf.Max(0.01f, windowSeconds);

        float width = viewportWidth;
        float height = laneCount * laneHeight;

        PinTopLeft(content);
        content.sizeDelta = new Vector2(width, laneCount * laneHeight);

        if (gridLayer == null) gridLayer = content;
        if (markerLayer == null) markerLayer = content;

        PinTopLeft(gridLayer);
        PinTopLeft(markerLayer);

        gridLayer.sizeDelta   = content.sizeDelta;
        markerLayer.sizeDelta = content.sizeDelta;

        for (int i = 0; i < laneRows.Length; i++)
        {
            if (laneRows[i] == null) continue;

            PinTopLeft(laneRows[i]);
            laneRows[i].anchoredPosition = new Vector2(0f, -(i * laneHeight));
            laneRows[i].sizeDelta = new Vector2(width, laneHeight);
        }

        
        if (gridLayer == null) gridLayer = content;
        if (markerLayer == null) markerLayer = content;

        
        if (showGrid)
            RenderBeatGrid(width);

        
        float minT = windowStartTime - leftPadSeconds;
        float maxT = windowStartTime + windowSeconds;

        _toRemove.Clear();
        foreach (var kv in markerByNote)
            _toRemove.Add(kv.Key);

        foreach (var n in notes)
        {
            float noteEnd = (n.type == EditorNoteType.Hold) ? n.endTime : n.time;

            if (noteEnd < minT) continue;
            if (n.time > maxT) continue;

            _toRemove.Remove(n);

            if (!markerByNote.TryGetValue(n, out var marker) || marker == null)
            {
                marker = CreateMarkerWindow(n);
                markerByNote[n] = marker;
            }

            marker.UpdateWindow(pixelsPerSecond, laneHeight, windowStartTime, n);
        }

        for (int i = 0; i < _toRemove.Count; i++)
        {
            var n = _toRemove[i];
            if (markerByNote.TryGetValue(n, out var marker) && marker != null)
                Destroy(marker.gameObject);
            markerByNote.Remove(n);
        }
    }

    private void RenderBeatGrid(float width)
    {
        
        lineUsed = 0;
        labelUsed = 0;

        if (gridLinePrefab == null) return;
        if (showLabels && gridLabelPrefab == null) showLabels = false;

        float secondsPerBeat = 60f / Mathf.Max(1f, bpm);
        int sub = Mathf.Max(1, subdivisionsPerBeat);
        float step = secondsPerBeat / sub;

        float minT = windowStartTime - leftPadSeconds;
        float maxT = windowStartTime + windowSeconds;

        
        int firstIndex = Mathf.CeilToInt(minT / step);
        float firstT = firstIndex * step;

        float height = laneCount * laneHeight;

        for (float t = firstT; t <= maxT + 0.0001f; t += step)
        {
            
            float beatF = t / secondsPerBeat;
            int beatIndex = Mathf.RoundToInt(beatF); 
            bool isBeat = Mathf.Abs(beatF - beatIndex) < 0.0005f;

            bool isBar = false;
            if (isBeat && beatsPerBar > 0)
                isBar = (beatIndex % beatsPerBar) == 0;

            
            float lineWidth = isBar ? 3f : (isBeat ? 2f : 1f);

            
            float x = (t - windowStartTime) * pixelsPerSecond;

            
            var line = GetLine();
            var rt = (RectTransform)line.transform;
            rt.SetParent(gridLayer, false);

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(lineWidth, height);

            
            if (showLabels && isBar && labelEveryBars > 0)
            {
                int barIndex = beatsPerBar > 0 ? (beatIndex / beatsPerBar) : beatIndex;
                if ((barIndex % labelEveryBars) == 0)
                {
                    var label = GetLabel();
                    var lrt = (RectTransform)label.transform;
                    lrt.SetParent(gridLayer, false);

                    lrt.anchorMin = new Vector2(0f, 1f);
                    lrt.anchorMax = new Vector2(0f, 1f);
                    lrt.pivot = new Vector2(0f, 1f);

                    
                    lrt.anchoredPosition = new Vector2(x + 4f, -2f);

                    
                    
                    label.text = $"Bar {barIndex + 1}  ({t:0.00}s)";
                }
            }
        }

        
        for (int i = lineUsed; i < linePool.Count; i++)
            if (linePool[i] != null) linePool[i].gameObject.SetActive(false);

        for (int i = labelUsed; i < labelPool.Count; i++)
            if (labelPool[i] != null) labelPool[i].gameObject.SetActive(false);
    }

    private Image GetLine()
    {
        if (lineUsed < linePool.Count && linePool[lineUsed] != null)
        {
            var l = linePool[lineUsed++];
            l.gameObject.SetActive(true);
            return l;
        }

        var created = Instantiate(gridLinePrefab, gridLayer);
        linePool.Add(created);
        lineUsed++;
        return created;
    }

    private TextMeshProUGUI GetLabel()
    {
        if (labelUsed < labelPool.Count && labelPool[labelUsed] != null)
        {
            var l = labelPool[labelUsed++];
            l.gameObject.SetActive(true);
            return l;
        }

        var created = Instantiate(gridLabelPrefab, gridLayer);
        labelPool.Add(created);
        labelUsed++;
        return created;
    }

    public void SetSelected(EditorNote note)
    {
        foreach (var kv in markerByNote)
            kv.Value.SetSelected(kv.Key == note);
    }

    private EditorNoteMarker CreateMarkerWindow(EditorNote n)
    {
        var go = Instantiate(noteMarkerPrefab, markerLayer);
        var marker = go.GetComponent<EditorNoteMarker>();
        if (marker == null) marker = go.AddComponent<EditorNoteMarker>();

        marker.SetupWindow(n, pixelsPerSecond, laneHeight, windowStartTime, onSelect);
        return marker;
    }

    private bool TryGetLaneTime(PointerEventData e, out int lane, out float time)
    {
        lane = 0;
        time = 0f;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content, e.position, e.pressEventCamera, out var local))
            return false;

        float x = local.x;
        float y = -local.y;

        lane = Mathf.Clamp(Mathf.FloorToInt(y / laneHeight), 0, laneCount - 1);

        float dt = Mathf.Clamp(x / pixelsPerSecond, 0f, windowSeconds);
        time = windowStartTime + dt;

        return true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!TryGetLaneTime(eventData, out int lane, out float time)) return;

        pointerDown = true;
        didDrag = false;
        pointerDownScreen = eventData.position;

        onDragStart?.Invoke(lane, time);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!pointerDown) return;

        if (!didDrag && Vector2.Distance(eventData.position, pointerDownScreen) >= dragThresholdPixels)
            didDrag = true;

        if (!TryGetLaneTime(eventData, out int lane, out float time)) return;
        onDragUpdate?.Invoke(lane, time);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pointerDown) return;
        pointerDown = false;

        if (!TryGetLaneTime(eventData, out int lane, out float time)) return;

        onDragEnd?.Invoke(lane, time);

        if (!didDrag)
            onClick?.Invoke(lane, time);
    }
}