using System.Collections.Generic;
using UnityEngine;
using System;

// MemoryChoiceEvent class: designed to be consumed by MemoryChoiceMetric class.
public class StageChoiceEvent : AbstractMetricEvent
{
    // Current object presented to the user (ie. in the Feeder game, current food user must decide to feed to the monster or not).
    public string _goodObject { get; }

    public string correct { get; }
    // Set of objects that are currently accepted (ie. in Feeder game, set of foods that the monster accepts)
    public List<Color> colorsShown { get; }

    // Choice that the user made on _object. If _object is a member of objectsSet, the user wants choice should be true.
    public string choice { get; }

    public bool colorChanged { get; }

    public Color colorOriginal { get; }

    public Color colorNew { get; }

    // Time the user makes the choice. the inherited variable eventTime records the time the choice is presented to the user. 
    // Subtracting the two will give the time it took for the user to decide.
    public System.DateTime choiceTime { get; }

    public StageChoiceEvent(System.DateTime eventTime, bool colorChanged, Color original, Color newColor, string rightChoice, List<Color> colorsShown, string choice, System.DateTime choiceTime) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidStageChoiceTimeException("StageChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.correct = rightChoice;
        this.colorsShown = colorsShown;
        this.colorChanged = colorChanged;
        this.colorOriginal = original;
        this.colorNew = newColor;
        this.choice = choice;
        this.choiceTime = choiceTime;
    }
}


[Serializable]
public class InvalidStageChoiceTimeException : Exception
{
    public InvalidStageChoiceTimeException() : base() { }
    public InvalidStageChoiceTimeException(string message) : base(message) { }
    public InvalidStageChoiceTimeException(string message, Exception inner) : base(message, inner) { }
}

