using System;
using UnityEngine;

public struct PlayerState : IEquatable<PlayerState>
{
    public Vector2Int position;
    public Vector2Int? slidingDirection;

    public PlayerState(Vector2Int position, Vector2Int? slidingDirection)
    {
        this.position = position;
        this.slidingDirection = slidingDirection;
    }

    public override readonly string ToString()
    {
        return $"Player({position}, {slidingDirection})";

    }

    public override readonly bool Equals(object obj)
    {
        return obj is PlayerState other
            && position == other.position
            && slidingDirection == other.slidingDirection;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(position, slidingDirection);
    }

    public readonly bool Equals(PlayerState other)
    {
        return position == other.position && slidingDirection == other.slidingDirection;
    }

    public static bool operator ==(PlayerState left, PlayerState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerState left, PlayerState right)
    {
        return !(left == right);
    }
}

public struct PuzzleState
{
    public PlayerState[] playerStates;
    public bool[] buttonStates;

    public PuzzleState(PlayerState[] playerStates, bool[] buttonStates)
    {
        this.playerStates = playerStates;
        this.buttonStates = buttonStates;
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is not PuzzleState other)
        {
            return false;
        }
        if (playerStates.Length != other.playerStates.Length || buttonStates.Length != other.buttonStates.Length)
        {
            return false;
        }
        for (int i = 0; i < playerStates.Length; i++)
        {
            if (playerStates[i] != other.playerStates[i]) return false;
        }
        for (int i = 0; i < buttonStates.Length; i++)
        {
            if (buttonStates[i] != other.buttonStates[i]) return false;
        }
        return true;
    }

    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var state in playerStates)
        {
            hash.Add(state);
        }
        foreach (var buttonState in buttonStates)
        {
            hash.Add(buttonState);
        }
        return hash.ToHashCode();
    }
}

public struct ReducedPuzzleState
{
    public PlayerState[] playerStates;
    public bool[] colorStates;

    public ReducedPuzzleState(PlayerState[] playerStates, bool[] colorStates)
    {
        this.playerStates = playerStates;
        this.colorStates = colorStates;
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is not ReducedPuzzleState other)
        {
            return false;
        }
        if (playerStates.Length != other.playerStates.Length || colorStates.Length != other.colorStates.Length)
        {
            return false;
        }
        for (int i = 0; i < playerStates.Length; i++)
        {
            if (playerStates[i] != other.playerStates[i]) return false;
        }
        for (int i = 0; i < colorStates.Length; i++)
        {
            if (colorStates[i] != other.colorStates[i]) return false;
        }
        return true;
    }

    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var state in playerStates)
        {
            hash.Add(state);
        }
        foreach (var colorState in colorStates)
        {
            hash.Add(colorState);
        }
        return hash.ToHashCode();
    }
}