using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
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
        var puzzle = level.GetPuzzle();

        var solver = new PuzzleSolver();
        var solution = solver.Solve(puzzle);
        Debug.Log(solution);

        puzzleObserver.ReplayPuzzle(puzzle, solution);
    }
}
