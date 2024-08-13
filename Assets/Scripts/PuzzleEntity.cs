using System;
using UnityEngine;
using Yeast;


[HasDerivedClasses(typeof(ButtonEntity), typeof(CrabPlayer), typeof(GenericEntity))]
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

    public static PuzzleEntity CreateEntity(EntityType type, Vector2Int position)
    {
        return type.basicType switch
        {
            PuzzleEntityType.Player => PlayerEntity.CreatePlayer(type.playerType, position),
            PuzzleEntityType.Button => new ButtonEntity(type.buttonColor, position),
            _ => new GenericEntity(position, type)
        };
    }
}

[IsDerivedClass("GenericEntity")]
public class GenericEntity : PuzzleEntity
{
    public GenericEntity() : this(Vector2Int.zero, new EntityType()) { }

    public GenericEntity(Vector2Int position, EntityType type) : base(position, type) { }
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
}

public enum ButtonColor
{
    Red, Blue
}

public enum PlayerType
{
    Crab, Octopus, Fish
}

public enum PuzzleEntityType
{
    None,
    Wall,
    Chest,
    Spike,
    Button,
    Player,
    Ice,
    Conveyor,
    PressurePlate
}

public enum Direction
{
    Right, Left, Up, Down
}

public static class DirectionExtensions
{
    public static Vector2Int ToVec(this Direction direction)
    {
        return direction switch
        {
            Direction.Right => Vector2Int.right,
            Direction.Left => Vector2Int.left,
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            _ => Vector2Int.zero
        };
    }
}

[Serializable]
public struct EntityType : IEquatable<EntityType>
{
    public PuzzleEntityType basicType;
    public ButtonColor buttonColor;
    public bool spikeInitialState;
    public PlayerType playerType;
    public Direction direction;

    public EntityType(PuzzleEntityType basicType, ButtonColor buttonColor = ButtonColor.Red, bool spikeInitialState = true, PlayerType playerType = PlayerType.Crab, Direction direction = Direction.Right)
    {
        this.basicType = basicType;
        this.buttonColor = buttonColor;
        this.spikeInitialState = spikeInitialState;
        this.playerType = playerType;
        this.direction = direction;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is EntityType type && Equals(type);
    }

    public readonly bool Equals(EntityType other)
    {
        if (basicType != other.basicType) return false;

        return basicType switch
        {
            PuzzleEntityType.Button => buttonColor == other.buttonColor,
            PuzzleEntityType.Spike => buttonColor == other.buttonColor && spikeInitialState == other.spikeInitialState,
            PuzzleEntityType.Player => playerType == other.playerType,
            PuzzleEntityType.Conveyor => direction == other.direction,
            PuzzleEntityType.PressurePlate => buttonColor == other.buttonColor,
            _ => true
        };
    }

    public override readonly int GetHashCode()
    {
        return basicType switch
        {
            PuzzleEntityType.Button => HashCode.Combine(basicType, buttonColor),
            PuzzleEntityType.Spike => HashCode.Combine(basicType, buttonColor, spikeInitialState),
            PuzzleEntityType.Player => HashCode.Combine(basicType, playerType),
            PuzzleEntityType.Conveyor => HashCode.Combine(basicType, direction),
            PuzzleEntityType.PressurePlate => HashCode.Combine(basicType, buttonColor),
            _ => HashCode.Combine(basicType)
        };
    }

    public static bool operator ==(EntityType left, EntityType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EntityType left, EntityType right)
    {
        return !(left == right);
    }

    public override readonly string ToString()
    {
        return basicType switch
        {
            PuzzleEntityType.Button => $"ButtonType({buttonColor})",
            PuzzleEntityType.Spike => $"SpikeType({buttonColor}, {spikeInitialState})",
            PuzzleEntityType.Player => $"PlayerType({playerType})",
            PuzzleEntityType.Conveyor => $"ConveyorType({direction})",
            PuzzleEntityType.PressurePlate => $"PressurePlateType({buttonColor})",
            _ => basicType.ToString() + "Type"
        };
    }
}
