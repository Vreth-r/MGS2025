using UnityEngine;

public class TapNote : NoteBase
{
    public override void OnHit(Judgement judgement)
    {
        Resolve();
        UltimateSystem.AddChargeForSuccessfulNote();
        Health.Regen(regenIncrement);
        Destroy(gameObject);
    }
}