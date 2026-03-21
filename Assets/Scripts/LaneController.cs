using UnityEngine;

public class LaneController : MonoBehaviour
{
    public int laneIndex;
    public Transform spawnPoint;
    public Transform hitZone;
    public float noteSpeed = 5f;

    private void Start()
    {
        InputManager.Instance.OnLanePressed += HandleLanePress;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnLanePressed -= HandleLanePress;
    }

    public void SpawnTypedNote(BeatmapData.NoteData data, string type)
    {
        if (spawnPoint == null || hitZone == null)
        {
            Debug.LogError($"[LaneController] Missing refs on lane {laneIndex}. " +
                        $"spawnPoint={(spawnPoint ? "OK" : "NULL")} hitZone={(hitZone ? "OK" : "NULL")}");
            return;
        }

        if (!GameManager.Instance.notePrefabs.TryGetValue(type, out var prefab) || prefab == null)
            return;

        var obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);

        if (obj.TryGetComponent<NoteBase>(out var note))
            note.Initialize(this, noteSpeed, data);

        if (UltimateSystem.UltimateActive)
        {
            var r = obj.GetComponentInChildren<SpriteRenderer>();
            if (r != null)
                r.color = GameManager.Instance.ChangeSaturation(r.color, GameManager.Instance.noteSaturation);
        }
    }

    private void HandleLanePress(int lane)
    {
        if (lane != laneIndex) return;

        NoteBase best = null;
        float bestTiming = float.MaxValue;

        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent(out NoteBase note)) continue;

           // var j = note.GetJudgement(hitZone);
            if (!note.CanBeHit(hitZone)) continue;

            float t = note.TimingSeconds(hitZone);
            if (t < bestTiming)
            {
                bestTiming = t;
                best = note;
            }
        }

        if (best == null)
        {
            ScoreManager.Instance.AddScore(1f, lane); // ghost tap
            return;
        }

        ScoreManager.Instance.AddScore(bestTiming, lane);
        //best.OnHit(best.GetJudgement(hitZone));
        var judgement = best.GetJudgement(hitZone);

        if (judgement != Judgement.Miss)
        {
            SoundEffectsEventHelper.OnSuccessfulHit?.Invoke(laneIndex, judgement);
        }

        best.OnHit(judgement);
    }
}