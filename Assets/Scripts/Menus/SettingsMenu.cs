using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// using UnityEngine.UIElements;

/// <summary>
/// The settings menu, allows the player to tweak the settings.
/// </summary>
public class SettingsMenu : BaseMenu
{
    [Header("Buttons")]
    [SerializeField] private Button returnButton;
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private int selectedIndex = 0;  
    private Button[] buttons;
    private Slider[] sliders;
    private float volumeIncrement = 0.05f;
    
    [Header("Panel Reference")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private float maxPanelScale = 1f;
    [Header("Animation Controls")]
    [SerializeField] float scaleTime = 1f;
    [SerializeField] AnimationCurve scaleCurve;
    private bool grow = true;
    private Coroutine scaleCoroutine;

    protected override void Awake()
    {
        base.Awake();

        buttons = new[] { null, null, returnButton };
        sliders = new[] { musicSlider, sfxSlider, null};

        musicSlider.value = SettingsManager.musicVolume;
        sfxSlider.value = SettingsManager.sfxVolume;

        returnButton.onClick.AddListener(ReturnBack);
    }

    protected override void OnOpen()
    {
        Debug.LogFormat($"Settings menu opened");
        HighlightElement(selectedIndex);
        musicSlider.value = SettingsManager.musicVolume;
        sfxSlider.value = SettingsManager.sfxVolume;
        grow = true;
        panel.localScale = new Vector3(maxPanelScale, maxPanelScale, maxPanelScale);
        //scaleCoroutine = StartCoroutine(scalePanel());
    }

    IEnumerator scalePanel()
    {
        float time = 0;
        panel.localScale = Vector3.zero;

        while (time < scaleTime)
        {
            float scale = scaleCurve.Evaluate(time/scaleTime);
            scale = grow ? scale : 1 - scale;
            panel.localScale = new Vector3(scale, scale, scale);
            time += Time.unscaledDeltaTime;
            yield return null;   
        }

        panel.localScale = new Vector3(maxPanelScale, maxPanelScale, maxPanelScale);

        if (!grow)
        {
            ReturnBack();
        }
        scaleCoroutine = null;
    }

    protected override void OnClose()
    {
        // Save the settings somewhere
        SettingsManager.SetMusicVolume(Mathf.Clamp01(musicSlider.value));
        SettingsManager.SetSFXVolume(Mathf.Clamp01(sfxSlider.value));

        panel.localScale = Vector3.zero;
        Debug.LogFormat($"Settings menu closed");
    }

    public override void HandleNavigate(Vector2 direction)
    {
        if (direction.y > 0.5f)
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            
            HighlightElement(selectedIndex);
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            HighlightElement(selectedIndex);
        }
        if (direction.x > 0.5f)
        {
            if (sliders[selectedIndex] != null)
            {
                sliders[selectedIndex].value += volumeIncrement;
                SettingsManager.SetMusicVolume(Mathf.Clamp01(musicSlider.value));
                SettingsManager.SetSFXVolume(Mathf.Clamp01(sfxSlider.value));
            }
        }
        else if (direction.x < -0.5f)
        {
            if (sliders[selectedIndex] != null)
            {
                sliders[selectedIndex].value -= volumeIncrement;
                SettingsManager.SetMusicVolume(Mathf.Clamp01(musicSlider.value));
                SettingsManager.SetSFXVolume(Mathf.Clamp01(sfxSlider.value));
            }
        }
        
    }
    
    public override void HandleSubmit()
    {
        /*
        if (buttons[selectedIndex] != null) 
            buttons[selectedIndex].onClick.Invoke();
        */

        var button = buttons[selectedIndex];
        /*
        var soundHelper = button.GetComponent<UISoundPlayer>();
        if (soundHelper != null)
        {
            soundHelper.PlayOnSubmit();
        }
        */
        button.onClick.Invoke();
    }

    public override void HandleCancel()
    {
        ReturnBack();
    }

    private void HighlightElement(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                var colors = buttons[i].colors;
                colors.normalColor = (i == index) ? Color.yellow : Color.white;
                buttons[i].colors = colors;   
            }
        }
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i] != null)
            {
                var colors = sliders[i].colors;
                colors.normalColor = (i == index) ? Color.yellow : Color.white;
                sliders[i].colors = colors;
            }
        }

        if (buttons[index] != null)
        {
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }

        else if (sliders[index] != null)
        {
            sliders[index].Select();
        }
    }

    private void shrinkPanel()
    {
        if (scaleCoroutine == null)
        {
            grow = false;
            scaleCoroutine = StartCoroutine(scalePanel());   
        }
    }

    private void ReturnBack()
    {
        Debug.Log("closing");
        MenuManager.Instance.CloseMenu();
    }
}