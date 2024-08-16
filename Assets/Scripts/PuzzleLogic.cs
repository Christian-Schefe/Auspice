using System.Collections.Generic;
using UnityEngine;

public class PuzzleLogic
{
    private readonly Puzzle puzzle;

    public PuzzleLogic(Puzzle puzzle)
    {
        this.puzzle = puzzle;
    }

    public List<(PuzzleState, List<TurnEvent>)> GetNextStates(PuzzleState state)
    {
        puzzle.SetState(state);

        var moves = new List<List<Vector2Int>>();
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].slidingDirection is Vector2Int slidingDir)
            {
                var slideTo = players[i].position + slidingDir;
                if (CanWalkStatic(puzzle, slideTo))
                {
                    moves.Add(new List<Vector2Int> { slideTo });
                    continue;
                }
            }

            var playerMoves = players[i].GetMovePositions(puzzle);
            var validMoves = new List<Vector2Int>();
            foreach (var move in playerMoves)
            {
                if (CanWalkStatic(puzzle, move))
                {
                    validMoves.Add(move);
                }
            }
            moves.Add(validMoves);
        }

        var permutations = GetUniqueChoicePermutations(moves);

        var nextStates = new List<(PuzzleState, List<TurnEvent>)>();

        foreach (var moveChoice in permutations)
        {
            if (TryMovePlayers(state, moveChoice, out var nextState, out var turnEvents))
            {
                nextStates.Add((nextState, turnEvents));
            }
        }

        return nextStates;
    }

    public bool TryMovePlayers(PuzzleState state, List<Vector2Int> to, out PuzzleState nextState, out List<TurnEvent> turnEvents)
    {
        nextState = default;
        turnEvents = new();

        puzzle.SetState(state);
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);

        var oldPositions = new List<Vector2Int>();

        for (int i = 0; i < players.Count; i++)
        {
            oldPositions.Add(players[i].position);
            players[i].position = to[i];
        }

        if (!DoShift(puzzle, players, turnEvents, out var shiftPositions)) return false;
        if (!DoPortal(puzzle, players, turnEvents)) return false;

        for (int i = 0; i < players.Count; i++)
        {
            var playerPos = players[i].position;
            if (puzzle.HasEntity(playerPos, PuzzleEntityType.Ice))
            {
                var dir = shiftPositions[i] ?? (playerPos - oldPositions[i]);
                dir.Clamp(new Vector2Int(-1, -1), new Vector2Int(1, 1));
                players[i].slidingDirection = dir;
            }
            else
            {
                players[i].slidingDirection = null;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            var playerPos = players[i].position;
            if (oldPositions[i] != playerPos && puzzle.HasEntity(playerPos, PuzzleEntityType.Button, out var buttonEntity))
            {
                var button = (ButtonEntity)buttonEntity;
                button.isPressed = !button.isPressed;
            }
        }

        puzzle.UpdateState();
        if (!TestSpikes(puzzle, players)) return false;

        nextState = puzzle.GetState();

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

    private bool DoShift(Puzzle puzzle, List<PlayerEntity> players, List<TurnEvent> turnEvents, out Vector2Int?[] shiftDirections)
    {
        var usedPositions = new HashSet<Vector2Int>();
        shiftDirections = new Vector2Int?[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            Vector2Int playerTo = player.position;

            if (puzzle.HasEntity(player.position, PuzzleEntityType.Conveyor, out var conveyor))
            {
                var dir = conveyor.GetEntityType().direction.ToVec();
                var shiftTo = player.position + dir;
                if (CanWalkStatic(puzzle, shiftTo))
                {
                    playerTo = shiftTo;
                    shiftDirections[i] = dir;
                    turnEvents.Add(new ShiftEvent(player, player.position));
                }
            }

            if (!usedPositions.Add(playerTo)) return false;
            player.position = playerTo;
        }

        return true;
    }

    private bool DoPortal(Puzzle puzzle, List<PlayerEntity> players, List<TurnEvent> turnEvents)
    {
        var usedPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            Vector2Int playerTo = player.position;

            if (puzzle.HasEntity<PortalEntity>(player.position, PuzzleEntityType.Portal, out var portal))
            {
                var teleportTo = portal.destination;
                if (!CanWalkStatic(puzzle, teleportTo)) return false;

                playerTo = teleportTo;
                turnEvents.Add(new TeleportEvent(player, player.position));
            }

            if (!usedPositions.Add(playerTo)) return false;
            player.position = playerTo;
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

    private static List<List<T>> GetUniqueChoicePermutations<T>(List<List<T>> source)
    {
        var result = new List<List<T>>();
        var current = new List<T>();
        var used = new HashSet<T>();

        void Generate(int depth)
        {
            if (depth == source.Count)
            {
                result.Add(new(current));
                return;
            }

            foreach (var item in source[depth])
            {
                if (!used.Add(item)) continue;
                current.Add(item);

                Generate(depth + 1);

                current.RemoveAt(current.Count - 1);
                used.Remove(item);
            }
        }

        Generate(0);

        return result;
    }
}
