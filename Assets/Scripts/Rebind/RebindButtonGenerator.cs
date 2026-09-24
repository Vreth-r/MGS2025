using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindButtonGenerator : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAsset; //the entire input action file
    private InputActionMap gameplayKeybinds; //the specific input action map that we are currently using (from the file above)

    public GameObject textPrefab; //the action textbox (ex - Lane 0 attack, Ultimate, etc)

    public Button buttonPrefab; //the clickable button for the keybinds (ex - keyboard A, Spacebar, etc)
    
    void Start()
    {
        /*
        //this is for when i need to delete the keybinds and reset to default. Basically uncomment this and just open the rebind scene to return to default
        //ill make this a return to default button later
        PlayerPrefs.DeleteKey("NewKeybinds");
        PlayerPrefs.Save();
        inputAsset.RemoveAllBindingOverrides();
        */

        gameplayKeybinds = inputAsset.FindActionMap("Gameplay"); //the specific action map for gameplay

        //if a rebinded control scheme exists, it will be called "NewKeybinds". It will then be loaded to be used instead of the defaults
        if (PlayerPrefs.HasKey("NewKeybinds")) 
        {
            inputAsset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("NewKeybinds"));
        }

        //loop through each action to sift through the specific keybinded controls of each action
        //ex - action = ultimate -> bindings = spacebar, xbox left bumper, guitar whammy
        foreach (var action in gameplayKeybinds.actions)
        {
            for (int i = 0; i < action.bindings.Count; i = i + 1)
            {
                if (action.bindings[i].path.StartsWith("<Keyboard>"))
                {
                    //the action (ex - Lane 0 attack, Ultimate, etc). Sits next to the actual rebind button (basically the text that says what action you are trying to rebind)
                    GameObject actionLabelBox = Instantiate(textPrefab, transform);
                    TMP_Text actionLabelText = actionLabelBox.GetComponentInChildren<Image>().GetComponentInChildren<TMP_Text>(); ;
                    actionLabelText.text = action.name; //for the textbox to show the action name 

                    //the players controls (ex - keyboard key, controller button)
                    Button keybindButton = Instantiate(buttonPrefab, transform);
                    RebindButton rebindButton = keybindButton.GetComponent<RebindButton>();
                    rebindButton.actionToRebind = InputActionReference.Create(action); //reference to the action (you cannot modify it directly, i think)
                    rebindButton.bindIndex = i; //used in RebindButton.cs to display the correct keybind name
                    TMP_Text keybindText = keybindButton.GetComponentInChildren<TMP_Text>();
                    keybindText.text = action.GetBindingDisplayString(i).ToUpper(); //for the textbox to show the keybind name 

                    keybindButton.gameObject.SetActive(true); //make the button actually active
                }
            }
        }
    }
}
