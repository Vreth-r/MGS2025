using UnityEngine;

public class DeadNote : NoteBase
{
    [SerializeField] private float inspectorDamageIncrement = 10f;

    private void Start()
    {
        damageIncrement = inspectorDamageIncrement;
        regenIncrement = 0f; // trap note: no reward ever
    }

    // Player HIT the trap note (bad)
    public override void OnHit(Judgement judgement)
    {
        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);

        // Count it like a miss in your current scoring system
        ScoreManager.Instance.AddScore(1f);

        ResolveNote();
        Destroy(gameObject);
    }

    // Player AVOIDED it (good) -> neutral outcome
    public override void Miss()
    {
        // No health gain, no ultimate, no score change.
        // Just mark it resolved and remove it.
        ResolveNote();
        Destroy(gameObject);
    }
}