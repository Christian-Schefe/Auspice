using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public PuzzleEditor editor;
    public Level level;
    public PuzzleReplayer puzzleObserver;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Solve();
        }
    }

    private void Solve()
    {
        editor.isPlaying = true;
        var puzzle = level.GetPuzzle();

        var solver = new PuzzleSolver();
        var solution = solver.Solve(puzzle);
        Debug.Log(solution);

        if (solution == null)
        {
            Debug.Log("No solution found");
            editor.isPlaying = false;
            return;
        }
        puzzleObserver.ReplayPuzzle(puzzle, solution, FinishedSolve);
    }

    private void FinishedSolve()
    {
        editor.isPlaying = false;
    }
}
