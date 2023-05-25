using System.Collections.Generic;
using UnityEngine;
using System;

// MemoryChoiceEvent class: designed to be consumed by MemoryChoiceMetric class.
public class CakeChoiceEvent : AbstractMetricEvent
{
    public int objectType;

    // Current object presented to the user (ie. in the Feeder game, current food user must decide to feed to the monster or not).
    public string _object { get; }

    // Choice that the user made on _object. If _object is a member of objectsSet, the user wants choice should be true.
    public int boxChoice { get; }

    public bool correct { get; }

    // Time the user makes the choice. the inherited variable eventTime records the time the choice is presented to the user. 
    // Subtracting the two will give the time it took for the user to decide.
    public System.DateTime choiceTime { get; }

    public CakeChoiceEvent(System.DateTime eventTime, int objectType,  string _object, int choice, bool correct, System.DateTime choiceTime) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidCakeChoiceTimeException("CakeChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.choiceTime = eventTime;
        this.correct = correct;
        this.objectType = objectType;
        this._object = _object;
        this.boxChoice = choice;
        this.choiceTime = choiceTime;
    }
}


[Serializable]
public class InvalidCakeChoiceTimeException : Exception
{
    public InvalidCakeChoiceTimeException() : base() { }
    public InvalidCakeChoiceTimeException(string message) : base(message) { }
    public InvalidCakeChoiceTimeException(string message, Exception inner) : base(message, inner) { }
}