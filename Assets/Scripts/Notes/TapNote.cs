using UnityEngine;

public class TapNote : NoteBase
{
    [SerializeField] private float inspectorDamageIncrement = 10f;
    [SerializeField] private float inspectorRegenIncrement = 10f;

    private void Start()
    {
        damageIncrement = inspectorDamageIncrement;
        regenIncrement = inspectorRegenIncrement;
    }

    public override void OnHit(Judgement judgement)
    {
        // you can scale rewards by judgement later if you want
        UltimateSystem.IncrementUltimate();
        Health.Regen(regenIncrement);

        Destroy(gameObject);
    }
}