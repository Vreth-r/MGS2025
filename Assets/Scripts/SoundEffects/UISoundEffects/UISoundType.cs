using UnityEngine;

//if a new type of sound is needed, add its name here. It will appear in the dropdown for sound types. Update the file UISoundPlayer.cs with the functionality if a new type is added
//pretty obvious they do
//Load is when the object loads
//Music is for looping music on load (so dont use Load for looping music, use Music)

public enum UISoundType
{
    Hover,
    Click,
    Load,
    Music,
}
