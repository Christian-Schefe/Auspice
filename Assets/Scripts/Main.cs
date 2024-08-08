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

        var solver = new PuzzleSolver();
        var solution = solver.Solve(puzzle);

        if (solution == null)
        {
            Debug.Log("No solution found!");
        }
        else
        {
            Debug.Log($"Solution found! ({solution.Count - 1} steps)");
            editor.PlaybackSolution(puzzle, solution);
        }
    }
}
