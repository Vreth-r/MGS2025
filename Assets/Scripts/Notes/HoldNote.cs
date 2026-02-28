using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// A hold note is held down! I will make a csv example here, later i if i remember
/// </summary>
public class HoldNote : NoteBase
{
    private bool isMissing = false;
    private bool hasStarted = false;

    private float heldTime = 0f;
    private float timeRemaining;
    private float totalTime; // the amount of time of the note in seconds
    private float noteLength; // the physical length of the note (dependant on speed)

    private Transform head; // reference to the head gameObject of the hold note
    private Transform tail; // reference to the tail gameObject of the hold note
    private LineRenderer lineRenderer; // reference to the line renderer

    private Transform hitzone; // not hit coords (GM)

    // These increments are applied every beat
    [SerializeField] private float inspectorDamageIncrement = 2f;
    [SerializeField] private float inspectorRegenIncrement = 2f;

    private void Start()
    {
        head = transform.Find("Head");
        tail = transform.Find("Tail");
        lineRenderer = transform.Find("Body").gameObject.GetComponent<LineRenderer>();

        totalTime = data.parameters["endTime"] - data.time;
        noteLength = totalTime * speed;
        timeRemaining = totalTime;

        tail.position = head.position + Vector3.left * noteLength; // sets the tails position

        // sets the line from the head to tail
        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        // set damage and regen increments
        damageIncrement = inspectorDamageIncrement;
        regenIncrement = inspectorRegenIncrement;

        // Subscribe to pulse event
        var gameManager = GameManager.Instance;
        gameManager.OnPulse += onPulse;

        // Set the hitzone
        hitzone = gameManager.hitZone;
    }

    public override bool IsInHitZone(Transform hitZone)
    {
        return base.IsInHitZone(hitZone) || transform.position.x <= hitZone.position.x; // if timing smaller than largest timing window OR head has gone past (you wont get score for this)
    }

    public override void OnKeyPressed()
    {
        if (!hasStarted && IsInHitZone(lane.hitZone))
        {
            hasStarted = true;
            isMissing = false;

            // snap the head position to the hitzone
            float deltaLength = this.lane.hitZone.position.x - transform.position.x;

            transform.position += Vector3.right * deltaLength;
            tail.position -= Vector3.right * deltaLength;

            float remainingLength = noteLength - (tail.position.x - head.position.x);
            timeRemaining -= remainingLength / speed;
        }
    }

    private void onPulse()
    {
        if (isMissing)
        {
            AnimationManager.Missed(this.lane);
            Health.TakeDamage(damageIncrement);
        }
        if(hasStarted && InputManager.Instance.IsLaneHeld(lane.laneIndex))
        {
            Health.Regen(regenIncrement);
        }
    }

    public override void Miss()
    {
        if (isMissing)
        {
            return;
        }
        isMissing = true;
    }

    protected override void Update()
    {
        base.Update();
        // *** DO NOT *** unprotect base.Update(). FOR ANY REASON! UNLESS CLEARED WITH ME!

        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        if (isMissing)
        {
            transform.position += Vector3.right * speed * Time.deltaTime; // stop the note from moving
            tail.position -= Vector3.right * speed * Time.deltaTime; // keep the tail moving closer so the note "shrinks"

            if (tail.position.x <= head.position.x)
                {
                    // unsubscribe
                    var gameManager = GameManager.Instance;
                    gameManager.OnPulse -= onPulse;

                    base.Miss();
                }
        }

        if (hasStarted && InputManager.Instance.IsLaneHeld(lane.laneIndex))
        {
            timeRemaining -= Time.deltaTime;
            heldTime += Time.deltaTime; // used for calculating if the player held down the entire note

            transform.position += Vector3.right * speed * Time.deltaTime; // when holding, stop the note from moving
            tail.position -= Vector3.right * speed * Time.deltaTime; // keep the tail moving closer so the note "shrinks"

                if (holdTimer >= (data.parameters["endTime"] - data.time))
                {
                    Destroy(gameObject);
                    UltimateSystem.IncrementUltimate(); //increments the ults progression bar
                    // successful note completion (might want to add something to NoteBase for this)
                }
            if (timeRemaining <= 0)
            {
                // unsubscribe
                var gameManager = GameManager.Instance;
                gameManager.OnPulse -= onPulse;

                Destroy(gameObject);
                // successful note completion (might want to add something to NoteBase for this)
            }

        }
        else if (hasStarted && !InputManager.Instance.IsLaneHeld(lane.laneIndex))
        {
            // unsubscribe
            var gameManager = GameManager.Instance;
            gameManager.OnPulse -= onPulse;

            // Calculate remaining damage
            float remainingPulses = (float) Math.Round((timeRemaining) / (gameManager.secondsPerBeat)); // oh man.
            damageIncrement *= (remainingPulses + 1);

            base.Miss();
            // hold released early, do any penalties before the destroy statement (same for success)
        }
    }
}