using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The main menu
/// </summary>
public class MainMenu : BaseMenu
{
    [Header("Button References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    private int selectedIndex = 0;
    private Button[] buttons;

    private GameObject EventSystem;

    protected override void Awake()
    {
        EventSystem = GameObject.Find("EventSystem");

        base.Awake();

        buttons = new[] { playButton, settingsButton, creditsButton, quitButton };

        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(OnQuit);
    }

    protected override void OnOpen()
    {
        Debug.LogFormat($"Main menu opened");
        HighlightButton(selectedIndex);
    }

    protected override void OnClose()
    {
    }

    public override void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    public override void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
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

    public override void HandleSubmit()
    {
        buttons[selectedIndex].onClick.Invoke();
    }

    public override void HandleCancel()
    {
        MenuManager.Instance.OpenPause();
    }

    private void HighlightButton(int index)
    {
        // work around
        // In the event system game object.. Make sure it cannot send navigation events.. thanks.
        var ev = EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();
        ev.SetSelectedGameObject(buttons[index].gameObject);
    }

    private void OnPlay()
    {
        // This just loads the scene, (BUG: The notes move before the scene is fully loaded...)
        var menuManager = MenuManager.Instance;

        menuManager.CloseMenu();

        SceneManager.LoadScene("Game");
    }

    private void OpenSettings()
    {
        Debug.Log("Opening settings menu...");
        MenuManager.Instance.OpenSettings();
    }

    private void OpenCredits()
    {
        // Replace when we have a credit screen
        Debug.Log("Opening credits screen...");
    }

    private void OnQuit()
    {
        Debug.Log("Quitting game...");
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
