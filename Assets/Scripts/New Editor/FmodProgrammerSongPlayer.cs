using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;

public class FmodProgrammerSongPlayer : MonoBehaviour
{
    [Header("FMOD Template Event (Programmer Instrument)")]
    [SerializeField] private EventReference programmerSongEvent;

    private EventInstance instance;
    private string currentFilePath;

    private GCHandle userDataHandle;
    private static EVENT_CALLBACK callback;

    private int lengthMsCached;

    private static bool s_quitting;

    private void OnApplicationQuit() => s_quitting = true;
    
    public bool ReadyForSeek { get; private set; }

    private class UserData
    {
        public string filePath;
        public FMOD.Sound sound;
        public bool soundCreated;
        public bool programmerSoundDestroyed;
    }

    public bool IsReady => instance.isValid(); 
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        callback ??= Callback;
    }

    public bool LoadUserFile(string filePath)
    {
        StopImmediate();

        ReadyForSeek = false;
        lengthMsCached = 0;

        if (programmerSongEvent.IsNull)
        {
            UnityEngine.Debug.LogError("[FmodProgrammerSongPlayer] programmerSongEvent not assigned.");
            return false;
        }

        currentFilePath = filePath;

        
        var ud = new UserData { filePath = currentFilePath };

        
        var coreResult = RuntimeManager.CoreSystem.createSound(
            ud.filePath,
            MODE.DEFAULT | MODE.CREATESTREAM | MODE.NONBLOCKING,
            out ud.sound);

        if (coreResult != RESULT.OK)
        {
            UnityEngine.Debug.LogError($"[FmodProgrammerSongPlayer] createSound failed: {coreResult}");
            return false;
        }

        ud.soundCreated = true;
        ud.programmerSoundDestroyed = false;

        
        instance = RuntimeManager.CreateInstance(programmerSongEvent);
        if (!instance.isValid())
        {
            UnityEngine.Debug.LogError("[FmodProgrammerSongPlayer] Failed to create instance.");
            
            ud.sound.release();
            ud.soundCreated = false;
            return false;
        }

        
        userDataHandle = GCHandle.Alloc(ud, GCHandleType.Normal);
        IntPtr udPtr = GCHandle.ToIntPtr(userDataHandle);
        instance.setUserData(udPtr);

        instance.setCallback(callback,
            EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.STOPPED);

        
        instance.start();
        instance.setPaused(true);
        IsPlaying = false;

        
        CoroutineRunner.I.StartCoroutine(WaitUntilReady());

        return true;
    }

    private IEnumerator WaitUntilReady()
    {
        if (s_quitting) yield break;
        instance.getUserData(out IntPtr udPtr);
        if (udPtr == IntPtr.Zero) yield break;

        var handle = GCHandle.FromIntPtr(udPtr);
        var ud = handle.Target as UserData;
        if (ud == null) yield break;

        const int maxFrames = 240; 
        for (int i = 0; i < maxFrames; i++)
        {
            if (!RuntimeManager.StudioSystem.isValid()) yield break;
            RuntimeManager.StudioSystem.update();

            if (ud.soundCreated)
            {
                ud.sound.getOpenState(out OPENSTATE state, out _, out _, out _);

                if (state == OPENSTATE.READY)
                {
                    
                    ud.sound.getLength(out uint lenMs, TIMEUNIT.MS);
                    lengthMsCached = (int)lenMs;

                    ReadyForSeek = true;
                    yield break;
                }
            }

            yield return null;
        }

        
        
        ReadyForSeek = true;
    }

    public void Play()
    {
        if (!instance.isValid()) return;
        if (!ReadyForSeek) return; 
        instance.setPaused(false);
        IsPlaying = true;
    }

    public void Pause()
    {
        if (!instance.isValid()) return;
        instance.setPaused(true);
        IsPlaying = false;
    }

    public void Stop()
    {
        if (!instance.isValid()) return;
        instance.setPaused(true);
        instance.setTimelinePosition(0);
        IsPlaying = false;
    }

    public void SeekSeconds(float seconds)
    {
        if (!instance.isValid()) return;
        if (!ReadyForSeek) return;

        int ms = Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f));
        bool wasPlaying = IsPlaying;

        instance.setPaused(true);
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        instance.start();
        instance.setPaused(true);
        instance.setTimelinePosition(ms);

        instance.setPaused(!wasPlaying);
        IsPlaying = wasPlaying;
    }

    public float GetTimeSeconds()
    {
        if (!instance.isValid()) return 0f;
        instance.getTimelinePosition(out int ms);
        return ms / 1000f;
    }

    public float GetLengthSeconds()
    {
        return (lengthMsCached > 0) ? (lengthMsCached / 1000f) : 0f;
    }

    public void StopImmediate()
    {
        ReadyForSeek = false;
        lengthMsCached = 0;
        IsPlaying = false;

        if (s_quitting)
        {
            HardReleaseNow();
            return;
        }

        if (!instance.isValid())
        {
            CleanupUserDataHandle();
            return;
        }

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        CoroutineRunner.I.StartCoroutine(DeferredRelease());
    }

    private IEnumerator DeferredRelease()
    {
        if (s_quitting) yield break;
        const int maxFrames = 12;
        for (int i = 0; i < maxFrames; i++)
        {
            if (!RuntimeManager.StudioSystem.isValid()) yield break;
            RuntimeManager.StudioSystem.update();
            yield return null;
        }

        if (instance.isValid())
        {
            instance.release();
            instance = default;
        }

        CleanupUserDataHandle();
    }

    private void HardReleaseNow()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
            instance = default;
        }

        if (userDataHandle.IsAllocated)
        {
            try
            {
                var ud = userDataHandle.Target as UserData;
                if (ud != null && ud.soundCreated)
                {
                    ud.sound.release();
                    ud.soundCreated = false;
                    ud.sound = default;
                }
            }
            catch { }

            userDataHandle.Free();
        }
    }

    private void CleanupUserDataHandle()
    {
        if (userDataHandle.IsAllocated)
        {
            
            try
            {
                var ud = userDataHandle.Target as UserData;
                if (ud != null && ud.soundCreated)
                {
                    ud.sound.release();
                    ud.soundCreated = false;
                    ud.sound = default;
                }
            }
            catch { }

            userDataHandle.Free();
        }
    }

    private void OnDestroy()
    {
        StopImmediate();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static FMOD.RESULT Callback(EVENT_CALLBACK_TYPE type, IntPtr eventInstancePtr, IntPtr parameterPtr)
    {
        var inst = new EventInstance(eventInstancePtr);

        inst.getUserData(out IntPtr udPtr);
        if (udPtr == IntPtr.Zero) return FMOD.RESULT.OK;

        var handle = GCHandle.FromIntPtr(udPtr);
        if (!(handle.Target is UserData ud)) return FMOD.RESULT.OK;

        switch (type)
        {
            case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
            {
                ud.programmerSoundDestroyed = false;

                var props = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(
                    parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));

                
                props.sound = ud.sound.handle;
                props.subsoundIndex = -1;
                Marshal.StructureToPtr(props, parameterPtr, false);
                break;
            }

            case EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
            {
                if (ud.soundCreated)
                {
                    ud.sound.release();
                    ud.soundCreated = false;
                    ud.sound = default;
                }

                ud.programmerSoundDestroyed = true;
                break;
            }

            case EVENT_CALLBACK_TYPE.STOPPED:
                break;
        }

        return FMOD.RESULT.OK;
    }
}