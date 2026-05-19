using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;
using Event;
using UnityEngine;

/// Central Animation programmed by: LogChiCha
/// Guitar Specific programmed by: kikiluvumba#5886
///
/// SUMMARY
/// The animation manager, changes the player character visuals depending on inputs and notes
///  (P1, P2, PDuo)

public class AnimationManager : MonoBehaviour
{
    [Header("Identifiers")] // "What player is this?"
    public bool isPlayer1 = false;
    public bool isDuo = false;

    [Header("Sprites")] //
    public Sprite sprBase;
    public Sprite sprAttack1;
    public Sprite sprAttack2;
    public Sprite sprOuch;
    public SpriteRenderer spriteRenderer;

    private Animator animator;

    //Off-screen Location Vector 
    Vector3 offscreen = new Vector3(-13f, 0.5f, 1.1f);
    public Vector3 baseGeneral;

    [SerializeField]
    private float resetcounter = 0;

    int resetmax = 50; //not changed in code, but variable if want to change it
    int attackcount = 0;

    public static int duoActive = 0;
    static int hurtActive = 0;


    int bouncetest = 0;
    int countin = 0;

    static List<int> hurtlanes = new List<int>();

    private SoundEffectsPlayer soundEffectsPlayer;
    
    int laneHeldGuitar = -1; //denotes which lane is currently held for the guitar controls

    public System.Action OnCharacterReset; //position reseter

    private void Start()
    {
        //do newPos when press
        InputManager.Instance.OnLanePressed += NewPos;

        //guitar stuff
        InputManager.Instance.OnLanePressedGuitar += LanePressedGuitar;
        InputManager.Instance.OnLaneReleasedGuitar += LaneReleasedGuitar;
        InputManager.Instance.OnGuitarAttackPressed += AttackAnimationGuitar;

        // Subscribe to pulse event
        GameManager.Instance.OnPulse += idleanim;

        Vector3 baseP1 = GameObject.Find("Lane1").transform.position;
        Vector3 baseP2 = GameObject.Find("Lane3").transform.position;
        Vector3 basePDuo = GameObject.Find("Lane2").transform.position;

        animator = GetComponent<Animator>();

        soundEffectsPlayer = GetComponent<SoundEffectsPlayer>();

        SoundEffectsEventHelper.OnSuccessfulHit += PlaySoundOnSuccessfulHit;

        //Set up general position
        // General positions are bugged due to new pivots, pls adjust
        if (isPlayer1 && !isDuo) //P1
        {                               //-5.65, 0.5, 2.6
            baseGeneral = new Vector3(baseP1.x - 7f, baseP1.y - 0.25f, baseP1.z + 1f);
            transform.position = baseGeneral;
        }
        else if (!isPlayer1 && !isDuo) // P2
        {
            baseGeneral = new Vector3(baseP2.x - 7f, baseP2.y - 0.25f, baseP2.z - 0.5f);
            transform.position = baseGeneral;
        }
        else if (isDuo) //hide duo as default
        {
            baseGeneral = new Vector3(basePDuo.x - 6.9f, basePDuo.y - 0.25f, basePDuo.z + 0.15f);
            transform.position = offscreen; // hide Duo as default
        }

    }//END OF START()
    
    
    
    

    private void OnDestroy()
    {
        // Unsubscribe from lane press event to prevent MissingReferenceException
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= NewPos;

            //guitar stuff
            InputManager.Instance.OnGuitarAttackPressed -= AttackAnimationGuitar;
            InputManager.Instance.OnLanePressedGuitar -= LanePressedGuitar;
            InputManager.Instance.OnLaneReleasedGuitar -= LaneReleasedGuitar;  
        }

