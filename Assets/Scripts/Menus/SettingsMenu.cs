using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The settings menu, allows the player to tweak the settings.
/// </summary>
public class SettingsMenu : BaseMenu
{
    [Header("Buttons")]
    [SerializeField] private Button returnButton;

    private int selectedIndex = 0;
    private Button[] buttons;

    protected override void Awake()
    {
        base.Awake();

        buttons = new[] { returnButton };

        returnButton.onClick.AddListener(ReturnBack);

        base.Close();
    }

    protected override void OnOpen()
    {
        Debug.LogFormat($"Settings menu opened");
        HighlightButton(selectedIndex);
    }

    protected override void OnClose()
    {
        // Save the settings somewhere
        Debug.LogFormat($"Settings menu closed");
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
        ReturnBack();
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

    private void ReturnBack()
    {
        MenuManager.Instance.CloseMenu();
    }
}