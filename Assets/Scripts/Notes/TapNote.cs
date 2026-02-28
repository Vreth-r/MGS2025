using UnityEngine;
using System;

public class TapNote : NoteBase
{
    [SerializeField] private float inspectorDamageIncrement = 10f;
    [SerializeField] private float inspectorRegenIncrement = 10f;

    private void Start()
    {
        damageIncrement = inspectorDamageIncrement;
        regenIncrement = inspectorRegenIncrement;
    }
    
    public override void OnKeyPressed()
    {
        Health.Regen();
        UltimateSystem.IncrementUltimate(); //increments the ults progression bar
        Health.Regen(inspectorRegenIncrement);
        Destroy(gameObject);
        // add more functionality later, like scoring, etc
    }
}