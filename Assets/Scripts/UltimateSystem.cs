using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UltimateSystem : MonoBehaviour
{
    private GameManager manager;

    [SerializeField] private Image inspectorUltimateBar;
    [SerializeField] private int inspectorUltimateDivider;
    [SerializeField] private float inspectorUltimateDuration;


    //ultimate settings
    public static Image ultimateBar; //the visual bar image
   
    private static int totalNoteCount; //grabs the total amount of notes that are going to be spawned in the level
    private static int ultimateNoteCountProgress = 0; //keeps track of how many notes have been successfully hit and counted towards building up the ult

    private static int ultimateNoteCountThreshold; //the exact number of notes needed to fully fill the ult bar
    public static int ultimateDivider = 3; //the divider that dictates the fraction of the total notes that gives you an ult (in this case, the divider being 3 means that a third of the notes are needed to activate the ult)
    private static int usedUltimateCount; //tracks the numebr of ults used (so we can disable it completely after the max number of uses)

    public static float ultimateDuration = 10f; //how long the ultimate lasts for

    //events (calls other components/functions that are subscribed to these events)
    public static event Action OnUltimateStarted;
    public static event Action OnUltimateFinished;

    //getter+setters
    public static UltimateSystem Instance { get; private set; }
    public static bool UltimateActive {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //attach the preliminary data that is available + some math to figure some numebrs out
        manager = GameManager.Instance;
        totalNoteCount = manager.noteCount;

        ultimateBar = inspectorUltimateBar;
        ultimateDivider = inspectorUltimateDivider;
        ultimateDuration = inspectorUltimateDuration;

        ultimateNoteCountThreshold = totalNoteCount / ultimateDivider;
    }

    //increments the ult bar and logic numbers
    public static void IncrementUltimate()
    {
        if (usedUltimateCount < ultimateDivider-1) //if the max number if ults is not used up yet
        {
            if (!UltimateActive) //stop incrementing ult bar if ult bar is already currently being used
            {
                if (ultimateNoteCountProgress + 1 > ultimateNoteCountThreshold) //if more notes are hit and the ult is already ready, just cap it out
                {
                    ultimateNoteCountProgress = ultimateNoteCountThreshold;
                }

                else
                {
                    ultimateNoteCountProgress = ultimateNoteCountProgress + 1; //add to the ult count progress
                }

                ultimateBar.fillAmount = (float)ultimateNoteCountProgress / (float)ultimateNoteCountThreshold; //updates the visual ult bar to show progress 
            } 
        }

        else
        {
            ultimateBar.enabled = false;
        }
    }

    //function that activates the ultimate
    public static void ActivateUltimate()
    {
        if (ultimateNoteCountProgress >= ultimateNoteCountThreshold) //note count matches the needed threshold for ultimate
        {
            ultimateNoteCountProgress = 0; //reset progress

            OnUltimateStarted?.Invoke();  //calls the functions that are subscribed to this event
            UltimateActive = true; //makes boolean true

            Instance.StartCoroutine(UltimateScoreModifier()); //calls the ult score modifier for big funny score multiplier
        }
    }

    //for the ult score multiplier and the visual ult bar decreasing over time
    private static IEnumerator UltimateScoreModifier()
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
