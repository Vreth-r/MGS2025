using System;
using System.Collections.Generic;
using UnityEngine;

public enum EditorNoteType { Tap, Hold, Dead }

[Serializable]
public class EditorNote
{
    public EditorNoteType type;
    public int lane;
    public float time;      
    public float endTime;   
}