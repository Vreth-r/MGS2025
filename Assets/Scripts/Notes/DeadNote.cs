using UnityEngine;

public class DeadNote : NoteBase
{
    
    public override void OnHit(Judgement judgement)
    {
        Resolve();
        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);
        ScoreManager.Instance.AddScore(1f, lane.laneIndex);
        Destroy(gameObject);
    }

    public override void Miss()
    {
        Resolve();
        Destroy(gameObject);
    }
}