using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menu References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    private BaseMenu activeMenu;
    private Stack<BaseMenu> OpenedMenus = new Stack<BaseMenu>(); // Stack of menus. So we can backtrack between opened menus.

    // When all menus are closed invoke this action
    // (This is used to "hand-over" input management to a different manager i.e: MenuManager -> MainMenuManager)
    // so far used in MainMenuManager and GameManager
    public event Action menusClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        var input = InputManager.Instance;

        input.OnPausePressed += OpenPause;
        input.OnNavigate += HandleNavigate;
        input.OnSubmit += HandleSubmit;
        input.OnCancel += HandleCancel;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;
        var input = InputManager.Instance;

        input.OnPausePressed -= OpenPause;
        input.OnNavigate -= HandleNavigate;
        input.OnSubmit -= HandleSubmit;
        input.OnCancel -= HandleCancel;
    }

    public void OpenPause()
    {
        OpenMenu(pauseMenu);
    }

    public void OpenSettings()
    {
        OpenMenu(settingsMenu);
    }

    public void OpenMenu(GameObject menuPrefab)
    {
        // Get the menu we want to open
        BaseMenu menuToOpen = menuPrefab.GetComponent<BaseMenu>();

        if (menuToOpen == null) // If it's not a menu don't open it
        {
            Debug.LogError($"Menu prefab {menuPrefab.name} is missing a BaseMenu component.");
            return;
        }

        // Hide the previous active menu
        if (activeMenu != null)
        {
            activeMenu.Hide();
        }

        // Setting the active menu to be the one we want opened
        activeMenu = menuToOpen;
        OpenedMenus.Push(activeMenu);
        activeMenu.Open(); // Hey we finally opened the menu

        if (OpenedMenus.Count == 1) InputManager.Instance.EnableUI(); // if this is the first menu opened lets enable ui inputs
    }

    public void CloseMenu()
    {
        if (activeMenu == null) return;

        // Close the active menu
        OpenedMenus.Pop();
        activeMenu.Close();

        // Set the active menu to the menu behind it, if it exists.
        if (OpenedMenus.Count >= 1)
        {
            activeMenu = OpenedMenus.Peek();
            activeMenu.Show();
        }
        else
        {
            activeMenu = null;
            menusClosed?.Invoke();
        }
    }

    // This method just closes all the opened menus
    private void CloseAllMenus()
    {
        if (activeMenu == null) return;

        foreach (BaseMenu openedMenu in OpenedMenus)
        {
            openedMenu.Close();
        }

        OpenedMenus.Clear();
        activeMenu = null;
        menusClosed?.Invoke();
    }

    private void HandleNavigate(Vector2 direction)
    {
        activeMenu?.HandleNavigate(direction);
    }

    private void HandleSubmit()
    {
        activeMenu?.HandleSubmit();
    }

    private void HandleCancel()
    {
        if (activeMenu != null)
        {
            activeMenu.HandleCancel();
        }
        else
        {
            OpenPause(); // fallback — close menu or open pause
        }
    }
}
