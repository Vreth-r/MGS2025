using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Endgame : MonoBehaviour
{
    public GameObject endgameScreenPrefab;
    public Sprite victorySprite;
    public Sprite gameOverSprite;

    [SerializeField] private float delayAfterEnd = 1f;

    private bool triggered;

    private void Update()
    {
        if (triggered) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        
        if (Health.IsDead())
        {
            triggered = true;
            StartCoroutine(ShowAfterDelay(0f));
            return;
        }

        float songTime = gm.SongTimeSeconds;
        float endTime = gm.BeatmapEndTimeSeconds;

        
        if (songTime >= endTime)
        {
            triggered = true;
            StartCoroutine(ShowAfterDelay(delayAfterEnd));
        }
    }

    private IEnumerator ShowAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Show();
    }

    private void Show()
    {
        if (endgameScreenPrefab == null) return;

        bool victory = !Health.IsDead();

        GameObject screen = Instantiate(endgameScreenPrefab);

        var titleT = screen.transform.Find("TitleText");
        if (titleT != null && titleT.TryGetComponent(out TextMeshProUGUI title))
            title.text = victory ? "VICTORY" : "GAME OVER";

        var spriteT = screen.transform.Find("ResultSprite");
        if (spriteT != null && spriteT.TryGetComponent(out Image img))
            img.sprite = victory ? victorySprite : gameOverSprite;

        var scoreT = screen.transform.Find("ScoreText");
        if (scoreT != null && scoreT.TryGetComponent(out TextMeshProUGUI scoreTmp) && ScoreManager.Instance != null)
            scoreTmp.text = $"score: {ScoreManager.Instance.TotalScore}";

        var btnT = screen.transform.Find("PlayAgainButton");
        if (btnT != null && btnT.TryGetComponent(out Button btn))
            btn.onClick.AddListener(Restart);
    }

    private void Restart()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}