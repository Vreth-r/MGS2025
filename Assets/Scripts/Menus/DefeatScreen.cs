using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatScreen : MonoBehaviour
{
    public Button restartBTN;
    public Button mainMenuBTN;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        restartBTN.onClick.AddListener(Restart);
        mainMenuBTN.onClick.AddListener(GoToMainMenu);
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
