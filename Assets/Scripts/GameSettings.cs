using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "MGS2025/GameSettings")]
public class GameSettings : ScriptableObject
{
    public bool isControlsSwapped = false;
    public bool autoUltimate = false;
    public bool godMode = false;
    public bool useFleshSoundVersions = true;
    public int gameMode = 0;
}
