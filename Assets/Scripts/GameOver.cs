using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverScreen;
    [SerializeField] private string retrySceneName = "Game";
    [SerializeField] private string quitSceneName = "MainMenu";

    void Start()
    {
        gameOverScreen.SetActive(false);
    }
    void Update()
    {
        if (Health.IsDead())
        {
            gameOverScreen.SetActive(true);
            Time.timeScale = 0f; 
        }
    }
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(retrySceneName);
    }
    public void Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(quitSceneName);
    }
}