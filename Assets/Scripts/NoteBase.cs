using UnityEngine;

public enum Judgement { Perfect, Awesome, Good, Okay, Miss }

public abstract class NoteBase : MonoBehaviour
{
    protected LaneController lane;
    protected BeatmapData.NoteData data;
    protected float speed;

    [Header("Tuning")]
    [SerializeField] protected float damageIncrement = 10f;
    [SerializeField] protected float regenIncrement = 10f;

    // timing windows (seconds)
    protected const float PERFECT = 0.10f;
    protected const float AWESOME = 0.20f;
    protected const float GOOD    = 0.30f;
    protected const float OKAY    = 0.40f;

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

    public float TimingSeconds(Transform hitZone)
        => Mathf.Abs(transform.position.x - hitZone.position.x) / Mathf.Max(0.0001f, speed);

    public Judgement GetJudgement(Transform hitZone)
    {
        float t = TimingSeconds(hitZone);
        if (t < PERFECT) return Judgement.Perfect;
        if (t < AWESOME) return Judgement.Awesome;
        if (t < GOOD)    return Judgement.Good;
        if (t < OKAY)    return Judgement.Okay;
        return Judgement.Miss;
    }

    public bool CanBeHit(Transform hitZone) => GetJudgement(hitZone) != Judgement.Miss;

    public abstract void OnHit(Judgement judgement);

    public virtual void Miss()
    {
        Resolve();
        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);
        ScoreManager.Instance.AddScore(1f); // miss
        Destroy(gameObject);
    }

    protected void Resolve()
    {
        if (data != null) data.resolved = true;
    }
}