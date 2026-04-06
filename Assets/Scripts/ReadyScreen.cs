using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;

public class ReadyScreen : MonoBehaviour
{
    // Keeps track of which character is selected
    public bool P1IsLu { get; private set; } = true;

    // USS Class Names for changing p1/p2 selector positions
    private const string LeftCharacterSelectClassName = "leftCharacterSelected";
    private const string RightCharacterSelectClassName = "rightCharacterSelected";

    // Action Map Names
    private const string UiSelectNamae = "Navigate";
    private const string UiConfirmName = "Submit";

    // Components
    private PlayerInput _playerInput;

    // UXML Elements
    private VisualElement _root;
    private VisualElement _p1Selector;
    private VisualElement _p2Selector;

    // Handles rotation for the Spinnythingy
    private float rotate = 0;

    public GameSettings gameSettings;

    public EventReference characterSelectConfirmedSound;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _root = GetComponent<UIDocument>().rootVisualElement;

        _p1Selector = _root.Q<VisualElement>("P1Selector");
        _p2Selector = _root.Q<VisualElement>("P2Selector");

    }

    private void OnEnable()
    {
        _playerInput.actions[UiConfirmName].started += ConfirmCharacters;
    }

    private void OnDisable()
    {
        _playerInput.actions[UiConfirmName].started -= ConfirmCharacters;
    }

    private void ConfirmCharacters(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Character Selected!");

        gameSettings.isControlsSwapped = false;

        RuntimeManager.PlayOneShot(characterSelectConfirmedSound);

        StartCoroutine(Transition());

        SceneManager.LoadScene("Game");
    }

    /*
     * TODO: Add a nice transition
     */
    private IEnumerator Transition()
    {
        yield return new WaitForSeconds(0.1f);
    }
}
