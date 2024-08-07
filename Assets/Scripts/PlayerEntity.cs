using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public abstract class PlayerEntity : PuzzleEntity
{
    public Vector2Int originalPosition;

    public PlayerEntity() : this(Vector2Int.zero)
    {
        originalPosition = Vector2Int.zero;
    }

    public PlayerEntity(Vector2Int position) : base(position)
    {
        originalPosition = position;
    }

    public abstract List<Vector2Int> GetMovePositions(Puzzle puzzle);
    public override EntityType GetEntityType()
    {
        return EntityType.Player;
    }
}

[IsDerivedClass("CrabPlayer")]
public class CrabPlayer : PlayerEntity
{

    public CrabPlayer() : this(Vector2Int.zero) { }

    public CrabPlayer(Vector2Int position) : base(position) { }

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