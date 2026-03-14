using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelect : MonoBehaviour
{
    // Keeps track of which character is selected
    public bool P1IsLu { get; private set; } = true;

    // USS Class Names for changing p1/p2 selector positions
    private const string LeftCharacterSelectClassName = "leftCharacterSelected";
    private const string RightCharacterSelectClassName = "rightCharacterSelected";

    // Action Map Names
    private const string UiSelectNamae = "Navigate";
    private const string UiConfirmName = "Submit";

    private const float SpinSpeed = 100f;

    // Components
    private PlayerInput _playerInput;

    // UXML Elements
    private VisualElement _root;
    private VisualElement _p1Selector;
    private VisualElement _p2Selector;
    private VisualElement _spinny;

    // Handles rotation for the Spinnythingy
    private float rotate = 0;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _root = GetComponent<UIDocument>().rootVisualElement;

        _p1Selector = _root.Q<VisualElement>("P1Selector");
        _p2Selector = _root.Q<VisualElement>("P2Selector");
        _spinny = _root.Q<VisualElement>("Spinny");
    }

    private void Update()
    {
        _spinny.style.rotate = new(new Rotate(rotate));
        rotate += SpinSpeed * Time.deltaTime;
    }

    private void OnEnable()
    {
        _playerInput.actions[UiSelectNamae].started += SwapCharacters;
        _playerInput.actions[UiConfirmName].started += ConfirmCharacters;
    }

    private void OnDisable()
    {
        _playerInput.actions[UiSelectNamae].started -= SwapCharacters;
        _playerInput.actions[UiConfirmName].started -= ConfirmCharacters;
    }

    private void SwapCharacters(InputAction.CallbackContext ctx)
    {
        //Debug.Log("P1: " + (_p1IsLu ? "Lu, P2: Mona" : "Mona, P2: Lu"));

        if (P1IsLu)
        {
            _p1Selector.RemoveFromClassList(LeftCharacterSelectClassName);
            _p2Selector.RemoveFromClassList(RightCharacterSelectClassName);

            _p1Selector.AddToClassList(RightCharacterSelectClassName);
            _p2Selector.AddToClassList(LeftCharacterSelectClassName);
        }
        else
        {
            _p1Selector.AddToClassList(LeftCharacterSelectClassName);
            _p2Selector.AddToClassList(RightCharacterSelectClassName);

            _p1Selector.RemoveFromClassList(RightCharacterSelectClassName);
            _p2Selector.RemoveFromClassList(LeftCharacterSelectClassName);
        }

        P1IsLu = !P1IsLu;
    }

    private void ConfirmCharacters(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Character Selected!");

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
