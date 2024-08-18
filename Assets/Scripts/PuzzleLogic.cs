using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yeast;

public class PuzzleLogic
{
    private readonly Puzzle puzzle;

    private static readonly List<PlayerMove> fourDirs = new() {
        new NoMove(),
        new WalkingPlayerMove(new(0, 1), 1),
        new WalkingPlayerMove(new(0, -1), 1),
        new WalkingPlayerMove(new(1, 0), 1),
        new WalkingPlayerMove(new(-1, 0), 1)
    };
    private static readonly List<PlayerMove> eightDirs = new() {
        new NoMove(),
        new WalkingPlayerMove(new(0, 1), 1),
        new WalkingPlayerMove(new(0, -1), 1),
        new WalkingPlayerMove(new(1, 0), 1),
        new WalkingPlayerMove(new(-1, 0), 1),
        new WalkingPlayerMove(new(1, 1), 1),
        new WalkingPlayerMove(new(1, -1), 1),
        new WalkingPlayerMove(new(-1, 1), 1),
        new WalkingPlayerMove(new(-1, -1), 1)
    };
    private static readonly List<PlayerMove> fishDirs = new() {
        new NoMove(),
        new WalkingPlayerMove(new(0, 1), 1),
        new WalkingPlayerMove(new(1, 0), 1),
        new WalkingPlayerMove(new(-1, -1), 1)
    };

    public PuzzleLogic(Puzzle puzzle)
    {
        this.puzzle = puzzle;
    }

    public List<PlayerMove> GetPlayerTypeMoveDirs(PlayerEntity player)
    {
        var type = player.GetEntityType().playerType;
        List<PlayerMove> moves = type switch
        {
            PlayerType.Crab => fourDirs,
            PlayerType.Octopus => eightDirs,
            PlayerType.Fish => fishDirs,
            PlayerType.Starfish => fourDirs,
            PlayerType.Penguin => fourDirs,
            _ => new()
        };

        if (type == PlayerType.Starfish)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                var dir = moves[i];
                if (dir is WalkingPlayerMove m && puzzle.HasEntity(player.position + m.direction, PuzzleEntityType.Wall))
                {
                    moves[i] = new JumpPlayerMove(m.direction * m.steps * 2);
                }
            }
        }

        return moves;
    }

    private List<PlayerMove> GetOnePlayerMoves(PlayerEntity player)
    {
        if (player.slidingDirection is Vector2Int slidingDir)
        {
            var slideTo = player.position + slidingDir;
            if (CanWalkStatic(puzzle, slideTo))
            {
                return new List<PlayerMove> { new WalkingPlayerMove(slidingDir, 1) };
            }
        }

        var playerMoves = GetPlayerTypeMoveDirs(player);
        var validMoves = new List<PlayerMove>();

        foreach (var move in playerMoves)
        {
            if (move.IsValid(puzzle, player.position))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }

    private List<List<PlayerMove>> GetPlayerMovePerms(List<PlayerEntity> players)
    {
        var moves = new List<List<PlayerMove>>();
        for (int i = 0; i < players.Count; i++)
        {
            moves.Add(GetOnePlayerMoves(players[i]));
        }
        return moves;
    }

    public List<(PuzzleState, TurnEvent[])> GetNextStates(PuzzleState state)
    {
        puzzle.SetState(state);

        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var moves = GetPlayerMovePerms(players);
        var permutations = GetUniqueMovePermutations(players, moves);


        var nextStates = new List<(PuzzleState, TurnEvent[])>();

        foreach (var moveChoice in permutations)
        {
            if (!IsValidMovePermutation(players, moves, moveChoice)) continue;
            if (TryMovePlayers(state, moveChoice, out var nextState, out var turnEvents))
            {
                nextStates.Add((nextState, turnEvents.ToArray()));
            }
        }

        return nextStates;
    }

    public bool TryMovePlayers(PuzzleState state, List<PlayerMove> moves, out PuzzleState nextState, out List<TurnEvent> turnEvents)
    {
        nextState = default;
        turnEvents = new();

        puzzle.SetState(state);
        var players = puzzle.GetEntities<PlayerEntity>(PuzzleEntityType.Player);

        var oldPositions = new List<Vector2Int>();

        for (int i = 0; i < players.Count; i++)
        {
            var oldPos = players[i].position;
            oldPositions.Add(oldPos);
            players[i].position = moves[i].CalcDestination(oldPos);
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

    private bool IsValidMovePermutation(List<PlayerEntity> players, List<List<PlayerMove>> choices, List<PlayerMove> movePerm)
    {
        var noMovePenguins = new List<int>();

        Vector2Int? penguinDir = null;
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var move = movePerm[i];

            if (player.GetEntityType().playerType == PlayerType.Penguin)
            {
                if (move is WalkingPlayerMove walkMove)
                {
                    if (penguinDir == null) penguinDir = walkMove.direction;
                    else if (penguinDir != walkMove.direction) return false;
                }
                else if (move is NoMove)
                {
                    noMovePenguins.Add(i);
                }
            }
        }
        if (penguinDir is not Vector2Int dir) return true;

        foreach (var player in noMovePenguins)
        {
            foreach (var choice in choices[player])
            {
                if (choice is WalkingPlayerMove walkMove && walkMove.direction == dir)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static List<List<PlayerMove>> GetUniqueMovePermutations(List<PlayerEntity> players, List<List<PlayerMove>> source)
    {
        var result = new List<List<PlayerMove>>();
        var current = new List<PlayerMove>();
        var used = new HashSet<Vector2Int>();

        void Generate(int depth)
        {
            if (depth == source.Count)
            {
                result.Add(new(current));
                return;
            }

            var player = players[depth];

            foreach (var item in source[depth])
            {
                var dest = item.CalcDestination(player.position);
                if (!used.Add(dest)) continue;
                current.Add(item);

                Generate(depth + 1);

                current.RemoveAt(current.Count - 1);
                used.Remove(dest);
            }
        }

        Generate(0);

        return result;
    }
}
