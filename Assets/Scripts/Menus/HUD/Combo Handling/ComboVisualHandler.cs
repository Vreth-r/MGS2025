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
    
    [SerializeField] private TMP_Text comboCountDisplay;
    [SerializeField] private Animator comboCounterAnimator; // Top Right Counter
    
    
    [SerializeField] private PlayerComboEmitter p1ComboEmitter; // Player 1 combo emitter
    private Animator p1ComboEmitterAnimator; // Player 1 combo animator
    
    [SerializeField] private PlayerComboEmitter p2ComboEmitter; // Player 2 combo emitter
    private Animator p2ComboEmitterAnimator; // Player 2 combo animator
  
    
    [SerializeField] private PlayerComboEmitter pDuoComboEmitter; // Player 2 combo emitter
    private Animator pDuoComboEmitterAnimator; // Player 2 combo animator
   



    public float timeBeforeFade;
    IEnumerator[] currentPopUp =  new IEnumerator[3];


    // animation name Constants

    // // counter (top right corner)
    // const string COUNT_DISPLAY_MISS = "MissCOMBO";
    // const string COUNT_DISPLAY_GOOD = "GoodCOMBO";
    // const string COUNT_DISPLAY_PERFECT = "PerfectCOMBO";

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
    
    
    
    
    private void OnDisable()
    {
        if (EventManager.Instance != null) EventManager.Instance.gameplay_events.OnPlayerCombo -= HandleComboVisuals;
    }

    private void Start()
    {
        EventManager.Instance.gameplay_events.OnPlayerCombo += HandleComboVisuals;
        
        p1ComboEmitterAnimator = p1ComboEmitter.gameObject.GetComponent<Animator>();
        p2ComboEmitterAnimator = p2ComboEmitter.gameObject.GetComponent<Animator>();
        pDuoComboEmitterAnimator = pDuoComboEmitter.gameObject.GetComponent<Animator>();

        comboCountDisplay.text = $"{1}x Combo";
    }


    public void HandleComboVisuals(int playerID, Judgement judgement, float currentComboCount)
    {
        if (judgement != Judgement.Tap) //ghost filter
            this.UpdateCount(currentComboCount, judgement);
        
        this.PopUpCombo(playerID, judgement);
    }
    
    #region PopUp Combos
    public void PopUpCombo(int playerID, Judgement judgement)
    {
        Animator currentAnimator;
        Debug.Log(playerID +" "+ judgement.ToString());
        
        
        
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
        {
            Debug.LogError("Current animator is null!");
            return;
        }

        if (currentPopUp[playerID] != null)
        { 
            StopCoroutine(currentPopUp[playerID]);
            currentAnimator.SetTrigger(STOP_FADE);
            currentPopUp[playerID] = null;
            currentAnimator.ResetTrigger(STOP_FADE);
        }
        
        
        Debug.Log("player id: "+playerID);
        currentPopUp[playerID] = PopUpCooldown(judgement, currentAnimator);
        StartCoroutine(currentPopUp[playerID]);
    }

    
    
    
    IEnumerator PopUpCooldown(Judgement combo, Animator currentAnimator)
    {

        currentAnimator.SetBool(ONGOING_COMBO, true);
        Debug.Log("ienumerator j: "+combo);

        // Pop Up Emision
        switch (combo)
        {
            case Judgement.Miss:
                {
                    // ANIMATE MISS
                    currentAnimator.SetTrigger(POP_UP_MISS);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case Judgement.Ok:
                {
                    // ANIMATE OK
                    currentAnimator.SetTrigger(POP_UP_OK);
                    yield return new WaitForSeconds(timeBeforeFade);

                    break;
                }
            case Judgement.Good:
                {
                    // ANIMATE GOOD
                    currentAnimator.SetTrigger(POP_UP_GOOD);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case Judgement.Awesome:
                {
                    // ANIMATE AWESOME
                    currentAnimator.SetTrigger(POP_UP_AWESOME);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case Judgement.Perfect:
                {
                    //ANIMATE PERFECT
                    currentAnimator.SetTrigger(POP_UP_PERFECT);
                    yield return new WaitForSeconds(timeBeforeFade);
                    break;
                }
            case Judgement.Tap:
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

    
    
    
    private void UpdateCount(float newComboCount, Judgement judgement)
    {
        switch (judgement)
        {
            case Judgement.Miss:
                {
                    newComboCount = 0;
                    break;
                }
            case Judgement.Ok:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case Judgement.Good:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case Judgement.Awesome:
                {
                    newComboCount += 0.5f;
                    break;
                }
            case Judgement.Perfect:
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
        Debug.Log(judgement.ToString() +", new combo count: " + newComboCount);
    
        
        
        
    }




}