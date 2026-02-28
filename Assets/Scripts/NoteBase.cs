using UnityEngine;

public enum Judgement
{
    Perfect,
    Awesome,
    Good,
    Okay,
    Miss
}

public abstract class NoteBase : MonoBehaviour
{
    protected float speed;
    protected LaneController lane;
    protected BeatmapData.NoteData data;

    protected float damageIncrement;
    protected float regenIncrement;

    // windows in seconds (match ScoreManager thresholds)
    protected const float PERFECT = 0.1f;
    protected const float AWESOME = 0.2f;
    protected const float GOOD = 0.3f;
    protected const float OKAY = 0.4f;

    public virtual void Initialize(LaneController lane, float speed, BeatmapData.NoteData data)
    {
        this.lane = lane;
        this.speed = speed;
        this.data = data;
    }

    protected virtual void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }

    public float GetTimingSeconds(Transform hitZone)
    {
        return Mathf.Abs(transform.position.x - hitZone.position.x) / Mathf.Max(0.0001f, speed);
    }

    public Judgement GetJudgement(Transform hitZone)
    {
        float t = GetTimingSeconds(hitZone);
        if (t < PERFECT) return Judgement.Perfect;
        if (t < AWESOME) return Judgement.Awesome;
        if (t < GOOD) return Judgement.Good;
        if (t < OKAY) return Judgement.Okay;
        return Judgement.Miss;
    }

    public virtual bool CanBeHit(Transform hitZone)
        => GetJudgement(hitZone) != Judgement.Miss;

    // Called by lane when a hit is confirmed
    public abstract void OnHit(Judgement judgement);

    public virtual void Miss()
    {
        if (data != null) data.resolved = true;

        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);
        ScoreManager.Instance.AddScore(1f); // treat as miss in scoring system

        Destroy(gameObject);
    }

    public void ResolveNote()
    {
        if (data != null) data.resolved = true;
    }
}