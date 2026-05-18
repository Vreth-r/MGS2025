using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }


    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Ultimate")]
    [SerializeField] private int ultimateScoreMultiplier = 4;

    public float TotalScore { get; private set; }
    public float ComboMultiplier { get; private set; } = 1;


    private int playerID = 0;

    private const float PERFECT = 0.05f;
    private const float AWESOME = 0.10f;
    private const float GOOD    = 0.15f;
    private const float OKAY    = 0.20f;  // this really should be bound to the enum some way


    
    
    

    private int activeUltMultiplier = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    private void Start()
    {
        UltimateSystem.Instance.OnUltimateStarted += UltOn;
        UltimateSystem.Instance.OnUltimateFinished += UltOff;
    }

    public void AddScore(float timing, int lane, bool missed = false)
    {
        
        if (GameManager.Instance != null && GameManager.Instance.GameIsDone()) return;
        
        

        if (lane < 2)
        {
            playerID = 0;
        }

        else if (lane > 2)
        {
            playerID = 1;
        }

        else
        {
            playerID = 2;
        }

        
        

        float baseScore;
        if (timing < PERFECT)
        {
  
            if(ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            baseScore = 10 * ComboMultiplier;

            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Perfect, ComboMultiplier);

        }
        else if (timing < AWESOME)
        {
            //ResetCombo();

            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            //baseScore = 7;
            baseScore = 7 * ComboMultiplier;

            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Awesome, ComboMultiplier);
        }
        else if (timing < GOOD)
        {
            //ResetCombo();
    
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 5;
            baseScore = 5 * ComboMultiplier;

            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Good, ComboMultiplier);


        }
        else if (timing < OKAY)
        {
            //ResetCombo();
     
           
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 3;
            baseScore = 3 * ComboMultiplier;

            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Ok, ComboMultiplier);

        }
        else if (missed)
        {
            ResetCombo();
            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Miss, 0);
            baseScore = 0;
        }
        else
        {
            //ResetCombo();
            baseScore = 0;
            EventManager.Instance.gameplay_events.ResolvePlayerCombo(playerID, Judgement.Tap, ComboMultiplier);
        }
        
        
        

        TotalScore += baseScore * activeUltMultiplier;
        UpdateUI();
    }

    public void AddBonus(int points)
    {
        if (points <= 0) return;
        TotalScore += points * activeUltMultiplier;
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
        UltimateSystem.Instance.OnUltimateStarted -= UltOn;
        UltimateSystem.Instance.OnUltimateFinished -= UltOff;
    }

    private void UltOn()  => activeUltMultiplier = ultimateScoreMultiplier;
    private void UltOff() => activeUltMultiplier = 1;
}