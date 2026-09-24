using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour
{
    public InputActionReference actionToRebind; //a reference to the input action that will be changed (ex - ultimate)

    public int bindIndex; //grabbed from RebindControls.cs -> so the displayed button text shows the correct keybind associated to the action

    bool isKeybinding = false; //safeguard so your not constantly in "change keybind mode" and blow up the scene

    public void Rebinder()
    {
        if (isKeybinding == false)
        {
            isKeybinding = true;

            //unity built in rebinding functions.
            //disables the chosen action so we can change it,
            //then when a new keybind is chosen, reenable and update the buttons text to show the updated keybind
            //then save the new rebinds
            //also can cancel out of it
            actionToRebind.action.Disable();
            actionToRebind.action.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(operation => { operation.Dispose(); actionToRebind.action.Enable(); isKeybinding = false; gameObject.GetComponentInChildren<TMP_Text>().text = actionToRebind.action.GetBindingDisplayString(bindIndex); SaveKeybinds(); })
                .OnCancel(operation => { operation.Dispose(); actionToRebind.action.Enable(); isKeybinding = false; gameObject.GetComponentInChildren<TMP_Text>().text = actionToRebind.action.GetBindingDisplayString(bindIndex); })
                .Start();
        }
    }

    //saves the keybinds to a json (built in unity func for that save).
    //this does not actually directly modify the InputActionMap
    //its its own thing that we load instead of the defaults if the rebinds exist
    public void SaveKeybinds()
    {
        PlayerPrefs.SetString("NewKeybinds", actionToRebind.action.actionMap.asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }
}
