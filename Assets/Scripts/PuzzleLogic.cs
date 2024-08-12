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
        var crates = puzzle.GetEntities<GenericEntity>(PuzzleEntityType.Crate);

        var oldPositions = new List<Vector2Int>();

        for (int i = 0; i < players.Count; i++)
        {
            var dir = to[i] - players[i].position;

            if (usedPositions.Contains(to[i])) return false;
            if (!CanWalk(puzzle, to[i], dir)) return false;

            var oldPos = players[i].position;
            oldPositions.Add(oldPos);
            usedPositions.Add(to[i]);
            players[i].position = to[i];

            if (puzzle.HasEntity(to[i], PuzzleEntityType.Crate, out var crateEntity))
            {
                var crateTo = to[i] + dir;
                if (usedPositions.Contains(crateTo)) return false;

                crateEntity.position = crateTo;
                usedPositions.Add(crateTo);
            }
        }

        var pushables = new List<PuzzleEntity>();
        pushables.AddRange(players);
        pushables.AddRange(crates);
        if (!DoShift(puzzle, pushables)) return false;

        for (int i = 0; i < players.Count; i++)
        {
            if (oldPositions[i] != to[i] && puzzle.HasEntity(to[i], PuzzleEntityType.Button, out var buttonEntity))
            {
                var button = (ButtonEntity)buttonEntity;
                button.isPressed = !button.isPressed;
            }
        }

        if (!TestSpikes(puzzle, players)) return false;

        return true;
    }

    private bool TestSpikes(Puzzle puzzle, List<PlayerEntity> players)
    {
        foreach (var player in players)
        {
            if (puzzle.HasEntity(player.position, PuzzleEntityType.Spike, out var spike))
            {
                var type = spike.GetEntityType();
                bool buttonToggleState = puzzle.GetButtonToggleState(type.buttonColor);

                if (type.spikeInitialState != buttonToggleState)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool DoShift(Puzzle puzzle, List<PuzzleEntity> pushables)
    {
        var usedPositions = new HashSet<Vector2Int>();
        foreach (var pushable in pushables)
        {
            if (puzzle.HasEntity(pushable.position, PuzzleEntityType.Conveyor, out var conveyor))
            {
                var dir = conveyor.GetEntityType().direction switch
                {
                    Direction.Up => Vector2Int.up,
                    Direction.Down => Vector2Int.down,
                    Direction.Left => Vector2Int.left,
                    Direction.Right => Vector2Int.right,
                    _ => Vector2Int.zero
                };
                var to = pushable.position + dir;
                pushable.position = to;

                if (!CanWalkStatic(puzzle, to)) return false;

                if (usedPositions.Contains(to)) return false;
                usedPositions.Add(to);
            }
            else
            {
                if (usedPositions.Contains(pushable.position)) return false;
                usedPositions.Add(pushable.position);
            }
        }

        return true;
    }

    private bool CanWalkStatic(Puzzle puzzle, Vector2Int pos)
    {
        if (!puzzle.IsValidPosition(pos))
        {
            return false;
        }
        if (puzzle.HasEntity(pos, PuzzleEntityType.Wall))
        {
            return false;
        }
        return true;
    }

    private bool CanWalk(Puzzle puzzle, Vector2Int pos, Vector2Int dir, bool allowCratePush = true)
    {
        if (!CanWalkStatic(puzzle, pos))
        {
            return false;
        }
        if (puzzle.HasEntity(pos, PuzzleEntityType.Crate))
        {
            return allowCratePush && CanWalkStatic(puzzle, pos + dir);
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
