using UnityEngine;
/*
 * Code Created by Knox Fouladi
 * 
 * This script acts as the overseer of all events.
 * It will be instanced in the Main Menu and will reference subcategories of events, such as UI_Events, Audio_Events, etc.
 * This allows for more efficient and easier event calls based on event type
 * 
 */ 
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }


    [Header("Event Containers")] // event type script references for easier calling
    public GameplayEvents gameplay_events { get; private set; }
    public UIEvents ui_events { get; private set; }

    private void Awake()
    {
        // Singleton Set up
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // initialize event containers
        gameplay_events = new GameplayEvents();
        ui_events = new UIEvents();

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {   
            Instance = null;
        }

        // Removes all subscriptions when destroyed
        //gameplay_events.ClearAll();
        //ui_events.ClearAll();
    }
}
