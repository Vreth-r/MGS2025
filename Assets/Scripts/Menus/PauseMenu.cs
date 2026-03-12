using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The pause menu. Handles resume, settings, and quitting.
/// </summary>
public class PauseMenu : BaseMenu
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private int selectedIndex = 0;
    private Button[] buttons;

    protected override void Awake()
    {
        base.Awake();

        buttons = new[] { resumeButton, settingsButton, quitButton };

        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);

        base.Close();
    }

    protected override void OnOpen()
    {
        Time.timeScale = 0f; // pause gameplay
        Debug.LogFormat($"Pause menu opened");
        HighlightButton(selectedIndex);
    }

    protected override void OnClose()
    {
        Time.timeScale = 1f; // resume gameplay
        Debug.LogFormat($"Pause menu closed");
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
        ResumeGame();
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

    private void ResumeGame()
    {
        MenuManager.Instance.CloseMenu();
    }

    private void OpenSettings()
    {
        Debug.Log("Opening settings menu...");
        MenuManager.Instance.OpenSettings();
    }

    private void QuitGame()
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
