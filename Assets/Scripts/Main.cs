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
        var solution = GetTimedSolution(puzzle, solver.Solve);
        puzzle.SetState(initialState);

        if (solution == null)
        {
            Debug.Log("No solution found!");
        }
        else
        {
            Debug.Log($"Solution found! ({solution.Count - 1} steps)");
            editor.PlaybackSolution(puzzle, solution);

            var solutionDict = solutions.Get();
            var index = editor.GetSelectedLevelIndex();

            if (!solutionDict.TryGetValue(index, out var oldSolution) || oldSolution.solution.Count <= solution.Count)
            {
                solutionDict[index] = new SolutionData() { solution = solution };
            }
            solutions.MarkDirty();
        }
    }

    private List<PuzzleState> GetTimedSolution(Puzzle puzzle, System.Func<Puzzle, List<PuzzleState>> solve)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var solution = solve(puzzle);
        sw.Stop();
        Debug.Log($"Solved in {sw.Elapsed.TotalSeconds} seconds");
        return solution;
    }
}
