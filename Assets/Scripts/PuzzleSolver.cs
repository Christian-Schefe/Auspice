using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolver
{
    public List<PuzzleState> Solve(Puzzle puzzle)
    {
        var logic = new PuzzleLogic(puzzle);
        var initialState = puzzle.GetState();
        var initialReducedState = puzzle.GetReducedPuzzleState();

        var queue = new Queue<PuzzleState>();
        queue.Enqueue(initialState);

        var visited = new HashSet<ReducedPuzzleState>() { initialReducedState };
        var cameFrom = new Dictionary<PuzzleState, PuzzleState>();

        while (queue.Count > 0)
        {
            var curState = queue.Dequeue();

            if (visited.Count > 100000)
            {
                Debug.Log("Too many iterations");
                break;
            }

            foreach (var state in logic.GetNextStates(curState))
            {
                puzzle.SetState(state);
                var newReducedState = puzzle.GetReducedPuzzleState();

                if (visited.Contains(newReducedState)) continue;
                visited.Add(newReducedState);

                cameFrom[state] = curState;

                if (puzzle.IsWon())
                {
                    return ReconstructPath(cameFrom, state);
                }

                queue.Enqueue(state);
            }
        }

        return null;
    }

    private List<PuzzleState> ReconstructPath(Dictionary<PuzzleState, PuzzleState> cameFrom, PuzzleState current)
    {
        var path = new List<PuzzleState> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();

        return path;
    }
}
