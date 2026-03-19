using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Endgame : MonoBehaviour
{
    public GameObject endgameScreenPrefab;
    public Transform spawnpoint;

    [SerializeField] private float delayAfterEnd = 1f;

    private bool triggered;


    private void Update()
    {

        if (triggered) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.SongTimeSeconds >= gm.BeatmapEndTimeSeconds)
        {
            triggered = true;
            StartCoroutine(ShowAfterDelay(delayAfterEnd));
            return;
        }

        if (Health.IsDead())
        {
            triggered = true;
            StartCoroutine(ShowAfterDelay(0f));
            return;
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

        GameObject screen = Instantiate(endgameScreenPrefab, spawnpoint);

        //var titleT = screen.transform.Find("TitleText");
        //if (titleT != null && titleT.TryGetComponent(out TextMeshProUGUI title))
        //    title.text = victory ? "VICTORY" : "GAME OVER";


        //var spriteT = screen.transform.Find("ResultSprite");
        
            GameObject gameOverType1 = screen.transform.Find("Defeat").gameObject;
            GameObject gameOverType2 = screen.transform.Find("Victory").gameObject;

        Debug.Log(gameOverType1);
        Debug.Log(gameOverType2);

        if (gameOverType1 != null && gameOverType1.TryGetComponent(out GameObject gameOverLoss) && gameOverType2 != null && gameOverType2.TryGetComponent(out GameObject gameOverWin))
        {
            GameObject screenToShow = victory ? gameOverWin : gameOverLoss; 

            gameOverLoss.SetActive(false);
            gameOverWin.SetActive(false);
            screenToShow.SetActive(true);
        }

        var scoreT = screen.transform.Find("ScoreText");
        if (scoreT != null && scoreT.TryGetComponent(out TextMeshProUGUI scoreTmp) && ScoreManager.Instance != null)
            scoreTmp.text = $"{ScoreManager.Instance.TotalScore}";


        // I am doing it this way bc y'all did not code this in a way we can have 2 fully different screens for results - Knox Fouladi
        var btnT1 = screen.transform.Find("PlayAgainButton1");
        var btnT2 = screen.transform.Find("PlayAgainButton2");
        if (btnT1 != null && btnT1.TryGetComponent(out Button btn1))
            btn1.onClick.AddListener(Restart);
        if (btnT2 != null && btnT2.TryGetComponent(out Button btn2))
            btn2.onClick.AddListener(Restart);

        var btnM1 = screen.transform.Find("MainMenuButton1");
        var btnM2 = screen.transform.Find("MainMenuButton2");
        if (btnM1 != null && btnM1.TryGetComponent(out Button btn3))
            btn3.onClick.AddListener(GoToMainMenu);
        if (btnM2 != null && btnT2.TryGetComponent(out Button btn4))
            btn4.onClick.AddListener(GoToMainMenu);
    }

    private void Restart()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}