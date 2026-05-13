using UnityEngine;

public class TapNote : NoteBase
{
    public override void OnHit(Judgement judgement)
    {
        Resolve();
        UltimateSystem.Instance.IncrementUltimate(judgement);
        Health.Regen(regenIncrement);

        float timing = TimingSeconds(lane.hitZone);
        ScoreManager.Instance.AddScore(timing, lane.laneIndex);

        if (judgement != Judgement.Miss)
        {
            SoundEffectsEventHelper.OnSuccessfulHit?.Invoke(lane.laneIndex, judgement);
        }

        movement = false;
        StartCoroutine(animPause(0.3f, "zombie die"));
    }
}