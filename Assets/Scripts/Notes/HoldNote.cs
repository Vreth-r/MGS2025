using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// A hold note is held down! I will make a csv example here, later i if i remember
/// </summary>
public class HoldNote : NoteBase
{
    private bool isHolding = false; // there are ways to do this throught the input system that i am choosing not to do
    private bool hasStarted = false;

    private float holdTimer = 0f;
    private float totalTime; // the amount of time of the note in seconds
    private float noteLength; // the physical length of the note (dependant on speed)

    private Transform head; // reference to the head gameObject of the hold note
    private Transform tail; // reference to the tail gameObject of the hold note
    private LineRenderer lineRenderer; // reference to the line renderer

    private void Start()
    {
        head = transform.Find("Head");
        tail = transform.Find("Tail");
        lineRenderer = transform.Find("Body").gameObject.GetComponent<LineRenderer>();

        totalTime = data.parameters["endTime"] - data.time;
        noteLength = totalTime * speed;

        tail.position = head.position + Vector3.left * noteLength; // sets the tails position

        // sets the line from the head to tail
        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);
    }

    public override void OnKeyPressed()
    {
        if (!hasStarted && IsInHitZone(lane.Zone))
        {
            isHolding = true;
            hasStarted = true;
        }
    }

    protected override void Update()
    {
        base.Update();
        // *** DO NOT *** unprotect base.Update(). FOR ANY REASON! UNLESS CLEARED WITH ME!

        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        if (hasStarted && isHolding)
        {
            if (InputManager.Instance.IsLaneHeld(lane.Index))
            {
                holdTimer += Time.deltaTime;

                transform.position += Vector3.right * speed * Time.deltaTime; // when holding, stop the note from moving
                tail.position -= Vector3.right * speed * Time.deltaTime; // keep the tail moving closer so the note "shrinks"

                if (holdTimer >= (data.parameters["endTime"] - data.time))
                {
                    Destroy(gameObject);
                    // successful note completion (might want to add something to NoteBase for this)
                }
            }

        }
        else if (hasStarted && !isHolding)
        {
            Destroy(gameObject);
            // hold released early, do any penalties before the destroy statement (same for success)
        }
    }
}
