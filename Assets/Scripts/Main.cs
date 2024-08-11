using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public PuzzleEditor editor;

    public void Solve()
    {
        if (editor.IsReplaying()) return;
        var puzzle = editor.BuildPuzzle();
        var initialState = puzzle.GetState();

        var solver = new PuzzleSolver();
        var solution = GetTimedSolution(puzzle, solver.Solve);
        puzzle.SetState(initialState);
        //var solution2 = GetTimedSolution(puzzle, solver.SolveAStar);
        //puzzle.SetState(initialState);


        if (solution == null)
        {
            Debug.Log("No solution found!");
        }
        else
        {
            //Debug.Log($"Solution 1: {solution.Count - 1} steps");
            //Debug.Log($"Solution 2: {solution2.Count - 1} steps");
            Debug.Log($"Solution found! ({solution.Count - 1} steps)");
            editor.PlaybackSolution(puzzle, solution);
        }
    }

    private List<PuzzleState> GetTimedSolution(Puzzle puzzle, System.Func<Puzzle, List<PuzzleState>> solve)
    {
        var startTime = Time.realtimeSinceStartup;
        var solution = solve(puzzle);
        var endTime = Time.realtimeSinceStartup;
        Debug.Log($"Solved in {endTime - startTime} seconds");
        return solution;
    }
}
