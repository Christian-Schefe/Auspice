using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public abstract class PlayerEntity : PuzzleEntity
{
    public Vector2Int? slidingDirection;

    public PlayerEntity(Vector2Int position, PlayerType playerType) : base(position, new EntityType(PuzzleEntityType.Player, playerType: playerType))
    {
        slidingDirection = null;
    }

    public abstract List<Vector2Int> GetMovePositions(Puzzle puzzle);

    public static PlayerEntity CreatePlayer(PlayerType playerType, Vector2Int position)
    {
        return playerType switch
        {
            PlayerType.Crab => new CrabPlayer(position),
            PlayerType.Octopus => new OctopusPlayer(position),
            PlayerType.Fish => new FishPlayer(position),
            PlayerType.Starfish => new StarfishPlayer(position),
            _ => throw new System.Exception("Invalid player type"),
        };
    }
}

[IsDerivedClass("CrabPlayer")]
public class CrabPlayer : PlayerEntity
{
    public CrabPlayer() : this(Vector2Int.zero) { }

    public CrabPlayer(Vector2Int position) : base(position, PlayerType.Crab) { }

    public override List<Vector2Int> GetMovePositions(Puzzle puzzle)
    {
        var dirs = new List<Vector2Int>
        {
            new(0, 0),
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0),
        };

        var movePositions = new List<Vector2Int>();
        foreach (var dir in dirs)
        {
            var movePosition = position + dir;
            movePositions.Add(movePosition);
        }
        return movePositions;
    }
}

[IsDerivedClass("OctopusPlayer")]
public class OctopusPlayer : PlayerEntity
{
    public OctopusPlayer() : this(Vector2Int.zero) { }

    public OctopusPlayer(Vector2Int position) : base(position, PlayerType.Octopus) { }

    public override List<Vector2Int> GetMovePositions(Puzzle puzzle)
    {
        var dirs = new List<Vector2Int>
        {
            new(0, 0),
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0),
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1),
        };

        var movePositions = new List<Vector2Int>();
        foreach (var dir in dirs)
        {
            var movePosition = position + dir;
            movePositions.Add(movePosition);
        }
        return movePositions;
    }
}

[IsDerivedClass("FishPlayer")]
public class FishPlayer : PlayerEntity
{
    public FishPlayer() : this(Vector2Int.zero) { }

    public FishPlayer(Vector2Int position) : base(position, PlayerType.Fish) { }

    public override List<Vector2Int> GetMovePositions(Puzzle puzzle)
    {
        var dirs = new List<Vector2Int>
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(-1, -1),
        };

        var movePositions = new List<Vector2Int>();
        foreach (var dir in dirs)
        {
            var movePosition = position + dir;
            movePositions.Add(movePosition);
        }
        return movePositions;
    }
}

[IsDerivedClass("StarfishPlayer")]
public class StarfishPlayer : PlayerEntity
{
    public StarfishPlayer() : this(Vector2Int.zero) { }

    public StarfishPlayer(Vector2Int position) : base(position, PlayerType.Starfish) { }

    public override List<Vector2Int> GetMovePositions(Puzzle puzzle)
    {
        var dirs = new List<Vector2Int>
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -1),
        };

        var movePositions = new List<Vector2Int>();
        foreach (var dir in dirs)
        {
            var movePosition = position + dir;

            if (puzzle.HasEntity(movePosition, PuzzleEntityType.Wall))
            {
                movePosition += dir;
            }
            movePositions.Add(movePosition);
        }
        return movePositions;
    }
}