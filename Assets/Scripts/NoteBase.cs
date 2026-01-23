using UnityEngine;

/// <summary>
/// A NoteBase is the abstraction of basic functionality of a hittable object that spawns at a given time in a lane.
/// If you need an explanation on abstraction, you are far better off googling it, however I will answer any questions.
/// </summary>
public abstract class NoteBase : MonoBehaviour
{
    protected float speed; // the speed at which the note moves (assigned on instantiation)
    protected LanePrefabController lane; // the lane it's assigned (assigned on instantiation)
    protected BeatmapData.NoteData data; // any other args to be passed to children of the appropriate type (instantiation)


    public bool testMove = true;
    /// <summary>
    /// Initialize(LaneController, float) is suprisingly not a Mono method and is literally a workaround because
    /// Mono's can't have constructors. This is called in LaneController on note instantiation.
    /// </summary>
    /// <param name="lane">The lane the note belongs to.</param>
    /// <param name="speed">The speed the note moves at down the lane.</param>
    public virtual void Initialize(LanePrefabController lane, float speed, BeatmapData.NoteData data)
    {
        this.lane = lane; // setters
        this.speed = speed;
        this.data = data;
    }

    /// <summary>
    /// Update() is called every frame. 
    /// </summary>
    protected virtual void Update()
    {
        if (testMove)
            lane.Scroll(LanePrefabController.Side.Left);
        //transform.position += Vector3.left * speed * Time.deltaTime; // translate its ass down the lane.
    }

    /// <summary>
    /// IsInHitZone(Transform) determines whether the note is within the confines of a hitzone.
    /// </summary>
    /// <param name="hitZone">The hitzone the note will possibly be in.</param>
    /// <returns>Boolean whether the note is in the hitzone or not.</returns>
    public virtual bool IsInHitZone(Transform hitZone)
    {
        var timing = Mathf.Abs(transform.position.x - hitZone.position.x);// / speed; // actual timing
        ScoreManager.Instance.AddScore(timing);
        return timing < 0.4f; // if timing smaller than largest timing window
    }

    public virtual void Miss()
    {
        data.resolved = true;
        AnimationManager.Missed(lane);
        Health.TakeDamage();
        Destroy(gameObject);
    }
    public abstract void OnKeyPressed();

    // Used for when note is hit
    public void ResolveNote()
    {
        data.resolved = true;
    }
}
