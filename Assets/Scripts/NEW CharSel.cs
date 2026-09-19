using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NEWCharSel : MonoBehaviour
{


    public EventReference characterSelectConfirmedSound;
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
    private void ConfirmCharacters()
    {
       // gameSettings.isControlsSwapped = _isControlsSwapped;
        //Debug.Log(gameSettings.isControlsSwapped);

        //RuntimeManager.PlayOneShot(characterSelectConfirmedSound);


        SceneManager.LoadScene("Game");
    }
}
