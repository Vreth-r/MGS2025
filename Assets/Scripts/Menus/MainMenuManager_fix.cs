using System;
using UnityEngine;

// All this script does is open the main menu when the scene loads in
public class MainMenuManagerFix : MonoBehaviour
{
    public static MainMenuManagerFix Instance { get; private set; }

    [Header("Menu Reference")]
    [SerializeField] private GameObject mainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) // Singleton
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.EnableUI();
        MenuManager.Instance.SetCurrentMenu(mainMenu);
    }
}