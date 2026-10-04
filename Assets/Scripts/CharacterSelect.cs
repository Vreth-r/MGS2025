using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : BaseMenu
{
    // Keeps track of which character is selected
    // If true, P1 is set to Mona and P2 is set to Lu
    private bool _isControlsSwapped = false;

    // Sprite References
    [Header("Game Object References")]
    [SerializeField] private GameObject _p1Cursor;
    [SerializeField] private GameObject _p2Cursor;
    [SerializeField] private GameObject _monaSprite;
    [SerializeField] private GameObject _luSprite;

    public GameSettings _gameSettings;

    public EventReference _characterSelectConfirmedSound;

    protected override void Awake()
    {
        base.Awake();

        Open();
    }

    private void Start()
    {
        InputManager.Instance.EnableUI();
    }

    public override void HandleNavigate(Vector2 _)
    {
        _isControlsSwapped = !_isControlsSwapped;

        Debug.Log("Character's Swapped!");
    }

    public override void HandleSubmit()
    {
        _gameSettings.isControlsSwapped = _isControlsSwapped;
        Debug.Log(_gameSettings.isControlsSwapped);

        RuntimeManager.PlayOneShot(_characterSelectConfirmedSound);

        StartCoroutine(Transition());

        SceneManager.LoadScene("Game");
    }

    public override void HandleCancel() {}

    /*
     * TODO: Add a nice transition
     */
    private IEnumerator Transition()
    {
        yield return new WaitForSeconds(0.1f);
    }
}
