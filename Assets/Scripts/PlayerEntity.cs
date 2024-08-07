using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerEntity : PuzzleEntity
{
    public abstract List<Vector2Int> GetMovePositions(Puzzle puzzle);
    public override EntityType GetEntityType()
    {
        return EntityType.Player;
    }
}

public class CrabPlayer : PlayerEntity
{
    public CrabPlayer(Vector2Int position)
    {
        this.position = position;
    }

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

    public override bool Equals(object obj)
    {
        if (obj is not CrabPlayer other)
        {
            return false;
        }
        return position == other.position;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}