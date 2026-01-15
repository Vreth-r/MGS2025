using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq.Expressions;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("Identifiers")] // "What player is this?"
    public bool isPlayer1 = false;
    public bool isDuo = false;

    [Header("Sprites")]
    public Sprite sprBase;
    public Sprite sprAttack1;
    public Sprite sprAttack2;
    public Sprite sprAttack3;
    public Sprite sprOuch;

    //Off-screen Location Vector 
    Vector3 offscreen = new Vector3(-10f, 0.5f, 1.1f);
    public Vector3 baseGeneral;

    [SerializeField]
    private float resetcounter = 0;

    int resetmax = 50; //not changed in code, but variable if want to change it
    int attackcount = 0;

    static int duoActive = 0;
    static int hurtActive = 0;

    static List<int> hurtlanes = new List<int>();

    private void Start()
    {
        //do newPos when press
        InputManager.Instance.OnLanePressed += NewPos;

        Vector3 baseP1 = GameObject.Find("Lane1").transform.position;
        Vector3 baseP2 = GameObject.Find("Lane3").transform.position;
        Vector3 basePDuo = GameObject.Find("Lane2").transform.position;

        //Set up general position
        if (isPlayer1 && !isDuo) //P1
        {                               //-5.65, 0.5, 2.6
            baseGeneral = new Vector3(baseP1.x - 5.65f, baseP1.y + 0.5f, baseP1.z + 0.6f);
            transform.position = baseGeneral;
        }
        else if (!isPlayer1 && !isDuo) // P2
        {
            baseGeneral = new Vector3(baseP2.x - 5.65f, baseP2.y + 0.5f, baseP2.z + 0.6f);
            transform.position = baseGeneral;
        }
        else if (isDuo) //hide duo as default
        {
            baseGeneral = new Vector3(basePDuo.x - 5.65f, basePDuo.y + 0.5f, basePDuo.z + 0.6f);
            transform.position = offscreen; // hide Duo as default
        }

    }//END OF START()

    private void OnDestroy()
    {
        // Unsubscribe from lane press event to prevent MissingReferenceException
        if (InputManager.Instance != null)
            InputManager.Instance.OnLanePressed -= NewPos;
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
                gameObject.GetComponent<SpriteRenderer>().sprite = sprOuch;
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
            gameObject.GetComponent<SpriteRenderer>().sprite = sprBase;
            attackcount = 0;

            if (isDuo) //PDuo
                transform.position = offscreen;
            else
                transform.position = baseGeneral;

        }


    }//END OF UPDATE()

    //newPos: Update where character is
    public void NewPos(int lane)
    {

        //Lane Placement Changer
        if (lane == 2)
        {
            duoActive = 2;
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
                gameObject.GetComponent<SpriteRenderer>().sprite = sprBase;
            }
            if (isDuo) //hide duo, show solo
                transform.position = offscreen;

            else if (isPlayer1 == (lane <= 2))
                transform.position = new Vector3(GameObject.Find("Lane" + lane).transform.position.x - 5.65f,
                                                 GameObject.Find("Lane" + lane).transform.position.y + 0.5f,
                                                 GameObject.Find("Lane" + lane).transform.position.z + 0.6f);

        }


        //Sprite Changer
        // Only operate on single character: 1 2 D
        if ((isPlayer1 == (lane <= 2)) || isDuo)  //is P1 & <2   or   P2 & >2   or   Duo
        {
            resetcounter = 0; //in here so it doesnt trigger for all, always
            if (attackcount == 0)
            {
                //gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                gameObject.GetComponent<SpriteRenderer>().sprite = sprAttack1;
            }
            else if (attackcount == 1)
                gameObject.GetComponent<SpriteRenderer>().sprite = sprAttack2;
            else if (attackcount == 2)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = sprAttack3;
                attackcount = -1; //cus of the +1 below
            }

            attackcount += 1;
        }

    }// END OF NEWPOS()

    public static void Missed(ILaneController lane)
    {
        hurtlanes.Add(lane.Index);

    }

}
