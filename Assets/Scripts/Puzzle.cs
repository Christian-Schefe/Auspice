using System;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    private readonly Dictionary<Vector2Int, int> positionIndices;
    private readonly Dictionary<Vector2Int, Dictionary<PuzzleEntityType, PuzzleEntity>> entitiesByPosition = new();
    private readonly Dictionary<PuzzleEntityType, List<PuzzleEntity>> entitiesByType = new();
    private readonly Dictionary<PuzzleEntity, Vector2Int> storedEntityPosition = new();
    private readonly Dictionary<ButtonColor, bool> buttonStates = new();

    public Puzzle(PuzzleData data)
    {
        var positionList = new List<Vector2Int>(data.positions);
        positionList.Sort((a, b) =>
        {
            if (a.x == b.x)
            {
                return a.y.CompareTo(b.y);
            }
            return a.x.CompareTo(b.x);
        });

        positionIndices = new();
        for (int i = 0; i < positionList.Count; i++)
        {
            var pos = positionList[i];
            positionIndices.Add(pos, i);
            entitiesByPosition.Add(pos, new());

            if (data.entities.ContainsKey(pos))
            {
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

    public ReducedPuzzleState GetReducedPuzzleState()
    {
        var players = GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = GetEntities<ButtonEntity>(PuzzleEntityType.Button);
        var pressurePlates = GetEntities<GenericEntity>(PuzzleEntityType.PressurePlate);

        var playerPosition = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            playerPosition[i] = positionIndices[player.position];
        }

        var buttonStates = new bool[ButtonEntity.buttonColors.Length];
        for (int i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            if (button.isPressed)
            {
                var color = (int)button.GetEntityType().buttonColor;
                buttonStates[color] = !buttonStates[color];
            }
        }

        for (int i = 0; i < pressurePlates.Count; i++)
        {
            var pressurePlate = pressurePlates[i];
            if (HasEntity(pressurePlate.position, PuzzleEntityType.Player))
            {
                var color = (int)pressurePlate.GetEntityType().buttonColor;
                buttonStates[color] = !buttonStates[color];
            }
        }

        return new ReducedPuzzleState(playerPosition, buttonStates);
    }

    public void SetState(PuzzleState state)
    {
        var players = GetEntities<PlayerEntity>(PuzzleEntityType.Player);
        var buttons = GetEntities<ButtonEntity>(PuzzleEntityType.Button);
        var pressurePlates = GetEntities<GenericEntity>(PuzzleEntityType.PressurePlate);

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            player.position = state.playerPosition[i];

            var from = storedEntityPosition[player];
            entitiesByPosition[from].Remove(PuzzleEntityType.Player);
        }

        foreach (var player in players)
        {
            entitiesByPosition[player.position].Add(PuzzleEntityType.Player, player);
            storedEntityPosition[player] = player.position;
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

        for (int i = 0; i < pressurePlates.Count; i++)
        {
            var pressurePlate = pressurePlates[i];
            var pressurePlateType = pressurePlate.GetEntityType();
            if (HasEntity(pressurePlate.position, PuzzleEntityType.Player))
            {
                buttonPressCounts[pressurePlateType.buttonColor]++;
            }
        }

        foreach (var buttonColor in ButtonEntity.buttonColors)
        {
            buttonStates[buttonColor] = buttonPressCounts[buttonColor] % 2 == 1;
        }
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

    public bool IsValidPosition(Vector2Int position)
    {
        return positionIndices.ContainsKey(position);
    }

    public bool HasEntity(Vector2Int position, PuzzleEntityType entityType)
    {
        return entitiesByPosition[position].ContainsKey(entityType);
    }

    public bool HasEntity(Vector2Int position, PuzzleEntityType type, out PuzzleEntity entity)
    {
        return entitiesByPosition[position].TryGetValue(type, out entity);
    }
}
