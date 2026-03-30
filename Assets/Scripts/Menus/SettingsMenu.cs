using System;
using System.Collections;
using UnityEngine;
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

        buttons = new[] { returnButton };

        returnButton.onClick.AddListener(shrinkPanel);
    }

    protected override void OnOpen()
    {
        Debug.LogFormat($"Settings menu opened");
        HighlightButton(selectedIndex);
        grow = true;
        scaleCoroutine = StartCoroutine(scalePanel());
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
            time += Time.deltaTime;
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
        Debug.LogFormat($"Settings menu closed");
    }

    public override void HandleNavigate(Vector2 direction)
    {
        if (direction.y > 0.5f)
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            HighlightButton(selectedIndex);
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            HighlightButton(selectedIndex);
        }
    }
    
    public override void HandleSubmit()
    {
        buttons[selectedIndex].onClick.Invoke();
    }

    public override void HandleCancel()
    {
        ReturnBack();
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var colors = buttons[i].colors;
            colors.normalColor = (i == index) ? Color.yellow : Color.white;
            buttons[i].colors = colors;
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