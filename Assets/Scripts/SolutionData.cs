using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SolutionData
{
    public List<SolutionStep> steps;

    public SolutionData(List<SolutionStep> steps)
    {
        this.steps = steps;
    }

    public readonly int StepCount =>  steps.Count - 1;
}

public struct SolutionStep
{
    public PuzzleState state;
    public List<TurnEvent> events;

    public SolutionStep(PuzzleState state, List<TurnEvent> events)
    {
        this.state = state;
        this.events = events;
    }
}