using System;



namespace Event
{
    
    //Realistically we should be able to get rid of the references that the event manager uses and just plop this into there instead.
    //Since this isn't to be a major refactoring thing yet I'm keeping this here for the meantime until we do
    //I'm trying to hold back the urge to refactor...
    public static class GameplayEvents
    {
        public static EventDispatcher<(int, Judgement, float)> ScoreUpdateEvent = new EventDispatcher<(int, Judgement, float)>();
        //replaces the OnCombo event that we had
        //this really shouldn't be needed. Creating a Level wrapper for the players, score management, and their 
        //abilities will cut down on the amount of redirection we have here
        
        
        //bandaid solution for the popup fix. I beg you to change this in the future.
        public static EventDispatcher<ValueTuple> PlayersMergeEvent = new EventDispatcher<ValueTuple>();
        //called when the players merge on the duo lane
        
        public static EventDispatcher<ValueTuple> PlayersSeparateEvent = new EventDispatcher<ValueTuple>();
        //called when the players separate from the duo lane
        //because of core jank this is currently called every time the duo value is < 2
    }
    
}

