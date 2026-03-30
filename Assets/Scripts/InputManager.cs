using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// if you cant read this without comments maybe leave this one alone
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Settings")]
    [Tooltip("Input Action Asset containing the 'Gameplay' action map.")]
    [SerializeField] private InputActionAsset inputAsset;

    private InputActionMap gameplayMap;
    private InputActionMap uiMap;

    // This is so we can actually unsubscribe these lambda functions
    private struct LaneBinding
    {
        public InputAction action;
        public Action<InputAction.CallbackContext> performed;
        public Action<InputAction.CallbackContext> canceled;
    }

    // Stores all lane bindings: lane index → LaneBinding (holds the lambdas and the action itself)
    private readonly Dictionary<int, LaneBinding> laneActions = new();

    // Lane press/release events
    public event Action<int> OnLanePressed;
    public event Action<int> OnLaneReleased;

    //guitar specific events
    public event Action<int> OnLanePressedGuitar;
    public event Action<int> OnLaneReleasedGuitar;
    public event Action OnGuitarAttackPressed;

    private InputDevice guitarDevice;

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

    private ControlsSwapper controlsSwapper;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //the controls stay swapped (if you chose to swap them during character select) after you exit the play mode testing.
        //Go to Assets/Scripts/Resources/ControlsSwapper.asset and uncheck "Is Controls Swapped" to reset to default controls for any testing that doesnt touch the chracter select scene.
        controlsSwapper = Resources.Load<ControlsSwapper>("ControlsSwapper"); 

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
        InitializeGameplayInputs();
        InitializeUIInputs();
    }

    private void InitializeLaneInputs()
    {
        laneActions.Clear();

        bool swapped = controlsSwapper.isControlsSwapped;

        // Auto-detect any actions named "Lane0", "Lane1", etc.
        int i = 0;
        while (true)
        {
            var action = gameplayMap.FindAction($"Lane{i}");
            if (action == null) break;

            int laneIndex = i; // Capture variable for closure

            int finalLaneIndex; //for after possible control swapping

            if (swapped == false) //default players
            {
                finalLaneIndex = laneIndex;
            }

            else //swapped players
            {
                Debug.Log("swapped controls");

                finalLaneIndex = laneIndex switch
                {
                    0 => 3,
                    1 => 4,
                    2 => 2,
                    3 => 0,
                    4 => 1,
                    _ => laneIndex
                };
            }

            var binding = new LaneBinding // store the data nicely in a struct
            {
                action = action,

                performed = ctx =>
                    {
                        var device = ctx.control.device;

                        if (guitarDevice == null && IsGuitar(device))
                            guitarDevice = device;

                        if (device == guitarDevice)
                            OnLanePressedGuitar?.Invoke(finalLaneIndex);
                        else
                        {
                            OnLanePressed?.Invoke(finalLaneIndex);
                        }
                    },

                canceled = ctx =>
                    {
                        var device = ctx.control.device;

                        if (device == guitarDevice)
                            OnLaneReleasedGuitar?.Invoke(finalLaneIndex);
                        else
                            OnLaneReleased?.Invoke(finalLaneIndex);
                    }
            };

            action.performed += binding.performed;
            action.canceled += binding.canceled;

            laneActions[finalLaneIndex] = binding;

            i++;
        }

        //Debug.Log($"ControlsManager initialized with {laneActions.Count} lanes.");
    }

    private void InitializeGameplayInputs()
    {
        var guitarAttackAction = gameplayMap.FindAction("GuitarAttack"); //the guitar flicky thing for attacking
        if (guitarAttackAction != null) //if it exists
        {
            guitarAttackAction.performed += ctx =>
            {
                var device = ctx.control.device;

                //bind device as guitar (from inputting the attack)
                if (guitarDevice == null && IsGuitar(device))
                {
                    guitarDevice = device;
                    Debug.Log("Added Guitar");
                }

                if (device == guitarDevice)
                {
                    OnGuitarAttackPressed?.Invoke();
                }
            };
        }

        //finds the keybind for "Ultimate" in the new input system
        var ultimateAction = gameplayMap.FindAction("Ultimate");
        if (ultimateAction != null) //if it exists
        {
            //when ult button is pressed, invoke the event "OnUltimatePressed" and any events subscribed to its event flag (like the function "UseUltimate" in UltimateSystem.cs)
            ultimateAction.performed += ctx => OnUltimatePressed?.Invoke(); 
        }

        // Assigning gamePause Action and Subscribing to Event
        gamePauseAction = gameplayMap.FindAction("Pause");
        gamePauseAction.performed += ctx => OnPausePressed?.Invoke();
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
        ResetLanes();
        InitializeLaneInputs();

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
        return laneActions.TryGetValue(laneIndex, out var binding) ? binding.action : null;
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

    private bool IsGuitar(InputDevice device)
    {
        //checks if guitar has shanwan in the description.
        //Probably only works for this guitar, might mess up if there are other shanwan type controllers (could be non-guitar)

        string deviceDescription = device.description.ToString().ToLower();

        bool isGuitar = deviceDescription.Contains("subtype");

        //bool isGuitar = deviceDescription.Contains("shanwan"); //old trashy guitar

        /*

        if (isGuitar)
        {
            Debug.Log("Is Guitar");
        }

        else
        {
            Debug.Log("Not Guitar");
        }

        */

        return isGuitar;
    }

    private void ResetLanes()
    {
        gameplayMap.Disable();

        // KILL ALL LANE ACTIONS!! KILL THE LANE BINDINGS!!! KILL THEM ALL!!!
        foreach (var binding in laneActions.Values)
        {
            binding.action.performed -= binding.performed;
            binding.action.canceled -= binding.canceled;
        }

        laneActions.Clear();

        gameplayMap.Enable();
    }
}
