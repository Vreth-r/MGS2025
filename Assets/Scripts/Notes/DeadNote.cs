using UnityEngine;

public class DeadNote : NoteBase
{

    public override void OnHit(Judgement judgement)
    {
        if (data.resolved)
        {
            return;
        }

        Resolve();
        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);
        ScoreManager.Instance.AddScore(1f, lane.laneIndex, true);
        Destroy(gameObject);
    }

    public override void Miss()
    {
        if (data.resolved)
        {
            return;
        }

        Resolve();
        UltimateSystem.Instance.IncrementUltimate(Judgement.Perfect);
        Health.Regen(regenIncrement);
        ScoreManager.Instance.AddScore(0f, lane.laneIndex);
        Destroy(gameObject);
    }
}