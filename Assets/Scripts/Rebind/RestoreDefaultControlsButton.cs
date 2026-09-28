using UnityEngine;
using UnityEngine.InputSystem;

public class RestoreDefaultControlsButton : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAsset; //the entire input action file
    [SerializeField] private RebindButtonGenerator rebindButtonGenerator;

    public void RestoreDefaultControls()
    {
        PlayerPrefs.DeleteKey("NewKeybinds");
        PlayerPrefs.Save();
        inputAsset.RemoveAllBindingOverrides();

        rebindButtonGenerator.UpdateButtons();
    }
}
