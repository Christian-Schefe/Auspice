using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolver
{
    public List<Puzzle.PuzzleState> Solve(Puzzle puzzle)
    {
        var logic = new PuzzleLogic(puzzle);
        var initialState = puzzle.GetState();

        var queue = new Queue<List<Puzzle.PuzzleState>>();
        queue.Enqueue(new() { initialState });
        var visited = new HashSet<Puzzle.PuzzleState>();


        while (queue.Count > 0)
        {
            var moveList = queue.Dequeue();

            var curState = moveList[^1];
            if (visited.Contains(curState)) continue;
            visited.Add(curState);

            if (moveList.Count > 1000)
            {
                Debug.Log("Too many iterations");
                break;
            }

            Debug.Log("state: " + curState);

            foreach (var state in logic.GetNextStates(curState))
            {
                var newMoveList = new List<Puzzle.PuzzleState>(moveList) { state };
                puzzle.SetState(state);
                if (puzzle.IsWon())
                {
                    return newMoveList;
                }

                queue.Enqueue(newMoveList);
            }
        }

        return null;
    }
}
