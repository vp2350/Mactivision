using System.Collections.Generic;
using UnityEngine;
using System;

// MemoryChoiceEvent class: designed to be consumed by MemoryChoiceMetric class.
public class RecipeChoiceEvent : AbstractMetricEvent
{

    // Set of objects that are currently accepted (ie. in Feeder game, set of foods that the monster accepts)
    public List<string> objectsSet { get; }

    // Current object presented to the user (ie. in the Feeder game, current food user must decide to feed to the monster or not).
    public string[] _object { get; }

    // Choice that the user made on _object. If _object is a member of objectsSet, the user wants choice should be true.
    public bool choice { get; }

    public bool correct { get; }

    // Time the user makes the choice. the inherited variable eventTime records the time the choice is presented to the user. 
    // Subtracting the two will give the time it took for the user to decide.
    public System.DateTime choiceTime { get; }

    public RecipeChoiceEvent(System.DateTime eventTime, List<string> objectsSet, string[] _object, bool choice, bool correct, System.DateTime choiceTime) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidRecipeChoiceTimeException("RecipeChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.correct = correct;
        this.objectsSet = objectsSet;
        this._object = _object;
        this.choice = choice;
        this.choiceTime = choiceTime;
    }
}


[Serializable]
public class InvalidRecipeChoiceTimeException : Exception
{
    public InvalidRecipeChoiceTimeException() : base() { }
    public InvalidRecipeChoiceTimeException(string message) : base(message) { }
    public InvalidRecipeChoiceTimeException(string message, Exception inner) : base(message, inner) { }
}