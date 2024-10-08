using System.Collections.Generic;
using System;
using UnityEngine;

public enum ButtonColor
{
    Red, Blue
}

public enum PlayerType
{
    Crab, Octopus, Fish, Starfish, Penguin
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
    PressurePlate,
    Portal
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

    public static EntityType None => new(PuzzleEntityType.None);
    public static EntityType Wall => new(PuzzleEntityType.Wall);
    public static EntityType Chest => new(PuzzleEntityType.Chest);
    public static EntityType RedOffSpike => new(PuzzleEntityType.Spike, buttonColor: ButtonColor.Red, spikeInitialState: false);
    public static EntityType RedOnSpike => new(PuzzleEntityType.Spike, buttonColor: ButtonColor.Red, spikeInitialState: true);
    public static EntityType BlueOffSpike => new(PuzzleEntityType.Spike, buttonColor: ButtonColor.Blue, spikeInitialState: false);
    public static EntityType BlueOnSpike => new(PuzzleEntityType.Spike, buttonColor: ButtonColor.Blue, spikeInitialState: true);
    public static EntityType RedButton => new(PuzzleEntityType.Button, buttonColor: ButtonColor.Red);
    public static EntityType BlueButton => new(PuzzleEntityType.Button, buttonColor: ButtonColor.Blue);
    public static EntityType Crab => new(PuzzleEntityType.Player, playerType: PlayerType.Crab);
    public static EntityType Octopus => new(PuzzleEntityType.Player, playerType: PlayerType.Octopus);
    public static EntityType Fish => new(PuzzleEntityType.Player, playerType: PlayerType.Fish);
    public static EntityType Starfish => new(PuzzleEntityType.Player, playerType: PlayerType.Starfish);
    public static EntityType Penguin => new(PuzzleEntityType.Player, playerType: PlayerType.Penguin);
    public static EntityType Ice => new(PuzzleEntityType.Ice);
    public static EntityType RightConveyor => new(PuzzleEntityType.Conveyor, direction: Direction.Right);
    public static EntityType LeftConveyor => new(PuzzleEntityType.Conveyor, direction: Direction.Left);
    public static EntityType UpConveyor => new(PuzzleEntityType.Conveyor, direction: Direction.Up);
    public static EntityType DownConveyor => new(PuzzleEntityType.Conveyor, direction: Direction.Down);
    public static EntityType RedPressurePlate => new(PuzzleEntityType.PressurePlate, buttonColor: ButtonColor.Red);
    public static EntityType BluePressurePlate => new(PuzzleEntityType.PressurePlate, buttonColor: ButtonColor.Blue);
    public static EntityType Portal => new(PuzzleEntityType.Portal);
}

public enum BuildEntityType
{
    Eraser, Wall, Chest, RedSpike, BlueSpike, RedButton, BlueButton, Crab, Octopus, Fish, Starfish, Penguin, Ice, Conveyor, RedPressurePlate, BluePressurePlate, Portal
}

public static class BuildEntityTypeRef
{
    private static Dictionary<EntityType, BuildEntityType> fromEntityType;
    private static Dictionary<BuildEntityType, List<EntityType>> toEntityType;

    private static List<BuildEntityType> allTypes;

    public static BuildEntityType GetBuildType(EntityType type)
    {
        if (fromEntityType == null) BuildTypeReference();
        return fromEntityType[type];
    }

    public static List<EntityType> GetEntityTypes(BuildEntityType type)
    {
        if (toEntityType == null) BuildTypeReference();
        return toEntityType[type];
    }

    public static List<BuildEntityType> GetAllTypes()
    {
        if (allTypes == null) BuildTypeReference();
        return allTypes;
    }

    private static void BuildTypeReference()
    {
        toEntityType = new Dictionary<BuildEntityType, List<EntityType>>()
        {
            {BuildEntityType.Eraser, new List<EntityType> {EntityType.None}},
            {BuildEntityType.Wall, new List<EntityType> {EntityType.Wall}},
            {BuildEntityType.Chest, new List<EntityType> {EntityType.Chest}},
            {BuildEntityType.RedSpike, new List<EntityType> {EntityType.RedOnSpike, EntityType.RedOffSpike}},
            {BuildEntityType.BlueSpike, new List<EntityType> {EntityType.BlueOnSpike, EntityType.BlueOffSpike}},
            {BuildEntityType.RedButton, new List<EntityType> {EntityType.RedButton}},
            {BuildEntityType.BlueButton, new List<EntityType> {EntityType.BlueButton}},
            {BuildEntityType.Crab, new List<EntityType> {EntityType.Crab}},
            {BuildEntityType.Octopus, new List<EntityType> {EntityType.Octopus}},
            {BuildEntityType.Fish, new List<EntityType> {EntityType.Fish}},
            {BuildEntityType.Starfish, new List<EntityType> {EntityType.Starfish}},
            {BuildEntityType.Penguin, new List<EntityType> {EntityType.Penguin}},
            {BuildEntityType.Ice, new List<EntityType> {EntityType.Ice}},
            {BuildEntityType.Conveyor, new List<EntityType> {EntityType.UpConveyor, EntityType.RightConveyor, EntityType.DownConveyor, EntityType.LeftConveyor}},
            {BuildEntityType.RedPressurePlate, new List<EntityType> {EntityType.RedPressurePlate}},
            {BuildEntityType.BluePressurePlate, new List<EntityType> {EntityType.BluePressurePlate}},
            {BuildEntityType.Portal, new List<EntityType> {EntityType.Portal} }
        };

        fromEntityType = new Dictionary<EntityType, BuildEntityType>();
        foreach (var (key, value) in toEntityType)
        {
            foreach (var entityType in value)
            {
                fromEntityType[entityType] = key;
            }
        }

        allTypes = new(toEntityType.Keys);
    }
}