/**
* Programmed by: Knox Fouladi
*
* SUMMARY
* This script acts as a visual handler for all achieved combo display needs
* 
* 
* TO DO:
* Animate other combos/miss
*/
using System;
using System.Collections;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class ComboVisualHandler : MonoBehaviour
{
    //[SerializeField]
    //private TMP_Text comboCountDisplay;
    //[SerializeField]
    //private Animator comboCounterAnimator; // Top Right Counter
    //[SerializeField]
    //private EventManager eventManager;
    [SerializeField]
    private TMP_Text comboCountDisplay;
    [SerializeField]
    private Animator comboCounterAnimator; // Top Right Counter
    [SerializeField]
    private PlayerComboEmitter p1ComboEmitter; // Player 1 combo emitter
    private Animator p1ComboEmitterAnimator; // Player 1 combo animator
    [SerializeField]
    private PlayerComboEmitter p2ComboEmitter; // Player 2 combo emitter
    private Animator p2ComboEmitterAnimator; // Player 2 combo animator
    [SerializeField]
    private PlayerComboEmitter pDuoComboEmitter; // Player 2 combo emitter
    private Animator pDuoComboEmitterAnimator; // Player 2 combo animator

    // testing stuff
    public int currentComboCount = 0;
    public int playerID = 0;
    //[field: SerializeField] public GameObject perfectCombo;
    //[field: SerializeField] public GameObject goodCombo;
    //[field: SerializeField] public GameObject missCombo;
    public float timeBeforeFade;
    IEnumerator[] currentPopUp =  new IEnumerator[3];


    // animation name Constants

    // counter (top right corner)
    const string COUNT_DISPLAY_MISS = "MissCOMBO";
    const string COUNT_DISPLAY_GOOD = "GoodCOMBO";
    const string COUNT_DISPLAY_PERFECT = "PerfectCOMBO";

    // Pop-ups (shown near player, uses object on player object)
    const string POP_UP_MISS = "POPUP_MISS";
    const string FADE = "FADE";
    const string POP_UP_OK = "POPUP_OK";
    const string POP_UP_GOOD = "POPUP_GOOD";
    const string POP_UP_AWESOME = "POPUP_AWESOME";
    const string POP_UP_PERFECT = "POPUP_PERFECT";
    const string POP_UP_TAP = "POPUP_TAP";
    const string ONGOING_COMBO = "OngoingCombo";
    const string STOP_FADE = "STOP_FADE";

    private void OnEnable()
    {
        //EventManager.Instance.gameplay_events.OnPlayerCombo += HandleComboVisuals;

    }

    private void OnDisable()
    {
        if (EventManager.Instance != null) EventManager.Instance.gameplay_events.OnPlayerCombo -= HandleComboVisuals;
    }

    private void Start()
    {
        EventManager.Instance.gameplay_events.OnPlayerCombo += HandleComboVisuals;

        //perfectCombo.GetComponent<Button>().onClick.AddListener(HandleComboVisualsPerfect);
        //goodCombo.GetComponent<Button>().onClick.AddListener(HandleComboVisualsOK);
        //missCombo.GetComponent<Button>().onClick.AddListener(HandleComboVisualsMiss);

        //comboCountDisplay.text = currentComboCount.ToString();

        p1ComboEmitterAnimator = p1ComboEmitter.gameObject.GetComponent<Animator>();
        p2ComboEmitterAnimator = p2ComboEmitter.gameObject.GetComponent<Animator>();
        pDuoComboEmitterAnimator = pDuoComboEmitter.gameObject.GetComponent<Animator>();

        comboCountDisplay.text = $"{1}x Combo";
    }

    // Temporary for debug
    #region Testing Code for Buttons 
    public void HandleComboVisualsMiss()
    {
        currentComboCount = 0;
        EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Miss,currentComboCount);
        
        //HandleComboVisuals(playerID, ComboType.Miss, currentComboCount);
        Debug.Log("MISS");
    }

    public void HandleComboVisualsOK()
    {
        currentComboCount++;
        EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Ok, currentComboCount);

        //HandleComboVisuals(playerID, ComboType.Ok, currentComboCount);
        Debug.Log("OK");

    }

    public void HandleComboVisualsPerfect()
    {
        currentComboCount++;
        EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Perfect, currentComboCount);

        //HandleComboVisuals(playerID, ComboType.Perfect, currentComboCount);
        Debug.Log("PERFECT");
    }
    #endregion

    public void HandleComboVisuals(int playerID, ComboType comboType, float currentComboCount)
    {
        if (comboType != ComboType.Tap) //ghost filter
            UpdateCount(currentComboCount, comboType);
        PopUpCombo(playerID, comboType);
    }
    
    #region PopUp Combos
    public void PopUpCombo(int playerID, ComboType comboType)
    {
        Animator currentAnimator;

        // Set Current Animator
        switch (playerID)
        {
            case 0:
                {
                    currentAnimator = p1ComboEmitterAnimator;
                    break;
                }
            case 1:
                {
                    currentAnimator = p2ComboEmitterAnimator;
                    break;
                }
            case 2:
                {
                    currentAnimator = pDuoComboEmitterAnimator;
                    break;
                }
            default:
                {
                    currentAnimator = null;
                    break;
                }
        }

        if (currentAnimator == null)
            return;

        if (currentPopUp[playerID] != null)
        { 
            StopCoroutine(currentPopUp[playerID]);
            currentAnimator.SetTrigger(STOP_FADE);
            currentPopUp[playerID] = null;
            currentAnimator.ResetTrigger(STOP_FADE);
        }

        currentPopUp[playerID] = PopUpCooldown(playerID, comboType, currentAnimator);
        StartCoroutine(currentPopUp[playerID]);
    }

    IEnumerator PopUpCooldown(int playerID, ComboType combo, Animator currentAnimator)
    {

        currentAnimator.SetBool(ONGOING_COMBO, true);

        // Pop Up Emision
        switch (combo)
        {
            case ComboType.Miss:
                {
                    // ANIMATE MISS
                    currentAnimator.SetTrigger(POP_UP_MISS);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case ComboType.Ok:
                {
                    // ANIMATE OK
                    currentAnimator.SetTrigger(POP_UP_OK);
                    yield return new WaitForSeconds(timeBeforeFade);

                    break;
                }
            case ComboType.Good:
                {
                    // ANIMATE GOOD
                    currentAnimator.SetTrigger(POP_UP_GOOD);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case ComboType.Awesome:
                {
                    // ANIMATE AWESOME
                    currentAnimator.SetTrigger(POP_UP_AWESOME);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case ComboType.Perfect:
                {
                    //ANIMATE PERFECT
                    currentAnimator.SetTrigger(POP_UP_PERFECT);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case ComboType.Tap:
                {
                    // ANIMATE MISS
                    currentAnimator.SetTrigger(POP_UP_TAP);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }

        }

        currentAnimator.SetTrigger(FADE);

    }

    #endregion

    #region Combo Counter
    //private void AnimateComboCount(ComboType combo)
    //{
    //    switch (combo)
    //    {
    //        case ComboType.Miss:
    //            {
    //                // ANIMATE MISS
    //                break;
    //            }
    //        case ComboType.Ok:
    //            {
    //                // ANIMATE Ok
    //                break;
    //            }
    //        case ComboType.Perfect:
    //            {
    //                //ANIMATE PERFECT
    //                //comboCounterAnimator.SetTrigger(COUNT_DISPLAY_PERFECT);
    //                break;
    //            }
    //        default:
    //            {
    //                return;
    //            }
    //    }
    //}

    private void UpdateCount(float newComboCount, ComboType comboType)
    {
        switch (comboType)
        {
            case ComboType.Miss:
                {
                    newComboCount = 0;
                    break;
                }
            case ComboType.Ok:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case ComboType.Good:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case ComboType.Awesome:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case ComboType.Perfect:
                {
                    newComboCount += 1;
                    break;
                }
            default:
                {
                    return;
                }
        }
        comboCountDisplay.text = $"{1+newComboCount}x Combo";
        Debug.Log(comboType.ToString() +", new combo count: " + newComboCount);
        //AnimateComboCount(comboType);
    }
    #endregion

    //// temporary until more integrated with programming�s mechanics
    //public enum ComboType
    //{
    //    Miss,
    //    Good,
    //    Perfect
    //}

}