using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatScreen : BaseMenu
{
    public Button restartBTN;
    public Button mainMenuBTN;

    private int selectedIndex = 0;
    private Button[] buttons;

    private GameObject EventSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        EventSystem = GameObject.Find("EventSystem");

        buttons = new[] { restartBTN, mainMenuBTN };

        restartBTN.onClick.AddListener(Restart);
        mainMenuBTN.onClick.AddListener(GoToMainMenu);
    }

    protected override void OnOpen()
    {
        HighlightButton(selectedIndex);
    }

    private void Restart()
    {
        MenuManager.Instance.CloseMenu();
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    private void GoToMainMenu()
    {
        MenuManager.Instance.CloseMenu();
        SceneManager.LoadScene("MainMenu");
    }

    public override void HandleNavigate(Vector2 direction)
    {
        if (direction.y > 0.5f)
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            HighlightButton(selectedIndex);
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            HighlightButton(selectedIndex);
        }
    }

    private void HighlightButton(int index)
    {
        // work around
        // In the event system game object.. Make sure it cannot send navigation events.. thanks.
        var ev = EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();
        ev.SetSelectedGameObject(buttons[index].gameObject);
    }

    public override void HandleSubmit()
    {
        buttons[selectedIndex].onClick.Invoke();
    }

    public override void HandleCancel()
    {
        // It is assumed that you want to quit the game
        // HOWEVER, I don't want people accidently quitting the game since there is no secondary check
        // (Do you want to quit? yes | no)

        Debug.Log("Cancel has been handled!");

        // FOR NOW, open the pause menu.
        // actually dont... WEIRD stuff happens...
        //MenuManager.Instance.OpenPause();

        // OnQuit();
    }
}
