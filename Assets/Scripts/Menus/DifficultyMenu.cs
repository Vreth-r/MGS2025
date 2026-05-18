using System;
using System.Collections;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

/// Programmed by: LogChiCha
///
/// SUMMARY
/// The difficulty menu, allows the player to adjust recovery and damage by difficulty level
/// 

public class DifficultyMenu : BaseMenu
{
    [Header("Buttons")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Sprites")]
    public Sprite sprEasy;
    public Sprite sprNormal;
    public Sprite sprHard;
    public Sprite sprNightmare;
    private Sprite[] sprites;
    private int diffVal = 1;

    [Header("Display Assets")]
    public GameObject display;
    public TextMeshProUGUI textbox;


    private int selectedIndex = 2;
    private bool onNext = true; //just remembers if you where on back or next when going to Return (feels better)
    private Button[] buttons;
    private String[] text;
    private int[] sizes;

    [Header("Panel Reference")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private float maxPanelScale = 1f;
    [Header("Animation Controls")]
    [SerializeField] float scaleTime = 1f;
    [SerializeField] AnimationCurve scaleCurve;
    private bool grow = true;
    private Coroutine scaleCoroutine;

    private static GameSettings gameSettings;

    protected override void Awake()
    {
        base.Awake();

        buttons = new[] { backButton, nextButton, returnButton };
        sprites = new[] { sprEasy, sprNormal, sprHard, sprNightmare };
        text = new[] { "we all start somewhere,\nit's the only way anything happens", //easy
            "keep your sanity\nwith a little challenge", //normal
            "oh, the rhythms are kicking in", //hard
            "inadvisable for all" }; //nightmare
        sizes = new[] { 40, 44, 46, 50 };

        //default settings
        var img = display.GetComponent<Image>();
        img.sprite = sprites[diffVal];
        textbox.text = text[diffVal];
        textbox.fontSize = sizes[diffVal];

        gameSettings = Resources.Load<GameSettings>("GameSettings");

        returnButton.onClick.AddListener(ReturnBack);
        nextButton.onClick.AddListener(nextDiff);
        backButton.onClick.AddListener(backDiff);
    }

    protected override void OnOpen()
    {
        Debug.LogFormat($"Difficulty menu opened");
        HighlightElement(selectedIndex);
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
            float scale = scaleCurve.Evaluate(time / scaleTime);
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

        panel.localScale = Vector3.zero;
        Debug.LogFormat($"Difficulty menu closed");
    }

    //horiz will move between back and next, and vertical will go to select, and it remembers your last placement
    public override void HandleNavigate(Vector2 direction)
    {
        if (direction.y > 0.5f || direction.y < -0.5f)
        {
            if (selectedIndex != 2)
                selectedIndex = 2;
            else if (onNext)
                selectedIndex = 1;
            else
                selectedIndex = 0;
        }
        if (direction.x > 0.5f)
        {
            selectedIndex = 1;
            onNext = true;
        }
        else if (direction.x < -0.5f)
        {
            selectedIndex = 0;
            onNext = false;
        }

        HighlightElement(selectedIndex);

    }

    public override void HandleSubmit()
    {
        /*
        if (buttons[selectedIndex] != null) 
            buttons[selectedIndex].onClick.Invoke();
        */

        var button = buttons[selectedIndex];

        var soundHelper = button.GetComponent<UISoundEffectsEventHelper>();
        if (soundHelper != null)
        {
            soundHelper.PlayOnSubmit();
        }

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

        if (buttons[index] != null)
        {
            var sound = buttons[index]?.GetComponent<UISoundEffectsEventHelper>();
            sound.PlaySound(UISoundEffectsType.Hover);
        }

    }

    // *****************************
    // Difficulty Specific Functions
    // **************************
    private void nextDiff()
    {

        diffVal = (diffVal + 1) % (sprites.Length); // +1, or loop if bottom if max
        var img = display.GetComponent<Image>();
        img.sprite = sprites[diffVal];
        textbox.text = text[diffVal];
        textbox.fontSize = sizes[diffVal];

    }

    private void backDiff()
    {
        diffVal = (diffVal - 1 + (sprites.Length)) % (sprites.Length); //-1, or loop to top if 0
        var img = display.GetComponent<Image>();
        img.sprite = sprites[diffVal];
        textbox.text = text[diffVal];
        textbox.fontSize = sizes[diffVal];
    }
  
    //////////////////////////////////

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
        gameSettings.gameMode = diffVal; //sets difficulty value on menu close
       // Debug.Log("closing with diff "+diffVal);
        MenuManager.Instance.CloseMenu();
    }
}
