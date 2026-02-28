using UnityEngine;

public class DeadNote : NoteBase
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
        AnimationManager.Missed(this.lane);
        Health.TakeDamage(damageIncrement); // take damage when the note is hit
        Destroy(gameObject);
    }

    public override void Miss()
    {
        Health.Regen(); // regen health or add score when missed
        UltimateSystem.IncrementUltimate(); //increments the ults progression bar
        ResolveNote(); // resolve the note
        Health.Regen(regenIncrement); // regen health or add score when missed
        Destroy(gameObject);
    }
}
