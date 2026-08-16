using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;

public class CharacterSelect : MonoBehaviour
{
    // Keeps track of which character is selected
    // If true, P1 is set to Mona and P2 is set to Lu
    private bool _isControlsSwapped = false;

    // USS Class Names for changing P1/P2 Cursor positions
    private const string LeftCharacterSelectClassName = "leftCharacterSelected";
    private const string RightCharacterSelectClassName = "rightCharacterSelected";

    // UXML Elements
    private VisualElement _root;
    private VisualElement _p1Cursor;
    private VisualElement _p2Cursor;

    public GameSettings gameSettings;

    public EventReference characterSelectConfirmedSound;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _p1Cursor = _root.Q<VisualElement>("P1Selector");
        _p2Cursor = _root.Q<VisualElement>("P2Selector");
    }

    private void Start()
    {
        InputManager.Instance.EnableUI();
    }

    private void OnEnable()
    {
        InputManager.Instance.OnSubmit += ConfirmCharacters;
        //InputManager.Instance.OnNavigate += SwapCharacters;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnSubmit -= ConfirmCharacters;
        //InputManager.Instance.OnNavigate -= SwapCharacters;
    }

    private void SwapCharacters(Vector2 _)
    {
        if (_isControlsSwapped)
        {
            _p1Cursor.RemoveFromClassList(LeftCharacterSelectClassName);
            _p2Cursor.RemoveFromClassList(RightCharacterSelectClassName);

            _p1Cursor.AddToClassList(RightCharacterSelectClassName);
            _p2Cursor.AddToClassList(LeftCharacterSelectClassName);
        }
        else
        {
            _p1Cursor.AddToClassList(LeftCharacterSelectClassName);
            _p2Cursor.AddToClassList(RightCharacterSelectClassName);

            _p1Cursor.RemoveFromClassList(RightCharacterSelectClassName);
            _p2Cursor.RemoveFromClassList(LeftCharacterSelectClassName);
        }

        _isControlsSwapped = !_isControlsSwapped;
    }

    private void ConfirmCharacters()
    {
        gameSettings.isControlsSwapped = _isControlsSwapped;
        Debug.Log(gameSettings.isControlsSwapped);

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
