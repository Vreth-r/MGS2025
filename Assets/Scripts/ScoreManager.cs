using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private EventManager eventManager;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Ultimate")]
    [SerializeField] private int ultimateScoreMultiplier = 4;

    public float TotalScore { get; private set; }
    public float ComboMultiplier { get; private set; } = 1;

    private int perfectStreak;

    private int playerID = 0;

    private const float PERFECT = 0.10f;
    private const float AWESOME = 0.20f;
    private const float GOOD    = 0.30f;
    private const float OKAY    = 0.40f;

    private int activeUltMultiplier = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    private void Start()
    {
        eventManager = GameObject.FindAnyObjectByType<EventManager>();

        if (eventManager != null)
        {
            //Debug.Log($"{this.name} found {eventManager.name}");
        }

        UltimateSystem.Instance.OnUltimateStarted += UltOn;
        UltimateSystem.Instance.OnUltimateFinished += UltOff;
    }

    public void AddScore(float timing, int lane, bool missed = false)
    {
        
        if (GameManager.Instance != null && GameManager.Instance.GameIsDone()) return;

        playerID = lane < 3 ? 0 : 1;

        float baseScore;
        if (timing < PERFECT)
        {
            perfectStreak++;
            if(ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            baseScore = 10 * ComboMultiplier;

            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Perfect, ComboMultiplier);

        }
        else if (timing < AWESOME)
        {
            //ResetCombo();
            perfectStreak++;
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier++; 
            //baseScore = 7;
            baseScore = 10 * ComboMultiplier;

            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Perfect, ComboMultiplier);
        }
        else if (timing < GOOD)
        {
            //ResetCombo();
            perfectStreak++;
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 5;
            baseScore = 5 * ComboMultiplier;

            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Ok, ComboMultiplier);


        }
        else if (timing < OKAY)
        {
            //ResetCombo();
            perfectStreak++;
           
            if (ComboMultiplier == 0) ComboMultiplier = 1;
            ComboMultiplier += 0.5f;
            //ComboMultiplier = 1 + (perfectStreak / 4);
            //baseScore = 3;
            baseScore = 5 * ComboMultiplier;

            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Ok, ComboMultiplier);

        }
        else if (missed)
        {
            ResetCombo();
            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Miss, 0);
            baseScore = 0;
        }
        else
        {
            //ResetCombo();
            baseScore = 0;
            eventManager.gameplay_events.ResolvePlayerCombo(playerID, ComboType.Miss, ComboMultiplier);
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
        perfectStreak = 0;
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