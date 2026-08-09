using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : BaseMenu
{
    [System.Serializable]
    public class MenuButton
    {
        public Button button;

        [Header("Sprites")]
        public Sprite defaultSprite;
        public Sprite hoverSprite;
        public Sprite selectedSprite;

        [HideInInspector] public Image image;
        [HideInInspector] public RectTransform rect;

        public void SetSprite(Sprite s)
        {
            image.sprite = s;
            image.SetNativeSize();
        }
    }

    [Header("Buttons")]
    [SerializeField] private MenuButton[] buttons;

    [Header("Semi-circle Settings")]
    [SerializeField] private float radius = 265f;
    [SerializeField] private float angleStep = 30f;

    [Header("Scaling")]
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private float unselectedScale = 1f;

    [Header("Animation")]
    [SerializeField] private float moveSpeed = 10f;

    private int selectedIndex = 0;
    private bool buttonPressed = false;
    private bool wasHidden = false;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].image = buttons[i].button.GetComponent<Image>();
            buttons[i].rect = buttons[i].button.GetComponent<RectTransform>();
            buttons[i].SetSprite(buttons[i].defaultSprite);
        }

        if (buttons.Length > 0) buttons[0].button.onClick.AddListener(ResumeGame);
        if (buttons.Length > 1) buttons[1].button.onClick.AddListener(OpenSettings);
        if (buttons.Length > 2) buttons[2].button.onClick.AddListener(RestartGame);
        if (buttons.Length > 3) buttons[3].button.onClick.AddListener(QuitGame);

        base.Close();

        UpdateButtonsInstant();
    }

    //pauses song while in pause menu
    protected override void OnOpen()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Game")) return; // early return if not in game scene

        if (GameManager.Instance.GameIsDone()) return; // early return if game is over

        
        if (GameManager.Instance.SongTimeSeconds > 0f)
        {
            GameManager.Instance.PauseSong();
        }

        Time.timeScale = 0f;

        buttonPressed = false;
        HighlightButton(selectedIndex);
    }

    protected override void OnClose()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Game")) return; // early return if not in game scene

        if (GameManager.Instance.GameIsDone()) return; // early return if game is over

        

        if (GameManager.Instance.SongTimeSeconds > 0f)
        {
            GameManager.Instance.ResumeSong();
        }

        Time.timeScale = 1f;
    }

    public override void Hide()
    {
        wasHidden = true;
        base.Hide();
    }


    public override void HandleNavigate(Vector2 direction)
    {
        buttonPressed = false;

        if (direction.y > 0.5f)
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
        else if (direction.y < -0.5f)
            selectedIndex = (selectedIndex + 1) % buttons.Length;
    }

    //use sorted button
    public override void HandleSubmit()
    {
        MenuButton b = buttons[selectedIndex];
        buttonPressed = true;
        b.SetSprite(b.selectedSprite);
        /*
        if (b.button.TryGetComponent<UISoundPlayer>(out var soundHelper))
        {
            soundHelper.PlayOnSubmit();
        }
        */
        b.button.onClick.Invoke();
    }

    //resumes game
    public override void HandleCancel()
    {
        ResumeGame();
    }

    //moves swaps and cnages sprties evry frame
    private void HighlightButton(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            MenuButton b = buttons[i];

            Vector2 targetPos = CalculateButtonPosition(i);
            b.rect.anchoredPosition = Vector2.Lerp(b.rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * moveSpeed);

            float targetScale = (i == index) ? selectedScale : unselectedScale;
            b.rect.localScale = Vector3.Lerp(b.rect.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * moveSpeed);

            if (i == index)
            {
                if (!buttonPressed)
                    b.SetSprite(b.hoverSprite);
            }
            else
            {
                b.SetSprite(b.defaultSprite);
            }

            b.rect.SetSiblingIndex(i == index ? buttons.Length - 1 : i);
        }

        var selectedButton = buttons[index].button;

        buttons[index].button.Select();
    }

    private void UpdateButtonsInstant()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            MenuButton b = buttons[i];
            b.rect.anchoredPosition = CalculateButtonPosition(i);
            b.rect.localScale = Vector3.one * ((i == selectedIndex) ? selectedScale : unselectedScale);
            b.SetSprite(i == selectedIndex ? b.hoverSprite : b.defaultSprite);
        }
    }

    private void Update()
    {
        if (wasHidden && isOpen)
        {
            wasHidden = false;
            buttonPressed = false;
            HighlightButton(selectedIndex);
        }
        if (isOpen)
            HighlightButton(selectedIndex);

        // var keyboard = UnityEngine.InputSystem.Keyboard.current;
        // if (keyboard == null) return;

        // if (keyboard.upArrowKey.wasPressedThisFrame)
        //     HandleNavigate(Vector2.up);
        // if (keyboard.downArrowKey.wasPressedThisFrame)
        //     HandleNavigate(Vector2.down);
        // if (keyboard.enterKey.wasPressedThisFrame)
        //     HandleSubmit();
    }

    //calculate button position on semi-circle relative to selected button
    private Vector2 CalculateButtonPosition(int index)
    {
        int offset = index - selectedIndex;
        float angle = offset * angleStep * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * radius;
        float y = -Mathf.Sin(angle) * radius;
        return new Vector2(x, y);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        MenuManager.Instance.CloseMenu();
    }

    public void OpenSettings()
    {
        Debug.Log("Opening settings menu...");
        MenuManager.Instance.OpenSettings();
    }

    private void RestartGame()
    {
        Debug.Log("Restarting...");
        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void QuitGame()
    {
        Debug.Log("Returning to main menu...");
        ResumeGame();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}