using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;

public class VictoryScreen : BaseMenu
{
    
    [Header("Button References")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] public SongCompleteScreen screen;

    private int selectedIndex = 0;
    private Button[] buttons;

    private GameObject EventSystem;

    protected override void Awake()
    {
        EventSystem = GameObject.Find("EventSystem");

        // Hardcoded because the main menu aint gonna be dynamic I HOPE.
        buttons = new[] { menuButton, quitButton };
        // Listeners
        menuButton.onClick.AddListener(OnMainMenu);
        quitButton.onClick.AddListener(OnQuit);
    }

    protected override void OnOpen()
    {
        screen.TrackFinish();
        UISoundEffectsEventHelper.surpressFirstHoverSound = true;
        StartCoroutine(DelaySelect());
    }

    private IEnumerator DelaySelect()
    {
        yield return null;
        HighlightButton(selectedIndex);
    }

    private void OnMainMenu()
    {
        // This just loads the scene, (BUG: The notes move before the scene is fully loaded...)
        MenuManager.Instance.CloseMenu();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnQuit()
    {
        // There should be a check to see if the player actually wants to quit
        // (Do you want to quit? yes | no)

        Debug.Log("Quitting game...");
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        var button = buttons[selectedIndex];

        var soundHelper = button.GetComponent<UISoundEffectsEventHelper>();
        if (soundHelper != null)
        {
            soundHelper.PlayOnSubmit();
        }

        button.onClick.Invoke();

    }

    public override void HandleCancel()
    {
        // It is assumed that you want to quit the game
        // HOWEVER, I don't want people accidently quitting the game since there is no secondary check
        // (Do you want to quit? yes | no)

        Debug.Log("Cancel has been handled!");

        // FOR NOW, open the pause menu.
        //MenuManager.Instance.OpenPause();

        // OnQuit();
    }
}