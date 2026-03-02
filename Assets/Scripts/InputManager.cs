using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// if you cant read this without comments maybe leave this one alone
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Settings")]
    [Tooltip("Input Action Asset containing the 'Gameplay' action map.")]
    [SerializeField] private InputActionAsset inputAsset;

    private InputActionMap gameplayMap;
    private InputActionMap uiMap;

    // Stores all lane actions: lane index → InputAction
    private readonly Dictionary<int, InputAction> laneActions = new();

    // Lane press/release events
    public event Action<int> OnLanePressed;
    public event Action<int> OnLaneReleased;

    //ultimate activation 
    public event Action OnUltimatePressed;

    // Gameplay Pause Action
    private InputAction gamePauseAction;

    // ui actions
    private InputAction pauseAction;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    public event Action OnPausePressed;
    public event Action<Vector2> OnNavigate;
    public event Action OnSubmit;
    public event Action OnCancel;

    public enum InputContext { Gameplay, UI }
    public InputContext CurrentContext { get; private set; } = InputContext.Gameplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        InitializeActionMaps();
    }

    private void OnEnable()
    {
        EnableGameplay();
    }
    private void OnDisable()
    {
        DisableAll();
    }

    private void InitializeActionMaps()
    {
        gameplayMap = inputAsset.FindActionMap("Gameplay", throwIfNotFound: true);
        uiMap = inputAsset.FindActionMap("UI", throwIfNotFound: true);

        InitializeLaneInputs();
        InitializeUIInputs();
    }

    private void InitializeLaneInputs()
    {
        laneActions.Clear();

        // Auto-detect any actions named "Lane0", "Lane1", etc.
        int i = 0;
        while (true)
        {
            var action = gameplayMap.FindAction($"Lane{i}");
            if (action == null) break;

            int laneIndex = i; // Capture variable for closure
            laneActions[laneIndex] = action;

            // Subscribe events
            action.performed += ctx => OnLanePressed?.Invoke(laneIndex);
            action.canceled += ctx => OnLaneReleased?.Invoke(laneIndex);

            i++;
        }

        //finds the keybind for "Ultimate" in the new input system
        var ultimateAction = gameplayMap.FindAction("Ultimate");
        if (ultimateAction != null) //if it exists
        {
            //when ult button is pressed, invoke the event "OnUltimatePressed" and any events subscribed to its event flag (like the function "UseUltimate" in UltimateSystem.cs)
            ultimateAction.performed += ctx => OnUltimatePressed?.Invoke(); 
        }

        // Assigning gamePause Action and Subscribing to Event
        gamePauseAction = uiMap.FindAction("Pause");
        gamePauseAction.performed += ctx => OnPausePressed?.Invoke();

        //Debug.Log($"ControlsManager initialized with {laneActions.Count} lanes.");
    }

    private void InitializeUIInputs()
    {
        //pauseAction = uiMap.FindAction("Pause"); - Commented out bcs we want to pause during gameplay
        navigateAction = uiMap.FindAction("Navigate");
        submitAction = uiMap.FindAction("Submit");
        cancelAction = uiMap.FindAction("Cancel");

        //pauseAction.performed += ctx => OnPausePressed?.Invoke();
        navigateAction.performed += ctx => OnNavigate?.Invoke(ctx.ReadValue<Vector2>());
        submitAction.performed += ctx => OnSubmit?.Invoke();
        cancelAction.performed += ctx => OnCancel?.Invoke();
    }

    /// Context switching
    /// because balls
    public void EnableGameplay()
    {
        DisableAll();
        gameplayMap.Enable();
        CurrentContext = InputContext.Gameplay;
        //Debug.Log("InputManager switched to Gameplay action map");
    }

    public void EnableUI()
    {
        DisableAll();
        uiMap.Enable();
        CurrentContext = InputContext.UI;
        //Debug.Log("InputManager switched to UI action map");
    }

    private void DisableAll()
    {
        gameplayMap?.Disable();
        uiMap?.Disable();
    }

    /// <summary>
    /// Returns the InputAction for a given lane index.
    /// </summary>
    public InputAction GetLaneAction(int laneIndex)
    {
        return laneActions.TryGetValue(laneIndex, out var action) ? action : null;
    }

    /// <summary>
    /// Returns true if the lane key is currently being held.
    /// </summary>
    public bool IsLaneHeld(int laneIndex)
    {
        var action = GetLaneAction(laneIndex);
        return action != null && action.ReadValue<float>() > 0.5f;
    }

    /// <summary>
    /// Start a runtime rebind for a specific lane.
    /// </summary>
    public void StartRebind(int laneIndex, Action onComplete = null)
    {
        var action = GetLaneAction(laneIndex);
        if (action == null)
        {
            //Debug.LogWarning($"No action found for lane {laneIndex}");
            return;
        }

        action.Disable();

        var rebind = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>") // optional
            .OnComplete(ctx =>
            {
                ctx.Dispose();
                action.Enable();
                onComplete?.Invoke();
                //Debug.Log($"Rebound Lane {laneIndex} to {action.bindings[0].effectivePath}");
            });

        rebind.Start();
    }

    // UI Action Accessors
    public bool IsPauseHeld => pauseAction != null && pauseAction.ReadValue<float>() > 0.5f; 
}
