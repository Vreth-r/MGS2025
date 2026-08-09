using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;

public class UISoundPlayer : MonoBehaviour, IPointerEnterHandler, ISelectHandler //these fancy things are for UI events, aka when you load in, click, and hover (which we use for sound effects)
{
    [System.Serializable]
    public struct SoundEffect //uses the file UISoundType.cs for its enum UISoundType (which is where the list of possible sound types are)
    {
        public UISoundType type; //what sound type is used
        public EventReference eventReference; //what the actual FMOD sound reference is (aka the sound/music)
    }

    public List<SoundEffect> sounds = new List<SoundEffect>();

    //music specific stuff (because adding looping means it needs extra stuff)
    private EventInstance musicInstance; //the instance of the song to be played
    private bool isMusicPlaying = false; //checks if the song is playing

    //for when a button is autoselected off rip (like when loading in to a scene), makes it so the hover effect doesnt just play off rip becasue that is kinda whack
    public static bool surpressFirstHoverSound = false;

    private Button button; //check the awake function at the bottom for why this is used

    //function to actually play the sound/music
    public void PlaySound(UISoundType soundType, List<SoundEffect> soundList)
    {
        if (soundList == null)
        {
            return;
        }

        for (int i = 0; i < soundList.Count; i = i + 1) //goes through the list of sounds added in the inspector
        {
            if (soundList[i].type == soundType && !soundList[i].eventReference.IsNull) //if its a real sound type and a real sound reference (aka, the alloted FMOD sound path is legit)
            {
                if (soundType == UISoundType.Music) //music specific check so it can do its own fancy stuff
                {
                    musicInstance = RuntimeManager.CreateInstance(soundList[i].eventReference);
                    musicInstance.start(); //music needs it to be this so it can loop -> PlayOneShot cant loop
                    isMusicPlaying = true;
                }

                else
                {
                    RuntimeManager.PlayOneShot(soundList[i].eventReference); //plays sounds (non music)
                }
            }
        }
    }

    //function that handles the sounds that play when the object is enabled (in this case, when its loaded)
    private void OnEnable()
    {
        PlaySound(UISoundType.Load, sounds);
        PlaySound(UISoundType.Music, sounds);
    }

    //function to destroy specifically the music to stop it from playing and not eat up more resources
    private void OnDestroy()
    {
        if (isMusicPlaying == true)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
            isMusicPlaying = false;
        }
    }

    //mouse specific -> function that handles the sounds that play when the object has the pointer on it (in this case, when the button is hovered by mouse)
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(UISoundType.Hover, sounds);
    }

    //keyboard+controller specific -> function that handles the sounds that play when the object has been highlighted (in this case, when the button is hovered by keyboard/controller)
    public void OnSelect(BaseEventData eventData)
    {
        if (surpressFirstHoverSound == true)
        {
            surpressFirstHoverSound = false;
            return;
        }

        PlaySound(UISoundType.Hover, sounds);
    }

    //because unity sucks, and this project is set up like spaghetti, i cant actually make the click/sumbit sounds work for keyboard???
    //apparetly its because the OnSubmit or some submit event is not working when using the keyboard to submit the button? May be due to the navigation and its event with how it works and setup???
    //so instead, we add a listener to make it use the mouse way of playing the sound when it clicks it (sort of like we are faking a mouse click to make the sound play)
    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                PlaySound(UISoundType.Click, sounds);
            });
        }
    }
}