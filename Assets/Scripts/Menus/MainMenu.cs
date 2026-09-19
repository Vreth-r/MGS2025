using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

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
    [SerializeField] private Button difficultyButton;
    [SerializeField] private Toggle autoUltToggle;
    [SerializeField] private Toggle godModeToggle;
    [SerializeField] private Toggle fleshSoundVersionsToggle;
    //[SerializeField] private TMP_Dropdown gameModeDropdown;

    [SerializeField] private VideoPlayer creditsVideoPlayer;
    [SerializeField] private RawImage creditsDisplay;

    private int selectedIndex = 0;
    private Button[] buttons;

    private GameObject EventSystem;
    private bool CreditsOpen = false;

    public GameSettings gameSettings;
    protected override void Awake()
    {
        EventSystem = GameObject.Find("EventSystem");

        base.Awake();

        buttons = new[] { playButton, settingsButton, creditsButton, quitButton, difficultyButton };

        gameSettings.gameMode = 1; //default is normal for difficulty

        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(OnQuit);
        difficultyButton.onClick.AddListener(OpenDifficulty);
    }

    protected void Update()
    {
        //update difficulty button visual
        // Debug.Log("should be now set as: " + gameSettings.gameMode);
        difficultyButton.GetComponent<Animator>().SetInteger("Diff", gameSettings.gameMode);
    }


    protected override void OnOpen()
    {
        Debug.LogFormat($"Main menu opened");
        UISoundPlayer.surpressFirstHoverSound = true;
        StartCoroutine(DelaySelect());
    }

    private IEnumerator DelaySelect()
    {
        yield return null;
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

        if (CreditsOpen) return;

        if (direction.y > 0.5f)
        {
            selectedIndex = (selectedIndex - 1 + (buttons.Length-1)) % (buttons.Length-1); //-1 offset as to avoid Difficulty Button in vertical
            HighlightButton(selectedIndex);
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex = (selectedIndex + 1) % (buttons.Length-1);
            HighlightButton(selectedIndex);
        }

        if (direction.x > 0.5f || direction.x < -0.5f)
        {
            if (selectedIndex != 4)
                selectedIndex = 4;
            else
                selectedIndex = 0;
            HighlightButton(selectedIndex);
        }
    }

    public override void HandleSubmit()
    {
        var button = buttons[selectedIndex];
        /*
        var soundHelper = button.GetComponent<UISoundPlayer>();
        if (soundHelper != null)
        {
            soundHelper.PlayOnSubmit();
        }
        */
        button.onClick.Invoke();

        //buttons[selectedIndex].onClick.Invoke();
    }

    public override void HandleCancel()
    {
        //MenuManager.Instance.OpenPause();
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
        // there's a glitch where having the credits open and pressing enter returns you to character select instead of main menu
        if (CreditsOpen)
        {
            return;
        }

        gameSettings.autoUltimate = autoUltToggle.isOn;
        gameSettings.godMode = godModeToggle.isOn;
        gameSettings.useFleshSoundVersions = fleshSoundVersionsToggle.isOn;
        Debug.Log("diff is: " + gameSettings.gameMode);
        //gameSettings.gameMode = gameModeDropdown.value;

        // This just loads the scene, (BUG: The notes move before the scene is fully loaded...)
        var menuManager = MenuManager.Instance;

        menuManager.CloseMenu();

        SceneManager.LoadScene("NEW CharSel");
    }

    private void OpenSettings()
    {
        Debug.Log("Opening settings menu...");
        MenuManager.Instance.OpenSettings();
    }
    private void OpenDifficulty()
    {
        Debug.Log("Opening difficulty menu...");
        MenuManager.Instance.OpenDifficulty();
    }

    private void OpenCredits()
    {
        CreditsOpen = true; //blocks menu interaction
        Hide();
        creditsVideoPlayer.Stop();
        creditsVideoPlayer.time = 0;
        creditsVideoPlayer.Prepare();
        creditsVideoPlayer.prepareCompleted += OnCreditsPrepared;
    }

    private void OnCreditsPrepared(VideoPlayer vp)
    {
        creditsVideoPlayer.prepareCompleted -= OnCreditsPrepared; //unsubscribes so it does not fire again
        creditsDisplay.gameObject.SetActive(true);
        creditsVideoPlayer.Play();
        creditsVideoPlayer.loopPointReached += CloseCredits;
        InputManager.Instance.OnSubmit += CloseCredits;
    }

    //loopPointReached needs this parameter, workaround to keep CloseCredits parameterless
    private void CloseCredits(VideoPlayer vp) 
    { 
        CloseCredits();
    }

    private void CloseCredits()
    {
        CreditsOpen = false; //unlocks menu interaction
        InputManager.Instance.OnSubmit -= CloseCredits;
        creditsVideoPlayer.Stop();
        creditsDisplay.gameObject.SetActive(false);
        creditsVideoPlayer.loopPointReached -= CloseCredits;
        Show();
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
