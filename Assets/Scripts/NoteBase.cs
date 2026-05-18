using UnityEngine;
using System.Collections;



public abstract class NoteBase : MonoBehaviour
{
    protected LaneController lane;
    protected BeatmapData.NoteData data;
    protected float speed;
    protected bool movement;

    private Animator animator;

    [Header("Tuning")]
    [SerializeField] protected float damageIncrement = 15f;
    [SerializeField] protected float regenIncrement = 5f;

    // timing windows (seconds)
    protected const float PERFECT = 0.05f;
    protected const float AWESOME = 0.10f;
    protected const float GOOD    = 0.15f;
    protected const float OKAY    = 0.20f;

    public virtual void Initialize(LaneController lane, float speed, BeatmapData.NoteData data)
    {
        this.lane = lane;
        this.speed = speed;
        this.data = data;
        this.movement = true;
    }

    protected virtual void Update()
    {
        if (movement)
            transform.position += Vector3.left * (speed * Time.deltaTime);
    }

    public float TimingSeconds(Transform hitZone)
        => Mathf.Abs(transform.position.x - hitZone.position.x) / Mathf.Max(0.0001f, speed);

    public Judgement GetJudgement(Transform hitZone)
    {
        float t = TimingSeconds(hitZone);
        if (t < PERFECT) return Judgement.Perfect;
        if (t < AWESOME) return Judgement.Awesome;
        if (t < GOOD)    return Judgement.Good;
        if (t < OKAY) return Judgement.Ok;
        return Judgement.Miss;
    }

    public bool CanBeHit(Transform hitZone)    
    {
        return (GetJudgement(hitZone) != Judgement.Miss) && data.resolved == false;
    }

    public abstract void OnHit(Judgement judgement);

    public virtual void Miss()
    {
        Resolve();
        AnimationManager.Missed(lane);
        Health.TakeDamage(damageIncrement);
        ScoreManager.Instance.AddScore(1f, lane.laneIndex, true); // miss
        StartCoroutine(animPause(0.4f,"zombie attack"));
    }

    protected void Resolve()
    {
        if (data != null) data.resolved = true;
    }

    protected IEnumerator animPause(float waitTime, string type)
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        animator.Play(type, -1, 0f);
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}