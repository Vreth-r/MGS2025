using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class UISoundEffectsPlayer : MonoBehaviour
{
    public void PlaySoundEvent(EventReference eventReference)
    {
        if (!eventReference.IsNull)
        {
            RuntimeManager.PlayOneShot(eventReference);
        }
    }
}
