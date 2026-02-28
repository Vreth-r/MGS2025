using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private GameManager GM;
    private int consecutive = 0;
    private int totalScore = 0;
    private int score = 0;

    [Header("Ultimate Score Stuff")] //ultimate score multiplier

    [SerializeField] private int inspectorUltimateScoreMultiplier = 4; //the chosen multiplier factor
    public static int ultimateScoreMultiplier = 1; //starts at 1 (aka no difference), because you dont start with the ult

    private void Awake()
    {
        Instance = this;
    }

    private int GetCombo(int consecutive)
    {
        if (consecutive % 2 == 0 && consecutive <= 8)
        {
            return consecutive / 2 + 1; //  new combo
        }
        return (consecutive-1) / 2 + 1; // return old combo
    }

    public void AddScore(float timing)
    {
        bool isfinished = GM.GameIsDone();
        if (isfinished) return;
        Debug.Log("added score");
        if (timing < 0.1f) // perfect
        {
            consecutive++;
            score = 10 * GetCombo(consecutive);
        }
        else if (timing < 0.2f) // awesome
        {
            consecutive = 0;
            score = 7;
        }
        else if (timing < 0.3f) // good
        {
            consecutive = 0;
            score = 5;
        }
        else if (timing < 0.4f) // okay
        {
            consecutive = 0;
            score = 3;
        }
        else // miss
        {
            consecutive = 0;
            score = 0;
        }

        score = Mathf.RoundToInt(score * ultimateScoreMultiplier); //multiplies the score by the ult multiplier (if ults not active, it just multiplies by 1)
        totalScore = totalScore + score; //adds the score buildup to the total score

        UpdateText();
    }

    private void UpdateText()
    {
        scoreText.text = "score: " + totalScore;
        if (consecutive == 0) 
        {
            comboText.text = "combo: 0x"; //displays 0x if not perfect
        }
        else
        {
            comboText.text = "combo: " + GetCombo(consecutive) + "x"; //displays 1x (or higher) if hit perfects
        }
            
    }

    private void OnEnable()
    {
        UltimateSystem.OnUltimateStarted += EnableMultiplier; //subscribes the function "EnableMultiplier", to be called whenever "OnUltimateStarted" is called
        UltimateSystem.OnUltimateFinished += DisableMultiplier; //subscribes the function "DisableMultiplier", to be called whenever "OnUltimateFinished" is called
    }

    private void OnDisable() 
    { 
        UltimateSystem.OnUltimateStarted -= EnableMultiplier; //unsubscribes the function
        UltimateSystem.OnUltimateFinished -= DisableMultiplier; //unsubscribes the function
    }

    //enables the multiplier
    private void EnableMultiplier()
    {
        ultimateScoreMultiplier = inspectorUltimateScoreMultiplier; //change the score multiplier to be the one chosen in the inspector
        Debug.Log("Ult On");
    }

    //disables the multiplier
    private void DisableMultiplier()
    {
        ultimateScoreMultiplier = 1; //back to no multiplier
        Debug.Log("Ult Off");
    }
}
