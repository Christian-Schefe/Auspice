using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolver
{
    public SolutionData? Solve(Puzzle puzzle)
    {
        var logic = new PuzzleLogic(puzzle);
        var initialState = puzzle.GetState();
        var initialReducedState = puzzle.GetReducedPuzzleState();

        var queue = new Queue<PuzzleState>();
        queue.Enqueue(initialState);

        var visited = new HashSet<ReducedPuzzleState>() { initialReducedState };
        var cameFrom = new Dictionary<PuzzleState, SolutionStep>();

        while (queue.Count > 0)
        {
            var curState = queue.Dequeue();

            if (visited.Count > 100000)
            {
                Debug.Log("Too many iterations");
                break;
            }

            foreach (var (nextState, turnEvents) in logic.GetNextStates(curState))
            {
                puzzle.SetState(nextState);
                var newReducedState = puzzle.GetReducedPuzzleState();

                if (visited.Contains(newReducedState)) continue;
                visited.Add(newReducedState);

                cameFrom[nextState] = new SolutionStep(curState, turnEvents);

                if (puzzle.IsWon())
                {
                    return ReconstructPath(cameFrom, nextState);
                }

                queue.Enqueue(nextState);
            }
        }

        return null;
    }

    private SolutionData ReconstructPath(Dictionary<PuzzleState, SolutionStep> cameFrom, PuzzleState current)
    {
        var path = new List<SolutionStep>();

        while (cameFrom.ContainsKey(current))
        {
            var step = cameFrom[current];

            path.Add(new(current, step.events));

            current = step.state;
        }
        path.Add(new(current, new List<TurnEvent>()));

        path.Reverse();

        return new SolutionData(path);
    }
}
