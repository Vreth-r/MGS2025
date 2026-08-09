using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableAudioPlayer : MonoBehaviour {
    public AudioSource audioSource;

    [SerializeField] private AssetReference audioClip;

    [SerializeField] private InputAction playAction;

    public AssetReference AudioClip { get => this.audioClip; private set => this.audioClip = value; }

    private void OnEnable() {
        // if not set through inspector, get from gameobject
        if (audioSource == null)
            audioSource = gameObject.GetComponent<AudioSource>();
        // if none on gameobject, create new
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        playAction.Enable();
        playAction.performed += (InputAction.CallbackContext context) => {
            if (audioClip.Asset == null) {
                audioClip.LoadAssetAsync<AudioClip>().Completed +=
                    (asyncOperationHandle) => {
                        if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded) {
                            audioSource.clip = asyncOperationHandle.Result;
                            audioSource.Play();
                        }
                    };
            }
            else
                audioSource.Play();
        };
    }

    // Update is called once per frame
    void Update() {
        //if (playAction..GetKeyDown(KeyCode.Slash)) {

        //} else if (Input.GetKeyDown(KeyCode.Quote)) {
        //    UnloadClip();
        //}
    }

    void UnloadClip() {
        audioSource.clip = null;
    }
}
