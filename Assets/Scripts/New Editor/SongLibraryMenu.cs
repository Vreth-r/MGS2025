using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SongLibraryMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform listParent;
    [SerializeField] private SongListItem itemPrefab;

    [SerializeField] private Button addUserFileButton;
    [SerializeField] private Button removeButton;

    [SerializeField] private GameObject importProgressRoot;
    [SerializeField] private Slider importProgressBar;
    [SerializeField] private TMP_Text importProgressLabel;

    [Header("Optional: add built-in event")]
    [SerializeField] private TMP_InputField addEventPathInput;
    [SerializeField] private Button addEventButton;

    [Header("Integration")]
    [SerializeField] private EditorSongTransport editorTransport;
    [SerializeField] private BeatmapEditorController editorController; 

    private readonly List<SongListItem> spawned = new();
    private string selectedId;

    private void Start()
    {
        SongLibraryManager.I.OnChanged += Rebuild;
        Rebuild();

        addUserFileButton.onClick.AddListener(AddUserFile);

        removeButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(selectedId))
                SongLibraryManager.I.Remove(selectedId);
        });

        if (addEventButton != null)
        {
            addEventButton.onClick.AddListener(() =>
            {
                var path = addEventPathInput != null ? addEventPathInput.text : "";
                SongLibraryManager.I.AddBuiltInEvent(path);
            });
        }
    }

    private void OnDestroy()
    {
        if (SongLibraryManager.I != null)
            SongLibraryManager.I.OnChanged -= Rebuild;
    }

    private void Rebuild()
    {
        foreach (var it in spawned) if (it != null) Destroy(it.gameObject);
        spawned.Clear();

        var data = SongLibraryManager.I.Data;
        foreach (var s in data.songs)
        {
            var it = Instantiate(itemPrefab, listParent);
            it.Set(s.displayName, s.sourceType == SongSourceType.FmodEvent ? "Event" : "File");
            it.SetSelected(s.id == selectedId);

            it.Button.onClick.AddListener(() => SelectAndLoad(s.id));
            spawned.Add(it);
        }
    }

    private void SelectAndLoad(string id)
    {
        selectedId = id;
        SongLibraryManager.I.Select(id);

        foreach (var it in spawned)
            it.SetSelected(it != null && it.Id == selectedId);

        var entry = SongLibraryManager.I.Selected;
        if (entry == null) return;

        if (editorTransport != null && editorTransport.LoadSong(entry))
        {   
            editorTransport.Stop();
            if (editorController != null)
                editorController.OnSongLoadedFromLibrary(); 
        }
    }

    private void AddUserFile()
    {
    #if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Import Song", "", "mp3,wav,ogg");
        if (string.IsNullOrEmpty(path)) return;

        StartCoroutine(ImportSongRoutine(path));
    #else
        Debug.LogWarning("Runtime file picker not implemented. Use a plugin like StandaloneFileBrowser.");
    #endif
    }

    private IEnumerator ImportSongRoutine(string path)
    {
        if (importProgressRoot != null) importProgressRoot.SetActive(true);
        if (importProgressBar != null) importProgressBar.value = 0f;
        if (importProgressLabel != null) importProgressLabel.text = "Copying...";

        StartCoroutine(SongImporter.ImportUserAudioCoroutine(
            path,
            onProgress: p =>
            {
                if (importProgressBar != null) importProgressBar.value = p;
                if (importProgressLabel != null) importProgressLabel.text = $"Copying... {(p * 100f):0}%";
            },
            onSuccess: (rel, name) =>
            {
                SongLibraryManager.I.AddImportedUserSong(rel, name);
                if (importProgressLabel != null) importProgressLabel.text = "Done!";
            },
            onError: err =>
            {
                Debug.LogError($"[SongLibrary] Import failed: {err}");
                if (importProgressLabel != null) importProgressLabel.text = $"Error: {err}";
            }
        ));

        // Hide after a short moment
        yield return new WaitForSeconds(0.2f);
        if (importProgressRoot != null) importProgressRoot.SetActive(false);
    }
}