using System;
using UnityEngine;



//this doesn't seem to be used...

public class UIEvents 
{
    // Pause event, should only be called when game timescale is set to 0
    public event Action OnPause;
    public void PauseGame()
    {
        OnPause?.Invoke();
    }

    // Pause event, should only be called when game timescale is set back to 1
    public event Action OnResume;
    public void ResumeGame()
    {
        OnResume?.Invoke();
    }

    internal void ClearAll()
    {
        OnPause = null;
        OnResume = null;
    }
}
