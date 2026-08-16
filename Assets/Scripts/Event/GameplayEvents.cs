using System;



namespace Event
{
    
    //Realistically we should be able to get rid of the references that the event manager uses and just plop the dispatchers into there instead.
    //Since this isn't to be a major refactoring thing yet I'm keeping this here for the meantime until we do
    //I'm trying to hold back the urge to refactor...
    
    //also if we're using this method, we should refactor everything else to also use this as well.
    //if we're not using this method, show me a better solution.
    public static class GameplayEvents
    {
        public static readonly EventDispatcher<(int, Judgement, float)> ScoreUpdateEvent = new EventDispatcher<(int, Judgement, float)>();
        //replaces the OnCombo event that we had
        //this really shouldn't be needed. Creating a Level wrapper for the players, score management, and their 
        //abilities will cut down on the amount of redirection we have here which should improve readibility
        //(then again I'm making you read an essay of comments here so maybe I should be watching out)
        
        
        //bandaid solution for the popup fix. I beg you to change this in the future.
        public static readonly EventDispatcher<ValueTuple> PlayersMergeEvent = new EventDispatcher<ValueTuple>();
        //called when the players merge on the duo lane
        
        public static readonly EventDispatcher<ValueTuple> PlayersSeparateEvent = new EventDispatcher<ValueTuple>();
        //called when the players separate from the duo lane
        //because of core jank this is currently called every time the duo value is < 2
        
        
        //called when the ultimate ability is activated
        public static readonly EventDispatcher<ValueTuple> UltimateStartEvent = new EventDispatcher<ValueTuple>();
        
        //called when the ultimate ability ends
        public static readonly EventDispatcher<ValueTuple> UltimateDepleteEvent = new EventDispatcher<ValueTuple>();
        
        public static readonly EventDispatcher<ValueTuple> GamePulseEvent = new EventDispatcher<ValueTuple>();

        public static readonly EventDispatcher<(int, Judgement)> NoteHitEvent = new EventDispatcher<(int, Judgement)>();
     }
    
}

