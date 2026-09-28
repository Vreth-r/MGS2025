using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour
{
    public InputActionReference actionToRebind; //a reference to the input action that will be changed (ex - ultimate)

    public int bindIndex; //grabbed from RebindControls.cs -> so the displayed button text shows the correct keybind associated to the action

    bool isRebinding = false; //safeguard so your not constantly in "change keybind mode" and blow up the scene

    public GameObject rebindScreenPanel; //a screen that overlays when you clicked to rebind a control (this prevents you from clicking other buttons)
    private GameObject rebindScreenText; //child object of rebindScreenPanel. This is the "You are Rebinding" message that appears on screen
    private GameObject retryRebindText; //child object of rebindScreenPanel. This pops up when you try to rebind to an already existing key (aka chosen control doesnt already have a function it does)

    public void Start()
    {
        //NOTE THESE ARE DEPENDENT ON THE ORDER OF THE TEXT OBJECTS, IF ITS GRABBING IN THE WRONG ORDER, THE HIERARCHY OF THESE OBJECTS MAY HAVE BEEN SWITCHED
        //Location is : Canvas -> Rebind Screen Panel -> Backdrop Image -> Is Rebinding Text, Retry Text
        rebindScreenText = rebindScreenPanel.transform.GetChild(0).GetChild(0).gameObject; //grabs the rebindScreenText child object
        retryRebindText = rebindScreenPanel.transform.GetChild(0).GetChild(1).gameObject; //grabs the retryRebindText child object
    }

    public void Rebinder()
    {
        if (isRebinding == false) 
        {
            isRebinding = true; //notes that we are rebinding, aka state tracking

            //unity built in rebinding functions.
            //disables the chosen action so we can change it,
            //then when a new keybind is chosen, reenable and update the buttons text to show the updated keybind
            //then save the new rebinds
            //also can cancel out of it

            rebindScreenPanel.SetActive(true); //make panel appear to block out clicking other buttons
            rebindScreenText.SetActive(true); //make the currently rebinding text stuff appear (it may be disabled due to someone trying to rebind to an already existing control)
            retryRebindText.SetActive(false); //make the retry rebind due to duplicate rebind text stuff hidden (it may be enabled due to someone trying to rebind to an already existing control)

            actionToRebind.action.Disable();
            StartRebind(); //start rebind process
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

    //function to deal with the actual rebinding
    public void StartRebind()
    {
        actionToRebind.action.PerformInteractiveRebinding(bindIndex)
                .WithControlsExcluding("Mouse") //do not let mouse actions be a rebind option 
                .WithCancelingThrough("<Keyboard>/escape") //do not let escape be a rebind option, this quits the rebinding 
                .OnComplete(operation => {
                    foreach (var action in actionToRebind.action.actionMap.actions) //checking all the actions
                    {
                        foreach (var binding in action.bindings) //checking all the actual bindings/controls in said actions
                        {
                            if (action == actionToRebind.action && binding == actionToRebind.action.bindings[bindIndex]) //you rebinded your thing to the same thing it already was
                            {
                                continue;
                            }

                            if (operation.selectedControl.path == binding.effectivePath.Replace("<Keyboard>", "/Keyboard")) //a duplicate binded control is found -> stop and warn player, try it again
                            {
                                actionToRebind.action.RemoveBindingOverride(bindIndex);

                                operation.Dispose();

                                rebindScreenText.SetActive(false); //disable "you are rebinding" text
                                retryRebindText.SetActive(true); //enable "you gotta try again" text

                                StartRebind(); //recursively calls itself again (to do the rebind again cause you tried to add a duplicate control)
                                return;
                            }
                        }
                    }

                    operation.Dispose(); 
                    actionToRebind.action.Enable(); 
                    isRebinding = false; //we not rebinding no more, so update the state tracker
                    gameObject.GetComponentInChildren<TMP_Text>().text = actionToRebind.action.GetBindingDisplayString(bindIndex); //update text on the button you rebinded to show your new control
                    rebindScreenPanel.SetActive(false);  //hide the "in progress of rebinding" panel, so you can actually rebind other stuff again
                    SaveKeybinds();
                })
                .OnCancel(operation => { operation.Dispose(); actionToRebind.action.Enable(); isRebinding = false; gameObject.GetComponentInChildren<TMP_Text>().text = actionToRebind.action.GetBindingDisplayString(bindIndex); rebindScreenPanel.SetActive(false); })
                .Start();
    }
}
