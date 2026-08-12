using System;
using System.Collections;
using Event;
using UnityEngine;
using UnityEngine.UI;



/*
 * [REFACTORING] 
 *  Refactor this to use a more modular system
 *  recommend using a generic ability system in case we decide to add other things
 *  And, this should probably be more strongly coupled to the players
 *  -TJ
 * 
 */

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
    

    //getter+setters
    public static UltimateSystem Instance { get; private set; }
    public bool UltimateActive { get; private set; }

    //visuals
    public GameObject fgvisual;
    public GameObject bgvisual;
    //ultimate foreground and background ui (I think)
    
    
    private CombatSoundPlayer _combatSoundPlayer;
    private GameSettings _gameSettings;

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
        _combatSoundPlayer = GetComponent<CombatSoundPlayer>();
        _gameSettings = Resources.Load<GameSettings>("GameSettings");
        
        if (ultimateReadyBorder == null)
            throw new InvalidOperationException("UltimateReadyBorder for the Ultimate System game object is null! set the object to a valid reference");
    }

    //increments the ult bar and logic numbers
    
    //[REFACTORING] this would be a good use case for events to call this function, especially if refactored into using a more modular ability system - TJ
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

        //Debug.Log("current judge - " + judgement);

        //stop incrementing ult bar if ult bar is already currently being used
        if (UltimateActive)
            return;

        ultimateNoteCountProgress += multiplier;
       // ultimateNoteCountProgress = ultimateNoteCountProgress + (1 * multiplier); //add to the ult count progress

        //updates the visual ult bar to show progress, and if more notes are hit and the ult is already ready, just cap it out
        ultimateBar.fillAmount = Mathf.Clamp01(ultimateNoteCountProgress / ultimateNoteCountThreshold);  //it's already a float the cast does nothing


        if (!(ultimateNoteCountProgress >= ultimateNoteCountThreshold)) 
            return;
        
        
        ultimateNoteCountProgress = ultimateNoteCountThreshold; //prevents overflow for note count progress

        if (!isUltReady)
        {
            _combatSoundPlayer.PlaySoundEffect("UltCharged_1");
            _combatSoundPlayer.PlayLoopingSoundEffect("UltChargedWaiting_1");
            isUltReady = true;
        }

        if (ultimateReadyBorder != null)
        {
            ultimateReadyBorder.SetActive(true);
            if (_gameSettings.autoUltimate)
                ActivateUltimate();
        }

    }

    //function that activates the ultimate
    public void ActivateUltimate()
    {
        if (UltimateActive)  //if the ultimate ability is currently active
            return;

        if (ultimateNoteCountProgress < ultimateNoteCountThreshold)  //if the required amount of notes is hit
            return;
        
        isUltReady = false;
        
        //todo replace this with a constant value instead of string literal
        _combatSoundPlayer.StopLoopingSoundEffect("UltChargedWaiting_1");
        ultimateReadyBorder.SetActive(false);
        
        
        if (fgvisual != null && bgvisual != null)
        {
            //fgvisual.SetActive(true);
            Color bgColor = bgvisual.GetComponent<SpriteRenderer>().color; //for when the ult happens, visuals pop in
            bgvisual.GetComponent<SpriteRenderer>().color = new Color(bgColor.r, bgColor.g, bgColor.b, 30f/255f);

            Color fgColor = fgvisual.GetComponent<Image>().color;
            fgvisual.GetComponent<Image>().color = new Color(fgColor.r, fgColor.g, fgColor.b, 1f);
        }

        ultimateNoteCountProgress = 0; //reset progress

        _combatSoundPlayer.PlaySoundEffect("UltActivate_1");
        manager.StartFade("UltLayer", 1f, 0.5f);

        GameplayEvents.UltimateStartEvent.CallEvent(ValueTuple.Create());
        
        
        UltimateActive = true; //makes boolean true
        Instance.StartCoroutine(UltimateScoreModifier()); //calls the ult score modifier for big funny score multiplier
        
    }
    
    //It would be probably a better idea to inline this into a ticking function instead of using an IEnumerator....
    //-TJ
    
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
        
        GameplayEvents.UltimateDepleteEvent.CallEvent(ValueTuple.Create());
        UltimateActive = false;

        manager.StartFade("UltLayer", 0f, 0.5f);
        StartCoroutine(FadeOutUltVisuals()); //for when the ult happens, visuals fade away
    }

    //calls ultimate
    //[REFACTORING] The private nature of this function is nullified by the public nature of ActivateUltimate(), rendering this function obsolete -TJ
    [Obsolete("Directly call ActivateUltimate() instead")]
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
            elapsedTime += Time.deltaTime;

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