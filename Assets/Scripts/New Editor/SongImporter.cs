using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class SongImporter
{
    public static string SongsDir => Path.Combine(Application.persistentDataPath, "Songs");

    public static bool Validate(string sourcePath, out string error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            error = "File does not exist.";
            return false;
        }

        string ext = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (ext != ".mp3" && ext != ".wav" && ext != ".ogg")
        {
            error = "Unsupported format. Use mp3, wav, or ogg.";
            return false;
        }

        return true;
    }

    public static IEnumerator ImportUserAudioCoroutine(
    string sourcePath,
    Action<float> onProgress,
    Action<string, string> onSuccess,
    Action<string> onError)
    {
        if (!Validate(sourcePath, out string err))
        {
            onError?.Invoke(err);
            yield break;
        }

        Directory.CreateDirectory(SongsDir);

        string ext = Path.GetExtension(sourcePath).ToLowerInvariant();
        string baseName = Path.GetFileNameWithoutExtension(sourcePath);
        string displayName = baseName;

        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{Sanitize(baseName)}_{stamp}{ext}";

        string destAbs = Path.Combine(SongsDir, fileName);
        string relative = Path.Combine("Songs", fileName).Replace("\\", "/");

        const int bufferSize = 1024 * 1024 * 4; 
        byte[] buffer = new byte[bufferSize];

        FileStream src = null;
        FileStream dst = null;

        long total = 0;
        long copied = 0;

        try
        {
            src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            dst = new FileStream(destAbs, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            total = src.Length;
        }
        catch (Exception e)
        {
            try { src?.Dispose(); } catch { }
            try { dst?.Dispose(); } catch { }
            try { if (File.Exists(destAbs)) File.Delete(destAbs); } catch { }

            onError?.Invoke(e.Message);
            yield break;
        }

        try
        {
            while (true)
            {
                int read = src.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;

                dst.Write(buffer, 0, read);
                copied += read;

                onProgress?.Invoke(total > 0 ? (float)copied / total : 0f);

                yield return null; 
            }

            dst.Flush(true);
        }
        finally
        {
            try { src?.Dispose(); } catch { }
            try { dst?.Dispose(); } catch { }
        }

        if (!File.Exists(destAbs) || (new FileInfo(destAbs).Length == 0))
        {
            try { if (File.Exists(destAbs)) File.Delete(destAbs); } catch { }
            onError?.Invoke("Copy failed.");
            yield break;
        }
        onProgress?.Invoke(1f);
        onSuccess?.Invoke(relative, displayName);
    }

    private static string Sanitize(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }
}