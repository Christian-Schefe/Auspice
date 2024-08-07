using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType
{
    Button,
    Player,
}

public abstract class PuzzleEntity
{
    public Vector2Int position;

    public abstract override bool Equals(object obj);
    public abstract override int GetHashCode();
    public abstract EntityType GetEntityType();
}

public class ButtonEntity : PuzzleEntity
{
    public bool isPressed;

    public ButtonEntity(Vector2Int position)
    {
        this.position = position;
        isPressed = false;
    }

    public override bool Equals(object obj)
    {
        if (obj is not ButtonEntity other)
        {
            return false;
        }
        return position == other.position && isPressed == other.isPressed;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(position, isPressed);
    }

    public override EntityType GetEntityType()
    {
        return EntityType.Button;
    }
}
