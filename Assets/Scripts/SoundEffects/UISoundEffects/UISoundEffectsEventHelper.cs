using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;

public class UISoundEffectsEventHelper : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISelectHandler
{
    public static bool surpressFirstHoverSound = false;

    [System.Serializable]
    public struct SoundEffect
    {
        public UISoundEffectsType type;
        public EventReference eventReference;
    }

    public UISoundEffectsPlayer soundEffectsPlayer;
    public List<SoundEffect> sounds = new List<SoundEffect>();

    public void PlaySound(UISoundEffectsType soundEffectsType)
    {
        for (int i = 0; i < sounds.Count; i = i + 1)
        {
            if (sounds[i].type == soundEffectsType && !sounds[i].eventReference.IsNull)
            {
                soundEffectsPlayer.PlaySoundEvent(sounds[i].eventReference);

            }
        }
    }

    private void OnEnable()
    {
        PlaySound(UISoundEffectsType.Load);
        //Debug.Log("Played On Load");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(UISoundEffectsType.Hover);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (surpressFirstHoverSound == true)
        {
            surpressFirstHoverSound = false;
            return;
        }

        PlaySound(UISoundEffectsType.Hover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(UISoundEffectsType.Click);
    }

    public void PlayOnSubmit()
    {
        PlaySound(UISoundEffectsType.Click);
    }
}
