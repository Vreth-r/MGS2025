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
    public bool autoActivateUltimate = false;

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

    //events (calls other components/functions that are subscribed to these events)
    public event Action OnUltimateStarted;
    public event Action OnUltimateFinished;

    //getter+setters
    public static UltimateSystem Instance { get; private set; }
    public bool UltimateActive { get; private set; }

    //visuals
    public GameObject fgvisual;
    public GameObject bgvisual;

    private SoundEffectsPlayer soundEffectsPlayer;
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

        soundEffectsPlayer = GetComponent<SoundEffectsPlayer>();
    }

    //increments the ult bar and logic numbers
    public void IncrementUltimate(Judgement judgement)
    {
        float multiplier = judgement switch
        {
            Judgement.Perfect => perfectGainMultiplier,
            Judgement.Awesome => awesomeGainMultiplier,
            Judgement.Good => goodGainMultiplier,
            Judgement.Okay => okayGainMultiplier,
            _ => 0f
        };

        Debug.Log("current judge - " + judgement);

        if (!UltimateActive) //stop incrementing ult bar if ult bar is already currently being used
        {
            ultimateNoteCountProgress = ultimateNoteCountProgress + (1 * multiplier); //add to the ult count progress

            if (ultimateNoteCountProgress > ultimateNoteCountThreshold) //if more notes are hit and the ult is already ready, just cap it out
            {
                ultimateNoteCountProgress = ultimateNoteCountThreshold;
            }

            ultimateBar.fillAmount = ultimateNoteCountProgress / (float)ultimateNoteCountThreshold; //updates the visual ult bar to show progress 

            if (ultimateNoteCountProgress >= ultimateNoteCountThreshold)
            {
                soundEffectsPlayer.PlaySoundEffect("UltCharged_1");

                soundEffectsPlayer.PlayLoopingSoundEffect("UltChargedWaiting_1");

                if (ultimateReadyBorder != null)
                {
                    ultimateReadyBorder.SetActive(true);

                    if (autoActivateUltimate == true)
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
        if (ultimateNoteCountProgress >= ultimateNoteCountThreshold) //note count matches the needed threshold for ultimate
        {
            soundEffectsPlayer.StopLoopingSoundEffect("UltChargedWaiting_1");

            if (ultimateReadyBorder != null)
            {
                ultimateReadyBorder.SetActive(false);
            }
            if (fgvisual != null && bgvisual != null)
            {
                fgvisual.SetActive(true);
                bgvisual.SetActive(true);
            }

            ultimateNoteCountProgress = 0; //reset progress

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

        //goes for the duration of the ultimate
        while (elapsedTime < ultimateDuration)
        {
            elapsedTime = elapsedTime + Time.deltaTime;

            float a = elapsedTime / ultimateDuration;
            ultimateBar.fillAmount = Mathf.Lerp(fillAmount, 0f, a); //lerps until the bar becomes empty (basically more elapsed time, means lerp goes to a smaller and smaller bar amount)

            yield return null;
        }

        ultimateBar.fillAmount = 0f; //just incase the bar somehow goes a bit over/under, force it to be exact 0

        OnUltimateFinished?.Invoke(); //calls all functions that listen to OnUltimateFinished
        UltimateActive = false;

        if (fgvisual != null && bgvisual != null)
        {
            fgvisual.SetActive(false);
            bgvisual.SetActive(false);
        }
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
}
