using System;
using UnityEngine;
using Yeast;

[HasDerivedClasses(typeof(GenericEntity), typeof(ButtonEntity), typeof(PressurePlateEntity), typeof(PortalEntity), typeof(PlayerEntity))]
public abstract class PuzzleEntity
{
    public Vector2Int position;
    public EntityType entityType;

    public PuzzleEntity(Vector2Int position, EntityType entityType)
    {
        this.position = position;
        this.entityType = entityType;
    }

    public EntityType GetEntityType() => entityType;

    public abstract PuzzleEntity Clone();
}

[IsDerivedClass("GenericEntity")]
public class GenericEntity : PuzzleEntity
{
    public GenericEntity() : this(Vector2Int.zero, new EntityType()) { }

    public GenericEntity(Vector2Int position, EntityType type) : base(position, type) { }

    public override PuzzleEntity Clone() => new GenericEntity(position, entityType);
}

[IsDerivedClass("ButtonEntity")]
public class ButtonEntity : PuzzleEntity
{
    public bool isPressed;

    public ButtonEntity() : this(ButtonColor.Red, Vector2Int.zero) { }

    public ButtonEntity(ButtonColor color, Vector2Int position) : base(position, new EntityType(PuzzleEntityType.Button, buttonColor: color))
    {
        isPressed = false;
    }

    public static ButtonColor[] buttonColors = new ButtonColor[] { ButtonColor.Red, ButtonColor.Blue };

    public override PuzzleEntity Clone()
    {
        return new ButtonEntity(entityType.buttonColor, position)
        {
            isPressed = isPressed
        };
    }
}

[IsDerivedClass("PressurePlateEntity")]
public class PressurePlateEntity : PuzzleEntity
{
    [NonSerialized] public bool isPressed;

    public PressurePlateEntity() : this(ButtonColor.Red, Vector2Int.zero) { }

    public PressurePlateEntity(ButtonColor color, Vector2Int position) : base(position, new EntityType(PuzzleEntityType.PressurePlate, buttonColor: color))
    {
        isPressed = false;
    }

    public override PuzzleEntity Clone()
    {
        return new PressurePlateEntity(entityType.buttonColor, position)
        {
            isPressed = isPressed
        };
    }
}

[IsDerivedClass("PortalEntity")]
public class PortalEntity : PuzzleEntity
{
    public Vector2Int destination;

    public PortalEntity() : this(Vector2Int.zero, Vector2Int.zero) { }

    public PortalEntity(Vector2Int position, Vector2Int destination) : base(position, new EntityType(PuzzleEntityType.Portal))
    {
        this.destination = destination;
    }

    public override PuzzleEntity Clone()
    {
        return new PortalEntity(position, destination)
        {
            destination = destination
        };
    }
}

[IsDerivedClass("PlayerEntity")]
public class PlayerEntity : PuzzleEntity
{
    [NonSerialized] public Vector2Int? slidingDirection;

    public PlayerEntity() : this(PlayerType.Crab, Vector2Int.zero) { }

    public PlayerEntity(PlayerType playerType, Vector2Int position) : base(position, new EntityType(PuzzleEntityType.Player, playerType: playerType))
    {
        slidingDirection = null;
    }

    public override PuzzleEntity Clone()
    {
        return new PlayerEntity(entityType.playerType, position)
        {
            slidingDirection = slidingDirection
        };
    }
}