using UnityEngine;
using System.Collections.Generic;

public class EditorManager : MonoBehaviour
{
    EditorManager Instance;

    // stuff from GM because it'll be annoying to reference GameManager.Instance.whatever every time, only for public fields tho
    // they should all be there since base is loaded first, but if it causes issues these can be removed.
    // not exactly sure if all of these will be utilized but I thought it was better to have them for now jic
    private Transform leftSpawnZone;

    public string beatmapFileName;
    public LanePrefabController[] lanes;
    public AudioSource audioSource;
    public Dictionary<string, GameObject> notePrefabs; 

    public GameObject tapNotePrefab;
    public GameObject holdNotePrefab;
    public GameObject deadNotePrefab;

    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        // remove if not used/needed
        if (GameManager.Instance != null)
        {
            leftSpawnZone = GameManager.Instance.hitZone;
            beatmapFileName = GameManager.Instance.beatmapFileName;
            lanes = GameManager.Instance.lanes;
            audioSource = GameManager.Instance.audioSource;
            notePrefabs = new Dictionary<string, GameObject>(GameManager.Instance.notePrefabs);
            tapNotePrefab  = GameManager.Instance.tapNotePrefab;
            holdNotePrefab = GameManager.Instance.holdNotePrefab;
            deadNotePrefab = GameManager.Instance.deadNotePrefab;
        }
    }


    public void PlayTrack() {}
    public void PauseTrack() {}
    public void SetTrackTime(float playbackPercent) {}
    // add functions here!
}
