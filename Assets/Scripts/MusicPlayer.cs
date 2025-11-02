using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class MusicPlayer : MonoBehaviour
{
    public string musicDirectory = "Assets/Audio/Music"; // Set your music folder path here
    public AudioSource audioSource;
    [Range(0f, 1f)]
    public float musicVolume = 0.33f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.volume = musicVolume; // Set volume from public variable

        // Load all .wav or .mp3 files from the directory
        string[] files = Directory.GetFiles(musicDirectory, "*.mp3");
        if (files.Length == 0)
            files = Directory.GetFiles(musicDirectory, "*.wav");

        if (files.Length > 0)
        {
            string filePath = files[0]; // Play the first found file
            StartCoroutine(LoadAndPlay(filePath));
        }
        else
        {
            Debug.LogWarning("No music files found in directory: " + musicDirectory);
        }
    }

    System.Collections.IEnumerator LoadAndPlay(string path)
    {
        string url = "file:///" + Path.GetFullPath(path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audio: " + www.error);
            }
            else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.Play();
            }
        }
    }
}
