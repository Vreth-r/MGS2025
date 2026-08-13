using System;
using Event;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    
    
    private const float PERFECT = 0.05f;
    private const float AWESOME = 0.10f;
    private const float GOOD    = 0.15f;
    private const float OKAY    = 0.20f;  // this really should be bound to the enum some way
    
    public static ScoreManager Instance { get; private set; }
    
    
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Ultimate")]
    [SerializeField] private int ultimateScoreMultiplier = 4;

    public float TotalScore { get; private set; }
    public float ComboMultiplier { get; private set; } = 1;
    

    private int _activeUltMultiplier = 1;
    private bool _playersMerged;
    
    

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    private void Start()
    {
        // UltimateSystem.Instance.OnUltimateStarted += UltOn;
        // UltimateSystem.Instance.OnUltimateFinished += UltOff;
        
        GameplayEvents.UltimateDepleteEvent.AddEventListener(UltOff);
        GameplayEvents.UltimateStartEvent.AddEventListener(UltOn);
        
        
        GameplayEvents.PlayersMergeEvent.AddEventListener(this.OnPlayersMerge);
        GameplayEvents.PlayersSeparateEvent.AddEventListener(this.OnPlayersSeparate);
    }

    //--------------------------Event listeners-------------------------------------

    //this really should be accessible via some central manager if we're talking about code cleanliness
    //perferably some level manager
    private void OnPlayersMerge(ValueTuple _)
    {
        this._playersMerged = true;
    }

    private void OnPlayersSeparate(ValueTuple _)
    {
        this._playersMerged = false;
    }
    
    
    private void UltOn(ValueTuple _)  => _activeUltMultiplier = ultimateScoreMultiplier;
    private void UltOff(ValueTuple _) => _activeUltMultiplier = 1;
    
    
    //---------------------------------------------------------------------------




    public void AddScore(float timing, int lane, bool missed = false)
    {
        if (GameManager.Instance != null && GameManager.Instance.GameIsDone())
            return;
    
        int playerID;
        if (this._playersMerged)
            playerID = 2;
        else
            playerID = (lane < 2) ? 0 : 1;
        

        float baseScore;
        if (timing < PERFECT)
        {
  
            if(ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            baseScore = 10 * ComboMultiplier;
            
            
            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Perfect, ComboMultiplier));
  

        }
        else if (timing < AWESOME)
        {
            //ResetCombo();

            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            //baseScore = 7;
            baseScore = 7 * ComboMultiplier;

            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Awesome, ComboMultiplier));
        }
        else if (timing < GOOD)
        {
            //ResetCombo();
    
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 5;
            baseScore = 5 * ComboMultiplier;

            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Good, ComboMultiplier));


        }
        else if (timing < OKAY)
        {
            //ResetCombo();
     
           
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 3;
            baseScore = 3 * ComboMultiplier;

            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Ok, ComboMultiplier));

        }
        else if (missed)
        {
            
            ResetCombo();
            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Miss, ComboMultiplier));
            baseScore = 0;
        }
        else
        {
            //ResetCombo();
            baseScore = 0;
            GameplayEvents.ScoreUpdateEvent.CallEvent((playerID, Judgement.Tap, ComboMultiplier));
        }
        
        
        

        TotalScore += baseScore * _activeUltMultiplier;
        UpdateUI();
    }

    public void AddBonus(int points)
    {
        if (points <= 0) return;
        TotalScore += points * _activeUltMultiplier;
        UpdateUI();
    }

    private void ResetCombo()
    {
        ComboMultiplier = 1;
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"{(int)TotalScore}";
        //if (comboText != null) comboText.text = $"{ComboMultiplier}x Combo";
    }

    private void OnDestroy()
    {
        // UltimateSystem.Instance.OnUltimateStarted -= UltOn;
        // UltimateSystem.Instance.OnUltimateFinished -= UltOff;
        
        GameplayEvents.UltimateDepleteEvent.RemoveEventListener(UltOff);
        GameplayEvents.UltimateStartEvent.RemoveEventListener(UltOn);
        
    }
    
}