using UnityEngine;

public class HoldNote : NoteBase
{
    private enum State { Waiting, Holding, Failing, Done }
    private State state = State.Waiting;

    private Transform head;
    private Transform tail;
    private LineRenderer body;

    private float totalTime;
    private float timeRemaining;
    private float noteLength;

    private GameManager gm;

    private void Start()
    {
        head = transform.Find("Head");
        tail = transform.Find("Tail");
        body = transform.Find("Body").GetComponent<LineRenderer>();

        totalTime = Mathf.Max(0.01f, data.parameters["endTime"] - data.time);
        timeRemaining = totalTime;
        noteLength = totalTime * speed;

        tail.position = head.position + Vector3.left * noteLength;
        body.SetPosition(0, head.position);
        body.SetPosition(1, tail.position);

        gm = GameManager.Instance;
        if (gm != null) gm.OnPulse += OnPulse;
    }

    private void OnDestroy()
    {
        if (gm != null) gm.OnPulse -= OnPulse;
    }

    public override void OnHit(Judgement judgement)
    {
        if (state != State.Waiting) return;
        state = State.Holding;

        
        float dx = lane.hitZone.position.x - transform.position.x;
        transform.position += Vector3.right * dx;
        tail.position -= Vector3.right * dx;
    }

    private void OnPulse()
    {
        if (state != State.Holding) return;

        if (InputManager.Instance.IsLaneHeld(lane.laneIndex))
        {
            Health.Regen(regenIncrement);
            ScoreManager.Instance.AddBonus(1);
        }
    }

    public override void Miss()
    {
        if (state == State.Done || state == State.Failing) return;
        FailOnce(damageIncrement);
    }

    protected override void Update()
    {
        base.Update();

        body.SetPosition(0, head.position);
        body.SetPosition(1, tail.position);

        if (state == State.Holding)
        {
            bool held = InputManager.Instance.IsLaneHeld(lane.laneIndex);

            if (!held)
            {
                
                float beatsLeft = (gm != null && gm.secondsPerBeat > 0f)
                    ? Mathf.Ceil(timeRemaining / gm.secondsPerBeat)
                    : Mathf.Ceil(timeRemaining / 0.5f);

                FailOnce(damageIncrement * (beatsLeft + 1f));
                return;
            }

            timeRemaining -= Time.deltaTime;

            
            transform.position += Vector3.right * speed * Time.deltaTime;
            tail.position -= Vector3.right * speed * Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                state = State.Done;
                Resolve();
                //UltimateSystem.Instance.IncrementUltimate(judgement);
                Destroy(gameObject);
            }
        }
        else if (state == State.Failing)
        {
            
            transform.position += Vector3.right * speed * Time.deltaTime;
            tail.position -= Vector3.right * speed * Time.deltaTime;

            if (tail.position.x <= head.position.x)
            {
                state = State.Done;
                Resolve();
                Destroy(gameObject);
            }
        }
    }

    private void FailOnce(float damage)
    {
        state = State.Failing;

        Resolve();
        AnimationManager.Missed(lane);
        Health.TakeDamage(damage);
        ScoreManager.Instance.AddScore(1f, lane.laneIndex);

        
        if (gm != null) gm.OnPulse -= OnPulse;
    }
}