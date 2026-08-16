using System;
using System.Collections.Generic;
using Event;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    public int laneIndex;
    public Transform spawnPoint;
    public Transform hitZone;
    public float noteSpeed = 5f;

    private List<int> heldGuitarLanes = new List<int>();

    public AnimationManager animationManager;

    private void Start()
    {
        //deals with lane holding and animations and logic for the guitar. If lanes are held, prioritize the latest one.
        /*
        InputManager.Instance.OnLanePressedGuitar += lane =>
        {
            if (LaneBelongsToThisCharacter(lane))
            {
                if (!heldGuitarLanes.Contains(lane))
                {
                    heldGuitarLanes.Add(lane);
                }
            }
        };
        */

        /*
        InputManager.Instance.OnLaneReleasedGuitar += lane =>
        {
            if (LaneBelongsToThisCharacter(lane))
            {
                heldGuitarLanes.Remove(lane);
            }
        };
        */

        /*
        InputManager.Instance.OnGuitarAttackPressed += () =>
        {
            if (heldGuitarLanes.Count > 0)
            {
                int lastLane = heldGuitarLanes[heldGuitarLanes.Count - 1];

                if (lastLane == laneIndex)
                {
                    animationManager.NewPosGuitar(lastLane);
                    HandleLanePress(lastLane);
                }
            }
        };
        */
        animationManager.OnCharacterReset += () =>
        {
            heldGuitarLanes.Clear();
        };

        InputManager.Instance.OnLanePressed += HandleLanePress;
        InputManager.Instance.OnLanePressedGuitar += HandleLanePressedGuitar;
        InputManager.Instance.OnLaneReleasedGuitar += HandleLaneReleasedGuitar;
        InputManager.Instance.OnGuitarAttackPressed += HandleGuitarAttackPressed;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= HandleLanePress;

            InputManager.Instance.OnLanePressedGuitar -= HandleLanePressedGuitar;
            InputManager.Instance.OnLaneReleasedGuitar -= HandleLaneReleasedGuitar;
            InputManager.Instance.OnGuitarAttackPressed -= HandleGuitarAttackPressed;
        }

    }

    private void HandleLanePressedGuitar(int lane)
    {
        if (LaneBelongsToThisCharacter(lane))
        {
            if (!heldGuitarLanes.Contains(lane))
            {
                heldGuitarLanes.Add(lane);
            }
        }
    }

    private void HandleLaneReleasedGuitar(int lane)
    {
        if (LaneBelongsToThisCharacter(lane))
        {
            heldGuitarLanes.Remove(lane);
        }
    }

    private void HandleGuitarAttackPressed()
    {
        if (heldGuitarLanes.Count > 0)
        {
            int lastLane = heldGuitarLanes[heldGuitarLanes.Count - 1];

            if (lastLane == laneIndex)
            {
                HandleLanePress(lastLane);
            }
        }
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

        if (UltimateSystem.Instance.UltimateActive)
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

        //ScoreManager.Instance.AddScore(bestTiming, lane);
        //best.OnHit(best.GetJudgement(hitZone));
        var judgement = best.GetJudgement(hitZone);

        /*
        if (judgement != Judgement.Miss)
        {
            SoundEffectsEventHelper.OnSuccessfulHit?.Invoke(laneIndex, judgement);
        }
        */
        best.OnHit(judgement);
    }

    //helps with the guitar controls, prevents multilane holding
    private bool LaneBelongsToThisCharacter(int lane)
    {
        if (animationManager.isDuo)
        {
            return lane == 2;
        }

        if (animationManager.isPlayer1)
        {
            return lane == 0 || lane == 1 || lane == 2;
        }

        else
        {
            return lane == 2 || lane == 3 || lane == 4;
        }
    }
}