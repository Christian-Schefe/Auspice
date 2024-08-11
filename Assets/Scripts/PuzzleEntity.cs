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
    Crate
}

[Serializable]
public struct EntityType : IEquatable<EntityType>
{
    public PuzzleEntityType basicType;
    public ButtonColor buttonColor;
    public bool spikeInitialState;
    public PlayerType playerType;

    public EntityType(PuzzleEntityType basicType, ButtonColor buttonColor = ButtonColor.Red, bool spikeInitialState = true, PlayerType playerType = PlayerType.Crab)
    {
        this.basicType = basicType;
        this.buttonColor = buttonColor;
        this.spikeInitialState = spikeInitialState;
        this.playerType = playerType;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is EntityType type && Equals(type);
    }

    public readonly bool Equals(EntityType other)
    {
        if (basicType != other.basicType) return false;
        if (basicType == PuzzleEntityType.Button && buttonColor != other.buttonColor) return false;
        if (basicType == PuzzleEntityType.Spike && (buttonColor != other.buttonColor || spikeInitialState != other.spikeInitialState)) return false;
        if (basicType == PuzzleEntityType.Player && playerType != other.playerType) return false;

        return true;
    }

    public override readonly int GetHashCode()
    {
        if (basicType == PuzzleEntityType.Button) return HashCode.Combine(basicType, buttonColor);
        if (basicType == PuzzleEntityType.Spike) return HashCode.Combine(basicType, buttonColor, spikeInitialState);
        if (basicType == PuzzleEntityType.Player) return HashCode.Combine(basicType, playerType);
        return HashCode.Combine(basicType);
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
        if (basicType == PuzzleEntityType.Button) return $"ButtonType({buttonColor})";
        if (basicType == PuzzleEntityType.Spike) return $"SpikeType({buttonColor}, {spikeInitialState})";
        if (basicType == PuzzleEntityType.Player) return $"PlayerType({playerType})";
        return basicType.ToString() + "Type";
    }
}
