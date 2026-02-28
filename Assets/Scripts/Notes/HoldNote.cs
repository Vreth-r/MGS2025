using System;
using UnityEngine;

public class HoldNote : NoteBase
{
    private enum HoldState { Waiting, Holding, Failing, Done }
    private HoldState state = HoldState.Waiting;

    private float heldTime = 0f;
    private float timeRemaining;
    private float totalTime;
    private float noteLength;

    private Transform head;
    private Transform tail;
    private LineRenderer lineRenderer;

    [SerializeField] private float inspectorDamageIncrement = 2f;
    [SerializeField] private float inspectorRegenIncrement = 2f;

    private GameManager gameManager;

    private void Start()
    {
        head = transform.Find("Head");
        tail = transform.Find("Tail");
        lineRenderer = transform.Find("Body").GetComponent<LineRenderer>();

        totalTime = data.parameters["endTime"] - data.time;
        totalTime = Mathf.Max(0.01f, totalTime);

        noteLength = totalTime * speed;
        timeRemaining = totalTime;

        tail.position = head.position + Vector3.left * noteLength;
        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        damageIncrement = inspectorDamageIncrement;
        regenIncrement = inspectorRegenIncrement;

        gameManager = GameManager.Instance;
        gameManager.OnPulse += OnPulse;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnPulse -= OnPulse;
    }

    public override void OnHit(Judgement judgement)
    {
        // This is the "press to start hold"
        if (state != HoldState.Waiting) return;

        state = HoldState.Holding;

        // Snap head to hitzone
        float delta = lane.hitZone.position.x - transform.position.x;
        transform.position += Vector3.right * delta;
        tail.position -= Vector3.right * delta;

        // adjust remaining time based on how much length cut
        float remainingLength = noteLength - (tail.position.x - head.position.x);
        timeRemaining -= remainingLength / Mathf.Max(0.0001f, speed);
        timeRemaining = Mathf.Max(0f, timeRemaining);
    }

    private void OnPulse()
    {
        if (state == HoldState.Holding && InputManager.Instance.IsLaneHeld(lane.laneIndex))
        {
            Health.Regen(regenIncrement);
        }
    }

    public override void Miss()
    {
        if (state == HoldState.Done || state == HoldState.Failing) return;

        // If you never started holding and it hits killzone is one-time fail
        FailAndStartShrink(singlePenalty: damageIncrement);
    }

    protected override void Update()
    {
        base.Update();

        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        if (state == HoldState.Holding)
        {
            // while held shrink over time
            bool held = InputManager.Instance.IsLaneHeld(lane.laneIndex);

            if (held)
            {
                timeRemaining -= Time.deltaTime;
                heldTime += Time.deltaTime;

                transform.position += Vector3.right * speed * Time.deltaTime;
                tail.position -= Vector3.right * speed * Time.deltaTime;

                if (timeRemaining <= 0f)
                {
                    // success
                    state = HoldState.Done;
                    ResolveNote();
                    UltimateSystem.IncrementUltimate();
                    Destroy(gameObject);
                }
            }
            else
            {
                // released early is a single remaining penalty, then fail-shrink
                float remainingPulses = Mathf.Ceil(timeRemaining / gameManager.secondsPerBeat);
                float penalty = damageIncrement * (remainingPulses + 1f);

                FailAndStartShrink(singlePenalty: penalty);
            }
        }
        else if (state == HoldState.Failing)
        {
            // visual shrink until tail meets head
            transform.position += Vector3.right * speed * Time.deltaTime;
            tail.position -= Vector3.right * speed * Time.deltaTime;

            if (tail.position.x <= head.position.x)
            {
                state = HoldState.Done;
                ResolveNote();
                Destroy(gameObject);
            }
        }
    }

    private void FailAndStartShrink(float singlePenalty)
    {
        state = HoldState.Failing;

        // one-time fail feedback and damage
        AnimationManager.Missed(lane);
        Health.TakeDamage(singlePenalty);
        ScoreManager.Instance.AddScore(1f);

        // IMPORTANT: stop pulse interaction immediately to prevent spam
        if (gameManager != null)
            gameManager.OnPulse -= OnPulse;
    }
}