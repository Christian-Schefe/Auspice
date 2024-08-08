using System;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    private readonly HashSet<Vector2Int> positions;
    private readonly Dictionary<Vector2Int, Dictionary<PuzzleEntityType, PuzzleEntity>> entitiesByPosition = new();
    private readonly Dictionary<PuzzleEntityType, List<PuzzleEntity>> entitiesByType = new();
    private readonly Dictionary<PuzzleEntity, Vector2Int> storedEntityPosition = new();
    private readonly Dictionary<ButtonColor, bool> buttonStates = new();

    public Puzzle(PuzzleData data)
    {
        positions = new(data.positions);

        foreach (var pos in data.positions)
        {
            if (data.entities.ContainsKey(pos))
            {
                entitiesByPosition.Add(pos, new());

                foreach (var (type, (entity, _)) in data.entities[pos])
                {
                    if (!entitiesByType.ContainsKey(type.basicType)) entitiesByType.Add(type.basicType, new() { entity });
                    else entitiesByType[type.basicType].Add(entity);

                    entitiesByPosition[pos].Add(type.basicType, entity);
                    storedEntityPosition.Add(entity, pos);
                }
            }
        }

        foreach (var buttonColor in ButtonEntity.buttonColors)
        {
            buttonStates.Add(buttonColor, false);
        }
    }

    public bool GetButtonToggleState(ButtonColor color)
    {
        return buttonStates[color];
    }

    public List<Vector2Int> GetEntityPositions(PuzzleEntityType type)
    {
        var entities = new List<Vector2Int>();
        if (entitiesByType.TryGetValue(type, out var list))
        {
            foreach (var entity in list)
            {
                entities.Add(entity.position);
            }
        }
        return entities;
    }

    public List<T> GetEntities<T>(PuzzleEntityType type) where T : PuzzleEntity
    {
        var entities = new List<T>();
        if (entitiesByType.TryGetValue(type, out var list))
        {
            foreach (var entity in list)
            {
                entities.Add((T)entity);
            }
        }
        return entities;
    }

    public PuzzleState GetState()
    {
        var players = GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = GetEntities<ButtonEntity>(PuzzleEntityType.Button);

        var playerPosition = new Vector2Int[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            playerPosition[i] = player.position;
        }

        var buttonStates = new bool[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            buttonStates[i] = button.isPressed;
        }

        return new PuzzleState(playerPosition, buttonStates);
    }

    public bool IsWon()
    {
        var players = GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        foreach (var player in players)
        {
            if (!HasEntity(player.position, PuzzleEntityType.Chest))
            {
                return false;
            }
        }
        return true;
    }

    public void SetState(PuzzleState state)
    {
        var players = GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = GetEntities<ButtonEntity>(PuzzleEntityType.Button);

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            player.position = state.playerPosition[i];
            UpdateEntityPosition(player);
        }

        var buttonPressCounts = new Dictionary<ButtonColor, int>();
        foreach (var buttonColor in ButtonEntity.buttonColors)
        {
            buttonPressCounts.Add(buttonColor, 0);
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            var buttonType = button.GetEntityType();
            button.isPressed = state.buttonStates[i];
            if (button.isPressed)
            {
                buttonPressCounts[buttonType.buttonColor]++;
            }
        }

        foreach (var buttonColor in ButtonEntity.buttonColors)
        {
            buttonStates[buttonColor] = buttonPressCounts[buttonColor] % 2 == 1;
        }
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return positions.Contains(position);
    }

    public bool HasEntity(Vector2Int position, PuzzleEntityType entityType)
    {
        return entitiesByPosition.TryGetValue(position, out var dict) && dict.ContainsKey(entityType);
    }

    public bool HasEntity(Vector2Int position, PuzzleEntityType type, out PuzzleEntity entity)
    {
        entity = null;
        return entitiesByPosition.TryGetValue(position, out var dict) && dict.TryGetValue(type, out entity);
    }

    public void UpdateEntityPosition(PuzzleEntity entity)
    {
        var type = entity.GetEntityType();
        var from = storedEntityPosition[entity];
        var to = entity.position;
        if (from == to) return;

        entitiesByPosition[from].Remove(type.basicType);
        if (entitiesByPosition[from].Count == 0)
        {
            entitiesByPosition.Remove(from);
        }

        if (!entitiesByPosition.ContainsKey(to))
        {
            entitiesByPosition.Add(to, new());
        }
        entitiesByPosition[to].Add(type.basicType, entity);

        storedEntityPosition[entity] = to;
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