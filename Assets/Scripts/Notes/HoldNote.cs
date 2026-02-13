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
    private float noteLength; // the physical length of the note (dependant on speed)

    private Transform head; // reference to the head gameObject of the hold note
    private Transform tail; // reference to the tail gameObject of the hold note
    private LineRenderer lineRenderer; // reference to the line renderer

    private void Start()
    {
        head = transform.Find("Head");
        tail = transform.Find("Tail");
        lineRenderer = transform.Find("Body").gameObject.GetComponent<LineRenderer>();

        noteLength = data.parameters["endTime"] - data.time;

        UpdateTail();

        // sets the line from the head to tail
        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);
    }

    public override void OnKeyPressed()
    {
        if (GameManager.Instance != null && !hasStarted && IsInHitZone(lane.specialZone))
        {
            isHolding = true;
            hasStarted = true;
            // Move out of 'Notes' object so its not affected by further changes to lane.Offset
            // It will now be responsible for its own animation / movement
            transform.SetParent(GameManager.Instance.transform, true);
        }
    }

    protected void FixedUpdate()
    {
        // *** DO NOT *** unprotect base.Update(). FOR ANY REASON! UNLESS CLEARED WITH ME!

        lineRenderer.SetPosition(0, head.position);
        lineRenderer.SetPosition(1, tail.position);

        if (hasStarted && isHolding)
        {
            if (InputManager.Instance.IsLaneHeld(lane.laneIndex))
            {
                holdTimer += Time.deltaTime;

                transform.position += Vector3.right * BaseManager.Instance.step; // when holding, stop the note from moving
                tail.position -= Vector3.right * BaseManager.Instance.step; // keep the tail moving closer so the note "shrinks"

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

    public void UpdateTail()
    {
        if (head != null && tail != null)
            tail.position = head.position + BaseManager.Instance.Scale * noteLength * Vector3.left; // sets the tails position
    }
}
