using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private readonly PersistentValue<Dictionary<int, SolutionData>> solutions = new("solutions", PersistenceMode.GlobalPersistence, new());

    public PuzzleEditor editor;
    public UIPauseMenu pauseMenu;

    private void Awake()
    {
        pauseMenu.window.onClose += Unpause;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.Open();
            editor.isPaused = true;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0.1f;
        }
    }

    private void Unpause()
    {
        editor.isPaused = false;
    }

    public void Solve()
    {
        if (editor.IsReplaying() || editor.isPaused) return;

        var puzzle = editor.BuildPuzzle();
        var initialState = puzzle.GetState();

        var solver = new PuzzleSolver();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var maybeSolution = solver.Solve(puzzle);
        sw.Stop();
        Debug.Log($"Solved in {sw.Elapsed.TotalSeconds} seconds");

        puzzle.SetState(initialState);

        if (maybeSolution is not SolutionData solution)
        {
            Debug.Log("No solution found!");
        }
        else
        {
            Debug.Log($"Solution found! ({solution.StepCount} steps)");
            editor.PlaybackSolution(puzzle, solution);

            var solutionDict = solutions.Get();
            var index = editor.GetSelectedLevelIndex();

            if (!solutionDict.TryGetValue(index, out var oldSolution) || oldSolution.steps.Count <= solution.StepCount)
            {
                solutionDict[index] = solution;
            }
            solutions.MarkDirty();
        }
    }
}
