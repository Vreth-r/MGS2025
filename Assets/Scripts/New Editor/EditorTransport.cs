using UnityEngine;

public class EditorTransport : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
    public float SongLength => (audioSource != null && audioSource.clip != null) ? audioSource.clip.length : 0f;
    public float TimeSeconds => audioSource != null ? audioSource.time : 0f;

    public void SetClip(AudioClip clip)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.time = 0f;
    }

    public void Play()
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.Play();
    }

    public void Pause()
    {
        if (audioSource == null) return;
        audioSource.Pause();
    }

    public void Stop()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.time = 0f;
    }

    public void Seek(float seconds)
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.time = Mathf.Clamp(seconds, 0f, audioSource.clip.length);
    }
}