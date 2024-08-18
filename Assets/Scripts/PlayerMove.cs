using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMove
{
    public abstract Vector2Int CalcDestination(Vector2Int position);
    public abstract bool IsValid(Puzzle puzzle, Vector2Int position);

    protected bool CanWalkStatic(Puzzle puzzle, Vector2Int dest)
    {
        return puzzle.IsValidPosition(dest) && !puzzle.HasEntity(dest, PuzzleEntityType.Wall);
    }
}

public class NoMove : PlayerMove
{
    public override Vector2Int CalcDestination(Vector2Int position)
    {
        return position;
    }

    public override bool IsValid(Puzzle puzzle, Vector2Int position)
    {
        return true;
    }
}

public class WalkingPlayerMove : PlayerMove
{
    public Vector2Int direction;
    public int steps;

    public WalkingPlayerMove(Vector2Int direction, int steps)
    {
        this.direction = direction;
        this.steps = steps;
    }

    public override Vector2Int CalcDestination(Vector2Int position)
    {
        return position + direction * steps;
    }

    public override bool IsValid(Puzzle puzzle, Vector2Int position)
    {
        for (int i = 1; i <= steps; i++)
        {
            var dest = position + direction * i;
            if (!CanWalkStatic(puzzle, dest)) return false;
        }

        return true;
    }
}

public class JumpPlayerMove : PlayerMove
{
    public Vector2Int offset;

    public JumpPlayerMove(Vector2Int offset)
    {
        this.offset = offset;
    }

    public override Vector2Int CalcDestination(Vector2Int position)
    {
        return position + offset;
    }

    public override bool IsValid(Puzzle puzzle, Vector2Int position)
    {
        return CanWalkStatic(puzzle, CalcDestination(position));
    }
}