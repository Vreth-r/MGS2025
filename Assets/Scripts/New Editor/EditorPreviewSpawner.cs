using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewSpawner : MonoBehaviour
{
    [SerializeField] private EditorTransport transport;
    [SerializeField] private LaneController[] lanes;
    [SerializeField] private float noteSpeed = 5f;

    public List<EditorNote> notes = new();

    private float lastTime;
    private int nextIndex;

    private float[] laneLead;

    private void Awake()
    {
        laneLead = new float[lanes.Length];
        for (int i = 0; i < lanes.Length; i++)
        {
            float dist = Mathf.Abs(lanes[i].spawnPoint.position.x - lanes[i].hitZone.position.x);
            laneLead[i] = dist / Mathf.Max(0.001f, noteSpeed);
        }
    }

    public void ResetPreviewToTime(float t)
    {
        ClearSpawnedNotes();
        lastTime = t;
        nextIndex = 0;

        
        notes.Sort((a, b) => a.time.CompareTo(b.time));

        
        while (nextIndex < notes.Count)
        {
            var n = notes[nextIndex];
            float lead = laneLead[Mathf.Clamp(n.lane, 0, lanes.Length - 1)];
            float spawnTime = n.time - lead;
            if (spawnTime >= t) break;
            nextIndex++;
        }
    }

    private void Update()
    {
        if (transport == null) return;

        float now = transport.TimeSeconds;

        
        if (now < lastTime - 0.05f)
            ResetPreviewToTime(now);

        SpawnBetween(lastTime, now);
        lastTime = now;
    }

    private void SpawnBetween(float from, float to)
    {
        if (to <= from) return;

        while (nextIndex < notes.Count)
        {
            var n = notes[nextIndex];
            int laneIdx = Mathf.Clamp(n.lane, 0, lanes.Length - 1);
            float lead = laneLead[laneIdx];
            float spawnTime = n.time - lead;

            if (spawnTime > to) break;
            if (spawnTime >= from)
                SpawnOne(n, lanes[laneIdx]);

            nextIndex++;
        }
    }

    private void SpawnOne(EditorNote n, LaneController lane)
    {
        
        var data = new BeatmapData.NoteData
        {
            type = n.type.ToString(),
            lane = n.lane,
            time = n.time,
            parameters = new Dictionary<string, float>(),
            resolved = false
        };

        if (n.type == EditorNoteType.Hold)
            data.parameters["endTime"] = n.endTime;

        lane.noteSpeed = noteSpeed;
        lane.SpawnTypedNote(data, data.type);
    }

    private void ClearSpawnedNotes()
    {
        foreach (var lane in lanes)
        {
            for (int i = lane.transform.childCount - 1; i >= 0; i--)
            {
                var child = lane.transform.GetChild(i);
                if (child.TryGetComponent<NoteBase>(out _))
                    Destroy(child.gameObject);
            }
        }
    }
}