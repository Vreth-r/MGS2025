using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Menu Settings")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Button References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    private int selectedIndex = 0;
    private Button[] buttons;

    private void Awake()
    {
        if (Instance != null && Instance != this) // Singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hardcoded because the main menu aint gonna be dynamic I HOPE.
        buttons = new[] { playButton, settingsButton, creditsButton, quitButton };
        // Listeners
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(OnQuit);
    }

    private void Start()
    {
        var menus = MenuManager.Instance;

        menus.menusClosed += ToggleInputControl;
        // Disable Menu Inputs
        menus.enabled = false;

        HighlightButton(selectedIndex);
        SubscribeInputs(); // Gain control over the inputs!!!!
    }

    private void OnDisable()
    {
        UnsubscribeInputs(); // Lose control over the inputs
    }

    // This method subscribes some methods to the input actions
    private void SubscribeInputs()
    {
        var input = InputManager.Instance;

        input.OnNavigate += HandleNavigate;
        input.OnSubmit += HandleSubmit;
        input.OnCancel += HandleCancel;

        // Enable UI input map
        input.EnableUI();
    }

    // This method unsubscribes the methods from the input actions
    private void UnsubscribeInputs()
    {
        if (InputManager.Instance == null) return;
        var input = InputManager.Instance;

        input.OnNavigate -= HandleNavigate;
        input.OnSubmit -= HandleSubmit;
        input.OnCancel -= HandleCancel;
    }

    // Toggle between having control / not having control
    private void ToggleInputControl()
    {
        var menus = MenuManager.Instance;
        if (menus.enabled)
        {
            menus.enabled = false;
            SubscribeInputs();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            menus.enabled = true;
            UnsubscribeInputs();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnPlay()
    {
        // This just loads the scene, (BUG: The notes move before the scene is fully loaded...)
        SceneManager.LoadScene("Game");
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

    private void OpenSettings()
    {
        // We hand over the controls to the menu manager
        ToggleInputControl();
        // Open menu
        Debug.Log("Opening Settings...");
        MenuManager.Instance.OpenSettings();
    }

    private void OpenCredits()
    {
        // Replace when we have a credit screen
        Debug.Log("Opening credits screen...");
    }

    private void HandleNavigate(Vector2 direction)
    {
        if (direction.y > 0.5f)
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            //HighlightButton(selectedIndex);
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            //HighlightButton(selectedIndex);
        }
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var colors = buttons[i].colors;
            colors.normalColor = (i == index) ? Color.yellow : Color.white;
            buttons[i].colors = colors;
        }
    }

    private void HandleSubmit()
    {
        //buttons[selectedIndex].onClick.Invoke();
    }

    private void HandleCancel()
    {
        // It is assumed that you want to quit the game
        // HOWEVER, I don't want people accidently quitting the game since there is no secondary check
        // (Do you want to quit? yes | no)

        Debug.Log("Cancel has been handled!");

        // FOR NOW, open the pause menu.
        ToggleInputControl();
        MenuManager.Instance.OpenPause();

        // OnQuit();
    }
}
