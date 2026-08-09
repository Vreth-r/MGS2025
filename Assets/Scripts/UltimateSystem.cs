using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UltimateSystem : MonoBehaviour
{
    private GameManager manager;

    //ultimate settings
    [Header("Ultimate Settings")]
    public int ultimateDivider = 3; //the divider that dictates the fraction of the total notes that gives you an ult (in this case, the divider being 3 means that a third of the notes are needed to activate the ult)
    public float ultimateDuration = 10f; //how long the ultimate lasts for
    //public bool autoActivateUltimate = false;

    [Header("Ult System Gain Multipliers Settings")]
    public float perfectGainMultiplier = 3f;
    public float awesomeGainMultiplier = 2f;
    public float goodGainMultiplier = 1f;
    public float okayGainMultiplier = 0.5f;

    [Header("Ultimate Visuals")]
    public Image ultimateBar; //the visual bar image

    public GameObject ultimateReadyBorder;

    private int totalNoteCount; //grabs the total amount of notes that are going to be spawned in the level
    private float ultimateNoteCountProgress = 0f; //keeps track of how many notes have been successfully hit and counted towards building up the ult

    private int ultimateNoteCountThreshold; //the exact number of notes needed to fully fill the ult bar

    private int usedUltimateCount; //tracks the number of ults used (so we can disable it completely after the max number of uses)
    private bool isUltReady = false;

    //events (calls other components/functions that are subscribed to these events)
    public event Action OnUltimateStarted;
    public event Action OnUltimateFinished;

    //getter+setters
    public static UltimateSystem Instance { get; private set; }
    public bool UltimateActive { get; private set; }

    //visuals
    public GameObject fgvisual;
    public GameObject bgvisual;

    private CombatSoundPlayer combatSoundPlayer;

    private GameSettings gameSettings;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //attach the preliminary data that is available + some math to figure some numbers out
        manager = GameManager.Instance;
        totalNoteCount = manager.noteCount;
        /*
        ultimateBar = inspectorUltimateBar;
        ultimateDivider = inspectorUltimateDivider;
        ultimateDuration = inspectorUltimateDuration;
        */

        ultimateNoteCountThreshold = totalNoteCount / ultimateDivider;

        ultimateBar.fillAmount = 0;

        combatSoundPlayer = GetComponent<CombatSoundPlayer>();

        gameSettings = Resources.Load<GameSettings>("GameSettings");
    }

    //increments the ult bar and logic numbers
    public void IncrementUltimate(Judgement judgement)
    {
        float multiplier = judgement switch
        {
            Judgement.Perfect => perfectGainMultiplier,
            Judgement.Awesome => awesomeGainMultiplier,
            Judgement.Good => goodGainMultiplier,
            Judgement.Ok => okayGainMultiplier,
            _ => 0f
        };

        Debug.Log("current judge - " + judgement);

        if (!UltimateActive) //stop incrementing ult bar if ult bar is already currently being used
        {
            ultimateNoteCountProgress = ultimateNoteCountProgress + (1 * multiplier); //add to the ult count progress

            //updates the visual ult bar to show progress, and if more notes are hit and the ult is already ready, just cap it out
            ultimateBar.fillAmount = Mathf.Clamp01(ultimateNoteCountProgress / (float)ultimateNoteCountThreshold);  

            if (ultimateNoteCountProgress >= ultimateNoteCountThreshold)
            {
                ultimateNoteCountProgress = ultimateNoteCountThreshold; //prevents overflow for note count progress

                if (isUltReady == false)
                {
                    combatSoundPlayer.PlaySoundEffect("UltCharged_1");
                    combatSoundPlayer.PlayLoopingSoundEffect("UltChargedWaiting_1");

                    isUltReady = true;
                }

                if (ultimateReadyBorder != null)
                {
                    ultimateReadyBorder.SetActive(true);

                    if (gameSettings.autoUltimate == true)
                    {
                        ActivateUltimate();
                    }
                }
            }
        }
    }

    //function that activates the ultimate
    public void ActivateUltimate()
    {
        if (UltimateActive == true)
        {
            return;
        }

        isUltReady = false;

        if (ultimateNoteCountProgress >= ultimateNoteCountThreshold) //note count matches the needed threshold for ultimate
        {
            combatSoundPlayer.StopLoopingSoundEffect("UltChargedWaiting_1");

            if (ultimateReadyBorder != null)
            {
                ultimateReadyBorder.SetActive(false);
            }
            if (fgvisual != null && bgvisual != null)
            {
                //fgvisual.SetActive(true);
                Color bgColor = bgvisual.GetComponent<SpriteRenderer>().color; //for when the ult happens, visuals pop in
                bgvisual.GetComponent<SpriteRenderer>().color = new Color(bgColor.r, bgColor.g, bgColor.b, 30f/255f);

                Color fgColor = fgvisual.GetComponent<Image>().color;
                fgvisual.GetComponent<Image>().color = new Color(fgColor.r, fgColor.g, fgColor.b, 1f);
            }

            ultimateNoteCountProgress = 0; //reset progress

            combatSoundPlayer.PlaySoundEffect("UltActivate_1");
            manager.StartFade("UltLayer", 1f, 0.5f);

            OnUltimateStarted?.Invoke();  //calls the functions that are subscribed to this event
            UltimateActive = true; //makes boolean true

            Instance.StartCoroutine(UltimateScoreModifier()); //calls the ult score modifier for big funny score multiplier
        }
    }

    //for the ult score multiplier and the visual ult bar decreasing over time
    private IEnumerator UltimateScoreModifier()
    {
        float elapsedTime = 0f; //accumulated time during the active ult

        float fillAmount = ultimateBar.fillAmount;

        //float regenTimer = 0f;

        //goes for the duration of the ultimate
        while (elapsedTime < ultimateDuration)
        {
            if (Health.IsDead())
            {
                break;
            }

            elapsedTime = elapsedTime + Time.deltaTime;

            //if you want literal 3 health per second
            /*
            regenTimer = regenTimer + Time.deltaTime;

            if (regenTimer >= 1f)
            {
                Health.Regen(3f);
                regenTimer = regenTimer - 1f;
                Debug.Log("ult health regen");
            }
            */

            Health.UltHealthRegen(3f * Time.deltaTime); //ult regens health while it runs

            float a = elapsedTime / ultimateDuration;
            ultimateBar.fillAmount = Mathf.Lerp(fillAmount, 0f, a); //lerps until the bar becomes empty (basically more elapsed time, means lerp goes to a smaller and smaller bar amount)

            yield return null;
        }

        ultimateBar.fillAmount = 0f; //just incase the bar somehow goes a bit over/under, force it to be exact 0

        OnUltimateFinished?.Invoke(); //calls all functions that listen to OnUltimateFinished
        UltimateActive = false;

        manager.StartFade("UltLayer", 0f, 0.5f);
        StartCoroutine(FadeOutUltVisuals()); //for when the ult happens, visuals fade away
    }

    //calls ultimate
    private void UseUltimate()
    {
        ActivateUltimate();
    }

    //subscribes the function "UseUltimate", to be called whenever "OnUltimatePressed" is called
    private void OnEnable()
    {
        //call "UseUltimate" when ultimate is pressed
        InputManager.Instance.OnUltimatePressed += UseUltimate;
    }

    //unsubscribes the function "UseUltimate" to no longer be called when "OnUltimatePressed" is called
    private void OnDisable()
    {
        //stops "UseUltimate" from being subscribed
        InputManager.Instance.OnUltimatePressed -= UseUltimate;
    }

    public Color ChangeSaturation(Color c, float newS)
    {
        Color.RGBToHSV(c, out float h, out _, out float v);
        return Color.HSVToRGB(h, newS, v);
    }

    private IEnumerator FadeOutUltVisuals() //for when the ult happens, visuals fade away
    {
        Color bgColor = bgvisual.GetComponent<SpriteRenderer>().color;
        Color fgColor = fgvisual.GetComponent<Image>().color;

        float elapsedTime = 0f; //accumulated time during the fadeout

        while (elapsedTime < 0.5f)
        {
            elapsedTime = elapsedTime + Time.deltaTime;

            float a = elapsedTime / 0.5f;
            float bgAlpha = Mathf.Lerp(bgColor.a, 0f, a); //lerps until alpha goes to 0
            float fgAlpha = Mathf.Lerp(fgColor.a, 0f, a); //lerps until alpha goes to 0

            bgvisual.GetComponent<SpriteRenderer>().color = new Color(bgColor.r, bgColor.g, bgColor.b, bgAlpha);
            fgvisual.GetComponent<Image>().color = new Color(fgColor.r, fgColor.g, fgColor.b, fgAlpha);

            yield return null;
        }

        bgvisual.GetComponent<SpriteRenderer>().color = new Color(bgColor.r, bgColor.g, bgColor.b, 0f);
        fgvisual.GetComponent<Image>().color = new Color(fgColor.r, fgColor.g, fgColor.b, 0f);
    }
}