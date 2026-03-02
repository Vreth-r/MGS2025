using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Ultimate")]
    [SerializeField] private int inspectorUltimateScoreMultiplier = 4;

    public int TotalScore { get; private set; }
    public int ComboMultiplier { get; private set; } = 1;

    private int perfectStreak;

    private const float PERFECT = 0.10f;
    private const float AWESOME = 0.20f;
    private const float GOOD    = 0.30f;
    private const float OKAY    = 0.40f;

    private int ultMultiplier = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    public void AddScore(float timing)
    {
        
        if (GameManager.Instance != null && GameManager.Instance.GameIsDone()) return;

        int baseScore;

        if (timing < PERFECT)
        {
            perfectStreak++;
            ComboMultiplier = 1 + (perfectStreak / 2); 
            baseScore = 10 * ComboMultiplier;
        }
        else if (timing < AWESOME)
        {
            ResetCombo();
            baseScore = 7;
        }
        else if (timing < GOOD)
        {
            ResetCombo();
            baseScore = 5;
        }
        else if (timing < OKAY)
        {
            ResetCombo();
            baseScore = 3;
        }
        else
        {
            ResetCombo();
            baseScore = 0;
        }

        TotalScore += baseScore * ultMultiplier;
        UpdateUI();
    }

    public void AddBonus(int points)
    {
        if (points <= 0) return;
        TotalScore += points * ultMultiplier;
        UpdateUI();
    }

    private void ResetCombo()
    {
        perfectStreak = 0;
        ComboMultiplier = 1;
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"score: {TotalScore}";
        if (comboText != null) comboText.text = $"combo: {ComboMultiplier}x";
    }

    private void OnEnable()
    {
        UltimateSystem.OnUltimateStarted += UltOn;
        UltimateSystem.OnUltimateFinished += UltOff;
    }

    private void OnDisable()
    {
        UltimateSystem.OnUltimateStarted -= UltOn;
        UltimateSystem.OnUltimateFinished -= UltOff;
    }

    private void UltOn()  => ultMultiplier = inspectorUltimateScoreMultiplier;
    private void UltOff() => ultMultiplier = 1;
}