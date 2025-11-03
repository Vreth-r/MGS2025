using UnityEngine;
using TMPro; // If using TextMeshPro

public class ScoreSystem : MonoBehaviour
{
    const int COMBO_HIT_POPUP = 2;
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;

    public TextMeshProUGUI scoreText;
    public GameObject comboPrefab;
    public Transform playerTransform;

    public void RegisterBeatHit(bool isHit)
    {
        if (isHit)
        {
            score += 1;
            combo += 1;
            if (combo > maxCombo)
                maxCombo = combo;

            // Spawn prefab when combo reaches 2
            if (combo == COMBO_HIT_POPUP && comboPrefab != null)
            {
                Vector3 spawnPos = playerTransform != null
                    ? playerTransform.position + new Vector3(1f, 3f, 0f) 
                    : new Vector3(1f, 3f, 0f);

                GameObject obj = Instantiate(comboPrefab, spawnPos, Quaternion.identity);

                // Set sorting order if it has a SpriteRenderer
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = 100; // Very high to be on top of everything

                Destroy(obj, 1f); // Destroy after 1 second
            }
        }
        else
        {
            combo = 0; // Reset combo on miss
        }

    }

    // Displays score and combo on screen
    void Update()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score} COMBO: {combo} MAX COMBO: {maxCombo}";
    }
}
