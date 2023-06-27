using System.Collections.Generic;
using UnityEngine;
using System;

// MemoryChoiceEvent class: designed to be consumed by MemoryChoiceMetric class.
public class StageChoiceEvent : AbstractMetricEvent
{
    // Set of objects that are currently accepted (ie. in Feeder game, set of foods that the monster accepts)
    public List<int> colorsShown { get; }

    public bool correct { get; }

    // Choice that the user made on _object. If _object is a member of objectsSet, the user wants choice should be true.
    public string choice { get; }

    public bool colorChanged { get; }

    public int colorOriginal { get; }

    public int colorNew { get; }

    public int color1;
    public int color2;
    public int color3;
    public int color4;
    public int color5;
    public int color6;
    public int color7;
    public int color8;  
    public int color9;

    // Time the user makes the choice. the inherited variable eventTime records the time the choice is presented to the user. 
    // Subtracting the two will give the time it took for the user to decide.
    public System.DateTime choiceTime { get; }

    public StageChoiceEvent(System.DateTime eventTime, bool colorChanged, int original, int newColor, bool rightChoice, string choice, System.DateTime choiceTime,
        int color1, int color2, int color3, int color4, int color5, int color6, int color7, int color8, int color9) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidStageChoiceTimeException("StageChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.choiceTime = eventTime;
        this.correct = rightChoice;
        this.colorsShown = colorsShown;
        this.colorChanged = colorChanged;
        this.colorOriginal = original;
        this.colorNew = newColor;
        this.choice = choice;
        this.choiceTime = choiceTime;
        this.color1 = color1;
        this.color2 = color2;
        this.color3 = color3;
        this.color4 = color4;
        this.color5 = color5;
        this.color6 = color6;
        this.color7 = color7;
        this.color8 = color8;
        this.color9 = color9;
    }
    public StageChoiceEvent(System.DateTime eventTime, bool colorChanged, int original, int newColor, bool rightChoice, string choice, System.DateTime choiceTime,
        int color1, int color2, int color3) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidStageChoiceTimeException("StageChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.choiceTime = eventTime;
        this.correct = rightChoice;
        this.colorsShown = colorsShown;
        this.colorChanged = colorChanged;
        this.colorOriginal = original;
        this.colorNew = newColor;
        this.choice = choice;
        this.choiceTime = choiceTime;
        this.color1 = color1;
        this.color2 = color2;
        this.color3 = color3;
        color4 = -1;
        color5 = -1;
        color6 = -1;
        color7 = -1;
        color8 = -1;
        color9 = -1;
    }
    public StageChoiceEvent(System.DateTime eventTime, bool colorChanged, int original, int newColor, bool rightChoice, string choice, System.DateTime choiceTime,
        int color1, int color2, int color3, int color4, int color5, int color6) : base(eventTime)
    {
        if (choiceTime < eventTime)
        {
            throw new InvalidStageChoiceTimeException("StageChoiceEvent cannot be created: choiceTime cannot be earlier than eventTime");
        }

        this.choiceTime = eventTime;
        this.correct = rightChoice;
        this.colorsShown = colorsShown;
        this.colorChanged = colorChanged;
        this.colorOriginal = original;
        this.colorNew = newColor;
        this.choice = choice;
        this.choiceTime = choiceTime;
        this.color1 = color1;
        this.color2 = color2;
        this.color3 = color3;
        this.color4 = color4;
        this.color5 = color5;
        this.color6 = color6;
        color7 = -1;
        color8 = -1;
        color9 = -1;
    }
}


[Serializable]
public class InvalidStageChoiceTimeException : Exception
{
    public InvalidStageChoiceTimeException() : base() { }
    public InvalidStageChoiceTimeException(string message) : base(message) { }
    public InvalidStageChoiceTimeException(string message, Exception inner) : base(message, inner) { }
}

