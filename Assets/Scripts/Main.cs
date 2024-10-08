using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Main : MonoBehaviour
{
    private readonly PersistentValue<Dictionary<string, SolutionData>> solutions = new("solutions", PersistenceMode.GlobalPersistence);
    private readonly PersistentValue<int?> selectedLevelIndex = new("selectedLevelIndex", PersistenceMode.GlobalRuntime);

    public PuzzleReplayer replayer;
    public PuzzleEditor editor;
    public UIPauseMenu pauseMenu;
    public LevelRegistry levelRegistry;

    private Puzzle puzzle;
    private PuzzleState initialState;
    private Task<SolutionData?> taskHandle;
    public MainState CurrentState { get; set; }

    public bool IsPaused { get; set; }

    public int? GetSelectedLevelIndex() => selectedLevelIndex.GetOrDefault(null);

    private void Awake()
    {
        pauseMenu.window.onClose += Unpause;
        CurrentState = MainState.Editing;
    }

    private void Update()
    {
        if (IsPaused) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.Open();
            IsPaused = true;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Slowing down");
            Time.timeScale *= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Speeding up");
            Time.timeScale *= 2f;
        }

        CheckSolveTask();
    }

    private void Unpause()
    {
        IsPaused = false;
    }

    private void CheckSolveTask()
    {
        if (CurrentState != MainState.Solving) return;
        if (!taskHandle.IsCompletedSuccessfully) return;
        var solution = taskHandle.Result;
        taskHandle.Dispose();
        OnSolve(solution);
    }

    public void Solve()
    {
        if (IsPaused || CurrentState != MainState.Editing) return;

        puzzle = editor.BuildPuzzle();
        initialState = puzzle.GetState();
        var solver = new PuzzleSolver();
        taskHandle = solver.SolveAsync(puzzle);

        CurrentState = MainState.Solving;
    }

    private void OnSolve(SolutionData? maybeSolution)
    {
        puzzle.SetState(initialState);

        if (maybeSolution is not SolutionData solution)
        {
            Debug.Log("No solution found!");
            CurrentState = MainState.Editing;
            editor.ShowImpossible();
        }
        else
        {
            Debug.Log($"Solution found! ({solution.StepCount} steps)");
            PlaybackSolution(solution);

            var solutionDict = solutions.GetOrCreateRef(new());
            var maybeIndex = GetSelectedLevelIndex();
            if (maybeIndex is not int index || editor.IsEditMode()) return;

            var hash = levelRegistry.GetPuzzleHash(index);

            if (!solutionDict.TryGetValue(hash, out var oldSolution) || oldSolution.StepCount < solution.StepCount)
            {
                solutionDict[hash] = solution;
                Debug.Log("New best solution!");
            }
            solutions.MarkDirty();
        }
    }

    private void PlaybackSolution(SolutionData solution)
    {
        CurrentState = MainState.Replaying;
        replayer.endCallback = () =>
        {
            CurrentState = MainState.Editing;
        };
        replayer.stepCallback = (step) =>
        {
            editor.runMenu.SetSteps(step);
        };
        replayer.ReplayPuzzle(puzzle, solution);
    }
}

public enum MainState
{
    Editing, Solving, Replaying
}