using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast;


[HasDerivedClasses(typeof(ButtonEntity), typeof(CrabPlayer), typeof(GenericEntity))]
public abstract class PuzzleEntity
{
    public Vector2Int position;
    public PuzzleEntityType entityType;

    public PuzzleEntity(Vector2Int position, PuzzleEntityType entityType)
    {
        this.position = position;
        this.entityType = entityType;
    }

    public PuzzleEntityType GetEntityType() => entityType;
}

[IsDerivedClass("GenericEntity")]
public class GenericEntity : PuzzleEntity
{
    public GenericEntity() : this(Vector2Int.zero, PuzzleEntityType.None) { }

    public GenericEntity(Vector2Int position, PuzzleEntityType type) : base(position, type) { }
}

[IsDerivedClass("ButtonEntity")]
public class ButtonEntity : PuzzleEntity
{
    public bool isPressed;

    public ButtonEntity() : this(Vector2Int.zero) { }

    public ButtonEntity(Vector2Int position) : base(position, PuzzleEntityType.Button)
    {
        isPressed = false;
    }
}
