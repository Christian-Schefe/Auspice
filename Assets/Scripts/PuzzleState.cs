using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PuzzleState
{
    public Vector2Int[] playerPosition;
    public bool[] buttonStates;

    public override readonly string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Puzzle(");
        for (int i = 0; i < playerPosition.Length; i++)
        {
            sb.Append($"Player {i + 1}: {playerPosition[i]}");
            if (i < playerPosition.Length - 1)
            {
                sb.Append(", ");
            }
        }
        for (int i = 0; i < buttonStates.Length; i++)
        {
            sb.Append($"Button {i + 1}: {buttonStates[i]}");
            if (i < buttonStates.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(")");
        return sb.ToString();
    }

    public PuzzleState(Vector2Int[] playerPosition, bool[] buttonStates)
    {
        this.playerPosition = playerPosition;
        this.buttonStates = buttonStates;
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is not PuzzleState other)
        {
            return false;
        }
        if (playerPosition.Length != other.playerPosition.Length || buttonStates.Length != other.buttonStates.Length)
        {
            return false;
        }
        for (int i = 0; i < playerPosition.Length; i++)
        {
            if (playerPosition[i] != other.playerPosition[i])
            {
                return false;
            }
        }
        for (int i = 0; i < buttonStates.Length; i++)
        {
            if (buttonStates[i] != other.buttonStates[i])
            {
                return false;
            }
        }
        return true;
    }

    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var position in playerPosition)
        {
            hash.Add(position);
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
    public int[] playerPositions;
    public bool[] buttonStates;

    public ReducedPuzzleState(int[] playerPositions, bool[] buttonStates)
    {
        this.playerPositions = playerPositions;
        this.buttonStates = buttonStates;
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is not ReducedPuzzleState other)
        {
            return false;
        }
        if (playerPositions.Length != other.playerPositions.Length || buttonStates.Length != other.buttonStates.Length)
        {
            return false;
        }
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (playerPositions[i] != other.playerPositions[i])
            {
                return false;
            }
        }
        for (int i = 0; i < buttonStates.Length; i++)
        {
            if (buttonStates[i] != other.buttonStates[i])
            {
                return false;
            }
        }
        return true;
    }

    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var position in playerPositions)
        {
            hash.Add(position);
        }
        foreach (var buttonState in buttonStates)
        {
            hash.Add(buttonState);
        }
        return hash.ToHashCode();
    }
}