        SoundEffectsEventHelper.OnSuccessfulHit -= PlaySoundOnSuccessfulHit;
    }

    private void OnDisable()
    {
        // Unsubscribe from lane press event to prevent MissingReferenceException
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLanePressed -= NewPos;

            //guitar stuff
            InputManager.Instance.OnGuitarAttackPressed -= AttackAnimationGuitar;
            InputManager.Instance.OnLanePressedGuitar -= LanePressedGuitar;
            InputManager.Instance.OnLaneReleasedGuitar -= LaneReleasedGuitar;
        }

        SoundEffectsEventHelper.OnSuccessfulHit -= PlaySoundOnSuccessfulHit;
    }

    //run every frame
    private void Update()
    {

        if (resetcounter <= resetmax) // just to not skyrocket the value when afk
            resetcounter += 1 * Time.deltaTime * 60;

        //Check what lane hurt is in, runs for that
        for (int i = hurtlanes.Count - 1; i >= 0; i--)
        {
            //runs if (P1 & 0,1,2)  (P2 & 2, 3 ,4)  (isDuo)   (lane is -1 [see below])  ---   maybe rewrite if possible
            if (((isPlayer1 == (hurtlanes[i] <= 2)) || isDuo || hurtlanes[i] == -1) && transform.position != offscreen)
            {
                animator.Play("Bounce", -1, 0f);
                // animator.Play("Bounce2");
                spriteRenderer.sprite = sprOuch;

                soundEffectsPlayer.PlaySoundEffect("MissHit_1");

                hurtActive = 3;
                if (hurtlanes[i] == 2)  //lets both Ps be hurt if in lane 2 (needs to be run twice)
                    hurtlanes.Add(-1);
                hurtlanes.Remove(hurtlanes[i]);
            }
        }
        //Apply stalling for all sprites while hurt
        if (hurtActive > 0)
        {
            hurtActive--;
            resetcounter = 0;
        }

        //Reset position of character code
        if (resetcounter >= resetmax)
        {
            spriteRenderer.sprite = sprBase;

                bouncetest = 1;
                animator.Play("Bounce-Idle");

            attackcount = 0;
            laneHeldGuitar = -1;

            if ((duoActive >= 2) == isDuo)
                transform.position = baseGeneral;
             else   
                transform.position = offscreen;

             OnCharacterReset?.Invoke();
        }


    }//END OF UPDATE()

    //newPos: Update where character is
    public void NewPos(int lane)
    {
        //Lane Placement Changer
        if (lane == 2)
        {
            duoActive = 2;
            
            //this is considered a bandaid solution for what we have here. Refactoring this is recommended.
            GameplayEvents.PlayersMergeEvent.CallEvent(ValueTuple.Create());
            
            
            resetcounter = 0;
            if (isDuo)//show duo, hide solo
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 6.2f,
                                                 GameObject.Find("Lane" + lane).transform.position.y - 0.25f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.15f);
            else
                transform.position = offscreen;
        }
        else
        {
            if (duoActive > 0 && !isDuo) // If just leaving Duo lane, make sure both charactes are there, and in base states
            {
                transform.position = baseGeneral;
                
                duoActive--;
                
                //this is considered a bandaid solution for what we have here. Refactoring this is recommended.
                GameplayEvents.PlayersSeparateEvent.CallEvent(ValueTuple.Create());
                
                
                spriteRenderer.sprite = sprBase;
            }
            
            if (isDuo)
                transform.position = offscreen;
            else if (isPlayer1 == (lane <= 2))
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 6.2f,
                                                 GameObject.Find("Lane" + lane).transform.position.y - 0.25f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.15f);

        }


        //Sprite Changer
        // Only operate on single character: 1 2 D
        if ((isPlayer1 == (lane <= 2)) || isDuo)  //is P1 & <2   or   P2 & >2   or   Duo
        {

            resetcounter = 0; //in here so it doesnt trigger for all, always
            if (attackcount == 0)
            {
                //gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                spriteRenderer.sprite = sprAttack1;
            }
            else if (attackcount == 1)
            {
                spriteRenderer.sprite = sprAttack2;
                attackcount = -1; //cus of the +1 below
            }

            animator.Play("Bounce", -1, 0f);

            //Commenting this out because there is no 2nd attack sprite yet
            //Commenting this back IN cus there IS a 2nd attack now, and this is a BUG that i must FIX
            attackcount += 1;


        }

    }// END OF NEWPOS()

    //sets some variables for guitar specific controls/lane shenanigans
    private void LanePressedGuitar(int lane)
    {
        laneHeldGuitar = lane;
        resetcounter = 0;

        MoveToLane(lane);
    }

    //essentially the first half of NewPos()
    private void MoveToLane(int lane) 
    {
        if (lane == 2)
        {
            duoActive = 2;
            
            //this is considered a bandaid solution for what we have here. Refactoring this is recommended.
            GameplayEvents.PlayersMergeEvent.CallEvent(ValueTuple.Create());
            
            resetcounter = 0;
            if (isDuo)//show duo, hide solo
                transform.position = baseGeneral;
            else
                transform.position = offscreen;
        }
        else if (lane != 2)
        {
            if (duoActive > 0 && !isDuo) // If just leaving Duo lane, make sure both charactes are there, and in base states
            {
                transform.position = baseGeneral;
                duoActive--;
                
                //this is considered a bandaid solution for what we have here. Refactoring this is recommended.
                GameplayEvents.PlayersSeparateEvent.CallEvent(ValueTuple.Create());
                
                
                
                spriteRenderer.sprite = sprBase;
            }

            if (isDuo)
                transform.position = offscreen;
            else if (isPlayer1 == (lane <= 2))
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 6.2f,
                                                 GameObject.Find("Lane" + lane).transform.position.y - 0.25f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.15f);

        }
    }
    private void AttackAnimationGuitar()
    {
        if (laneHeldGuitar != -1) //only works when MoveToLane is currently active (aka player waiting to attack in lane)
        {
            NewPosGuitar(laneHeldGuitar);
        }
    }

    private void LaneReleasedGuitar(int lane)
    {
        //reset animations when lane button is let go
        if (laneHeldGuitar == lane)
        {
            laneHeldGuitar = -1;
            transform.position = baseGeneral;
            spriteRenderer.sprite = sprBase;

            if (isDuo)
            {
                transform.position = offscreen;
            }
            else
            {
                transform.position = baseGeneral;
            }
        }
    }

    public void NewPosGuitar(int lane)
    {
        //Lane Placement Changer
        if (lane == 2)
        {
            duoActive = 2;
            resetcounter = 0;
            if (isDuo)//show duo, hide solo
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 6.2f,
                                                 GameObject.Find("Lane" + lane).transform.position.y - 0.25f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.15f);
            else
                transform.position = offscreen;
        }
        else if (lane != 2)
        {
            if (duoActive > 0 && !isDuo) // If just leaving Duo lane, make sure both charactes are there, and in base states
            {
                transform.position = baseGeneral;
                duoActive--;
                spriteRenderer.sprite = sprBase;
            }

            if (isDuo)
                transform.position = offscreen;
            else if (isPlayer1 == (lane <= 2))
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 6.2f,
                                                 GameObject.Find("Lane" + lane).transform.position.y - 0.25f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.15f);

        }


        //Sprite Changer
        // Only operate on single character: 1 2 D
        if ((isPlayer1 == (lane <= 2)) || isDuo)  //is P1 & <2   or   P2 & >2   or   Duo
        {

            resetcounter = 0; //in here so it doesnt trigger for all, always
            Debug.Log("below");
            Debug.Log(attackcount);
            Debug.Log("above");

            if (attackcount == 0)
            {
                //gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                spriteRenderer.sprite = sprAttack1;
            }
            else if (attackcount == 1)
            {
                spriteRenderer.sprite = sprAttack2;
                attackcount = -1; //cus of the +1 below
            }

            animator.Play("Bounce", -1, 0f);

            //Commenting this out because there is no 2nd attack sprite yet
            //Commenting this back IN cus there IS a 2nd attack now, and this is a BUG that i must FIX
            attackcount += 1;


        }

    }
    public static void Missed(LaneController lane)
    {
        hurtlanes.Add(lane.laneIndex);

    }

    private void idleanim()
    {
        if (countin == 0 && resetcounter >= resetmax)
        {
            if (bouncetest != 2)
            {
                bouncetest = 2;
                animator.Play("Bounce-Idle2");
            }
            else if (bouncetest != 3)
            {
                bouncetest = 3;
                animator.Play("Bounce-Idle3");
            }
            countin = 1;
        }
        else
        {
            countin = 0;
        }

    }

    private string GetNextAttackSound()
    {
        if (isDuo)
        {
            if (attackcount == 1)
            {
                return "DuoAttackHit_1";
            }

            else
            {
                return "DuoAttackHit_1";
            }
        }

        else
        {
            if (attackcount == 1)
            {
                return "AttackHit_1";
            }

            else
            {
                return "AttackHit_2";
            }
        }
    }
    private void PlaySoundOnSuccessfulHit(int laneIndex, Judgement judgement)
    {
        if (LaneIdentifier(laneIndex) == true)
        {
            string soundName = GetNextAttackSound();
            soundEffectsPlayer.PlaySoundEffect(soundName);
        }
    }

    private bool LaneIdentifier(int lane)
    {
        if (isDuo)
        {
            return lane == 2;
        }

        else if (isPlayer1)
        {
            return lane <= 1;
        }

        else
        {
            return lane >= 3;
        }
    }
}
