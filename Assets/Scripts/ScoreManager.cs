using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private GameManager GM;
    private int consecutive = 0;
    private int score = 0;
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
            score += 10 * GetCombo(consecutive);
        }
        else if (timing < 0.2f) // awesome
        {
            consecutive = 0;
            score += 7;
        }
        else if (timing < 0.3f) // good
        {
            consecutive = 0;
            score += 5;
        }
        else if (timing < 0.4f) // okay
        {
            consecutive = 0;
            score += 3;
        }
        else // miss
        {
            consecutive = 0;
        }
        UpdateText();
    }

    private void UpdateText()
    {
        scoreText.text = "score: " + score;
        if (consecutive == 0) 
        {
            comboText.text = "combo: 0x"; //displays 0x if not perfect
        }
        else
        {
            comboText.text = "combo: " + GetCombo(consecutive) + "x"; //displays 1x (or higher) if hit perfects
        }
            
    }
}
