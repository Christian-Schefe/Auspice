using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolver
{
    public List<PuzzleState> Solve(Puzzle puzzle)
    {
        var logic = new PuzzleLogic(puzzle);
        var initialState = puzzle.GetState();

        var queue = new Queue<List<PuzzleState>>();
        queue.Enqueue(new List<PuzzleState> { initialState });
        //var visited = new HashSet<PuzzleState>();
        var visited = new HashSet<ReducedPuzzleState>();

        while (queue.Count > 0)
        {
            var moveList = queue.Dequeue();

            var curState = moveList[^1];

            puzzle.SetState(curState);
            var curReducedState = puzzle.GetReducedPuzzleState();
            if (visited.Contains(curReducedState)) continue;
            visited.Add(curReducedState);

            //if (visited.Contains(curState)) continue;
            //visited.Add(curState);

            if (visited.Count > 100000)
            {
                Debug.Log("Too many iterations");
                break;
            }

            foreach (var state in logic.GetNextStates(curState))
            {
                var newMoveList = new List<PuzzleState>(moveList) { state };
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

    public List<PuzzleState> SolveAStar(Puzzle puzzle)
    {
        var logic = new PuzzleLogic(puzzle);
        var initialState = puzzle.GetState();

        var openSet = new List<PuzzleState> { initialState };
        var gScore = new Dictionary<PuzzleState, int> { { initialState, 0 } };
        var fScore = new Dictionary<PuzzleState, int> { { initialState, HeuristicCostEstimate(puzzle, initialState) } };
        var cameFrom = new Dictionary<PuzzleState, PuzzleState>();

        while (openSet.Count > 0)
        {
            var current = GetLowestFScoreState(openSet, fScore);
            puzzle.SetState(current);
            if (puzzle.IsWon())
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in logic.GetNextStates(current))
            {
                var tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(puzzle, neighbor);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private int HeuristicCostEstimate(Puzzle puzzle, PuzzleState state)
    {
        var chests = puzzle.GetEntityPositions(PuzzleEntityType.Chest);
        var maxChestDistance = 0;

        for (int i = 0; i < state.playerPosition.Length; i++)
        {
            var player = state.playerPosition[i];
            var closestChestDistance = int.MaxValue;
            foreach (var chest in chests)
            {
                var distance = Mathf.Abs(player.x - chest.x) + Mathf.Abs(player.y - chest.y);
                if (distance < closestChestDistance)
                {
                    closestChestDistance = distance;
                }
            }
            if (closestChestDistance > maxChestDistance)
            {
                maxChestDistance = closestChestDistance;
            }
        }
        return maxChestDistance;
    }

    private PuzzleState GetLowestFScoreState(List<PuzzleState> openSet, Dictionary<PuzzleState, int> fScore)
    {
        var lowestFScore = int.MaxValue;
        PuzzleState lowestFScoreState = openSet[0];

        foreach (var state in openSet)
        {
            if (fScore[state] < lowestFScore)
            {
                lowestFScore = fScore[state];
                lowestFScoreState = state;
            }
        }

        return lowestFScoreState;
    }

    private List<PuzzleState> ReconstructPath(Dictionary<PuzzleState, PuzzleState> cameFrom, PuzzleState current)
    {
        var path = new List<PuzzleState> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
