using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorNoteMarker : MonoBehaviour
{
    public EditorNote Note { get; private set; }

    private RectTransform rt;
    private Image img;
    private Action<EditorNote> onSelect;
    private TextMeshProUGUI label;

    private float lastPixelsPerSecond;
    private float lastLaneHeight;

    public void SetupWindow(EditorNote note, float pixelsPerSecond, float laneHeight, float windowStartTime, Action<EditorNote> onSelect)
    {
        Note = note;
        this.onSelect = onSelect;

        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        if (img == null) img = gameObject.AddComponent<Image>();
        if (img.color.a < 0.05f) img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);

        label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = note.type.ToString();

        if (label != null)
        {
            if (note.type == EditorNoteType.Hold)
                label.text = $"Hold\n{(note.endTime - note.time):0.00}s";
            else
                label.text = note.type.ToString();
        }

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => this.onSelect?.Invoke(Note));
        }

        lastPixelsPerSecond = pixelsPerSecond;
        lastLaneHeight = laneHeight;

        UpdateWindow(pixelsPerSecond, laneHeight, windowStartTime, note);
    }

    public void UpdateWindow(float pixelsPerSecond, float laneHeight, float windowStartTime, EditorNote note)
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        float x = (Note.time - windowStartTime) * pixelsPerSecond;
        float y = Note.lane * laneHeight;

        float width = 12f;
        if (Note.type == EditorNoteType.Hold)
            width = Mathf.Max(12f, (Note.endTime - Note.time) * pixelsPerSecond);

        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(width, laneHeight * 0.75f);

        lastPixelsPerSecond = pixelsPerSecond;
        lastLaneHeight = laneHeight;

        if (label != null)
        {
            if (note.type == EditorNoteType.Hold)
                label.text = $"Hold\n{(note.endTime - note.time):0.00}s";
            else
                label.text = note.type.ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
    }
}