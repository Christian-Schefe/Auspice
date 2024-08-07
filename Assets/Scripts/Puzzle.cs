using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PuzzleObject
{
    Wall,
    Water,
    Chest,
    OffSpike,
    OnSpike
}

public class Puzzle
{
    private readonly PuzzleData data;

    private readonly Dictionary<PuzzleObject, List<Vector2Int>> objectsByType = new();
    private readonly Dictionary<EntityType, List<PuzzleEntity>> entitiesByType = new();

    public Puzzle(PuzzleData data)
    {
        this.data = data;

        foreach (var pos in data.positions)
        {
            if (data.puzzleObjects.ContainsKey(pos))
            {
                foreach (var obj in data.puzzleObjects[pos])
                {
                    if (!objectsByType.ContainsKey(obj)) objectsByType.Add(obj, new() { pos });
                    else objectsByType[obj].Add(pos);
                }
            }

            if (data.puzzleEntities.ContainsKey(pos))
            {
                foreach (var (type, entity) in data.puzzleEntities[pos])
                {
                    if (!entitiesByType.ContainsKey(type)) entitiesByType.Add(type, new() { entity });
                    else entitiesByType[type].Add(entity);
                }
            }
        }
    }

    public bool GetButtonToggleState()
    {
        int pressedButtonCount = 0;
        var buttons = GetEntities<ButtonEntity>(EntityType.Button);
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            if (button.isPressed) pressedButtonCount++;
        }
        return pressedButtonCount % 2 == 1;
    }

    public List<T> GetEntities<T>(EntityType type) where T : PuzzleEntity
    {
        if (!entitiesByType.ContainsKey(type))
        {
            return new();
        }
        return entitiesByType[type].Cast<T>().ToList();
    }

    public List<Vector2Int> GetObjects(PuzzleObject puzzleObject)
    {
        if (objectsByType.TryGetValue(puzzleObject, out var positions))
        {
            return positions;
        }
        else
        {
            return new();
        }
    }

    public PuzzleState GetState()
    {
        var players = GetEntities<PlayerEntity>(EntityType.Player);
        var buttons = GetEntities<ButtonEntity>(EntityType.Button);

        var playerPosition = new Vector2Int[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            playerPosition[i] = players[i].position;
        }

        var buttonStates = new bool[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            buttonStates[i] = buttons[i].isPressed;
        }

        return new PuzzleState(playerPosition, buttonStates);
    }

    public bool IsWon()
    {
        var players = GetEntities<PlayerEntity>(EntityType.Player);
        foreach (var player in players)
        {
            if (!HasObject(player.position, PuzzleObject.Chest))
            {
                return false;
            }
        }
        return true;
    }

    public void SetState(PuzzleState state)
    {
        var players = GetEntities<PlayerEntity>(EntityType.Player);
        var buttons = GetEntities<ButtonEntity>(EntityType.Button);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].position = state.playerPosition[i];
        }

        int pressedButtonCount = 0;
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            button.isPressed = state.buttonStates[i];
            if (button.isPressed) pressedButtonCount++;
        }
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return data.positions.Contains(position);
    }

    public bool HasObject(Vector2Int position, PuzzleObject puzzleObject)
    {
        return data.puzzleObjects.TryGetValue(position, out var set) && set.Contains(puzzleObject);
    }

    public bool HasEntity(Vector2Int position, EntityType type, out PuzzleEntity entity)
    {
        entity = null;
        return data.puzzleEntities.TryGetValue(position, out var set) && set.TryGetValue(type, out entity);
    }
}

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