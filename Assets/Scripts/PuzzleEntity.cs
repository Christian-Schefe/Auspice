using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;

public enum EntityType
{
    Button,
    Player,
}

[HasDerivedClasses(typeof(ButtonEntity), typeof(CrabPlayer))]
public abstract class PuzzleEntity
{
    public Vector2Int position;

    public PuzzleEntity(Vector2Int position)
    {
        this.position = position;
    }
    public abstract EntityType GetEntityType();
}

[IsDerivedClass("ButtonEntity")]
public class ButtonEntity : PuzzleEntity
{
    public bool isPressed;

    public ButtonEntity() : this(Vector2Int.zero) { }

    public ButtonEntity(Vector2Int position) : base(position)
    {
        isPressed = false;
    }

    public override EntityType GetEntityType()
    {
        return EntityType.Button;
    }
}
