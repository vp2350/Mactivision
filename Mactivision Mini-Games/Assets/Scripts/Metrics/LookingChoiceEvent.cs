using System.Collections.Generic;
using UnityEngine;
using System;

// MemoryChoiceEvent class: designed to be consumed by MemoryChoiceMetric class.
public class LookingChoiceEvent : AbstractMetricEvent
{
    // Current object presented to the user (ie. in the Feeder game, current food user must decide to feed to the monster or not).
    public string _goodObject { get; }

    // Set of objects that are currently accepted (ie. in Feeder game, set of foods that the monster accepts)
    public List<string> objectsShown { get; }

    // Choice that the user made on _object. If _object is a member of objectsSet, the user wants choice should be true.
    public string choice { get; }

    // Time the user makes the choice. the inherited variable eventTime records the time the choice is presented to the user. 
    // Subtracting the two will give the time it took for the user to decide.
    public System.DateTime choiceTime { get; }

    public LookingChoiceEvent(System.DateTime eventTime, string _goodObject, List<string> objectsShown, string choice, System.DateTime choiceTime) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidLookingChoiceTimeException("LookingChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.objectsShown = objectsShown;
        this._goodObject = _goodObject;
        this.choice = choice;
        this.choiceTime = choiceTime;
    }
}


[Serializable]
public class InvalidLookingChoiceTimeException : Exception
{
    public InvalidLookingChoiceTimeException() : base() { }
    public InvalidLookingChoiceTimeException(string message) : base(message) { }
    public InvalidLookingChoiceTimeException(string message, Exception inner) : base(message, inner) { }
}                 

