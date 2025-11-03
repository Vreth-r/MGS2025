using UnityEngine;
using System.Collections.Generic;

public class BeatMapper : MonoBehaviour
{
    const int PERFECT_WINDOW_MS = 200;
    const int GOOD_WINDOW_MS = 500;

    public MusicPlayer musicPlayer;

    public GameObject perfectHitPrefab;
    public GameObject goodHitPrefab;

    // 130f
    public float bpm = 120f;

    // time in milliseconds : beat has been hit (to avoid possible race conditions)
    private Dictionary<int, bool> beatMap = new();
    private float secondsPerBeat;

    // Reference to player position for spawning hit visual effects
    public Transform playerTransform;

    void Start()
    {
        Debug.Log("Initializing BeatMapper...");

        if (musicPlayer?.audioSource?.clip == null)
        {
            Debug.LogError("MusicPlayer or its AudioSource/Clip is not assigned!");
            return;
        }

        secondsPerBeat = 60f / bpm;
        MapBeats();
    }

    void MapBeats()
    {
        Debug.Log("Mapping beats...");
        float songLength = musicPlayer.audioSource.clip.length;
        for (float t = 0f; t < songLength; t += secondsPerBeat)
        {
            // store beat time in milliseconds, to avoid float precision issues
            // (eg. for t=1.59s, store key=1590)
            int key = Mathf.RoundToInt(t * 1000);
            beatMap[key] = false;
        }

        Debug.Log($"Mapped {beatMap.Count} beats for song length {songLength}s at {bpm} BPM.");
    }
    bool BeatExistsAndNotHitYet(int ts, int window)
    {
        int closestBeat = -1;
        int minDiff = int.MaxValue;

        foreach (var beatTime in beatMap.Keys)
        {
            int diff = Mathf.Abs(beatTime - ts);
            if (diff < minDiff)
            {
                minDiff = diff;
                closestBeat = beatTime;
            }
        }

        if (closestBeat != -1 && minDiff <= window && !beatMap[closestBeat])
        {
            beatMap[closestBeat] = true;
            return true;
        }
        return false;
    }


    public bool IsBeatHit(float timestamp)
    {
        int normalizedTimestamp = Mathf.RoundToInt(timestamp * 1000);

        // Offset positions: perfect to the left, good to the right (both above)
        Vector3 perfectSpawn = playerTransform != null
            ? playerTransform.position + new Vector3(-1f, 2f, 0f)
            : new Vector3(-1f, 2f, 0f);

        Vector3 goodSpawn = playerTransform != null
            ? playerTransform.position + new Vector3(1f, 2f, 0f)
            : new Vector3(1f, 2f, 0f);

        if (BeatExistsAndNotHitYet(normalizedTimestamp, PERFECT_WINDOW_MS))
        {
            Debug.Log("Perfect Beat Hit!");
            if (perfectHitPrefab != null)
            {
                // This spawns the effect on top of the player for better visibility
                GameObject obj = Instantiate(perfectHitPrefab, perfectSpawn, Quaternion.identity);
                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, 0f);
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = 10; // Higher than player
                Destroy(obj, 2f);
            }
            return true;
        }
        if (BeatExistsAndNotHitYet(normalizedTimestamp, GOOD_WINDOW_MS))
        {
            Debug.Log("Good Beat Hit!");
            if (goodHitPrefab != null)
            {
                GameObject obj = Instantiate(goodHitPrefab, goodSpawn, Quaternion.identity);
                Destroy(obj, 2f);
            }
            return true;
        }
        Debug.Log("Missed Beat.");
        return false; // miss
    }

}