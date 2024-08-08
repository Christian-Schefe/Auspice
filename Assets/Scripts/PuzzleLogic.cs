using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLogic
{
    private readonly Puzzle puzzle;

    public PuzzleLogic(Puzzle puzzle)
    {
        this.puzzle = puzzle;
    }

    public List<PuzzleState> GetNextStates(PuzzleState state)
    {
        puzzle.SetState(state);

        var moves = new List<List<Vector2Int>>();
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        for (int i = 0; i < players.Count; i++)
        {
            var playerMoves = players[i].GetMovePositions(puzzle);
            moves.Add(playerMoves);
        }

        var permutations = GetChoicePermutations(moves);

        var nextStates = new List<PuzzleState>();

        foreach (var moveChoice in permutations)
        {
            if (TryMovePlayers(state, moveChoice, out var nextState))
            {
                nextStates.Add(nextState);
            }
        }

        return nextStates;
    }

    public bool TryMovePlayers(PuzzleState state, List<Vector2Int> to, out PuzzleState nextState)
    {
        nextState = default;

        var usedPositions = new HashSet<Vector2Int>();
        puzzle.SetState(state);
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        for (int i = 0; i < players.Count; i++)
        {
            if (usedPositions.Contains(to[i]))
            {
                return false;
            }
            if (!CanWalkStatic(puzzle, to[i]))
            {
                return false;
            }

            var oldPos = players[i].position;
            usedPositions.Add(to[i]);
            players[i].position = to[i];

            if (oldPos != to[i] && puzzle.HasEntity(to[i], PuzzleEntityType.Button, out var buttonEntity))
            {
                var button = (ButtonEntity)buttonEntity;
                button.isPressed = !button.isPressed;
            }
        }

        bool buttonToggleState = puzzle.GetButtonToggleState();

        for (int i = 0; i < players.Count; i++)
        {
            if (!buttonToggleState && puzzle.HasObject(to[i], PuzzleEntityType.OnSpike))
            {
                return false;
            }
            else if (buttonToggleState && puzzle.HasObject(to[i], PuzzleEntityType.OffSpike))
            {
                return false;
            }
        }

        nextState = puzzle.GetState();
        return true;
    }

    private bool CanWalkStatic(Puzzle puzzle, Vector2Int pos)
    {
        if (!puzzle.IsValidPosition(pos))
        {
            return false;
        }
        if (puzzle.HasObject(pos, PuzzleEntityType.Wall))
        {
            return false;
        }
        if (puzzle.HasObject(pos, PuzzleEntityType.Water))
        {
            return false;
        }
        return true;
    }

    private static List<List<T>> GetChoicePermutations<T>(List<List<T>> source)
    {
        var result = new List<List<T>>();
        var current = new List<T>();

        void Generate(int depth)
        {
            if (depth == source.Count)
            {
                result.Add(new(current));
                return;
            }

            foreach (var item in source[depth])
            {
                current.Add(item);
                Generate(depth + 1);
                current.RemoveAt(current.Count - 1);
            }
        }

        Generate(0);

        return result;
    }
}
