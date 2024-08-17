using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SolutionData
{
    public SolutionStep[] steps;

    public SolutionData(SolutionStep[] steps)
    {
        this.steps = steps;
    }

    public readonly int StepCount => steps.Length - 1;
}

public struct SolutionStep
{
    public PuzzleState state;
    public TurnEvent[] events;

    public SolutionStep(PuzzleState state, TurnEvent[] events)
    {
        this.state = state;
        this.events = events;
    }
}